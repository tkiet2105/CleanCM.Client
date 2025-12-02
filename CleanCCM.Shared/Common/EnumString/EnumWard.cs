
namespace CleanCCM.Shared.Common.EnumString;

public static class EnumWard
{
    public const string Phuong1 = nameof(Phuong1);
    public const string Phuong2 = nameof(Phuong2);
    public const string Phuong3 = nameof(Phuong3);
    public const string Phuong4 = nameof(Phuong4);
    public const string Phuong5 = nameof(Phuong5);
    public const string Phuong8 = nameof(Phuong8);
    public const string Phuong9 = nameof(Phuong9);
    public const string Khac = nameof(Khac);



    private static readonly Dictionary<string, string> _vietNameseNames = new()
    {
        { Phuong1, "Phường 1" },
        { Phuong2, "Phường 2" },
        { Phuong3, "Phường 3" },
        { Phuong4, "Phường 4" },
        { Phuong5, "Phường 5" },
        { Phuong8, "Phường 8" },
        { Phuong9, "Phường 9" },  
        { Khac,  "Khác" },
        

    };

    public static string GetVietnameseName(string wardKey)
    {
        if (_vietNameseNames.TryGetValue(wardKey, out var vn))
            return vn;
        return wardKey; // fallback
    }

    public static IEnumerable<string> GetAllKeys()
    {
        return _vietNameseNames.Keys;
    }
}

