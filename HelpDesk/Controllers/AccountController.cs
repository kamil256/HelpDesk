using HelpDesk.DAL;
using HelpDesk.Infrastructure.Abstract;
using HelpDesk.Infrastructure.Concrete;
using HelpDesk.Models;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static HelpDesk.Infrastructure.Utilities;

namespace HelpDesk.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private IUnitOfWork unitOfWork;
        private IAuthProvider authProvider;

        public AccountController()//IUnitOfWork unitOfWork, IAuthProvider authProvider)
        {
            this.unitOfWork = new UnitOfWork();//unitOfWork;
            this.authProvider = new FormsAuthProvider(unitOfWork);//authProvider;
        }

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            AccountLoginViewModel model = new AccountLoginViewModel { ReturnUrl = returnUrl };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(AccountLoginViewModel model)
        {
            if (!ModelState.IsValid || !authProvider.Authenticate(model.Email, model.Password))
            {
                ModelState.AddModelError("", "Incorrect email or password");
                return View(model);
            }
            if (model.ReturnUrl != null)
                return Redirect(model.ReturnUrl);
            else
                return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public ActionResult LogOff()
        {
            authProvider.LogOut();
            return RedirectToAction("Index", "Home");
        }
    }
}