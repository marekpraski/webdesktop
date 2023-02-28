
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace UniwersalnyDesktop
{
    public class DesktopProfile
    {
        public string name { get; set; }
        public string id { get; set; }
        public string domena { get; set; }
        public string ldap { get; set; }
        public string serwer { get => getSerwerName(); }

        public string configXlm { get; set; }
        public StartxmlType startxmlType { get; set; } = StartxmlType.Brak;
        public string confPathXml { get; set; }
        public byte[] logoImageAsBytes { get; set; }
        /// <summary>
        /// kluczem jest id aplikacji
        /// </summary>
        public Dictionary<string, IProfileItem> applications { get; } = new Dictionary<string, IProfileItem>();
        /// <summary>
        /// tylko te aplikacje, które spełniają kryteria ważności, m.in mają nazwę i ścieżkę wywołania; tylko dla takich są generowane przyciski Desktopu; kluczem jest id aplikacji
        /// </summary>
        public SortedDictionary<string, IProfileItem> desktopApplications { get => getDesktopApplications(); }

        /// <summary>
        /// kluczem jest id użytkownika; wartością jest DesktopUser
        /// </summary>
        public Dictionary<string, IProfileItem> users { get; } = new Dictionary<string, IProfileItem>();

        #region konstruktory
        public DesktopProfile()
        {
        }

        public DesktopProfile(string profileId, string profileName)
        {
            this.name = profileName;
            this.id = profileId;
        }
        #endregion

        public bool hasApp(App app)
        {
            return applications.ContainsKey(app.id);
        }

        #region dodawanie elementów do profilu
        public void addAppToProfile(App aplikacja)
        {
            if (!applications.ContainsKey(aplikacja.id))
                applications.Add(aplikacja.id, aplikacja);
        }
        public void addUserToProfile(DesktopUser user)
        {
            if (!users.ContainsKey(user.id))
                users.Add(user.id, user);
        }
        #endregion

        #region usuwanie elementów z profilu
        /// <summary>
        /// w parametrze id aplikacji; jeżeli profil nie ma tej aplikacji, metoda nic nie robi
        /// </summary>
        internal void removeAppFromProfile(string idApp)
        {
            if (applications.ContainsKey(idApp))
                applications.Remove(idApp);
        }

        /// <summary>
        /// w parametrze id użytkownika; jeżeli profil nie ma tego użytkownika, metoda nic nie robi
        /// </summary>
        public void removeUserFromProfile(string userId)
        {
            if (users.ContainsKey(userId))
                users.Remove(userId);
        }
        #endregion

        #region metody pomocnicze
        private SortedDictionary<string, IProfileItem> getDesktopApplications()
        {
            if (this.applications == null)
                return null;
            SortedDictionary<string, IProfileItem> items = new SortedDictionary<string, IProfileItem>();
            foreach (string id in this.applications.Keys)
            {
                if ((applications[id] as App).isValidDesktopApp)
                    items.Add(id, applications[id]);
            }
            return items;
        }

        private string getSerwerName()
        {
            if (String.IsNullOrEmpty(configXlm))
                return "";
            XElement el;
            try
            {
                el = XElement.Parse(configXlm);
            }
            catch (Exception)
            {
                return "";
            }
            string serverName = el.Element("server").Value == null ? "" : el.Element("server").Value;
            return serverName;
        } 
        #endregion

    }
}
