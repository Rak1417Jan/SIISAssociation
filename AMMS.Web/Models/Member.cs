namespace AMMS.Web.Models;

public class Member
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FatherName { get; set; } = string.Empty;
    public string MobileNo { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Designation { get; set; }
    public string? Education { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public DateTime? DateOfAnniversary { get; set; }
    public string? AadharCardNo { get; set; }
    public string? FirmName { get; set; }
    public string? PhotoFileName { get; set; }
    public string? LeaseDeedFileName { get; set; }
    public string? RegistrationFileName { get; set; }
    public string? GstCopyFileName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public enum MembershipPlanType
{
    Yearly = 0,
    Lifetime = 1
}

public static class MembershipPlanHelper
{
    public const decimal YearlyBaseAmount = 1000m;
    public const decimal LifetimeBaseAmount = 11000m;
    public const decimal PlatformCharges = 99m;
    public const decimal GstRatePercent = 18m;

    public static string DisplayName(MembershipPlanType type) => type switch
    {
        MembershipPlanType.Yearly => "Yearly",
        MembershipPlanType.Lifetime => "Lifetime",
        _ => type.ToString()
    };

    public static decimal BaseAmount(MembershipPlanType type) => type switch
    {
        MembershipPlanType.Yearly => YearlyBaseAmount,
        MembershipPlanType.Lifetime => LifetimeBaseAmount,
        _ => 0
    };
}

public class MemberMembership
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public MembershipPlanType PlanType { get; set; }
    public decimal BaseAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
}

public class MemberPayment
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public int? MembershipId { get; set; }
    public decimal PlanAmount { get; set; }
    public decimal PlatformCharges { get; set; }
    public decimal SubTotal { get; set; }
    public decimal GstAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? TransactionRef { get; set; }
    public string Status { get; set; } = "Completed";
}

public enum ApplicationStatus
{
    Pending = 0,
    Approved = 1,
    Hold = 2,
    Rejected = 3
}

public class MemberApplication
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
    public string? DiscrepancyRemarks { get; set; }
    public DateTime? ReviewedAt { get; set; }
}

/// <summary>In-memory store for demo; replace with database in production.</summary>
public static class MemberStore
{
    private static readonly List<Member> _members = new();
    private static readonly List<MemberMembership> _memberships = new();
    private static readonly List<MemberPayment> _payments = new();
    private static readonly List<MemberApplication> _applications = new();
    private static int _memberId = 1;
    private static int _membershipId = 1;
    private static int _paymentId = 1;
    private static int _applicationId = 1;
    private static readonly object _lock = new();

    public const string RegisteredMobileForDemo = "9829010083";

    static MemberStore()
    {
        // Seed a known registered member for demo (linked to mobile 9829010083)
        var registeredMember = new Member
        {
            Id = _memberId++,
            Name = "Demo Registered Member",
            FatherName = "Father Name",
            MobileNo = "9460141285",
            Email = "member@example.com",
            Designation = "Member",
            Education = "Graduate",
            DateOfBirth = new DateTime(1990, 5, 15),
            DateOfAnniversary = new DateTime(2015, 6, 20),
            AadharCardNo = "1234-5678-9012",
            FirmName = "Demo Firm",
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };
        _members.Add(registeredMember);

        _memberships.Add(new MemberMembership
        {
            Id = _membershipId++,
            MemberId = registeredMember.Id,
            PlanType = MembershipPlanType.Yearly,
            BaseAmount = MembershipPlanHelper.YearlyBaseAmount,
            StartDate = DateTime.UtcNow.AddMonths(-3),
            EndDate = DateTime.UtcNow.AddMonths(9),
            IsActive = true
        });

        _payments.Add(new MemberPayment
        {
            Id = _paymentId++,
            MemberId = registeredMember.Id,
            MembershipId = 1,
            PlanAmount = 1000,
            PlatformCharges = 99,
            SubTotal = 1099,
            GstAmount = 197.82m,
            TotalAmount = 1296.82m,
            PaymentDate = DateTime.UtcNow.AddMonths(-3),
            TransactionRef = "TXN001",
            Status = "Completed"
        });

        _applications.Add(new MemberApplication
        {
            Id = _applicationId++,
            MemberId = registeredMember.Id,
            Status = ApplicationStatus.Approved,
            ReviewedAt = DateTime.UtcNow.AddMonths(-3)
        });

        // Seed additional demo members for admin listing
        var random = new Random(42);
        var firms = new[]
        {
            "Shree Engineering Works",
            "TEST Tools Pvt Ltd",
            "Om Industries",
            "Shakti Engineering",
            "Sunrise Components",
            "Global Tech Industries",
            "Prime Machinery",
            "Elite Fabricators"
        };

        var statuses = new[]
        {
            ApplicationStatus.Pending,
            ApplicationStatus.Approved,
            ApplicationStatus.Hold,
            ApplicationStatus.Rejected
        };

        for (var i = 1; i <= 100; i++)
        {
            var demoMember = new Member
            {
                Id = _memberId++,
                Name = $"Member {i}",
                FatherName = $"Father {i}",
                MobileNo = NormalizeMobile($"98{i:00000000}"),
                Email = $"member{i}@example.com",
                Designation = "Member",
                Education = "Graduate",
                DateOfBirth = DateTime.UtcNow.AddYears(-25).AddDays(i),
                DateOfAnniversary = DateTime.UtcNow.AddYears(-5).AddDays(i),
                AadharCardNo = $"1234-5678-9{i:000}",
                FirmName = firms[random.Next(firms.Length)],
                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(0, 120))
            };
            _members.Add(demoMember);

            var status = statuses[random.Next(statuses.Length)];
            _applications.Add(new MemberApplication
            {
                Id = _applicationId++,
                MemberId = demoMember.Id,
                Status = status,
                DiscrepancyRemarks = status == ApplicationStatus.Rejected
                    ? "Please upload clear copy of registration document."
                    : null,
                ReviewedAt = status == ApplicationStatus.Pending ? null : DateTime.UtcNow.AddDays(-random.Next(1, 30))
            });
        }
    }

    public static Member? GetByMobile(string mobile)
    {
        var normalized = NormalizeMobile(mobile);
        if (normalized.Length < 10) normalized = new string(mobile.Where(char.IsDigit).ToArray());
        lock (_lock) return _members.FirstOrDefault(m => NormalizeMobile(m.MobileNo) == normalized || m.MobileNo == normalized);
    }

    public static Member? GetById(int id)
    {
        lock (_lock) return _members.FirstOrDefault(m => m.Id == id);
    }

    public static IReadOnlyList<Member> GetAll()
    {
        lock (_lock) return _members.OrderByDescending(m => m.CreatedAt).ToList();
    }

    public static void Add(Member member)
    {
        lock (_lock)
        {
            member.Id = _memberId++;
            member.MobileNo = NormalizeMobile(member.MobileNo);
            _members.Add(member);
        }
    }

    public static bool Update(Member member)
    {
        lock (_lock)
        {
            var idx = _members.FindIndex(m => m.Id == member.Id);
            if (idx < 0) return false;
            _members[idx] = member;
            return true;
        }
    }

    public static IReadOnlyList<MemberMembership> GetMembershipsByMemberId(int memberId)
    {
        lock (_lock) return _memberships.Where(m => m.MemberId == memberId).OrderByDescending(m => m.StartDate).ToList();
    }

    public static void AddMembership(MemberMembership m)
    {
        lock (_lock)
        {
            m.Id = _membershipId++;
            _memberships.Add(m);
        }
    }

    public static IReadOnlyList<MemberPayment> GetPaymentsByMemberId(int memberId)
    {
        lock (_lock) return _payments.Where(p => p.MemberId == memberId).OrderByDescending(p => p.PaymentDate).ToList();
    }

    public static void AddPayment(MemberPayment p)
    {
        lock (_lock)
        {
            p.Id = _paymentId++;
            _payments.Add(p);
        }
    }

    public static MemberApplication? GetApplicationByMemberId(int memberId)
    {
        lock (_lock) return _applications.FirstOrDefault(a => a.MemberId == memberId);
    }

    public static void AddOrUpdateApplication(MemberApplication app)
    {
        lock (_lock)
        {
            var existing = _applications.FirstOrDefault(a => a.MemberId == app.MemberId);
            if (existing != null)
            {
                existing.Status = app.Status;
                existing.DiscrepancyRemarks = app.DiscrepancyRemarks;
                existing.ReviewedAt = app.ReviewedAt;
            }
            else
            {
                app.Id = _applicationId++;
                _applications.Add(app);
            }
        }
    }

    private static string NormalizeMobile(string mobile)
    {
        if (string.IsNullOrWhiteSpace(mobile)) return string.Empty;
        return new string(mobile.Where(char.IsDigit).ToArray()).TrimStart('0');
    }
}
