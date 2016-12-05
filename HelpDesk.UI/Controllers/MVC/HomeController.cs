using HelpDesk.DAL.Abstract;
using HelpDesk.DAL.Entities;
using HelpDesk.UI.Infrastructure;
using HelpDesk.UI.ViewModels.Home;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace HelpDesk.UI.Controllers.MVC
{
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IdentityHelper identityHelper;

        public HomeController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
            identityHelper = new IdentityHelper();
        }

        [OverrideAuthorization]
        [Authorize]
        public ActionResult Index()
        {
            IndexViewModel model = new IndexViewModel();

            string adminRoleId = identityHelper.RoleManager.Roles.Single(r => r.Name == "Admin").Id;

            DateTime tenMinutesAgo = DateTime.Now.AddMinutes(-10);
            DateTime sevenDaysAgo = DateTime.Now.AddDays(-7);
            DateTime thirtyDaysAgo = DateTime.Now.AddDays(-30);

            if (identityHelper.IsCurrentUserAnAdministrator())
            {
                model.TotalTicketsCount = unitOfWork.TicketRepository.Count();
                model.TotalUsersCount = identityHelper.UserManager.Users.Count();
                model.LoggedInUsersCount = identityHelper.UserManager.Users.Count(u => u.LastActivity >= tenMinutesAgo);
            }
            else
            {
                model.CreatedTickets = unitOfWork.TicketRepository.Count(t => t.CreatorId == identityHelper.CurrentUser.Id);
                model.RequestedTickets = unitOfWork.TicketRepository.Count(t => t.RequesterId == identityHelper.CurrentUser.Id);
                int solvedTickets = unitOfWork.TicketRepository.Count(t => (t.CreatorId == identityHelper.CurrentUser.Id || t.RequesterId == identityHelper.CurrentUser.Id) && t.Status == "Solved");
                int totalTicketsCount = unitOfWork.TicketRepository.Count(t => t.CreatorId == identityHelper.CurrentUser.Id || t.RequesterId == identityHelper.CurrentUser.Id);
                model.SolvedTicketsPercentage = totalTicketsCount == 0 ? 0 : (int)Math.Round((double)solvedTickets / totalTicketsCount * 100);
            }

            model.Last7DaysTicketsByStatusCounts = new Dictionary<string, int>();
            if (identityHelper.IsCurrentUserAnAdministrator())
            {
                model.Last7DaysTicketsByStatusCounts.Add("New", unitOfWork.TicketRepository.Count(t => t.CreateDate >= sevenDaysAgo && t.Status == "New"));
                model.Last7DaysTicketsByStatusCounts.Add("In progress", unitOfWork.TicketRepository.Count(t => t.CreateDate >= sevenDaysAgo && t.Status == "In progress"));
                model.Last7DaysTicketsByStatusCounts.Add("Solved", unitOfWork.TicketRepository.Count(t => t.CreateDate >= sevenDaysAgo && t.Status == "Solved"));
                model.Last7DaysTicketsByStatusCounts.Add("Closed", unitOfWork.TicketRepository.Count(t => t.CreateDate >= sevenDaysAgo && t.Status == "Closed"));
            }
            else
            {
                model.Last7DaysTicketsByStatusCounts.Add("New", unitOfWork.TicketRepository.Count(t => t.CreateDate >= sevenDaysAgo && t.Status == "New" && (t.CreatorId == identityHelper.CurrentUser.Id || t.RequesterId == identityHelper.CurrentUser.Id)));
                model.Last7DaysTicketsByStatusCounts.Add("In progress", unitOfWork.TicketRepository.Count(t => t.CreateDate >= sevenDaysAgo && t.Status == "In progress" && (t.CreatorId == identityHelper.CurrentUser.Id || t.RequesterId == identityHelper.CurrentUser.Id)));
                model.Last7DaysTicketsByStatusCounts.Add("Solved", unitOfWork.TicketRepository.Count(t => t.CreateDate >= sevenDaysAgo && t.Status == "Solved" && (t.CreatorId == identityHelper.CurrentUser.Id || t.RequesterId == identityHelper.CurrentUser.Id)));
                model.Last7DaysTicketsByStatusCounts.Add("Closed", unitOfWork.TicketRepository.Count(t => t.CreateDate >= sevenDaysAgo && t.Status == "Closed" && (t.CreatorId == identityHelper.CurrentUser.Id || t.RequesterId == identityHelper.CurrentUser.Id)));
            }

            model.Last30DaysTicketsByStatusCounts = new Dictionary<string, int>();
            if (identityHelper.IsCurrentUserAnAdministrator())
            {
                model.Last30DaysTicketsByStatusCounts.Add("New", unitOfWork.TicketRepository.Count(t => t.CreateDate < sevenDaysAgo && t.CreateDate >= thirtyDaysAgo && t.Status == "New"));
                model.Last30DaysTicketsByStatusCounts.Add("In progress", unitOfWork.TicketRepository.Count(t => t.CreateDate < sevenDaysAgo && t.CreateDate >= thirtyDaysAgo && t.Status == "In progress"));
                model.Last30DaysTicketsByStatusCounts.Add("Solved", unitOfWork.TicketRepository.Count(t => t.CreateDate < sevenDaysAgo && t.CreateDate >= thirtyDaysAgo && t.Status == "Solved"));
                model.Last30DaysTicketsByStatusCounts.Add("Closed", unitOfWork.TicketRepository.Count(t => t.CreateDate < sevenDaysAgo && t.CreateDate >= thirtyDaysAgo && t.Status == "Closed"));
            }
            else
            {
                model.Last30DaysTicketsByStatusCounts.Add("New", unitOfWork.TicketRepository.Count(t => t.CreateDate < sevenDaysAgo && t.CreateDate >= thirtyDaysAgo && t.Status == "New" && (t.CreatorId == identityHelper.CurrentUser.Id || t.RequesterId == identityHelper.CurrentUser.Id)));
                model.Last30DaysTicketsByStatusCounts.Add("In progress", unitOfWork.TicketRepository.Count(t => t.CreateDate < sevenDaysAgo && t.CreateDate >= thirtyDaysAgo && t.Status == "In progress" && (t.CreatorId == identityHelper.CurrentUser.Id || t.RequesterId == identityHelper.CurrentUser.Id)));
                model.Last30DaysTicketsByStatusCounts.Add("Solved", unitOfWork.TicketRepository.Count(t => t.CreateDate < sevenDaysAgo && t.CreateDate >= thirtyDaysAgo && t.Status == "Solved" && (t.CreatorId == identityHelper.CurrentUser.Id || t.RequesterId == identityHelper.CurrentUser.Id)));
                model.Last30DaysTicketsByStatusCounts.Add("Closed", unitOfWork.TicketRepository.Count(t => t.CreateDate < sevenDaysAgo && t.CreateDate >= thirtyDaysAgo && t.Status == "Closed" && (t.CreatorId == identityHelper.CurrentUser.Id || t.RequesterId == identityHelper.CurrentUser.Id)));
            }

            model.OlderThan30DaysTicketsByStatusCounts = new Dictionary<string, int>();
            if (identityHelper.IsCurrentUserAnAdministrator())
            {
                model.OlderThan30DaysTicketsByStatusCounts.Add("New", unitOfWork.TicketRepository.Count(t => t.CreateDate < thirtyDaysAgo && t.Status == "New"));
                model.OlderThan30DaysTicketsByStatusCounts.Add("In progress", unitOfWork.TicketRepository.Count(t => t.CreateDate < thirtyDaysAgo && t.Status == "In progress"));
                model.OlderThan30DaysTicketsByStatusCounts.Add("Solved", unitOfWork.TicketRepository.Count(t => t.CreateDate < thirtyDaysAgo && t.Status == "Solved"));
                model.OlderThan30DaysTicketsByStatusCounts.Add("Closed", unitOfWork.TicketRepository.Count(t => t.CreateDate < thirtyDaysAgo && t.Status == "Closed"));
            }
            else
            {
                model.OlderThan30DaysTicketsByStatusCounts.Add("New", unitOfWork.TicketRepository.Count(t => t.CreateDate < thirtyDaysAgo && t.Status == "New" && (t.CreatorId == identityHelper.CurrentUser.Id || t.RequesterId == identityHelper.CurrentUser.Id)));
                model.OlderThan30DaysTicketsByStatusCounts.Add("In progress", unitOfWork.TicketRepository.Count(t => t.CreateDate < thirtyDaysAgo && t.Status == "In progress" && (t.CreatorId == identityHelper.CurrentUser.Id || t.RequesterId == identityHelper.CurrentUser.Id)));
                model.OlderThan30DaysTicketsByStatusCounts.Add("Solved", unitOfWork.TicketRepository.Count(t => t.CreateDate < thirtyDaysAgo && t.Status == "Solved" && (t.CreatorId == identityHelper.CurrentUser.Id || t.RequesterId == identityHelper.CurrentUser.Id)));
                model.OlderThan30DaysTicketsByStatusCounts.Add("Closed", unitOfWork.TicketRepository.Count(t => t.CreateDate < thirtyDaysAgo && t.Status == "Closed" && (t.CreatorId == identityHelper.CurrentUser.Id || t.RequesterId == identityHelper.CurrentUser.Id)));
            }

            string currentUserId = identityHelper.CurrentUser.Id;

            DateTime currentMonthDate = DateTime.Now;
            model.CurrentMonthName = currentMonthDate.ToString("MMMM", CultureInfo.InvariantCulture);
            model.YearInCurrentMonth = currentMonthDate.Year;
            if (identityHelper.IsCurrentUserAnAdministrator())
            {
                model.CurrentMonthTicketsByCategoryCounts = unitOfWork.CategoryRepository.Get().Select(c => new IndexViewModel.Category { Name = c.Name, TicketsCount = c.Tickets.Count(t => t.CreateDate.Month == currentMonthDate.Month) }).ToList();
                model.CurrentMonthTicketsByCategoryCounts.Add(new IndexViewModel.Category
                {
                    Name = "No category",
                    TicketsCount = unitOfWork.TicketRepository.Count(t => t.Category == null && t.CreateDate.Month == currentMonthDate.Month)
                });
            }
            else
                model.CurrentMonthTicketsByCategoryCounts = unitOfWork.CategoryRepository.Get(filters: new Expression<Func<Category, bool>>[] { c => c.Tickets.Where(t => t.CreateDate.Month == currentMonthDate.Month && (t.CreatorId == currentUserId || t.RequesterId == currentUserId)).Count() > 0 }).Select(c => new IndexViewModel.Category { Name = c.Name, TicketsCount = c.Tickets.Count(t => t.CreateDate.Month == currentMonthDate.Month) }).ToList();
            model.CurrentMonthTicketsByCategoryCounts = model.CurrentMonthTicketsByCategoryCounts.OrderByDescending(c => c.TicketsCount).ToList();

            DateTime lastMonthDate = DateTime.Now.AddMonths(-1);
            model.LastMonthName = lastMonthDate.ToString("MMMM", CultureInfo.InvariantCulture);
            model.YearInLastMonth = lastMonthDate.Year;
            if (identityHelper.IsCurrentUserAnAdministrator())
            {
                model.LastMonthTicketsByCategoryCounts = unitOfWork.CategoryRepository.Get().Select(c => new IndexViewModel.Category { Name = c.Name, TicketsCount = c.Tickets.Count(t => t.CreateDate.Month == lastMonthDate.Month) }).ToList();
                model.LastMonthTicketsByCategoryCounts.Add(new IndexViewModel.Category
                {
                    Name = "No category",
                    TicketsCount = unitOfWork.TicketRepository.Count(t => t.Category == null && t.CreateDate.Month == lastMonthDate.Month)
                });
            }
            else
                model.LastMonthTicketsByCategoryCounts = unitOfWork.CategoryRepository.Get(filters: new Expression<Func<Category, bool>>[] { c => c.Tickets.Where(t => t.CreateDate.Month == lastMonthDate.Month && (t.CreatorId == currentUserId || t.RequesterId == currentUserId)).Count() > 0 }).Select(c => new IndexViewModel.Category { Name = c.Name, TicketsCount = c.Tickets.Count(t => t.CreateDate.Month == lastMonthDate.Month) }).ToList();
            model.LastMonthTicketsByCategoryCounts = model.LastMonthTicketsByCategoryCounts.OrderByDescending(c => c.TicketsCount).ToList();

            DateTime monthBeforeLastDate = DateTime.Now.AddMonths(-2);
            model.MonthBeforeLastName = monthBeforeLastDate.ToString("MMMM", CultureInfo.InvariantCulture);
            model.YearInMonthBeforeLast = monthBeforeLastDate.Year;
            if (identityHelper.IsCurrentUserAnAdministrator())
            {
                model.MonthBeforeLastTicketsByCategoryCounts = unitOfWork.CategoryRepository.Get().Select(c => new IndexViewModel.Category { Name = c.Name, TicketsCount = c.Tickets.Count(t => t.CreateDate.Month == monthBeforeLastDate.Month) }).ToList();
                model.MonthBeforeLastTicketsByCategoryCounts.Add(new IndexViewModel.Category
                {
                    Name = "No category",
                    TicketsCount = unitOfWork.TicketRepository.Count(t => t.Category == null && t.CreateDate.Month == monthBeforeLastDate.Month)
                });
            }
            else
                model.MonthBeforeLastTicketsByCategoryCounts = unitOfWork.CategoryRepository.Get(filters: new Expression<Func<Category, bool>>[] { c => c.Tickets.Where(t => t.CreateDate.Month == monthBeforeLastDate.Month && (t.CreatorId == currentUserId || t.RequesterId == currentUserId)).Count() > 0 }).Select(c => new IndexViewModel.Category { Name = c.Name, TicketsCount = c.Tickets.Count(t => t.CreateDate.Month == monthBeforeLastDate.Month) }).ToList();
            model.MonthBeforeLastTicketsByCategoryCounts = model.MonthBeforeLastTicketsByCategoryCounts.OrderByDescending(c => c.TicketsCount).ToList();

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