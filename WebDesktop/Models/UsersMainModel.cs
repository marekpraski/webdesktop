
using System.Collections.Generic;
using DatabaseInterface;
using UniwersalnyDesktop;

namespace WebDesktop.Models
{
    public class UsersMainModel
    {
        public List<DesktopUser> users { get; private set; }

        public UsersMainModel()
        {
            getDesktopUsers();
        }

        private void getDesktopUsers()
        {
            List<DesktopUser> users = new List<DesktopUser>();

            string query = "select [ID_user], [imie_user], [nazwisko_user], [login_user], [windows_user], [oddzial] from [users_list]";
            QueryData readData = new DBReader(LoginForm.dbConnection).readFromDB(query);

            for (int i = 0; i < readData.dataRowsNumber; i++)
            {
                users.Add(new DesktopUser() { id = readData.getDataValue(i, "ID_user").ToString(),
                    firstName = readData.getDataValue(i, "imie_user").ToString(), 
                    surname = readData.getDataValue(i, "nazwisko_user").ToString(), 
                    sqlLogin = readData.getDataValue(i, "login_user").ToString(),
                    windowsLogin = readData.getDataValue(i, "windows_user").ToString(), 
                    oddzial = readData.getDataValue(i, "oddzial").ToString()
                });
            }

            this.users = users;
        }
    }
}
