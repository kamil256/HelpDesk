using HelpDesk.DAL.Abstract;
using HelpDesk.DAL.Concrete;
using HelpDesk.DAL.Entities;
using HelpDesk.UI.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace HelpDesk.UI.Controllers.MVC
{
    public class AccountController : Controller
    {
        private IUnitOfWork unitOfWork;

        public AccountController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public ActionResult Login(string returnUrl)
        {
            AccountLoginViewModel model = new AccountLoginViewModel { ReturnUrl = returnUrl };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(AccountLoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                UserManager userManager = HttpContext.GetOwinContext().GetUserManager<UserManager>();
                IAuthenticationManager AuthManager = HttpContext.GetOwinContext().Authentication;
                User user = await userManager.FindAsync(model.Email, model.Password);
                if (user == null)
                    ModelState.AddModelError("", "Incorrect email or password");
                else
                {
                    AuthManager.SignOut();
                    AuthManager.SignIn(await userManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie));
                    return Redirect(model.ReturnUrl ?? "/");
                }
            }
            return View(model);
        }

        public ActionResult LogOff()
        {
            IAuthenticationManager AuthManager = HttpContext.GetOwinContext().Authentication;
            AuthManager.SignOut();
            return RedirectToAction("Index", "Home");
        }
    }
}