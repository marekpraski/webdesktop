
using System;
using System.Collections.Generic;
using System.Text;
using DatabaseInterface;

namespace UniwersalnyDesktop
{
    /// <summary>
    /// rola użytkownika w stosunku do aplikacji; w ramach roli użytkownik ma dostęp do różnych modułów na poziomie uprawnień zapis lub odczyt
    /// </summary>
    public class Rola
    {
        internal bool isUserAssigned { get => assertIsUserAssigned(); }

        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public App app { get; set; }
        public bool hasModules { get => moduleDict.Count > 0; }


        /// <summary>
        /// nie zawiera wszystkich modułów aplikacji tylko te, które są przypisane do danej roli
        /// </summary>
        public Dictionary<string, AppModule> moduleDict { get; }        //kluczem jest id modułu

        public Rola()
        {
            moduleDict = new Dictionary<string, AppModule>();
        }

        public void addModule(AppModule module)
        {
            moduleDict.Add(module.id, module);
        }

        #region gettery
        /// <summary>
        /// zwraca wartość grantApp modułu
        /// </summary>
        public int getModuleAccessRight(string moduleId)
        {
            if(moduleDict.ContainsKey(moduleId))
                return moduleDict[moduleId].grantApp;
            return 0;
        }

        internal string getModuleUprId(string moduleId)
        {
            if (moduleDict.ContainsKey(moduleId))
                return moduleDict[moduleId].idRolaModUpr;
            return "";
        }
        /// <summary>
        /// zwraca true jeżeli moduł jest aktywnie przypisany do tej roli, tzn dostęp AppModuleAccessType jest inny niż NoAccess
        /// </summary>
        public bool hasAciveModule(AppModule module)
        {
            if (!this.hasModules || String.IsNullOrEmpty(module.id))
                return false;
            if (this.moduleDict.ContainsKey(module.id) && this.moduleDict[module.id].accessRights != AccessType.NoAccess)
                return true;
            return false;
        }
        #endregion

        public Rola clone ()
        {
            Rola newRola = new Rola();
            newRola.id = this.id;
            newRola.name = this.name;
            newRola.description = this.description;
            newRola.app = this.app;
            return newRola;
        }

        #region zapisywanie ról do bazy danych
        internal void saveRange(List<Rola> rolaList)
        {
            DBWriter writer = new DBWriter(LoginForm.dbConnection);
            string query = generateRolaSaveQuery(rolaList);
            if (writer.executeQuery(query))
            {
                this.app.refreshRolaList(rolaList);
                //MyMessageBox.display("Zapisano");
            }
        }

        private string generateRolaSaveQuery(List<Rola> rolaList)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < rolaList.Count; i++)
            {
                sb.Append(generateSingleRolaSaveQuery(rolaList[i]));
            }
            return sb.ToString();
        }

        private string generateSingleRolaSaveQuery(Rola rola)
        {
            if (String.IsNullOrEmpty(rola.id))
                return generateRolaInsertQuery(rola);
            return generateRolaUpdateQuery(rola);
        }

        private string generateRolaInsertQuery(Rola rola)
        {
            return "insert into rola_app (ID_app, name_rola, descr_rola) values(" + rola.app.id + ", '" + rola.name + "', '" + rola.description + "'); ";
        }

        private string generateRolaUpdateQuery(Rola rola)
        {
            return "update rola_app set name_rola = '" + rola.name + "', descr_rola = '" + rola.description + "' where ID_rola = " + rola.id + "; ";
        }

        #endregion

        #region usuwanie ról z bazy danych
        internal bool deleteRange(List<Rola> rolaList)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < rolaList.Count; i++)
            {
                sb.Append(getRolaDeleteQuery(rolaList[i]));
            }
            if(new DBWriter(LoginForm.dbConnection).executeQuery(sb.ToString()))
            {
                this.app.removeRola(rolaList);
                return true;
            }
            return false;
        }

        private string getRolaDeleteQuery(Rola rola)
        {
            string query = "delete from rola_upr where ID_rola = " + rola.id + "; ";
            query += "delete from rola_app where ID_rola = " + rola.id + "; ";
            return query;
        } 
        #endregion

        #region zapisywanie uprawnień do modułów danych do bazy
        internal bool saveModuleAccessRights(List<AppModule> modules)
        {
            DBWriter writer = new DBWriter(LoginForm.dbConnection);
            string query = generateModuleSaveQuery(modules);
            if (writer.executeQuery(query))
            {
                updateModules(modules);
                //MyMessageBox.display("Zapisano");
                return true;
            }
            return false;
        }

        private void updateModules(List<AppModule> modules)
        {
            for (int i = 0; i < modules.Count; i++)
            {
                if (moduleDict.ContainsKey(modules[i].id))
                {
                    AppModule rolaModule = moduleDict[modules[i].id];
                    rolaModule.idRolaModUpr = modules[i].idRolaModUpr;
                    rolaModule.setAccessRights(modules[i].grantApp);
                }
                else
                    addModule(modules[i]);
            }
        }

        private string generateModuleSaveQuery(List<AppModule> modules)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < modules.Count; i++)
            {
                sb.Append(generateSingleModuleSaveQuery(modules[i]));
            }
            return sb.ToString();
        }

        private string generateSingleModuleSaveQuery(AppModule module)
        {
            if (String.IsNullOrEmpty(module.idRolaModUpr))
                return generateModuleInsertQuery(module);
            else if (module.accessRights == AccessType.NoAccess)
                return generateModuleDeleteQuery(module);
            return generateModuleUpdateQuery(module);
        }

        private string generateModuleInsertQuery(AppModule module)
        {
            return "insert into rola_upr (ID_rola, ID_mod, Grant_app) values(" + this.id + ", " + module.id + ", " + module.grantApp + "); ";
        }

        private string generateModuleDeleteQuery(AppModule module)
        {
            return "delete from rola_upr where ID_rola_upr = " + module.idRolaModUpr + "; ";
        }

        private string generateModuleUpdateQuery(AppModule module)
        {
            return "update rola_upr set Grant_app = " + module.grantApp + " where ID_rola_upr = " + module.idRolaModUpr + "; ";
        }

        #endregion

        #region czytanie użytkownika z bazy danych

        private bool assertIsUserAssigned()
        {
            string query = "select * from rola_users where ID_rola = " + this.id;
            return new DBReader(LoginForm.dbConnection).readFromDB(query).dataRowsNumber > 0;
        } 
        #endregion

    }
}
