using HelpDesk.DAL;
using HelpDesk.Entities;
using HelpDesk.Models.Tickets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace HelpDesk.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ApiTicketsController : ApiController
    {
        private IUnitOfWork unitOfWork;
        private HelpDeskContext context;

        public ApiTicketsController()
        {
            unitOfWork = new UnitOfWork();
            context = new HelpDeskContext();
        }

        [OverrideAuthorization]
        [HttpGet]
        public IEnumerable<HelpDesk.Models.Tickets.Ticket> GetTickets(string status, string sortBy, bool descSort)
        {
            List<Expression<Func<Entities.Ticket, bool>>> filters = new List<Expression<Func<Entities.Ticket, bool>>>();

            //IQueryable<Ticket> query = context.Tickets; 
            //if (!await isCurrentUserAnAdminAsync())
            //{
            //    string currentUserId = (await getCurrentUserAsync()).Id;
            //    query = query.Where(t => t.CreatedByID == currentUserId);

            //    ModelState.Remove("Status");
            //    ModelState.Remove("AssignedToID");
            //    ModelState.Remove("CategoryID");
            //}
            //else
            //{
            if (status != "All")
                filters.Add(t => t.Status == status);
            //    if (model.AssignedToID != null)
            //        query = query.Where(t => t.AssignedToID == model.AssignedToID);
            //    if (model.CategoryID != null)
            //        query = query.Where(t => t.CategoryID == model.CategoryID);
            //}

            //if (!string.IsNullOrWhiteSpace(model.Search))
            //{
            //    query = query.Where(t => t.Title.ToLower().Contains(model.Search.ToLower()));
            //    if (model.AdvancedSearch)
            //        query = query.Where(t => t.Content.ToLower().Contains(model.Search.ToLower()) ||
            //                                 t.Solution.ToLower().Contains(model.Search.ToLower()));
            //}




            Func<IQueryable<Entities.Ticket>, IOrderedQueryable<Entities.Ticket>> orderBy = null;
            
            switch (sortBy)
            {
                case "Created on":
                    if (descSort)
                        orderBy = query => query.OrderByDescending(t => t.CreatedOn);
                    else
                        orderBy = query => query.OrderBy(t => t.CreatedOn);
                    break;
                case "Requested by":
                    if (descSort)
                        orderBy = query => query.OrderByDescending(t => t.RequestedBy.FirstName + t.RequestedBy.LastName);
                    else
                        orderBy = query => query.OrderBy(t => t.RequestedBy.FirstName + t.RequestedBy.LastName);
                    break;
                case "Title":
                    if (descSort)
                        orderBy = query => query.OrderByDescending(t => t.Title);
                    else
                        orderBy = query => query.OrderBy(t => t.Title);
                    break;
                case "Category":
                    if (descSort)
                        orderBy = query => query.OrderByDescending(t => t.Category.Name);
                    else
                        orderBy = query => query.OrderBy(t => t.Category.Name);
                    break;
                case "Status":
                    if (descSort)
                        orderBy = query => query.OrderByDescending(t => t.Status);
                    else
                        orderBy = query => query.OrderBy(t => t.Status);
                    break;
            }

            var tickets = unitOfWork.TicketRepository.GetAll(filters: filters, orderBy: orderBy);
            List<HelpDesk.Models.Tickets.Ticket> model = new List<HelpDesk.Models.Tickets.Ticket>();
            foreach (var ticket in tickets)
            {
                model.Add(new Models.Tickets.Ticket
                {
                    CreatedOn = ((ticket.CreatedOn - new DateTime(1970, 1, 1)).Ticks / 10000).ToString(),
                    RequestedBy = ticket.RequestedBy?.FirstName + " " + ticket.RequestedBy?.LastName,
                    Title = ticket.Title,
                    Category = ticket.Category?.Name,
                    Status = ticket.Status
                });
            }
            return model;
            

            


            //model.Tickets = query.ToPagedList(model.Page, 5);

            //string adminRoleId = roleManager.Roles.Single(r => r.Name == "Admin").Id;
            //model.Admins = userManager.Users.Where(u => u.Roles.FirstOrDefault().RoleId == adminRoleId);// unitOfWork.UserRepository.GetAll(u => u.Role == "Admin", orderBy: o => o.OrderBy(t => t.FirstName));
            //model.Categories = context.Categories.OrderBy(c => c.Order);

            //return View(model);
        }
    }
}
