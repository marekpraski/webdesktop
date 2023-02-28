
using DatabaseInterface;
using System;
using System.Collections.Generic;
using System.Text;

namespace UniwersalnyDesktop
{
    /// <summary>
    /// aplikacja zdefiniowana w bazie desktopu
    /// </summary>
    public class App : IProfileItem
    {
        private string _displayName;

        #region właściwości publiczne aplikacji czytane z bazy danych
        public string displayName { get => String.IsNullOrEmpty(_displayName) ? name : _displayName; set => _displayName = value; }
        public string name { get; set; }
        public string id { get; set; }
        public string executionPath { get; set; }
        public AppRunEnvironment runEnvironment { get; private set; } = AppRunEnvironment.Undefined;
        public string exeParameters { get; set; }

        #endregion

        #region właściwości publiczne bool
        /// <summary>
        /// zwraca true jeżeli aplikacja spełnia warunki umożliwiające jej wyświetlenie na przycisku i uruchomienie, 
        /// tj. posiada zdefiniowaną w bazie nazwę do wyświetlenia i ścieżkę do uruchomienia
        /// </summary>
        public bool isValid { get => assertAppDataIsValid(); }
        /// <summary>
        /// zwraca true jeżeli aplikacja może być uruchamiana z Desktopu, tj. spełnia warunki ważności i jest aplikacją windows lub excel
        /// </summary>
        public bool isValidDesktopApp { get => assertIsValidDesktopApp(); }

        /// <summary>
        /// zwraca true jeżeli aplikacja może być uruchamiana z Microstation, tj. spełnia warunki ważności i jest aplikacją MicroStation
        /// </summary>
        public bool isValidMicrostationApp { get => assertIsValidMicrostationApp(); }
        public bool hasRola { get => rolaIdList.Count > 0; }
        public bool hasModules { get => moduleList.Count > 0; } 
        #endregion

        #region właściwości publiczne aplikacji tylko do odczytu
        public List<string> rolaIdList { get; }     //zawiera ID ról
        public List<Rola> rolaList { get; }
        /// <summary>
        /// kluczem jest Id roli
        /// </summary>
        public Dictionary<string, Rola> rolaDict { get; }

        /// <summary>
        /// pełna lista modułów aplikacji; tylko informacyjnie; żeby uzyskać uprawnienia dostępu do modułu w każdej roli, trzeba sięgnąć do listy modułów przypisanych do roli
        /// </summary>
        public List<AppModule> moduleList { get; }

        #endregion


        public App()
        {
            rolaIdList = new List<string>();
            rolaList = new List<Rola>();
            moduleList = new List<AppModule>();
            rolaDict = new Dictionary<string, Rola>();
        }

        #region settery publiczne
        public void setAppRunEnvironment(string runEnvironment)
        {
            runEnvironment = runEnvironment.ToLower();
            switch (runEnvironment)
            {
                case "windows":
                    this.runEnvironment = AppRunEnvironment.Windows;
                    break;
                case "microstation":
                    this.runEnvironment = AppRunEnvironment.Microstation;
                    break;
                case "excel":
                    this.runEnvironment = AppRunEnvironment.Excel;
                    break;
                case "internet":
                    this.runEnvironment = AppRunEnvironment.Internet;
                    break;
            }
        }
        #endregion

        #region gettery publiczne
        public Rola getRola(string rolaId)
        {
            Rola rola = null; ;
            if (rolaDict.ContainsKey(rolaId))
            {
                rolaDict.TryGetValue(rolaId, out rola);
            }
            return rola;
        }

        public List<string> getModuleNameList()
        {
            return getParameterList(getModuleName);
        }

        public List<string> getModuleIds()
        {
            return getParameterList(getModuleId);
        }

        internal List<string> getRolaModUprIds()
        {
            return getParameterList(getModuleRolaUprId);
        }

        private delegate string moduleParameterGetter(AppModule module);
        private string getModuleName(AppModule module)
        {
            return module.name;
        }
        private string getModuleId(AppModule module)
        {
            return module.id;
        }
        private string getModuleRolaUprId(AppModule module)
        {
            return module.idRolaModUpr;
        }

        private List<string> getParameterList(moduleParameterGetter parameterGetter)
        {
            List<string> moduleParams = new List<string>();
            if (hasModules)
            {
                foreach (AppModule module in moduleList)
                {
                    moduleParams.Add(parameterGetter(module));
                }
            }
            return moduleParams;
        }

        #endregion

        #region zarządzanie rolami
        /// <summary>
        /// jeżeli aplikacja nie ma tej roli, dodaje ją; jeżeli aplikacja ma tę rolę, aktualizuje właściwości tej roli
        /// </summary>
        internal void refreshRolaList(List<Rola> rolaList)
        {
            for (int i = 0; i < rolaList.Count; i++)
            {
                if (rolaDict.ContainsKey(rolaList[i].id))
                    updateRolaDict(rolaList[i]);
                else
                    addRola(rolaList[i]);
            }
        }

        private void updateRolaDict(Rola rola)
        {
            rolaDict[rola.id].name = rola.name;
            rolaDict[rola.id].description = rola.description;
        }

        internal void addRola(Rola rola)
        {
            if (String.IsNullOrEmpty(rola.id))
                rola.id = getRolaIdFromDB(rola);
            rolaIdList.Add(rola.id);
            rolaList.Add(rola);
            rolaDict.Add(rola.id, rola);
        }

        internal void removeRola(List<Rola> rolaList)
        {
            for (int i = 0; i < rolaList.Count; i++)
            {
                if (rolaDict.ContainsKey(rolaList[i].id))
                {
                    rolaDict.Remove(rolaList[i].id);
                    rolaIdList.Remove(rolaList[i].id);
                    this.rolaList.Remove(rolaList[i]);
                }
            }
        }

        #endregion

        #region czytanie id nowej roli z bazy danych
        private string getRolaIdFromDB(Rola rola)
        {
            string query = "select ID_rola from rola_app where ID_app = " + rola.app.id + " and name_rola = '" + rola.name + "' and descr_rola = '" + rola.description + "'";
            return new DBReader(LoginForm.dbConnection).readScalarFromDB(query).ToString();
        }
        #endregion

        #region zarządzanie modułami

        internal void addModule(AppModule module)
        {
            if(String.IsNullOrEmpty(module.id))
                module.id = getModuleIdFromDB(module);
            moduleList.Add(module);
        }

        private void removeModules(List<AppModule> modulesToRemove)
        {
            for (int i = 0; i < modulesToRemove.Count; i++)
            {
                removeModule(modulesToRemove[i]);
            }
        }

        private void removeModule(AppModule module)
        {
            for (int i = 0; i < this.moduleList.Count; i++)
            {
                if(this.moduleList[i].id == module.id)
                {
                    this.moduleList.RemoveAt(i);
                    break;
                }
            }
        }

        /// <summary>
        /// jeżeli aplikacja nie ma tego modułu, dodaje go; jeżeli aplikacja ma ten moduł, aktualizuje jego nazwę
        /// </summary>
        internal void refreshModuleList(List<AppModule> moduleList)
        {
            for (int i = 0; i < moduleList.Count; i++)
            {
                if (this.moduleList.Contains(moduleList[i]))
                    updateModule(moduleList[i]);
                else
                    addModule(moduleList[i]);
            }
        }

        private void updateModule(AppModule appModule)
        {
            for (int i = 0; i < this.moduleList.Count; i++)
            {
                if (this.moduleList[i].id == appModule.id)
                {
                    this.moduleList[i].name = appModule.name;
                    break;
                }
            }
        }
        #endregion

        #region zapisywanie modułów do bazy danych
        internal bool saveModulesToDB(List<AppModule> editedModules)
        {
            DBWriter writer = new DBWriter(LoginForm.dbConnection);
            string query = generateModuleSaveQuery(editedModules);
            if (writer.executeQuery(query))
            {
                refreshModuleList(editedModules);
                //MyMessageBox.display("Zapisano");
                return true;
            }
            return false;
        }

        private string generateModuleSaveQuery(List<AppModule> maduleList)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < maduleList.Count; i++)
            {
                sb.Append(generateSingleModuleSaveQuery(maduleList[i]));
            }
            return sb.ToString();
        }

        private string generateSingleModuleSaveQuery(AppModule module)
        {
            if (String.IsNullOrEmpty(module.id))
                return generateModuleInsertQuery(module);
            return generateModuleUpdateQuery(module);
        }

        private string generateModuleInsertQuery(AppModule module)
        {
            return "insert into mod_app (ID_app, name_mod) values(" + this.id + ", '" + module.name + "'); ";
        }

        private string generateModuleUpdateQuery(AppModule module)
        {
            return "update mod_app set name_mod = '" + module.name + "' where ID_mod = " + module.id + "; ";
        }

        #endregion

        #region usuwanie modułów z bazy danych
        internal bool deleteModules(List<AppModule> moduleList)
        {
            if (moduleList.Count == 0)
                return false;
            StringBuilder sb = new StringBuilder();
            List<AppModule> modulesToDelete = new List<AppModule>();
            for (int i = 0; i < moduleList.Count; i++)
            {
                if (!assertIsModuleUsed(moduleList[i]))
                {
                    sb.Append(getModuleDeleteQuery(moduleList[i]));
                    modulesToDelete.Add(moduleList[i]);
                }
            }
            if (new DBWriter(LoginForm.dbConnection).executeQuery(sb.ToString()))
            {
                this.removeModules(modulesToDelete);
                //if (moduleList.Count > modulesToDelete.Count)
                //    MyMessageBox.display("Niektóre moduły są w użyciu i nie zostały usunięte");
                return true;
            }
            return false;
        }

        private string getModuleDeleteQuery(AppModule module)
        {
            return "delete from mod_app where ID_mod = " + module.id + "; ";
        }
        #endregion

        #region czytanie id nowego modułu z bazy danych
        private string getModuleIdFromDB(AppModule module)
        {
            string query = "select ID_mod from mod_app where ID_app = " + this.id + " and name_mod = '" + module.name + "'";
            return new DBReader(LoginForm.dbConnection).readScalarFromDB(query).ToString();
        }
        #endregion

        #region metody pomocnicze prywatne
        private bool assertIsModuleUsed(AppModule module)
        {
            for (int i = 0; i < this.rolaList.Count; i++)
            {
                if (this.rolaList[i].hasAciveModule(module))
                    return true;
            }
            return false;
        }

        private bool assertAppDataIsValid()
        {
            return !String.IsNullOrEmpty(this.displayName) && !String.IsNullOrEmpty(this.executionPath) && !String.IsNullOrEmpty(this.name);
        }

        private bool assertIsValidDesktopApp()
        {
            return this.isValid && (this.runEnvironment == AppRunEnvironment.Windows || this.runEnvironment == AppRunEnvironment.Excel || this.runEnvironment == AppRunEnvironment.Internet);
        }

        private bool assertIsValidMicrostationApp()
        {
            return this.isValid && this.runEnvironment == AppRunEnvironment.Microstation;
        }

        #endregion

    }
}
