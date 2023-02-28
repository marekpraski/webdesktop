
namespace UniwersalnyDesktop
{
    public enum AccessType
    {
        ReadWrite,
        Readonly,
        /// <summary>
        /// wykorzystywany w przypadku aplikacji mającej role, gdy dostęp do aplikacji definiowany jest przez dostęp do roli
        /// </summary>
        DeterminedByRola,
        NoAccess
    }
}
