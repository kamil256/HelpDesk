using HelpDesk.DAL;
using HelpDesk.Entities;
using HelpDesk.Models;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace HelpDesk.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private IUnitOfWork unitOfWork;
        private HelpDeskContext context;

        public SettingsController()
        {
            unitOfWork = new UnitOfWork();// HttpContext.GetOwinContext().GetUserManager<AppUserManager>());
            context = new HelpDeskContext();
        }

        public ActionResult Index()
        {
            SettingsIndexViewModel model = new SettingsIndexViewModel();
            model.Categories = unitOfWork.CategoryRepository.Get(filters: null, orderBy: q => q.OrderBy(c => c.Order)).ToArray();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index([Bind(Include = "NewTicketsNotifications,SolvedTicketsNotifications,UsersPerPage,TicketsPerPage,CategoriesName,CategoriesId")] SettingsIndexViewModel model)
        {
            try
            {
                if (!await isCurrentUserAnAdminAsync())
                {
                    ModelState.Remove("CategoriesName");
                    ModelState.Remove("CategoriesId");
                }
                if (ModelState.IsValid)
                {
                    if (await isCurrentUserAnAdminAsync())
                    {
                        IEnumerable<Category> categories = context.Categories.Include("Tickets");
                        foreach (Category category in categories)
                        {
                            if (!model.CategoriesId.Contains(category.CategoryID))
                            {
                                if (context.Entry(category).State == EntityState.Deleted)
                                    context.Categories.Attach(category);
                                context.Categories.Remove(category);
                            }
                            else
                            {
                                int index = Array.IndexOf(model.CategoriesId, category.CategoryID);
                                category.Name = model.CategoriesName[index];
                                context.Categories.Attach(category);
                                context.Entry(category).State = EntityState.Modified;
                            }
                        }
                        for (int i = 0; i < model.CategoriesId.Length; i++)
                        {
                            if (model.CategoriesId[i] == 0)
                            {
                                context.Categories.Add(new Category { Name = model.CategoriesName[i], Order = i });
                            }
                            else
                            {
                                var category = categories.Single(c => c.CategoryID == model.CategoriesId[i]);
                                category.Order = i;
                                context.Entry(category).State = EntityState.Modified;
                            }
                        }
                    }
                    await context.SaveChangesAsync();
                    return RedirectToAction("Index", "Home");
                }
            }
            catch
            {
                
            }
            model.Categories = unitOfWork.CategoryRepository.Get(filters: null, orderBy: q => q.OrderBy(c => c.Order)).ToArray();
            return View(model);
        }

        private AppUserManager userManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<AppUserManager>();
            }
        }

        private AppRoleManager roleManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<AppRoleManager>();
            }
        }

        private async Task<AppUser> getCurrentUserAsync()
        {
            return await userManager.FindByEmailAsync(User.Identity.Name);
        }

        private async Task<bool> isCurrentUserAnAdminAsync()
        {
            return await userManager.IsInRoleAsync((await getCurrentUserAsync()).Id, "Admin");
        }
    }
}