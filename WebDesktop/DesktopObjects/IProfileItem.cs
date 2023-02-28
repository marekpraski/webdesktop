
namespace UniwersalnyDesktop
{
    public interface IProfileItem
    {
        string id { get; set; }
        string displayName { get; }
        bool isValid { get; }
    }
}
