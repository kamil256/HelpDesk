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

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid || !authProvider.Authenticate(model.Email, model.Password))
            {                return View(model);
            }
            return Redirect(returnUrl);
        }

        

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(NewUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (unitOfWork.UserRepository.GetAll(u => u.Email.ToLower() == model.Email.ToLower()) == null)
                {
                    var user = new User { Email = model.Email, Password = model.Password };
                    user.Salt = Guid.NewGuid().ToString();
                    user.Password = HashPassword(user.Password, user.Salt);
                    unitOfWork.UserRepository.Insert(user);
                    return Redirect(Url.Action("Index", "Home"));
                }
            }                
            return View(model);
        }
    }
}