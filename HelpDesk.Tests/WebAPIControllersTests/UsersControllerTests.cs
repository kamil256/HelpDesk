using HelpDesk.DAL.Abstract;
using HelpDesk.DAL.Concrete;
using HelpDesk.DAL.Entities;
using HelpDesk.UI.Controllers.WebAPI;
using HelpDesk.UI.Infrastructure.Abstract;
using HelpDesk.UI.ViewModels.Users;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

namespace HelpDesk.Tests.WebAPIControllersTests
{
    [TestFixture]
    class UsersControllerTests
    {
        private User[] usersFactory()
        {
            return new User[]
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
                      Active = true
                }
            };
        }

        [Test]
        public void GetUsers_ByDefault_ReturnsAllUsers()
        {
            IRepository<User, string> userRepository = Substitute.For<IRepository<User, string>>();
            List<Expression<Func<User, bool>>> filters = new List<Expression<Func<User, bool>>>();
            Func<IQueryable<User>, IOrderedQueryable<User>> orderBy = query => query.OrderBy(u => u.LastName);
            userRepository.Get(filters, orderBy, 0, 0, "").Returns(usersFactory());
            IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.UserRepository.Returns(userRepository);
            IIdentityHelper identityHelper = Substitute.For<IIdentityHelper>();
            UsersController usersController = new UsersController(unitOfWork, identityHelper);

            IHttpActionResult actionResult = usersController.GetUsers(sortBy: "Last name");
            OkNegotiatedContentResult<UserResponse> resultContent = actionResult as OkNegotiatedContentResult<UserResponse>;
            //var res = userRepository.Get(filters, orderBy, 0, 0, "");

            //Assert.AreEqual(1, resultContent.Content.Users.Count());
            userRepository.Received().Get(Arg.Is<List<Expression<Func<User, bool>>>>(x => filters.SequenceEqual(x)), Arg.Is(orderBy), 0, 0, "");
        }
    }
}
