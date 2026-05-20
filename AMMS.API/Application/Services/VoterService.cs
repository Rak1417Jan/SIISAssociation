using MVEA.Application.DTOs.Request;
using MVEA.Application.DTOs.Response;
using MVEA.Application.Interfaces;
using MVEA.Domain.Entities;
using MVEA.Domain.Interfaces;

namespace MVEA.Application.Services;

/// <summary>
/// Voter service implementation with Unit of Work pattern
/// </summary>
public class VoterService : IVoterService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IVoterRepository _voterRepository;
    private readonly IFamilyMemberRepository _familyMemberRepository;
    private readonly IAssemblyRepository _assemblyRepository;
    private readonly IBoothRepository? _boothRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<VoterService> _logger;

    public VoterService(
        IUnitOfWork unitOfWork,
        IVoterRepository voterRepository,
        IFamilyMemberRepository familyMemberRepository,
        IAssemblyRepository assemblyRepository,
        IUserRepository userRepository,
        ILogger<VoterService> logger)
    {
        _unitOfWork = unitOfWork;
        _voterRepository = voterRepository;
        _familyMemberRepository = familyMemberRepository;
        _assemblyRepository = assemblyRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<VoterVerificationResponse> VerifyVoterAsync(VerifyVoterRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Verify assembly exists
            var assembly = await _assemblyRepository.GetByIdAsync(request.AssemblyId, cancellationToken);
            if (assembly == null)
            {
                return new VoterVerificationResponse
                {
                    IsVerified = false,
                    Message = "Invalid Assembly ID"
                };
            }

            // Verify booth exists (optional check)
            if (_boothRepository != null)
            {
                var booth = await _boothRepository.GetByIdAsync(request.BoothId, cancellationToken);
                if (booth == null)
                {
                    return new VoterVerificationResponse
                    {
                        IsVerified = false,
                        Message = "Invalid Booth ID"
                    };
                }
            }

            // Find voter by Assembly, Booth, and Serial Number
            var voter = await _voterRepository.GetBySerialNumberAsync(
                request.AssemblyId, 
                request.BoothId, 
                request.SerialNumber, 
                cancellationToken);

            if (voter == null)
            {
                return new VoterVerificationResponse
                {
                    IsVerified = false,
                    Message = "Voter not found with the provided Assembly, Booth, and Serial Number"
                };
            }

            return new VoterVerificationResponse
            {
                IsVerified = true,
                VoterId = voter.Id,
                VoterName = voter.Name,
                Message = "Voter verified successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying voter");
            return new VoterVerificationResponse
            {
                IsVerified = false,
                Message = "An error occurred while verifying voter"
            };
        }
    }

    public async Task<VoterProfileResponse> GetVoterProfileAsync(int voterId, CancellationToken cancellationToken = default)
    {
        var voter = await _voterRepository.GetByIdAsync(voterId, cancellationToken);
        if (voter == null)
        {
            throw new KeyNotFoundException($"Voter with ID {voterId} not found");
        }

        // Get user details
        var user = await _userRepository.GetByIdAsync(voter.UserId, cancellationToken);

        // Get assembly details
        var assembly = await _assemblyRepository.GetByIdAsync(voter.AssemblyId, cancellationToken);

        // Get booth details (if booth repository available)
        string boothNumber = string.Empty;
        if (_boothRepository != null)
        {
            var booth = await _boothRepository.GetByIdAsync(voter.BoothId, cancellationToken);
            boothNumber = booth?.BoothNumber ?? string.Empty;
        }

        // Get family members
        var familyMembers = await _familyMemberRepository.GetByVoterIdAsync(voterId, cancellationToken);
        var familyMemberResponses = familyMembers.Select(fm => new FamilyMemberResponse
        {
            Id = fm.Id,
            Name = fm.Name,
            DateOfBirth = fm.DateOfBirth,
            MobileNumber = fm.MobileNumber,
            HasConsent = fm.HasConsent,
            ConsentDate = fm.ConsentDate
        }).ToList();

        return new VoterProfileResponse
        {
            Id = voter.Id,
            UserId = voter.UserId,
            MobileNumber = user?.MobileNumber ?? string.Empty,
            Email = user?.Email,
            AssemblyId = voter.AssemblyId,
            AssemblyNumber = assembly?.AssemblyNumber ?? string.Empty,
            AssemblyName = assembly?.AssemblyName ?? string.Empty,
            BoothId = voter.BoothId,
            BoothNumber = boothNumber,
            SerialNumber = voter.SerialNumber,
            Name = voter.Name,
            DateOfBirth = voter.DateOfBirth,
            FatherName = voter.FatherName,
            Address = voter.Address,
            FamilyMembers = familyMemberResponses,
            CreatedAt = voter.CreatedAt
        };
    }

    public async Task<FamilyMemberResponse> AddFamilyMemberAsync(AddFamilyMemberRequest request, int voterId, CancellationToken cancellationToken = default)
    {
        if (!request.HasConsent)
        {
            throw new InvalidOperationException("Consent is mandatory for adding family members");
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // Verify voter exists
            var voter = await _voterRepository.GetByIdAsync(voterId, cancellationToken);
            if (voter == null)
            {
                throw new KeyNotFoundException($"Voter with ID {voterId} not found");
            }

            // Check if family member with same name already exists
            var existingMember = await _familyMemberRepository.GetByVoterIdAndNameAsync(voterId, request.Name, cancellationToken);
            if (existingMember != null)
            {
                // Update existing family member
                existingMember.DateOfBirth = request.DateOfBirth;
                existingMember.MobileNumber = request.MobileNumber;
                existingMember.HasConsent = request.HasConsent;
                existingMember.ConsentDate = request.HasConsent ? DateTime.UtcNow : null;
                existingMember.UpdatedAt = DateTime.UtcNow;

                _familyMemberRepository.Update(existingMember);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                _logger.LogInformation("Family member {Name} updated for voter {VoterId}", request.Name, voterId);

                return new FamilyMemberResponse
                {
                    Id = existingMember.Id,
                    Name = existingMember.Name,
                    DateOfBirth = existingMember.DateOfBirth,
                    MobileNumber = existingMember.MobileNumber,
                    HasConsent = existingMember.HasConsent,
                    ConsentDate = existingMember.ConsentDate
                };
            }
            else
            {
                // Create new family member
                var familyMember = new FamilyMember
                {
                    VoterId = voterId,
                    Name = request.Name,
                    DateOfBirth = request.DateOfBirth,
                    MobileNumber = request.MobileNumber,
                    HasConsent = request.HasConsent,
                    ConsentDate = request.HasConsent ? DateTime.UtcNow : null,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                var createdMember = await _familyMemberRepository.AddAsync(familyMember, cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                _logger.LogInformation("Family member {Name} added for voter {VoterId}", request.Name, voterId);

                return new FamilyMemberResponse
                {
                    Id = createdMember.Id,
                    Name = createdMember.Name,
                    DateOfBirth = createdMember.DateOfBirth,
                    MobileNumber = createdMember.MobileNumber,
                    HasConsent = createdMember.HasConsent,
                    ConsentDate = createdMember.ConsentDate
                };
            }
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
