using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using HelpDesk.DAL;
using HelpDesk.Models;
using HelpDesk.Entities;
using static HelpDesk.Infrastructure.Utilities;
using System.Linq.Expressions;
using PagedList;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.Owin;

namespace HelpDesk.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private IUnitOfWork unitOfWork;
        private HelpDeskContext context;

        public TicketsController()
        {
            unitOfWork = new UnitOfWork();
            context = new HelpDeskContext();
        }

        public ActionResult Index([Bind(Include = "Status,AssignedToID,CategoryID,Search,AdvancedSearch,SortBy,DescSort,Page")] TicketsIndexViewModel model)
        {
            List<Expression<Func<Ticket, bool>>> filters = new List<Expression<Func<Ticket, bool>>>();
            if (model.Status != "All")
                filters.Add(t => t.Status == model.Status);
            if (model.AssignedToID != null)
                filters.Add(t => t.AssignedToID == model.AssignedToID);
            if (model.CategoryID != null)
                filters.Add(t => t.CategoryID == model.CategoryID);
            if (!string.IsNullOrWhiteSpace(model.Search))
            {
                if (model.AdvancedSearch)
                    filters.Add(t => t.Title.ToLower().Contains(model.Search.ToLower()) ||
                                     t.Content.ToLower().Contains(model.Search.ToLower()) ||
                                     t.Solution.ToLower().Contains(model.Search.ToLower()));
                else
                    filters.Add(t => t.Title.ToLower().Contains(model.Search.ToLower()));
            }
            
            Func<IQueryable<Ticket>, IOrderedQueryable<Ticket>> orderBy = null;
            switch (model.SortBy)
            {
                case "CreatedOn":
                    orderBy = q => model.DescSort ? q.OrderByDescending(t => t.CreatedOn) : q.OrderBy(t => t.CreatedOn);
                    break;
                case "RequestedBy":
                    orderBy = q => model.DescSort ? q.OrderByDescending(t => t.RequestedBy.UserName) : q.OrderBy(t => t.RequestedBy.UserName);
                    break;
                case "Title":
                    orderBy = q => model.DescSort ? q.OrderByDescending(t => t.Title) : q.OrderBy(t => t.Title);
                    break;
                case "Category":
                    orderBy = q => model.DescSort ? q.OrderByDescending(t => t.Category.Name) : q.OrderBy(t => t.Category.Name);
                    break;
                case "Status":
                    orderBy = q => model.DescSort ? q.OrderByDescending(t => t.Status) : q.OrderBy(t => t.Status);
                    break;
                case "AssignedTo":
                    orderBy = q => model.DescSort ? q.OrderByDescending(t => t.AssignedTo.UserName) : q.OrderBy(t => t.AssignedTo.UserName);
                    break;
            }

            IQueryable<Ticket> query = context.Tickets;
            foreach (var filter in filters)
                if (filter != null)
                    query = query.Where(filter);
            if (orderBy != null)
                query = orderBy(query);
            model.Tickets = query.ToPagedList(model.Page, 5);

            string adminRoleId = roleManager.Roles.Single(r => r.Name == "Admin").Id;
            model.Admins = userManager.Users.Where(u => u.Roles.FirstOrDefault().RoleId == adminRoleId);// unitOfWork.UserRepository.GetAll(u => u.Role == "Admin", orderBy: o => o.OrderBy(t => t.FirstName));
            model.Categories = context.Categories.OrderBy(c => c.Order);
            
            return View(model);
        }

        public async Task<ActionResult> Create()
        {
            TicketsCreateViewModel model = new TicketsCreateViewModel();
            model.RequestedBy = await userManager.FindByEmailAsync(User.Identity.Name);
            
            //model.RequestedBy = unitOfWork.UserRepository.GetAll(u => u.Email == User.Identity.Name).Single();
            model.RequestedByID = model.RequestedBy.Id;
            model.Categories = context.Categories.OrderBy(c => c.Order);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "RequestedByID,CategoryID,Title,Content")] TicketsCreateViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Ticket ticket = new Ticket()
                    {
                        CreatedByID = (await userManager.FindByEmailAsync(User.Identity.Name)).Id,
                        //CreatedBy = unitOfWork.UserRepository.GetAll(u => u.Email == User.Identity.Name).Single(),
                        RequestedByID = model.RequestedByID,
                        CreatedOn = DateTime.Now,
                        Status = "New",
                        CategoryID = model.CategoryID ?? 0,
                        Title = model.Title,
                        Content = model.Content
                    };
                    context.Tickets.Add(ticket);
                    await context.SaveChangesAsync();
                    //unitOfWork.TicketRepository.Insert(ticket);
                    //unitOfWork.Save();
                    TempData["Success"] = "Successfully added new ticket!";
                    return RedirectToAction("Index");
                }
            }
            catch
            {
                ModelState.AddModelError("", "Cannot create ticket. Try again!");
            }

            model.RequestedBy = await userManager.FindByIdAsync(model.RequestedByID);
            //model.RequestedBy = unitOfWork.UserRepository.GetById(model.RequestedByID ?? 0);
            model.Categories = unitOfWork.CategoryRepository.GetAll(filter: null, orderBy: c => c.OrderBy(o => o.Order));

            return View(model);
        }

        public async Task<ActionResult> Edit(int? id)
        {
            Ticket ticket = await context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            TicketsEditViewModel model = new TicketsEditViewModel
            {
                TicketID = ticket.TicketID,
                RequestedByID = ticket.RequestedByID,
                AssignedToID = ticket.AssignedToID,
                Status = ticket.Status,
                CategoryID = ticket.CategoryID,
                Title = ticket.Title,
                Content = ticket.Content,
                Solution = ticket.Solution,
                CreatedBy = ticket.CreatedBy,
                CreatedOn = ticket.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                RequestedBy = ticket.RequestedBy,                
                AssignedTo = ticket.AssignedTo
            };

            string adminRoleId = roleManager.Roles.Single(r => r.Name == "Admin").Id;
            model.Admins = userManager.Users.Where(u => u.Roles.FirstOrDefault().RoleId == adminRoleId);
            //model.Admins = unitOfWork.UserRepository.GetAll(u => u.Role == "Admin", orderBy: o => o.OrderBy(t => t.FirstName));
            model.Categories = unitOfWork.CategoryRepository.GetAll(filter: null, orderBy: c => c.OrderBy(o => o.Order));
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "TicketID,RequestedByID,AssignedToID,Status,CategoryID,Title,Content,Solution")] TicketsEditViewModel model)
        {
            Ticket ticket = await context.Tickets.FindAsync(model.TicketID);
            if (ticket == null)
            {
                return HttpNotFound();
            }

            model.RequestedBy = await userManager.FindByIdAsync(model.RequestedByID);//unitOfWork.UserRepository.GetById(model.RequestedByID ?? 0);
            model.AssignedTo = await userManager.FindByIdAsync(model.AssignedToID);//unitOfWork.UserRepository.GetById(model.AssignedToID ?? 0);
            try
            {
                if (ModelState.IsValid)
                {
                    ticket.RequestedByID = model.RequestedByID;
                    ticket.AssignedToID = model.AssignedToID;
                    ticket.Status = model.Status;
                    ticket.CategoryID = model.CategoryID;
                    ticket.Title = model.Title;
                    ticket.Content = model.Content;
                    ticket.Solution = model.Solution;
                    context.Tickets.Attach(ticket);
                    context.Entry(ticket).State = EntityState.Modified;
                    await context.SaveChangesAsync();
                    //unitOfWork.TicketRepository.Update(ticket);
                    //unitOfWork.Save();
                    TempData["Success"] = "Successfully edited ticket!";
                    return RedirectToAction("Index");
                }
            }
            catch
            {
                ModelState.AddModelError("", "Cannot edit ticket. Try again!");
            }
            model.CreatedBy = ticket.CreatedBy;
            model.CreatedOn = ticket.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss");

            string adminRoleId = roleManager.Roles.Single(r => r.Name == "Admin").Id;
            model.Admins = userManager.Users.Where(u => u.Roles.FirstOrDefault().RoleId == adminRoleId);
            //model.Admins = unitOfWork.UserRepository.GetAll(u => u.Role == "Admin", orderBy: o => o.OrderBy(t => t.FirstName));
            model.Categories = unitOfWork.CategoryRepository.GetAll(filter: null, orderBy: c => c.OrderBy(o => o.Order));

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            try
            { 
                unitOfWork.TicketRepository.Delete(id);
                unitOfWork.Save();
                TempData["Success"] = "Successfully deleted ticket!";
            }
            catch
            {
                TempData["Fail"] = "Cannot delete ticket. Try again!";
            }
            return RedirectToAction("Index");
        }

        public JsonResult FindUsers(string search)
        {
            var result = from x in userManager.Users.Where//unitOfWork.UserRepository.GetAll
                         (
                              u => (u.FirstName + " " + u.LastName).ToLower().Contains(search.ToLower()) ||
                              u.Email.ToLower().Contains(search.ToLower())
                         )
                         select new
                         {
                             UserID = x.Id,
                             FirstName = x.FirstName,
                             LastName = x.LastName,
                             Email = x.Email
                         };
            return Json(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<RedirectResult> AssignUserToTicket(string assignUserID, int assignTicketID, string returnUrl)
        {
            try
            {
                AppUser user = await userManager.FindByIdAsync(assignUserID);//unitOfWork.UserRepository.GetById(assignUserID);
                Ticket ticket = unitOfWork.TicketRepository.GetById(assignTicketID);
                ticket.AssignedToID = user.Id;
                ticket.Status = "In progress";
                unitOfWork.TicketRepository.Update(ticket);
                unitOfWork.Save();
                TempData["Success"] = "Successfully assigned user to ticket!";
            }
            catch
            {
                TempData["Fail"] = "Cannot assign user to ticket. Try again!";
            }
            return Redirect(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<RedirectResult> SolveTicket(string solveUserID, int solveTicketID, string solution, string returnUrl)
        {
            try
            { 
                AppUser user = await userManager.FindByIdAsync(solveUserID);//unitOfWork.UserRepository.GetById(solveUserID);
                Ticket ticket = unitOfWork.TicketRepository.GetById(solveTicketID);
                ticket.AssignedToID = user.Id;
                ticket.Status = "Solved";
                ticket.Solution = solution;
                unitOfWork.TicketRepository.Update(ticket);
                unitOfWork.Save();
                TempData["Success"] = "Successfully solved ticket!";
            }
            catch
            {
                TempData["Fail"] = "Cannot solve ticket. Try again!";
            }
            return Redirect(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<RedirectResult> CloseTicket(int closeTicketID, string returnUrl)
        {
            try
            { 
                AppUser user = await userManager.FindByEmailAsync(User.Identity.Name);
                Ticket ticket = unitOfWork.TicketRepository.GetById(closeTicketID);
                ticket.AssignedToID = user.Id;
                ticket.Status = "Closed";
                unitOfWork.TicketRepository.Update(ticket);
                unitOfWork.Save();
                TempData["Success"] = "Successfully closed ticket!";
            }
            catch
            {
                TempData["Fail"] = "Cannot close ticket. Try again!";
            }
            return Redirect(returnUrl);
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
    }
}
