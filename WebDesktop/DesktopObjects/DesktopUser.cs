using System;
using System.Collections.Generic;
using System.Text;
using DatabaseInterface;

namespace UniwersalnyDesktop
{
    public class DesktopUser : IProfileItem
    {
        public string firstName { get; set; }
        public string surname { get; set; }
        public string id { get; set; }
        public string windowsLogin { get; set; }
        public string sqlLogin { get; set; }
        public string sqlPassword { get; set; }
        public string oddzial { get; set; }
        /// <summary>
        /// profil używany przez użytkownika w ostatniej sesji, zapisywany do pliku i czytany na starcie Desktopu; 
        /// jeżeli brak było zapisanych ustawień (pierwsze uruchomienie Desktopu przez danego użytkownika), zwraca -1
        /// </summary>
        public string lastUsedProfileId { get; private set; }
        public bool isValid { get => assertUserIsValid(); }

        public UserType type { get; set; } = UserType.Undefined;
        public Dictionary<App, UserAppPrivilegesItem> userAppDict { get; }
        public List<DesktopProfile> profiles { get; }

        public string displayName => getDisplayName();

        private string getDisplayName()
        {
            if (String.IsNullOrEmpty(this.firstName) && String.IsNullOrEmpty(this.surname))
                return "";
            else if (String.IsNullOrEmpty(this.firstName))
                return surname;
            else if (String.IsNullOrEmpty(this.surname))
                return this.firstName;
            return this.firstName + " " + this.surname;
        }
        //[ID_user], [imie_user], [nazwisko_user], [login_user], [windows_user], [oddzial]
        internal void updateInDB()
        {
            StringBuilder sb = new StringBuilder("update users_list set ");
            sb.Append("imie_user='" + this.firstName + "',");
            sb.Append("nazwisko_user='" + this.surname + "',");
            sb.Append("login_user='" + this.sqlLogin + "',");
            sb.Append("windows_user='" + this.windowsLogin + "',");
            sb.Append("oddzial='" + this.oddzial + "' ");
            sb.Append(" where ID_user = " + this.id);
            new DBWriter(LoginForm.dbConnection).executeQuery(sb.ToString());
        }

        private bool assertUserIsValid()
        {
            return userHasFirstNameOrSurname() && !String.IsNullOrEmpty(sqlLogin);
        }
        private bool userHasFirstNameOrSurname()
        {
            if (String.IsNullOrEmpty(firstName) && String.IsNullOrEmpty(surname))
                return false;
            return true;
        }

        public DesktopUser()
        {
            userAppDict = new Dictionary<App, UserAppPrivilegesItem>();
            profiles = new List<DesktopProfile>();
        }

        public void setLastUsedProfileId(string profileId)
        {
            if (String.IsNullOrEmpty(profileId))
                this.lastUsedProfileId = "";
            this.lastUsedProfileId = profileId;
        }

        /// <summary>
        /// dodaje jeżeli nie ma, aktualizuje rolę lub grantApp jeżeli jest; usuwa jeżeli grantApp jest 0
        /// </summary>
        public void tryUpdateUserApps(App app, Rola rola, int grantApp)
        {
            if (!userAppDict.ContainsKey(app))
                tryAddApp(app, rola, grantApp);
            else if (userAppDict.ContainsKey(app) && AccessTypeConverter.getAccessType(grantApp) == AccessType.DeterminedByRola && rola == null)
                tryDeleteApp(app);
            else if (userAppDict.ContainsKey(app) && AccessTypeConverter.getAccessType(grantApp) == AccessType.NoAccess)
                tryDeleteApp(app);
            else
                updateApp(app, rola, grantApp);
        }

        public void addProfile(DesktopProfile profile)
        {
            this.profiles.Add(profile);
        }

        /// <summary>
        /// przypisuje użytkownikowi takie same uprawnienia do aplikacji które ma użytkownik przekazany w parametrze
        /// </summary>
        internal void copyUserAppData(DesktopUser other)
        {
            foreach(App app in other.getApps())
            {
                UserAppPrivilegesItem appData = other.getAppData(app);
                if (this.hasApp(app))
                    this.updateApp(app, appData.rola, appData.grantApp);
                else
                    this.tryAddApp(app, appData.rola, appData.grantApp);
            }

            foreach(App app in this.getApps())
            {
                if (!other.hasApp(app))
                    this.deleteApp(app);
            }
        }

        private void tryAddApp(App app, Rola rola, int grantApp)
        {
            if (rola != null || grantApp > 0)
            {
                UserAppPrivilegesItem appData = new UserAppPrivilegesItem(app);

                appData.rola = rola;
                appData.setAppAccessType(grantApp);
                userAppDict.Add(app, appData);
            }
        }


        private void updateApp(App app, Rola rola, int grantApp)
        {
            UserAppPrivilegesItem appData = userAppDict[app];

            appData.rola = rola;
            appData.setAppAccessType(grantApp);
        }

        /// <summary>
        /// gdy użytkownik ma aplikację, zostaje ona usunięta i zwrócone jest true; 
        /// gdy użytkownik nie miał tej aplikacji, zwrócone jest false
        /// </summary>
        public bool tryDeleteApp(App app)
        {
            if (userAppDict.ContainsKey(app))
            {
                userAppDict.Remove(app);
                return true;
            }
            return false;
        }

        public string getRolaId(App app)
        {
            if (userAppDict.ContainsKey(app))
            {
                UserAppPrivilegesItem appData;
                userAppDict.TryGetValue(app, out appData);
                return appData.rola == null ? "" : appData.rola.id;
            }
            else
                return "";
        }

        /// <summary>
        /// zwraca null jeżeli użytkownik w ogóle nie ma uprawnień do tej aplikacji
        /// </summary>
        public UserAppPrivilegesItem getAppData(App app)
        {
            if (userAppDict.ContainsKey(app))
            {
                UserAppPrivilegesItem appData;
                userAppDict.TryGetValue(app, out appData);
                return appData;
            }
            else
            {
                return null;
            }
        }

        public List<App> getApps()
        {
            if (userAppDict.Count > 0)
            {
                List<App> apps = new List<App>();
                foreach (App app in userAppDict.Keys)
                {
                    if(this.hasApp(app))
                        apps.Add(app);
                }
                return apps;
            }
            else
                return null;
        }

        /// <summary>
        /// zwraca true jeżeli użytkownik ma aplikację, dodatkowo sprawdzany jest warunek, że aplikacja jest aktywna
        /// </summary>
        public bool hasApp(App app)
        {
            return userAppDict.ContainsKey(app) && this.getAppData(app).isAppEnabled;
        }

        /// <summary>
        /// usuwa podaną aplikację z listy aplikacji użytkownika sprawdzając wcześniej, że użytkownik ją posiada; jeżeli nie posiada, metoda nic nie robi
        /// </summary>
        public void deleteApp(App app)
        {
            if (userAppDict.ContainsKey(app))
            {
                userAppDict.Remove(app);
            }
        }

        public DesktopUser clone()
        {
            DesktopUser other = new DesktopUser();

            other.firstName = this.firstName;
            other.surname = this.surname;
            other.id = this.id;
            other.windowsLogin = this.windowsLogin;
            other.sqlLogin = this.sqlLogin;

            foreach (App app in this.userAppDict.Keys)
            {
                UserAppPrivilegesItem appData = (UserAppPrivilegesItem) this.getAppData(app).clone();

                other.userAppDict.Add(app, appData);
            }

            return other;
        }
    }
}
