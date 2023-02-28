
namespace UniwersalnyDesktop
{
    public class AccessTypeConverter
    {
        /// <summary>
        /// podanie wartości parametru innego niż 1 lub 2 zwraca NoAccess
        /// </summary>
        public static AccessType getAccessType(int grantApp)
        {
            if (grantApp == 1)
                return AccessType.ReadWrite;
            else if (grantApp == 2)
                return AccessType.Readonly;
            else if (grantApp == -1)
                return AccessType.DeterminedByRola;
            return AccessType.NoAccess;
        }

        /// <summary>
        /// zwraca odpowiadającą wartość integera grantApp; w przypadku AccessType.Undetermined zwraca -1
        /// </summary>
        public static int getGrantApp(AccessType accessType)
        {
            if (accessType == AccessType.ReadWrite)
                return 1;
            else if (accessType == AccessType.Readonly)
                return 2;
            else if (accessType == AccessType.DeterminedByRola)
                return -1;
            return 0;
        }
    }
}
