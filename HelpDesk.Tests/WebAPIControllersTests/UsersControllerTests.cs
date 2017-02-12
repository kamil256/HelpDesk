using HelpDesk.BLL;
using HelpDesk.BLL.Abstract;
using HelpDesk.DAL.Abstract;
using HelpDesk.DAL.Concrete;
using HelpDesk.DAL.Entities;
using HelpDesk.UI.Controllers.WebAPI;
using HelpDesk.UI.Infrastructure.Abstract;
using HelpDesk.UI.ViewModels.Users;
using Microsoft.AspNet.Identity.EntityFramework;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;

namespace HelpDesk.Tests.WebAPIControllersTests
{
    [TestFixture]
    class UsersControllerTests
    {
        private Role[] roles = new Role[]
        {
            new Role
            {
                Id = "1",
                Name = "Admin"
            },
            new Role
            {
                Id = "2",
                Name = "User"
            }
        };

        private Ticket[] tickets = new Ticket[]
        {
            new Ticket
            {
                TicketId = 1
            },
            new Ticket
            {
                TicketId = 2
            },
            new Ticket
            {
                TicketId = 3
            },
            new Ticket
            {
                TicketId = 4
            },
            new Ticket
            {
                TicketId = 5
            }
        };

        private User[] users
        {
            get
            { 
                User[] users  = new User[]
                {
                    new User
                    {
                            Id = "1",
                            FirstName = "FirstName1",
                            LastName = "LastName1",
                            Email = "email1@example.com",
                            Phone = "Phone1",
                            MobilePhone = "MobilePhone1",
                            Company = "Company1",
                            Department = "Department1",
                            Active = true,
                            LastActivity = new DateTime(2017, 02, 12, 12, 23, 34)
                    },
                    new User
                    {
                            Id = "2",
                            FirstName = "FirstName2",
                            LastName = "LastName2",
                            Email = "email2@example.com",
                            Phone = "Phone2",
                            MobilePhone = "MobilePhone2",
                            Company = "Company2",
                            Department = "Department2",
                            Active = true,
                            LastActivity = new DateTime(2017, 02, 11, 16, 01, 17)
                    },
                    new User
                    {
                            Id = "3",
                            FirstName = "FirstName3",
                            LastName = "LastName3",
                            Email = "email3@example.com",
                            Phone = "Phone3",
                            MobilePhone = "MobilePhone3",
                            Company = "Company3",
                            Department = "Department3",
                            Active = false,
                            LastActivity = null
                    }
                };

                users[0].Roles.Add(new IdentityUserRole()
                {
                    RoleId = roles.First(r => r.Name == "Admin").Id
                });

                users[0].CreatedTickets = new Ticket[] { tickets[0], tickets[1], tickets[2] };
                users[0].RequestedTickets = new Ticket[] { tickets[2], tickets[3], tickets[4] };

                users[1].Roles.Add(new IdentityUserRole()
                {
                    RoleId = roles.First(r => r.Name == "User").Id
                });

                users[1].CreatedTickets = new Ticket[] { tickets[0] };
                users[1].RequestedTickets = new Ticket[] {  };

                users[2].Roles.Add(new IdentityUserRole()
                {
                    RoleId = roles.First(r => r.Name == "User").Id
                });

                users[2].CreatedTickets = new Ticket[] {  };
                users[2].RequestedTickets = new Ticket[] { tickets[2], tickets[4] };

                return users;
            }
        }

        [Test]
        public void GetUsers_ByDefault_CallsProperMethodAndReturnsAllUsers()
        {
            // Arrange
            string loggedInUserId = "loggedInUserId";
            IUserService userService = Substitute.For<IUserService>();
            userService.GetPagedUsersList(Arg.Is<string>(loggedInUserId), Arg.Is<bool?>((bool?)null), Arg.Is<string>((string)null), Arg.Is<string>((string)null), Arg.Is<bool>(false), Arg.Is<string>("Last name"), Arg.Is<bool>(false), Arg.Is<int?>(0), Arg.Is<int?>((int?)null)).Returns(new PagedUsersList()
            {
                Users = users,
                NumberOfPages = 1,
                FoundItemsCount = 3,
                TotalItemsCount = 3
            });
            IRoleService roleService = Substitute.For<IRoleService>();
            roleService.AdminRoleId.Returns("1");
            IIdentityHelper identityHelper = Substitute.For<IIdentityHelper>();
            identityHelper.CurrentUserId.Returns(loggedInUserId);
            UsersController usersController = new UsersController(userService, roleService, identityHelper);

            // Act
            IHttpActionResult actionResult = usersController.GetUsers();
            OkNegotiatedContentResult<UserResponse> result = actionResult as OkNegotiatedContentResult<UserResponse>;
            UserResponse resultContent = result.Content;

            userService.Received().GetPagedUsersList(loggedInUserId, null, null, null, false, "Last name", false, 0, null);
            Assert.IsNotNull(resultContent);
            Assert.IsNotNull(resultContent.Users);
            Assert.AreEqual(3, resultContent.Users.Count());

            Assert.AreEqual("1", resultContent.Users.ElementAt(0).UserId);
            Assert.AreEqual("FirstName1", resultContent.Users.ElementAt(0).FirstName);
            Assert.AreEqual("LastName1", resultContent.Users.ElementAt(0).LastName);
            Assert.AreEqual("email1@example.com", resultContent.Users.ElementAt(0).Email);
            Assert.AreEqual("Phone1", resultContent.Users.ElementAt(0).Phone);
            Assert.AreEqual("MobilePhone1", resultContent.Users.ElementAt(0).MobilePhone);
            Assert.AreEqual("Company1", resultContent.Users.ElementAt(0).Company);
            Assert.AreEqual("Department1", resultContent.Users.ElementAt(0).Department);
            Assert.AreEqual("Admin", resultContent.Users.ElementAt(0).Role);
            Assert.AreEqual(true, resultContent.Users.ElementAt(0).Active);
            Assert.AreEqual("2017-02-12 12:23", resultContent.Users.ElementAt(0).LastActivity);
            Assert.AreEqual(5, resultContent.Users.ElementAt(0).TicketsCount);

            Assert.AreEqual("2", resultContent.Users.ElementAt(1).UserId);
            Assert.AreEqual("FirstName2", resultContent.Users.ElementAt(1).FirstName);
            Assert.AreEqual("LastName2", resultContent.Users.ElementAt(1).LastName);
            Assert.AreEqual("email2@example.com", resultContent.Users.ElementAt(1).Email);
            Assert.AreEqual("Phone2", resultContent.Users.ElementAt(1).Phone);
            Assert.AreEqual("MobilePhone2", resultContent.Users.ElementAt(1).MobilePhone);
            Assert.AreEqual("Company2", resultContent.Users.ElementAt(1).Company);
            Assert.AreEqual("Department2", resultContent.Users.ElementAt(1).Department);
            Assert.AreEqual("User", resultContent.Users.ElementAt(1).Role);
            Assert.AreEqual(true, resultContent.Users.ElementAt(1).Active);
            Assert.AreEqual("2017-02-11 16:01", resultContent.Users.ElementAt(1).LastActivity);
            Assert.AreEqual(1, resultContent.Users.ElementAt(1).TicketsCount);

            Assert.AreEqual("3", resultContent.Users.ElementAt(2).UserId);
            Assert.AreEqual("FirstName3", resultContent.Users.ElementAt(2).FirstName);
            Assert.AreEqual("LastName3", resultContent.Users.ElementAt(2).LastName);
            Assert.AreEqual("email3@example.com", resultContent.Users.ElementAt(2).Email);
            Assert.AreEqual("Phone3", resultContent.Users.ElementAt(2).Phone);
            Assert.AreEqual("MobilePhone3", resultContent.Users.ElementAt(2).MobilePhone);
            Assert.AreEqual("Company3", resultContent.Users.ElementAt(2).Company);
            Assert.AreEqual("Department3", resultContent.Users.ElementAt(2).Department);
            Assert.AreEqual("User", resultContent.Users.ElementAt(2).Role);
            Assert.AreEqual(false, resultContent.Users.ElementAt(2).Active);
            Assert.AreEqual("Never", resultContent.Users.ElementAt(2).LastActivity);
            Assert.AreEqual(2, resultContent.Users.ElementAt(2).TicketsCount);

            Assert.AreEqual(1, resultContent.NumberOfPages);
            Assert.AreEqual(3, resultContent.FoundItemsCount);
            Assert.AreEqual(3, resultContent.TotalItemsCount);
        }

        [Test]
        public void GetUsers_WhenPassingAllNotDefaultArgumentsWhichDontFitToAnyUser_CallsProperMethodAndReturnsNoUsers()
        {
            // Arrange
            string loggedInUserId = "loggedInUserId";
            IUserService userService = Substitute.For<IUserService>();
            userService.GetPagedUsersList(Arg.Is<string>(loggedInUserId), Arg.Is<bool?>(false), Arg.Is<string>("User"), Arg.Is<string>("nonexisting string"), Arg.Is<bool>(true), Arg.Is<string>("Tickets"), Arg.Is<bool>(true), Arg.Is<int?>(0), Arg.Is<int?>(10)).Returns(new PagedUsersList()
            {
                Users = new User[] { },
                NumberOfPages = 1,
                FoundItemsCount = 0,
                TotalItemsCount = 3
            });
            IRoleService roleService = Substitute.For<IRoleService>();
            roleService.AdminRoleId.Returns("1");
            IIdentityHelper identityHelper = Substitute.For<IIdentityHelper>();
            identityHelper.CurrentUserId.Returns(loggedInUserId);
            UsersController usersController = new UsersController(userService, roleService, identityHelper);

            // Act
            IHttpActionResult actionResult = usersController.GetUsers(false, "User", "nonexisting string", true, "Tickets", true, 0, 10);
            OkNegotiatedContentResult<UserResponse> result = actionResult as OkNegotiatedContentResult<UserResponse>;
            UserResponse resultContent = result.Content;

            // Assert
            userService.Received().GetPagedUsersList(loggedInUserId, false, "User", Arg.Is<string>("nonexisting string"), true, "Tickets", true, 0, 10);
            Assert.IsNotNull(resultContent);
            Assert.IsNotNull(resultContent.Users);
            Assert.AreEqual(0, resultContent.Users.Count());

            Assert.AreEqual(1, resultContent.NumberOfPages);
            Assert.AreEqual(0, resultContent.FoundItemsCount);
            Assert.AreEqual(3, resultContent.TotalItemsCount);
        }
    }
}
