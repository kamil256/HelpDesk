using HelpDesk.DAL.Abstract;
using HelpDesk.DAL.Concrete;
using HelpDesk.DAL.Entities;
using HelpDesk.UI.Infrastructure;
using HelpDesk.UI.Infrastructure.Abstract;
using HelpDesk.UI.ViewModels;
using HelpDesk.UI.ViewModels.Settings;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace HelpDesk.UI.Controllers.MVC
{
    [Authorize(Roles = "Admin")]
    public class SettingsController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IIdentityHelper identityHelper;

        public SettingsController(IUnitOfWork unitOfWork, IIdentityHelper identityHelper)
        {
            this.unitOfWork = unitOfWork;
            this.identityHelper = identityHelper;
        }

        [OverrideAuthorization]
        [Authorize]
        public ViewResult Index()
        {
            Settings settings = identityHelper.CurrentUser.Settings;

            IndexViewModel model = new IndexViewModel
            {
                NewTicketsNotifications = settings.NewTicketsNotifications,
                SolvedTicketsNotifications = settings.SolvedTicketsNotifications,
                ClosedTicketsNotifications = settings.ClosedTicketsNotifications,
                TicketsPerPage = settings.TicketsPerPage
            };

            if (identityHelper.IsCurrentUserAnAdministrator())
            {
                model.AssignedTicketsNotifications = settings.AssignedTicketsNotifications;
                model.UsersPerPage = settings.UsersPerPage;
                model.Categories = unitOfWork.CategoryRepository.Get(orderBy: q => q.OrderBy(c => c.Order)).ToList();
            }
            else
            {
                model.Categories = new List<Category>();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [OverrideAuthorization]
        [Authorize]
        public ActionResult Index([Bind(Include = "NewTicketsNotifications,AssignedTicketsNotifications,SolvedTicketsNotifications,ClosedTicketsNotifications,UsersPerPage,TicketsPerPage,Categories")] IndexViewModel model)
        {
            if (!identityHelper.IsCurrentUserAnAdministrator())
            {
                ModelState.Remove("AssignedTicketsNotifications");
                ModelState.Remove("UsersPerPage");
                ModelState.Remove("Categories");
            }
            else
            {
                int order = 0;
                model.Categories.ForEach(c => c.Order = order++);
            }

            try
            {
                if (ModelState.IsValid)
                {
                    Settings settings = unitOfWork.SettingsRepository.GetById(identityHelper.CurrentUser.Id);
                    settings.NewTicketsNotifications = model.NewTicketsNotifications;
                    settings.SolvedTicketsNotifications = model.SolvedTicketsNotifications;
                    settings.ClosedTicketsNotifications = model.ClosedTicketsNotifications;
                    settings.TicketsPerPage = model.TicketsPerPage;

                    if (identityHelper.IsCurrentUserAnAdministrator())
                    {
                        settings.AssignedTicketsNotifications = model.AssignedTicketsNotifications;

                        settings.UsersPerPage = model.UsersPerPage;

                        IEnumerable<Category> existingCategories = unitOfWork.CategoryRepository.Get(includeProperties: "Tickets");

                        foreach (Category existingCategory in existingCategories)
                        {
                            if (model.Categories.SingleOrDefault(c => c.CategoryId == existingCategory.CategoryId) == null)
                                unitOfWork.CategoryRepository.Delete(existingCategory);
                        }

                        foreach (Category newCategory in model.Categories)
                        {
                            Category category = existingCategories.SingleOrDefault(c => c.CategoryId == newCategory.CategoryId);
                            if (category == null)
                                unitOfWork.CategoryRepository.Insert(new Category
                                {
                                    Name = newCategory.Name,
                                    Order = newCategory.Order
                                });
                            else
                            {
                                category.Name = newCategory.Name;
                                category.Order = newCategory.Order;
                                unitOfWork.CategoryRepository.Update(category);
                            }
                        }
                    }
                    unitOfWork.SettingsRepository.Update(settings);
                    unitOfWork.Save();
                    TempData["Success"] = "Successfully saved settings.";
                    return RedirectToAction("Index");
                }
            }
            catch
            {
                TempData["Fail"] = "Unable to save settings. Try again, and if the problem persists contact your system administrator.";
            }

            return View(model);
        }

        //protected override void OnException(ExceptionContext filterContext)
        //{
        //    if (!filterContext.ExceptionHandled)
        //    {
        //        filterContext.Result = new RedirectResult("~/Content/Error.html");
        //        filterContext.ExceptionHandled = true;
        //    }
        //}
    }    
}