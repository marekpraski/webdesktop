using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UniwersalnyDesktop;

namespace WebDesktop.Controllers
{
    public class UsersController : Controller
    {
        Models.UsersMainModel users = new Models.UsersMainModel();
        public IActionResult Index()
        {
            return View(users);
        }

        // GET: UsersController/Details/5
        public IActionResult Details(int id)
        {
            return View(users.users[id]);
        }

        //GET
        public IActionResult Edit(int id)
        {
            return View(users.users[id]);
        }

        //In ASP.NET MVC controller actions always take/pass view models from/to views.
        //View models are classes specifically designed for a given view.
        //So in your case your controller action could take a view model which has a property called Id
        //so that the default model binder does the job of mapping it from the url.
        [HttpPost]
        public IActionResult Index(DesktopUser user)
        {
            updateUsers(user);
            return View(users);
        }

        private void updateUsers(DesktopUser user)
        {
            int index = new UtilityTools.NumberHandler().tryGetInt(user.id);
            user.id = users.users[index].id;
            users.users[index] = user;
            user.updateInDB();
        }
    }
}
