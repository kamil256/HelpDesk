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

namespace HelpDesk.UI.Controllers.WebAPI
{
    [Authorize(Roles = "Admin")]
    public class TicketsController : ApiController
    {
        private AppUserManager UserManager
        {
            get
            {
                return HttpContext.Current.Request.GetOwinContext().GetUserManager<AppUserManager>();
            }
        }

        private AppRoleManager RoleManager
        {
            get
            {
                return HttpContext.Current.Request.GetOwinContext().GetUserManager<AppRoleManager>();
            }
        }

        private AppUser CurrentUser
        {
            get
            {
                return UserManager.FindByNameAsync(User.Identity.Name).Result;
            }
        }

        private IUnitOfWork unitOfWork;

        public TicketsController()//IUnitOfWork unitOfWork)
        {
            this.unitOfWork = new UnitOfWork();
        }

        [OverrideAuthorization]
        [HttpGet]
        public PagedTickets GetTickets([FromUri] TicketFilteringModel model)
        {
            List<Expression<Func<Ticket, bool>>> filters = new List<Expression<Func<Ticket, bool>>>();
            if (!UserManager.IsInRole(CurrentUser.Id, "Admin"))
            {
                filters.Add(ticket => ticket.CreatedByID == CurrentUser.Id);

                ModelState.Remove("Status");
                ModelState.Remove("AssignedToID");
                ModelState.Remove("CategoryID");
            }
            else
            {
                if (model.UserId != null)
                    filters.Add(ticket => ticket.CreatedByID == model.UserId);
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

            Func<IQueryable<Ticket>, IOrderedQueryable<Ticket>> orderBy = null;

            switch (model.SortBy)
            {
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
                case "Created on":
                default:
                    if (model.DescSort)
                        orderBy = query => query.OrderByDescending(t => t.CreatedOn);
                    else
                        orderBy = query => query.OrderBy(t => t.CreatedOn);
                    break;
            }

            int ticketsPerPage = CurrentUser.Settings.TicketsPerPage;
            int numberOfTickets = unitOfWork.TicketRepository.Get(filters: filters, orderBy: orderBy).Count();
            int numberOfPages = (int)Math.Ceiling((decimal)numberOfTickets / ticketsPerPage);

            PagedTickets pagedTickets = new PagedTickets();
            pagedTickets.Tickets = unitOfWork.TicketRepository.Get(filters: filters, orderBy: orderBy, skip: (model.Page - 1) * ticketsPerPage, take: ticketsPerPage).Select(t => new TicketDTO
            {
                TicketId = t.TicketID,
                CreatedOn = ((t.CreatedOn - new DateTime(1970, 1, 1)).Ticks / 10000).ToString(),
                CreatedBy = t.CreatedBy != null ? t.CreatedBy.FirstName + " " + t.CreatedBy.LastName : null,
                RequestedBy = t.RequestedBy != null ? t.RequestedBy.FirstName + " " + t.RequestedBy.LastName : null,
                AssignedTo = t.AssignedTo != null ? t.AssignedTo.FirstName + " " + t.AssignedTo.LastName : null,
                CreatedById = t.CreatedByID,
                RequestedById = t.RequestedByID,
                AssignedToId = t.AssignedToID,
                Title = t.Title,
                Category = t.Category?.Name,
                Status = t.Status
            });

            pagedTickets.NumberOfPages = numberOfPages;
            return pagedTickets;
        }
    }
}
