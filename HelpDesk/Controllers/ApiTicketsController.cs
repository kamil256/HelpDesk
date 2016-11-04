using HelpDesk.DAL;
using HelpDesk.Entities;
using HelpDesk.Models.Tickets;
using Microsoft.AspNet.Identity.Owin;
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
        public async Task<PagedTickets> GetTickets([FromUri] TicketFilteringModel model)
        {
            List<Expression<Func<Entities.Ticket, bool>>> filters = new List<Expression<Func<Entities.Ticket, bool>>>();

            if (!await isCurrentUserAnAdminAsync())
            {
                string currentUserId = (await getCurrentUserAsync()).Id;
                filters.Add(ticket => ticket.CreatedByID == currentUserId);

                ModelState.Remove("Status");
                ModelState.Remove("AssignedToID");
                ModelState.Remove("CategoryID");
            }
            else
            {
                if (model.Status != "All")
                    filters.Add(ticket => ticket.Status == model.Status);
                if (model.AssignedToID != null)
                    filters.Add(ticket => ticket.AssignedToID == model.AssignedToID);
                if (model.CategoryID != null)
                    filters.Add(ticket => ticket.CategoryID == model.CategoryID);
            }
            
            if (!string.IsNullOrWhiteSpace(model.Search))
            {
                if (!model.AdvancedSearch)
                    filters.Add(ticket => ticket.Title.ToLower().Contains(model.Search.ToLower()));
                else
                    filters.Add(ticket => ticket.Title.ToLower().Contains(model.Search.ToLower()) ||
                                          ticket.Content.ToLower().Contains(model.Search.ToLower()) ||
                                          ticket.Solution.ToLower().Contains(model.Search.ToLower()));
            }

            Func<IQueryable<Entities.Ticket>, IOrderedQueryable<Entities.Ticket>> orderBy = null;
            
            switch (model.SortBy)
            {
                case "Created on":
                    if (model.DescSort)
                        orderBy = query => query.OrderByDescending(t => t.CreatedOn);
                    else
                        orderBy = query => query.OrderBy(t => t.CreatedOn);
                    break;
                case "Requested by":
                    if (model.DescSort)
                        orderBy = query => query.OrderByDescending(t => t.RequestedBy.FirstName + t.RequestedBy.LastName);
                    else
                        orderBy = query => query.OrderBy(t => t.RequestedBy.FirstName + t.RequestedBy.LastName);
                    break;
                case "Title":
                    if (model.DescSort)
                        orderBy = query => query.OrderByDescending(t => t.Title);
                    else
                        orderBy = query => query.OrderBy(t => t.Title);
                    break;
                case "Category":
                    if (model.DescSort)
                        orderBy = query => query.OrderByDescending(t => t.Category.Name);
                    else
                        orderBy = query => query.OrderBy(t => t.Category.Name);
                    break;
                case "Status":
                    if (model.DescSort)
                        orderBy = query => query.OrderByDescending(t => t.Status);
                    else
                        orderBy = query => query.OrderBy(t => t.Status);
                    break;
            }

            int ticketsPerPage = 2;
            int numberOfTickets = unitOfWork.TicketRepository.GetAll(filters, orderBy).Count();
            int numberOfPages = (int)Math.Ceiling((decimal)numberOfTickets / ticketsPerPage);

            PagedTickets pagedTickets = new PagedTickets();
            pagedTickets.Tickets = unitOfWork.TicketRepository.GetAll(filters, orderBy, skip: (model.Page - 1) * ticketsPerPage, take: ticketsPerPage).Select(t => new TicketDTO
            {
                TicketId = t.TicketID,
                CreatedOn = ((t.CreatedOn - new DateTime(1970, 1, 1)).Ticks / 10000).ToString(),
                RequestedBy = t.RequestedBy?.FirstName + " " + t.RequestedBy?.LastName,
                Title = t.Title,
                Category = t.Category?.Name,
                Status = t.Status
            });

            pagedTickets.NumberOfPages = numberOfPages;
            return pagedTickets;
        }

        //public void PutTicket(Ticket model)
        //{
        //    if (ModelState.IsValid)
        //        unitOfWork.TicketRepository.Update()
        //}

        private AppUserManager userManager
        {
            get
            {
                return Request.GetOwinContext().GetUserManager<AppUserManager>();
            }
        }

        private AppRoleManager roleManager
        {
            get
            {
                return Request.GetOwinContext().GetUserManager<AppRoleManager>();
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
