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
using Microsoft.AspNet.Identity;
using System.Text;

namespace HelpDesk.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TicketsController : Controller
    {
        private IUnitOfWork unitOfWork;
        private HelpDeskContext context;

        public TicketsController()
        {
            unitOfWork = new UnitOfWork();
            context = new HelpDeskContext();
        }

        [OverrideAuthorization]
        public async Task<ActionResult> Index([Bind(Include = "Status,AssignedToID,CategoryID,Search,AdvancedSearch,SortBy,DescSort,Page")] TicketsIndexViewModel model)
        {
            IQueryable<Ticket> query = context.Tickets;
            if (!await isCurrentUserAnAdminAsync())
            {
                string currentUserId = (await getCurrentUserAsync()).Id;
                query = query.Where(t => t.CreatedByID == currentUserId);
            
                ModelState.Remove("Status");
                ModelState.Remove("AssignedToID");
                ModelState.Remove("CategoryID");
            }
            else
            {
                if (model.Status != "All")
                    query = query.Where(t => t.Status == model.Status);
                if (model.AssignedToID != null)
                    query = query.Where(t => t.AssignedToID == model.AssignedToID);
                if (model.CategoryID != null)
                    query = query.Where(t => t.CategoryID == model.CategoryID);
            }

            if (!string.IsNullOrWhiteSpace(model.Search))
            {
                query = query.Where(t => t.Title.ToLower().Contains(model.Search.ToLower()));
                if (model.AdvancedSearch)
                    query = query.Where(t => t.Content.ToLower().Contains(model.Search.ToLower()) ||
                                             t.Solution.ToLower().Contains(model.Search.ToLower()));                    
            }
            
            switch (model.SortBy)
            {
                case "CreatedOn":
                    query = model.DescSort ? query.OrderByDescending(t => t.CreatedOn) : query.OrderBy(t => t.CreatedOn);
                    break;
                case "RequestedBy":
                    query = model.DescSort ? query.OrderByDescending(t => t.RequestedBy.UserName) : query.OrderBy(t => t.RequestedBy.UserName);
                    break;
                case "Title":
                    query = model.DescSort ? query.OrderByDescending(t => t.Title) : query.OrderBy(t => t.Title);
                    break;
                case "Category":
                    query = model.DescSort ? query.OrderByDescending(t => t.Category.Name) : query.OrderBy(t => t.Category.Name);
                    break;
                case "Status":
                    query = model.DescSort ? query.OrderByDescending(t => t.Status) : query.OrderBy(t => t.Status);
                    break;
                case "AssignedTo":
                    query = model.DescSort ? query.OrderByDescending(t => t.AssignedTo.UserName) : query.OrderBy(t => t.AssignedTo.UserName);
                    break;
            }

            
            model.Tickets = query.ToPagedList(model.Page, 5);

            string adminRoleId = roleManager.Roles.Single(r => r.Name == "Admin").Id;
            model.Admins = userManager.Users.Where(u => u.Roles.FirstOrDefault().RoleId == adminRoleId);// unitOfWork.UserRepository.GetAll(u => u.Role == "Admin", orderBy: o => o.OrderBy(t => t.FirstName));
            model.Categories = context.Categories.OrderBy(c => c.Order);
            
            return View("ApiIndex"/*model*/);
        }

        [OverrideAuthorization]
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
        [OverrideAuthorization]
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
                        CategoryID = model.CategoryID,
                        Title = model.Title,
                        Content = model.Content
                    };
                    context.Tickets.Add(ticket);
                    await context.SaveChangesAsync();
                    //unitOfWork.TicketRepository.Insert(ticket);
                    //unitOfWork.Save();
                    TempData["Success"] = "Successfully added new ticket!";
                    AppUser user = await userManager.FindByEmailAsync(User.Identity.Name);
                    //if (await userManager.IsInRoleAsync(user.Id, "Admin"))
                        return RedirectToAction("Index");
                    //else
                    //    return RedirectToAction("IndexOwn");
                }
            }
            catch
            {
                ModelState.AddModelError("", "Cannot create ticket. Try again!");
            }

            model.RequestedBy = await userManager.FindByIdAsync(model.RequestedByID);
            //model.RequestedBy = unitOfWork.UserRepository.GetById(model.RequestedByID ?? 0);
            model.Categories = unitOfWork.CategoryRepository.Get(filters: null, orderBy: c => c.OrderBy(o => o.Order));

            return View(model);
        }

        [OverrideAuthorization]
        public async Task<ActionResult> Edit(int? id)
        {
            Ticket ticket = await context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            if (!await isCurrentUserAnAdminAsync() && ticket.CreatedByID != (await getCurrentUserAsync()).Id)
            {
                TempData["Fail"] = "You can't modify ticket which you didn't create!";
                return RedirectToAction("Index", "Home");
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
            model.Categories = unitOfWork.CategoryRepository.Get(filters: null, orderBy: c => c.OrderBy(o => o.Order));
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [OverrideAuthorization]
        public async Task<ActionResult> Edit([Bind(Include = "TicketID,RequestedByID,AssignedToID,Status,CategoryID,Title,Content,Solution")] TicketsEditViewModel model)
        {
            Ticket ticket = await context.Tickets.FindAsync(model.TicketID);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            if (!await isCurrentUserAnAdminAsync())
            {
                if (ticket.CreatedByID != (await getCurrentUserAsync()).Id)
                {
                    TempData["Fail"] = "You can't modify ticket which you didn't create!";
                    return RedirectToAction("Index", "Home");
                }

                ModelState.Remove("AssignedToID");
                ModelState.Remove("Status");
                ModelState.Remove("Solution");
            }

            model.RequestedBy = await userManager.FindByIdAsync(model.RequestedByID);//unitOfWork.UserRepository.GetById(model.RequestedByID ?? 0);
            model.AssignedTo = await userManager.FindByIdAsync(model.AssignedToID);//unitOfWork.UserRepository.GetById(model.AssignedToID ?? 0);
            try
            {
                if (ModelState.IsValid)
                {
                    ticket.RequestedByID = model.RequestedByID;
                    ticket.CategoryID = model.CategoryID;
                    ticket.Title = model.Title;
                    ticket.Content = model.Content;                    

                    if (await isCurrentUserAnAdminAsync())
                    {
                        ticket.Status = model.Status;
                        ticket.AssignedToID = model.AssignedToID;
                        ticket.Solution = model.Solution;
                    }

                    context.Tickets.Attach(ticket);
                    context.Entry(ticket).State = EntityState.Modified;
                    await context.SaveChangesAsync();
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
            model.Categories = unitOfWork.CategoryRepository.Get(filters: null, orderBy: c => c.OrderBy(o => o.Order));

            return View(model);
        }

        [OverrideAuthorization]
        public async Task<ActionResult> History(int id)
        {

            try
            {
                Ticket ticket = unitOfWork.TicketRepository.GetById(id);
                if (ticket == null)
                    throw new Exception($"Ticket id {id} doesn't exist");

                AppUser currentUser = await getCurrentUserAsync();
                if (!await isCurrentUserAnAdminAsync() && ticket.CreatedByID != currentUser.Id && ticket.RequestedByID != currentUser.Id)
                    return new HttpUnauthorizedResult();

                TicketsHistoryViewModel model = new TicketsHistoryViewModel
                {
                    TicketID = id.ToString(),
                    Logs = new List<Log>()
                };
                foreach (var log in context.TicketsHistory.Where(l => l.TicketId == ticket.TicketID.ToString()).OrderByDescending(l => l.ChangeDate))
                {
                    AppUser changeAuthor = unitOfWork.UserRepository.GetById(log.ChangeAuthorId);
                    string logContent = String.Format("User [{0}] with ID [{1}] ", changeAuthor != null ? changeAuthor.FirstName + " " + changeAuthor.LastName : "deleted user", log.ChangeAuthorId);
                    switch (log.ActionType)
                    {
                        case "UPDATE":
                            logContent += $"changed [{log.ColumnName}] from [{log.OldValue}] to [{log.NewValue}]";
                            break;
                        case "INSERT":
                            logContent += "created ticket";
                            break;
                        case "DELETE":
                            logContent += "deleted ticket";
                            break;
                    }
                    model.Logs.Add(new Log
                    {
                        Date = log.ChangeDate,
                        Content = logContent
                    }
                    );
                }
                return View(model);
            }
            catch
            {
                TempData["Fail"] = "Poblem with reading user history. Try again!";
                return RedirectToAction("Index", "Home");
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            Ticket ticket = await context.Tickets.FindAsync(id);
            AppUser user = await userManager.FindByEmailAsync(User.Identity.Name);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            if (!(await userManager.IsInRoleAsync(user.Id, "Admin")) && ticket.CreatedByID != user.Id)
                return RedirectToAction("IndexOwn");
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

        [OverrideAuthorization]
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

        public async Task<FileResult> DownloadTicketsAsCSV()
        {
            StringBuilder csv = new StringBuilder();
            csv.AppendLine("Created on;Created by;Requested by;Assigned to;Status;Category;Title;Content;Solution");
            foreach (Ticket ticket in await context.Tickets.OrderByDescending(t => t.CreatedOn).ToListAsync())
            {
                csv.AppendLine($"{ticket.CreatedOn};{ticket.CreatedBy?.FirstName} {ticket.CreatedBy?.LastName};{ticket.RequestedBy?.FirstName} {ticket.RequestedBy?.LastName};{ticket.AssignedTo?.FirstName} {ticket.AssignedTo?.LastName};{ticket.Status};{ticket.Category?.Name};{ticket.Title};{ticket.Content};{ticket.Solution};");
            }
            return File(Encoding.GetEncoding("ISO-8859-2").GetBytes(csv.ToString()), "text/plain", string.Format("tickets.csv"));
        }

        [HttpPost]
        public async Task<JsonResult> AssignUserToTicket(string userId, int ticketId)
        {
            try
            {
                AppUser user = await userManager.FindByIdAsync(userId);
                Ticket ticket = unitOfWork.TicketRepository.GetById(ticketId);
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
            return Json(new { });
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<RedirectResult> AssignUserToTicket(string assignUserID, int assignTicketID, string returnUrl)
        //{
        //    try
        //    {
        //        AppUser user = await userManager.FindByIdAsync(assignUserID);//unitOfWork.UserRepository.GetById(assignUserID);
        //        Ticket ticket = unitOfWork.TicketRepository.GetById(assignTicketID);
        //        ticket.AssignedToID = user.Id;
        //        ticket.Status = "In progress";
        //        unitOfWork.TicketRepository.Update(ticket);
        //        unitOfWork.Save();
        //        TempData["Success"] = "Successfully assigned user to ticket!";
        //    }
        //    catch
        //    {
        //        TempData["Fail"] = "Cannot assign user to ticket. Try again!";
        //    }
        //    return Redirect(returnUrl);
        //}

        [HttpPost]
        public async Task<JsonResult> SolveTicket(string userId, int ticketId, string solution)
        {
            try
            {
                AppUser user = await userManager.FindByIdAsync(userId);//unitOfWork.UserRepository.GetById(solveUserID);
                Ticket ticket = unitOfWork.TicketRepository.GetById(ticketId);
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
            return Json(new { });
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<RedirectResult> SolveTicket(string solveUserID, int solveTicketID, string solution, string returnUrl)
        //{
        //    try
        //    { 
        //        AppUser user = await userManager.FindByIdAsync(solveUserID);//unitOfWork.UserRepository.GetById(solveUserID);
        //        Ticket ticket = unitOfWork.TicketRepository.GetById(solveTicketID);
        //        ticket.AssignedToID = user.Id;
        //        ticket.Status = "Solved";
        //        ticket.Solution = solution;
        //        unitOfWork.TicketRepository.Update(ticket);
        //        unitOfWork.Save();
        //        TempData["Success"] = "Successfully solved ticket!";
        //    }
        //    catch
        //    {
        //        TempData["Fail"] = "Cannot solve ticket. Try again!";
        //    }
        //    return Redirect(returnUrl);
        //}

        [HttpPost]
        public async Task<JsonResult> CloseTicket(int ticketId)
        {
            try
            {
                AppUser user = await userManager.FindByEmailAsync(User.Identity.Name);
                Ticket ticket = unitOfWork.TicketRepository.GetById(ticketId);
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
            return Json(new { });
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<RedirectResult> CloseTicket(int closeTicketID, string returnUrl)
        //{
        //    try
        //    { 
        //        AppUser user = await userManager.FindByEmailAsync(User.Identity.Name);
        //        Ticket ticket = unitOfWork.TicketRepository.GetById(closeTicketID);
        //        ticket.AssignedToID = user.Id;
        //        ticket.Status = "Closed";
        //        unitOfWork.TicketRepository.Update(ticket);
        //        unitOfWork.Save();
        //        TempData["Success"] = "Successfully closed ticket!";
        //    }
        //    catch
        //    {
        //        TempData["Fail"] = "Cannot close ticket. Try again!";
        //    }
        //    return Redirect(returnUrl);
        //}

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
