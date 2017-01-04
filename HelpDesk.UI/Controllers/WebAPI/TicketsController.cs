using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web;
using HelpDesk.DAL.Concrete;
using HelpDesk.DAL.Entities;
using HelpDesk.DAL.Abstract;
using HelpDesk.UI.ViewModels.Tickets;
using HelpDesk.UI.Infrastructure;

namespace HelpDesk.UI.Controllers.WebAPI
{
    [Authorize(Roles = "Admin")]
    public class TicketsController : ApiController
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IdentityHelper identityHelper;

        public TicketsController()
        {
            this.unitOfWork = new UnitOfWork();
            identityHelper = new IdentityHelper();
        }
                
        [HttpGet]
        [OverrideAuthorization]
        [Authorize]
        public HttpResponseMessage GetTickets(string userId = null, string status = null, string assignedToId = null, int? categoryId = null, string search = null, bool advancedSearch = false, string sortBy = null, bool descSort = false, int page = 0)
        {
            List<Expression<Func<Ticket, bool>>> filters = new List<Expression<Func<Ticket, bool>>>();
            if (!identityHelper.IsCurrentUserAnAdministrator())
                filters.Add(ticket => ticket.CreatorId == identityHelper.CurrentUser.Id || ticket.RequesterId == identityHelper.CurrentUser.Id);
            else
            {
                if (userId != null)
                    filters.Add(ticket => ticket.CreatorId == userId || ticket.RequesterId == userId);
                if (status != null)
                    filters.Add(ticket => ticket.Status == status);
                if (assignedToId != null)
                    filters.Add(ticket => ticket.AssignedUserId == assignedToId);
                if (categoryId != null)
                    filters.Add(ticket => ticket.CategoryId == categoryId);
            }

            if (!string.IsNullOrEmpty(search))
            {
                if (!advancedSearch)
                    filters.Add(ticket => ticket.Title.ToLower().Contains(search.ToLower()));
                else
                    filters.Add(ticket => ticket.Title.ToLower().Contains(search.ToLower()) ||
                                          ticket.Content.ToLower().Contains(search.ToLower()) ||
                                          ticket.Solution.ToLower().Contains(search.ToLower()));
            }

            Func<IQueryable<Ticket>, IOrderedQueryable<Ticket>> orderBy = null;

            switch (sortBy)
            {
                case "Requested by":
                    if (descSort)
                        orderBy = query => query.OrderByDescending(t => t.Requester.FirstName + t.Requester.LastName);
                    else
                        orderBy = query => query.OrderBy(t => t.Requester.FirstName + t.Requester.LastName);
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
                case "Created on":
                default:
                    if (descSort)
                        orderBy = query => query.OrderByDescending(t => t.CreateDate);
                    else
                        orderBy = query => query.OrderBy(t => t.CreateDate);
                    break;
            }

            int ticketsPerPage = identityHelper.CurrentUser.Settings.TicketsPerPage;
            int numberOfTickets = unitOfWork.TicketRepository.Get(filters: filters).Count();
            int numberOfPages;

            IEnumerable<Ticket> tickets;
            if (page != 0)
            {
                numberOfPages = (int)Math.Ceiling((decimal)numberOfTickets / ticketsPerPage);
                tickets = unitOfWork.TicketRepository.Get(filters: filters, orderBy: orderBy, skip: (page - 1) * ticketsPerPage, take: ticketsPerPage);                
            }
            else
            {
                numberOfPages = 1;
                tickets = unitOfWork.TicketRepository.Get(filters: filters, orderBy: orderBy);                
            }
            
            return Request.CreateResponse(HttpStatusCode.OK, new 
            {
                Tickets = tickets.Select(t => new TicketDTO
                {
                    TicketId = t.TicketId,
                    CreateDate = ((t.CreateDate - new DateTime(1970, 1, 1)).Ticks / 10000).ToString(),
                    CreatorName = t.Creator != null ? t.Creator.FirstName + " " + t.Creator.LastName : null,
                    RequesterName = t.Requester != null ? t.Requester.FirstName + " " + t.Requester.LastName : null,
                    AssignedUserName = t.AssignedUser != null ? t.AssignedUser.FirstName + " " + t.AssignedUser.LastName : null,
                    CreatorId = t.CreatorId,
                    RequesterId = t.RequesterId,
                    AssignedUserId = t.AssignedUserId,
                    Title = t.Title,
                    Category = t.Category?.Name,
                    Status = t.Status
                }),
                NumberOfPages = numberOfPages,
                FoundItemsCount = numberOfTickets,
                TotalItemsCount = unitOfWork.TicketRepository.Count()
            });
        }
    }
}
