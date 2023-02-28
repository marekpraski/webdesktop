
namespace UniwersalnyDesktop
{

    /// <summary>
    /// przechowuje uprawnienia użytkownika do aplikacji i roli użytkownika w danej aplikacji;
    /// w aplikacji użytkownik może mieć tylko jedną rolę
    /// </summary>
    public class UserAppPrivilegesItem
    {
        public App app { get; }

        /// <summary>
        /// true oznacza że użytkownik ma w ogóle uprawnienia do uruchomienia aplikacji;
        /// </summary>
        public bool isAppEnabled { get => this.appAccessType != AccessType.NoAccess; }

        /// <summary>
        /// rola użytkownika w aplikacji; w aplikacji użytkownik może mieć tylko jedną rolę; rola inna niż null
        /// determinuje poziom dostępu użytkownika do tej aplikacji
        /// </summary>
        public Rola rola { get; set; }

        /// <summary>
        /// określa rodzaj dostępu do aplikacji; w przypadku, gdy aplikacja ma moduły, dostęp określany jest przez rolę aplikacji;
        /// wtedy rola == null oznacza brak dostępu; do określenia czy użytkownik ma dostęp do aplikacji niezależnie od
        /// tego czy aplikacja ma role czy nie. należy użyć właściwości isAppEnabled
        /// </summary>
        public AccessType appAccessType { get; set; } = AccessType.NoAccess;

        /// <summary>
        /// zwraca wartość Grant_app do zapisu do bazy na podstawie wartości appAccessType
        /// </summary>
        public int grantApp { get => getGrantApp(); }

        private int getGrantApp()
        {
            if (!isAppEnabled)
                return AccessTypeConverter.getGrantApp(AccessType.NoAccess);

            return AccessTypeConverter.getGrantApp(appAccessType);
        }

        public void setAppAccessType(int grantApp)
        {
            if (this.rola != null)
                this.appAccessType = AccessType.DeterminedByRola;
            else
                this.appAccessType = AccessTypeConverter.getAccessType(grantApp);
        }

        public UserAppPrivilegesItem(App app)
        {
            this.app = app;
        }

        public UserAppPrivilegesItem clone()
        {
            UserAppPrivilegesItem newItem = new UserAppPrivilegesItem(app);
            if (this.rola != null)
                newItem.rola = this.rola.clone();

            newItem.appAccessType = this.appAccessType;

            return newItem;
        }

        internal bool equals(UserAppPrivilegesItem other)
        {
            return this.app.id == other.app.id && assertRolaIsTheSame(other) && this.appAccessType == other.appAccessType;
        }

        private bool assertRolaIsTheSame(UserAppPrivilegesItem other)
        {
            if (this.rola == null)
                return other.rola == null;
            else if (this.rola == null && other.rola != null)
                return false;
            else if (this.rola != null && other.rola == null)
                return false;
            else
                return this.rola.id == other.rola.id;
        }
    }
}
