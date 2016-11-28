using HelpDesk.DAL.Abstract;
using HelpDesk.DAL.Concrete;
using HelpDesk.DAL.Entities;
using HelpDesk.UI.ViewModels;
using HelpDesk.UI.ViewModels.Settings;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace HelpDesk.UI.Controllers.MVC
{
    [Authorize]
    public class SettingsController : Controller
    {
        private UserManager UserManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<UserManager>();
            }
        }

        private RoleManager RoleManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<RoleManager>();
            }
        }

        private User CurrentUser
        {
            get
            {
                return UserManager.FindByNameAsync(User.Identity.Name).Result;
            }
        }

        private IUnitOfWork unitOfWork;

        public SettingsController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public ViewResult Index()
        {
            Settings settings = CurrentUser.Settings; 
            IndexViewModel model = new IndexViewModel();
            model.NewTicketsNotifications = settings.NewTicketsNotifications;
            model.SolvedTicketsNotifications = settings.SolvedTicketsNotifications;
            model.UsersPerPage = settings.UsersPerPage;
            model.TicketsPerPage = settings.TicketsPerPage;
            model.Categories = unitOfWork.CategoryRepository.Get(filters: null, orderBy: q => q.OrderBy(c => c.Order)).ToArray();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index([Bind(Include = "NewTicketsNotifications,SolvedTicketsNotifications,UsersPerPage,TicketsPerPage,CategoriesName,CategoriesId")] IndexViewModel model)
        {
            //try
            {
                if (!UserManager.IsInRole(CurrentUser.Id, "Admin"))
                {
                    ModelState.Remove("CategoriesName");
                    ModelState.Remove("CategoriesId");
                    ModelState.Remove("UsersPerPage");
                }
                if (ModelState.IsValid)
                {
                    Settings settings = CurrentUser.Settings;
                    settings.NewTicketsNotifications = model.NewTicketsNotifications;
                    settings.SolvedTicketsNotifications = model.SolvedTicketsNotifications;
                    settings.TicketsPerPage = model.TicketsPerPage ?? 10;
                    // todo: copy whole model with categories to categories in DB. If new then insert, else update without checking if it's different (addorupdate?)
                    if (UserManager.IsInRole(CurrentUser.Id, "Admin"))
                    {
                        settings.UsersPerPage = model.UsersPerPage ?? 10;
                        IEnumerable<Category> categories = unitOfWork.CategoryRepository.Get(includeProperties: "Tickets");
                        foreach (Category category in categories)
                        {
                            if (!model.CategoriesId.Contains(category.CategoryId))
                                unitOfWork.CategoryRepository.Delete(category);
                            else
                            {
                                int index = Array.IndexOf(model.CategoriesId, category.CategoryId);
                                category.Name = model.CategoriesName[index];
                                unitOfWork.CategoryRepository.Update(category);
                            }
                        }
                        for (int i = 0; i < model.CategoriesId.Length; i++)
                        {
                            if (model.CategoriesId[i] == 0)
                            {
                                unitOfWork.CategoryRepository.Insert(new Category { Name = model.CategoriesName[i], Order = i });
                            }
                            else
                            {
                                var category = categories.Single(c => c.CategoryId == model.CategoriesId[i]);
                                category.Order = i;
                                unitOfWork.CategoryRepository.Update(category);
                            }
                        }
                    }
                    unitOfWork.SettingsRepository.Update(settings);
                    unitOfWork.Save();
                    return RedirectToAction("Index", "Home");
                }
            }
            //catch
            //{

            //}
            model.Categories = unitOfWork.CategoryRepository.Get(orderBy: x => x.OrderBy(c => c.Order)).ToArray();
            return View(model);
        }
    }
}