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
using HelpDesk.UI.Infrastructure.Abstract;

namespace HelpDesk.UI.Controllers.WebAPI
{
    [Authorize(Roles = "Admin")]
    public class TicketsController : ApiController
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IIdentityHelper identityHelper;

        public TicketsController(IUnitOfWork unitOfWork, IIdentityHelper identityHelper)
        {
            this.unitOfWork = unitOfWork;
            this.identityHelper = identityHelper;
        }

        [HttpGet]
        [OverrideAuthorization]
        [Authorize]
        public HttpResponseMessage GetTickets(string userId = null, string status = null, string assignedToId = null, int? categoryId = null, string search = null, bool searchAllWords = false, bool advancedSearch = false, string sortBy = "Date", bool descSort = true, int page = 0)
        {
            List<Expression<Func<Ticket, bool>>> filters = new List<Expression<Func<Ticket, bool>>>();
            if (!identityHelper.IsCurrentUserAnAdministrator())
                filters.Add(ticket => ticket.CreatorId == identityHelper.CurrentUser.Id || ticket.RequesterId == identityHelper.CurrentUser.Id);
            else
            {
                if (userId != null)
                    filters.Add(ticket => ticket.CreatorId == userId || ticket.RequesterId == userId);
                if (status != null && new[] { "New", "In progress", "Solved", "Closed" }.Contains(status))
                    filters.Add(ticket => ticket.Status == status);
                if (assignedToId != null && (assignedToId == "0" || identityHelper.UserManager.FindById(assignedToId) != null))
                    filters.Add(ticket => ticket.AssignedUserId == (assignedToId == "0" ? null : assignedToId));
                if (categoryId != null && (categoryId == 0 || unitOfWork.CategoryRepository.GetById(categoryId ?? 0) != null))
                    filters.Add(ticket => ticket.CategoryId == (categoryId == 0 ? null : categoryId));
            }

            search = search.RemoveExcessSpaces();
            if (!string.IsNullOrEmpty(search))
            {
                string[] words = search.Split(' ');
                if (!advancedSearch)
                    if (searchAllWords)
                        filters.Add(t => words.All(w => t.Title.ToLower().Contains(w.ToLower()) ||
                                                        (t.CreateDate.Year.ToString() + "-" + (t.CreateDate.Month < 10 ? "0" + t.CreateDate.Month.ToString() : t.CreateDate.Month.ToString()) + "-" + (t.CreateDate.Day < 10 ? "0" + t.CreateDate.Day.ToString() : t.CreateDate.Day.ToString()) + " " + (t.CreateDate.Hour < 10 ? "0" + t.CreateDate.Hour.ToString() : t.CreateDate.Hour.ToString()) + ":" + (t.CreateDate.Minute < 10 ? "0" + t.CreateDate.Minute.ToString() : t.CreateDate.Minute.ToString()) + ":" + (t.CreateDate.Second < 10 ? "0" + t.CreateDate.Second.ToString() : t.CreateDate.Second.ToString())).Contains(w) ||
                                                        (t.Requester.FirstName + " " + t.Requester.LastName).ToLower().Contains(w.ToLower())));
                    else
                        filters.Add(t => words.Any(w => t.Title.ToLower().Contains(w.ToLower()) ||
                                                        (t.CreateDate.Year.ToString() + "-" + (t.CreateDate.Month < 10 ? "0" + t.CreateDate.Month.ToString() : t.CreateDate.Month.ToString()) + "-" + (t.CreateDate.Day < 10 ? "0" + t.CreateDate.Day.ToString() : t.CreateDate.Day.ToString()) + " " + (t.CreateDate.Hour < 10 ? "0" + t.CreateDate.Hour.ToString() : t.CreateDate.Hour.ToString()) + ":" + (t.CreateDate.Minute < 10 ? "0" + t.CreateDate.Minute.ToString() : t.CreateDate.Minute.ToString()) + ":" + (t.CreateDate.Second < 10 ? "0" + t.CreateDate.Second.ToString() : t.CreateDate.Second.ToString())).Contains(w) ||
                                                        (t.Requester.FirstName + " " + t.Requester.LastName).ToLower().Contains(w.ToLower())));
                else
                    if (searchAllWords)
                        filters.Add(t => words.All(w => t.Title.ToLower().Contains(w.ToLower()) ||
                                                        (t.CreateDate.Year.ToString() + "-" + (t.CreateDate.Month < 10 ? "0" + t.CreateDate.Month.ToString() : t.CreateDate.Month.ToString()) + "-" + (t.CreateDate.Day < 10 ? "0" + t.CreateDate.Day.ToString() : t.CreateDate.Day.ToString()) + " " + (t.CreateDate.Hour < 10 ? "0" + t.CreateDate.Hour.ToString() : t.CreateDate.Hour.ToString()) + ":" + (t.CreateDate.Minute < 10 ? "0" + t.CreateDate.Minute.ToString() : t.CreateDate.Minute.ToString()) + ":" + (t.CreateDate.Second < 10 ? "0" + t.CreateDate.Second.ToString() : t.CreateDate.Second.ToString())).Contains(w) ||
                                                        (t.Requester.FirstName + " " + t.Requester.LastName).ToLower().Contains(w.ToLower()) ||
                                                        t.Content.ToLower().Contains(w.ToLower()) || 
                                                        t.Solution.ToLower().Contains(w.ToLower())));
                    else
                        filters.Add(t => words.Any(w => t.Title.ToLower().Contains(w.ToLower()) ||
                                                        (t.CreateDate.Year.ToString() + "-" + (t.CreateDate.Month < 10 ? "0" + t.CreateDate.Month.ToString() : t.CreateDate.Month.ToString()) + "-" + (t.CreateDate.Day < 10 ? "0" + t.CreateDate.Day.ToString() : t.CreateDate.Day.ToString()) + " " + (t.CreateDate.Hour < 10 ? "0" + t.CreateDate.Hour.ToString() : t.CreateDate.Hour.ToString()) + ":" + (t.CreateDate.Minute < 10 ? "0" + t.CreateDate.Minute.ToString() : t.CreateDate.Minute.ToString()) + ":" + (t.CreateDate.Second < 10 ? "0" + t.CreateDate.Second.ToString() : t.CreateDate.Second.ToString())).Contains(w) ||
                                                        (t.Requester.FirstName + " " + t.Requester.LastName).ToLower().Contains(w.ToLower()) ||
                                                        t.Content.ToLower().Contains(w.ToLower()) || 
                                                        t.Solution.ToLower().Contains(w.ToLower())));
            }

            Func<IQueryable<Ticket>, IOrderedQueryable<Ticket>> orderBy = null;

            switch (sortBy)
            {
                case "User":
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
                case "Date":
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
                    CreateDate = t.CreateDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    CreatorName = t.Creator != null ? t.Creator.FirstName + " " + t.Creator.LastName : null,
                    RequesterName = t.Requester != null ? t.Requester.FirstName + " " + t.Requester.LastName : null,
                    AssignedUserName = t.AssignedUser != null ? t.AssignedUser.FirstName + " " + t.AssignedUser.LastName : null,
                    CreatorId = t.CreatorId,
                    RequesterId = t.RequesterId,
                    AssignedUserId = t.AssignedUserId,
                    Title = t.Title,
                    Category = t.Category?.Name,
                    Status = t.Status,
                    Solution = t.Solution
                }),
                NumberOfPages = numberOfPages,
                FoundItemsCount = numberOfTickets,
                TotalItemsCount = unitOfWork.TicketRepository.Count()
            });
        }
    }
}
