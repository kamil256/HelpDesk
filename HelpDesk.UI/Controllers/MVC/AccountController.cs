using HelpDesk.DAL.Abstract;
using HelpDesk.DAL.Concrete;
using HelpDesk.DAL.Entities;
using HelpDesk.UI.Infrastructure.Abstract;
using HelpDesk.UI.Infrastructure.Concrete;
using HelpDesk.UI.ViewModels;
using HelpDesk.UI.ViewModels.Account;
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
    [Authorize(Roles = "Admin")]
    public class AccountController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IIdentityHelper identityHelper;

        public AccountController(IUnitOfWork unitOfWork, IIdentityHelper identityHelper)
        {
            this.unitOfWork = unitOfWork;
            this.identityHelper = identityHelper;
        }

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("LogOut");
            }

            LoginViewModel model = new LoginViewModel
            {
                ReturnUrl = returnUrl
            };
            TempData["Success"] = "Demonstracyjny login: demo@example.com, hasło: password. Nie możesz wprowadzać zmian w kontach administratorów.";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<ActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await identityHelper.UserManager.FindAsync(model.Email, model.Password);
                if (user == null)
                    ModelState.AddModelError("", "Incorrect email or password");
                else if (!user.Active)
                    ModelState.AddModelError("", "Account is inactive");
                else
                {
                    IAuthenticationManager authenticationManager = HttpContext.GetOwinContext().Authentication;
                    authenticationManager.SignOut();
                    authenticationManager.SignIn(await identityHelper.UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie));
                    return Redirect(model.ReturnUrl ?? "/");
                }
            }
            TempData["Success"] = "Demo username: demo@example.com, demo password: password. You can't modify own or other administrator's account.";
            return View(model);
        }

        [OverrideAuthorization]
        [Authorize]
        public ActionResult LogOut()
        {
            IAuthenticationManager authenticationManager = HttpContext.GetOwinContext().Authentication;
            authenticationManager.SignOut();
            return RedirectToAction("Index", "Home");
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            if (!filterContext.ExceptionHandled)
            {
                filterContext.Result = new RedirectResult("~/Content/Error.html");
                filterContext.ExceptionHandled = true;
            }
        }
    }
}