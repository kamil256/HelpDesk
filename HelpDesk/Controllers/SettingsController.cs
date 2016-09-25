using HelpDesk.DAL;
using HelpDesk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HelpDesk.Controllers
{
    public class SettingsController : Controller
    {
        private IUnitOfWork unitOfWork;

        public SettingsController()
        {
            unitOfWork = new UnitOfWork();
        }

        public ActionResult Index()
        {
            SettingsIndexViewModel model = new SettingsIndexViewModel();
            model.Categories = unitOfWork.CategoryRepository.GetAll(filter: null, orderBy: q => q.OrderBy(c => c.Order)).ToArray();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(SettingsIndexViewModel model, string[] Categories2)
        {
            return View(model);
        }
    }
}