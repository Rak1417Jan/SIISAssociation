namespace AMMS.Web.Models;

public class Firm
{
    public int Id { get; set; }
    public string FirmCode { get; set; } = string.Empty;
    public string FirmName { get; set; } = string.Empty;
    public FirmType Type { get; set; }
    public DateTime? DateOfEstablishment { get; set; }
    public string? GstNo { get; set; }
    public string? RegNo { get; set; }
    public string? Address { get; set; }
    public string? OfficeAddress { get; set; }
    public string? TelephoneNo { get; set; }
    public string? MobileNo { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? Products { get; set; } // Comma-separated
}

public enum FirmType
{
    Individual = 0,
    Partnership = 1,
    PrivateLimitedCompany = 2,
    PublicLimitedCompany = 3,
    CooperativeSociety = 4,
    JointVenture = 5
}

public static class FirmTypeHelper
{
    public static string DisplayName(FirmType type) => type switch
    {
        FirmType.Individual => "Individual",
        FirmType.Partnership => "Partnership",
        FirmType.PrivateLimitedCompany => "Private Limited Company",
        FirmType.PublicLimitedCompany => "Public Limited Company",
        FirmType.CooperativeSociety => "Cooperative Society",
        FirmType.JointVenture => "Joint Venture",
        _ => type.ToString()
    };

    public static readonly List<(FirmType Value, string Text)> All = new()
    {
        (FirmType.Individual, "Individual"),
        (FirmType.Partnership, "Partnership"),
        (FirmType.PrivateLimitedCompany, "Private Limited Company"),
        (FirmType.PublicLimitedCompany, "Public Limited Company"),
        (FirmType.CooperativeSociety, "Cooperative Society"),
        (FirmType.JointVenture, "Joint Venture")
    };
}

/// <summary>In-memory store for demo; replace with database in production.</summary>
public static class FirmStore
{
    private static readonly List<Firm> _firms = new();
    private static int _nextId = 1;
    private static readonly object _lock = new();

    static FirmStore()
    {
        _firms.Add(new Firm
        {
            Id = _nextId++,
            FirmCode = "F001",
            FirmName = "Sample Industries",
            Type = FirmType.PrivateLimitedCompany,
            DateOfEstablishment = new DateTime(2015, 3, 10),
            GstNo = "27AABCU9603R1ZM",
            RegNo = "REG001",
            Address = "123 Main Street, Mumbai",
            OfficeAddress = "123 Main Street, Mumbai",
            TelephoneNo = "022-12345678",
            MobileNo = "9876543210",
            Email = "info@sample.com",
            Website = "https://sample.com",
            Products = "Steel, Machinery, Tools"
        });
    }

    public static IReadOnlyList<Firm> GetAll()
    {
        lock (_lock) return _firms.ToList();
    }

    public static Firm? GetById(int id)
    {
        lock (_lock) return _firms.FirstOrDefault(f => f.Id == id);
    }

    public static void Add(Firm firm)
    {
        lock (_lock)
        {
            firm.Id = _nextId++;
            _firms.Add(firm);
        }
    }

    public static bool Update(Firm firm)
    {
        lock (_lock)
        {
            var idx = _firms.FindIndex(f => f.Id == firm.Id);
            if (idx < 0) return false;
            _firms[idx] = firm;
            return true;
        }
    }

    public static bool Delete(int id)
    {
        lock (_lock)
        {
            var idx = _firms.FindIndex(f => f.Id == id);
            if (idx < 0) return false;
            _firms.RemoveAt(idx);
            return true;
        }
    }
}
