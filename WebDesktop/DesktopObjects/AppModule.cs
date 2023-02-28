
namespace UniwersalnyDesktop
{
    /// <summary>
    /// moduł wewnętrzny aplikacji; w stosunku do modułu sprawdzane są uprawnienia użytkownika; aplikacja może zawierać wiele modułów
    /// </summary>
    public class AppModule
    {
        public string id { get; set; }
        public string name { get; set; }

        public string appId { get; set; }
        /// <summary>
        /// pole ID_rola_upr, klucz główny tablicy łączącej rola_upr (łączy role aplikacji z modułami aplikacji i określa uprawnienia (grantApp) do modułów w każdej roli);
        /// ma wartość tylko wtedy, gdy istnieje wpis w tablic łączącej
        /// </summary>
        public string idRolaModUpr { get; set; }
        public AccessType accessRights { get; private set; } = AccessType.NoAccess;

        /// <summary>
        /// uprawnienia do modułu (ważne z konkretną rolą aplikacji)
        /// </summary>
        public int grantApp { get => getGrantApp(); }

        /// <summary>
        /// grantApp to wartość z pola grant_app; metoda ustawia wartość właściwości accessRights
        /// </summary>
        public void setAccessRights(int grantApp)
        {
            accessRights = AccessTypeConverter.getAccessType(grantApp);
        }
        private int getGrantApp()
        {
            return AccessTypeConverter.getGrantApp(this.accessRights);
        }
    }
}
