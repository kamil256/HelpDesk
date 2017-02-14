using HelpDesk.BLL;
using HelpDesk.BLL.Concrete;
using HelpDesk.DAL.Abstract;
using HelpDesk.DAL.Entities;
using Microsoft.AspNet.Identity.EntityFramework;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Tests
{
    [TestFixture]
    public class UserServiceTests
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
                User[] users = new User[]
                {
                    new User
                    {
                            Id = "1",
                            FirstName = "Sylvester",
                            LastName = "Stallone",
                            Email = "sylvester.stallone@example.com",
                            Phone = "123-456-789",
                            MobilePhone = "4323453",
                            Company = "Microsoft",
                            Department = "Electricity",
                            Active = true,
                            LastActivity = new DateTime(2017, 02, 12, 12, 23, 34),
                            Settings = new Settings
                            {
                                UsersPerPage = 1
                            }
                    },
                    new User
                    {
                            Id = "2",
                            FirstName = "Arnold",
                            LastName = "Schwarzenegger",
                            Email = "arnold.schwarzengger@example.com",
                            Phone = "56 654 5645",
                            MobilePhone = "2342342",
                            Company = "Google",
                            Department = "Accounting",
                            Active = true,
                            LastActivity = new DateTime(2017, 02, 11, 16, 01, 17),
                            Settings = new Settings
                            {
                                UsersPerPage = 2
                            }
                    },
                    new User
                    {
                            Id = "3",
                            FirstName = "Jean Claude",
                            LastName = "Van Damme",
                            Email = "van.damme@example.com",
                            Phone = "4323453",
                            MobilePhone = "32434654",
                            Company = "Apple",
                            Department = "IT",
                            Active = false,
                            LastActivity = null,
                            Settings = new Settings
                            {
                                UsersPerPage = 3
                            }
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
                users[1].RequestedTickets = new Ticket[] { };

                users[2].Roles.Add(new IdentityUserRole()
                {
                    RoleId = roles.First(r => r.Name == "User").Id
                });

                users[2].CreatedTickets = new Ticket[] { };
                users[2].RequestedTickets = new Ticket[] { tickets[2], tickets[4] };

                return users;
            }
        }

        private IEnumerable<User> getUsers(IEnumerable<Expression<Func<User, bool>>> filters = null, Func<IQueryable<User>, IOrderedQueryable<User>> orderBy = null, int skip = 0, int take = 0, string includeProperties = "")
        {
            IQueryable<User> query = users.AsQueryable();
            if (filters != null)
                foreach (var filter in filters)
                    if (filter != null)
                        query = query.Where(filter);
            if (orderBy != null)
                query = orderBy(query);
            if (skip != 0)
                query = query.Skip(skip);
            if (take != 0)
                query = query.Take(take);
            return query;
        }

        private int usersCount(params Expression<Func<User, bool>>[] filters)
        {
            IQueryable<User> query = users.AsQueryable();
            if (filters != null)
                foreach (var filter in filters)
                    if (filter != null)
                        query = query.Where(filter);
            return query.Count();
        }

        private IEnumerable<Role> getRoles(IEnumerable<Expression<Func<Role, bool>>> filters = null, Func<IQueryable<Role>, IOrderedQueryable<Role>> orderBy = null, int skip = 0, int take = 0, string includeProperties = "")
        {
            IQueryable<Role> query = roles.AsQueryable();
            if (filters != null)
                foreach (var filter in filters)
                    if (filter != null)
                        query = query.Where(filter);
            if (orderBy != null)
                query = orderBy(query);
            if (skip != 0)
                query = query.Skip(skip);
            if (take != 0)
                query = query.Take(take);
            return query;
        }

        [Test]
        public void GetPagedUsersList_ByDefault_ReturnsAllUsers()
        {
            // Arrange
            IRepository<User, string> userRepository = Substitute.For<IRepository<User, string>>();
            userRepository.Get(Arg.Any<List<Expression<Func<User, bool>>>>(), Arg.Any<Func<IQueryable<User>, IOrderedQueryable<User>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getUsers((List<Expression<Func<User, bool>>>)x[0], (Func<IQueryable<User>, IOrderedQueryable<User>>)x[1], (int)x[2], (int)x[3], (string)x[4]));
            userRepository.GetById(Arg.Any<string>()).Returns(x => users.Where(u => u.Id == (string)x[0]).Single());
            userRepository.Count(Arg.Any<Expression<Func<User, bool>>[]>()).Returns(x => usersCount((Expression<Func<User, bool>>[])x[0]));

            IRepository<Role, string> roleRepository = Substitute.For<IRepository<Role, string>>();
            roleRepository.Get(Arg.Any<List<Expression<Func<Role, bool>>>>(), Arg.Any<Func<IQueryable<Role>, IOrderedQueryable<Role>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getRoles((List<Expression<Func<Role, bool>>>)x[0], (Func<IQueryable<Role>, IOrderedQueryable<Role>>)x[1], (int)x[2], (int)x[3], (string)x[4]));

            IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.UserRepository.Returns(userRepository);
            unitOfWork.RoleRepository.Returns(roleRepository);

            UserService userService = new UserService(unitOfWork);

            // Act
            var result = userService.GetPagedUsersList("1");

            // Assert
            Assert.AreEqual(3, result.Users.Count());
            Assert.AreEqual(1, result.NumberOfPages);
            Assert.AreEqual(3, result.FoundItemsCount);
            Assert.AreEqual(3, result.TotalItemsCount);
        }

        [Test]
        public void GetPagedUsersList_WithFalseInactiveParam_ReturnsFilteredUsers()
        {
            // Arrange
            IRepository<User, string> userRepository = Substitute.For<IRepository<User, string>>();
            userRepository.Get(Arg.Any<List<Expression<Func<User, bool>>>>(), Arg.Any<Func<IQueryable<User>, IOrderedQueryable<User>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getUsers((List<Expression<Func<User, bool>>>)x[0], (Func<IQueryable<User>, IOrderedQueryable<User>>)x[1], (int)x[2], (int)x[3], (string)x[4]));
            userRepository.GetById(Arg.Any<string>()).Returns(x => users.Where(u => u.Id == (string)x[0]).Single());
            userRepository.Count(Arg.Any<Expression<Func<User, bool>>[]>()).Returns(x => usersCount((Expression<Func<User, bool>>[])x[0]));

            IRepository<Role, string> roleRepository = Substitute.For<IRepository<Role, string>>();
            roleRepository.Get(Arg.Any<List<Expression<Func<Role, bool>>>>(), Arg.Any<Func<IQueryable<Role>, IOrderedQueryable<Role>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getRoles((List<Expression<Func<Role, bool>>>)x[0], (Func<IQueryable<Role>, IOrderedQueryable<Role>>)x[1], (int)x[2], (int)x[3], (string)x[4]));

            IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.UserRepository.Returns(userRepository);
            unitOfWork.RoleRepository.Returns(roleRepository);

            UserService userService = new UserService(unitOfWork);

            // Act
            var result = userService.GetPagedUsersList(loggedInUserId: "1", active: false);

            // Assert
            Assert.AreEqual(1, result.Users.Count());
            Assert.AreEqual("3", result.Users.ElementAt(0).Id);
            Assert.AreEqual(1, result.NumberOfPages);
            Assert.AreEqual(1, result.FoundItemsCount);
            Assert.AreEqual(3, result.TotalItemsCount);
        }

        [Test]
        public void GetPagedUsersList_WithTrueActiveParam_ReturnsActiveUsers()
        {
            // Arrange
            IRepository<User, string> userRepository = Substitute.For<IRepository<User, string>>();
            userRepository.Get(Arg.Any<List<Expression<Func<User, bool>>>>(), Arg.Any<Func<IQueryable<User>, IOrderedQueryable<User>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getUsers((List<Expression<Func<User, bool>>>)x[0], (Func<IQueryable<User>, IOrderedQueryable<User>>)x[1], (int)x[2], (int)x[3], (string)x[4]));
            userRepository.GetById(Arg.Any<string>()).Returns(x => users.Where(u => u.Id == (string)x[0]).Single());
            userRepository.Count(Arg.Any<Expression<Func<User, bool>>[]>()).Returns(x => usersCount((Expression<Func<User, bool>>[])x[0]));

            IRepository<Role, string> roleRepository = Substitute.For<IRepository<Role, string>>();
            roleRepository.Get(Arg.Any<List<Expression<Func<Role, bool>>>>(), Arg.Any<Func<IQueryable<Role>, IOrderedQueryable<Role>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getRoles((List<Expression<Func<Role, bool>>>)x[0], (Func<IQueryable<Role>, IOrderedQueryable<Role>>)x[1], (int)x[2], (int)x[3], (string)x[4]));

            IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.UserRepository.Returns(userRepository);
            unitOfWork.RoleRepository.Returns(roleRepository);

            UserService userService = new UserService(unitOfWork);

            // Act
            var result = userService.GetPagedUsersList(loggedInUserId: "1", active: true);

            // Assert
            Assert.AreEqual(2, result.Users.Count());
            Assert.AreEqual("2", result.Users.ElementAt(0).Id);
            Assert.AreEqual("1", result.Users.ElementAt(1).Id);
            Assert.AreEqual(1, result.NumberOfPages);
            Assert.AreEqual(2, result.FoundItemsCount);
            Assert.AreEqual(3, result.TotalItemsCount);
        }

        [Test]
        public void GetPagedUsersList_WithAdminRoleParam_ReturnsAdminUsers()
        {
            // Arrange
            IRepository<User, string> userRepository = Substitute.For<IRepository<User, string>>();
            userRepository.Get(Arg.Any<List<Expression<Func<User, bool>>>>(), Arg.Any<Func<IQueryable<User>, IOrderedQueryable<User>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getUsers((List<Expression<Func<User, bool>>>)x[0], (Func<IQueryable<User>, IOrderedQueryable<User>>)x[1], (int)x[2], (int)x[3], (string)x[4]));
            userRepository.GetById(Arg.Any<string>()).Returns(x => users.Where(u => u.Id == (string)x[0]).Single());
            userRepository.Count(Arg.Any<Expression<Func<User, bool>>[]>()).Returns(x => usersCount((Expression<Func<User, bool>>[])x[0]));

            IRepository<Role, string> roleRepository = Substitute.For<IRepository<Role, string>>();
            roleRepository.Get(Arg.Any<List<Expression<Func<Role, bool>>>>(), Arg.Any<Func<IQueryable<Role>, IOrderedQueryable<Role>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getRoles((List<Expression<Func<Role, bool>>>)x[0], (Func<IQueryable<Role>, IOrderedQueryable<Role>>)x[1], (int)x[2], (int)x[3], (string)x[4]));

            IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.UserRepository.Returns(userRepository);
            unitOfWork.RoleRepository.Returns(roleRepository);

            UserService userService = new UserService(unitOfWork);

            // Act
            var result = userService.GetPagedUsersList(loggedInUserId: "1", role: "Admin");

            // Assert
            Assert.AreEqual(1, result.Users.Count());
            Assert.AreEqual("1", result.Users.ElementAt(0).Id);
            Assert.AreEqual(1, result.NumberOfPages);
            Assert.AreEqual(1, result.FoundItemsCount);
            Assert.AreEqual(3, result.TotalItemsCount);
        }

        [Test]
        public void GetPagedUsersList_WithUserRoleParam_ReturnsNonAdminUsers()
        {
            // Arrange
            IRepository<User, string> userRepository = Substitute.For<IRepository<User, string>>();
            userRepository.Get(Arg.Any<List<Expression<Func<User, bool>>>>(), Arg.Any<Func<IQueryable<User>, IOrderedQueryable<User>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getUsers((List<Expression<Func<User, bool>>>)x[0], (Func<IQueryable<User>, IOrderedQueryable<User>>)x[1], (int)x[2], (int)x[3], (string)x[4]));
            userRepository.GetById(Arg.Any<string>()).Returns(x => users.Where(u => u.Id == (string)x[0]).Single());
            userRepository.Count(Arg.Any<Expression<Func<User, bool>>[]>()).Returns(x => usersCount((Expression<Func<User, bool>>[])x[0]));

            IRepository<Role, string> roleRepository = Substitute.For<IRepository<Role, string>>();
            roleRepository.Get(Arg.Any<List<Expression<Func<Role, bool>>>>(), Arg.Any<Func<IQueryable<Role>, IOrderedQueryable<Role>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getRoles((List<Expression<Func<Role, bool>>>)x[0], (Func<IQueryable<Role>, IOrderedQueryable<Role>>)x[1], (int)x[2], (int)x[3], (string)x[4]));

            IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.UserRepository.Returns(userRepository);
            unitOfWork.RoleRepository.Returns(roleRepository);

            UserService userService = new UserService(unitOfWork);

            // Act
            var result = userService.GetPagedUsersList(loggedInUserId: "1", role: "User");

            // Assert
            Assert.AreEqual(2, result.Users.Count());
            Assert.AreEqual("2", result.Users.ElementAt(0).Id);
            Assert.AreEqual("3", result.Users.ElementAt(1).Id);
            Assert.AreEqual(1, result.NumberOfPages);
            Assert.AreEqual(2, result.FoundItemsCount);
            Assert.AreEqual(3, result.TotalItemsCount);
        }

        [Test]
        public void GetPagedUsersList_WithEmptySearchParamAndTrueSearchAllWordsParam_ReturnsAllUsers()
        {
            // Arrange
            IRepository<User, string> userRepository = Substitute.For<IRepository<User, string>>();
            userRepository.Get(Arg.Any<List<Expression<Func<User, bool>>>>(), Arg.Any<Func<IQueryable<User>, IOrderedQueryable<User>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getUsers((List<Expression<Func<User, bool>>>)x[0], (Func<IQueryable<User>, IOrderedQueryable<User>>)x[1], (int)x[2], (int)x[3], (string)x[4]));
            userRepository.GetById(Arg.Any<string>()).Returns(x => users.Where(u => u.Id == (string)x[0]).Single());
            userRepository.Count(Arg.Any<Expression<Func<User, bool>>[]>()).Returns(x => usersCount((Expression<Func<User, bool>>[])x[0]));

            IRepository<Role, string> roleRepository = Substitute.For<IRepository<Role, string>>();
            roleRepository.Get(Arg.Any<List<Expression<Func<Role, bool>>>>(), Arg.Any<Func<IQueryable<Role>, IOrderedQueryable<Role>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getRoles((List<Expression<Func<Role, bool>>>)x[0], (Func<IQueryable<Role>, IOrderedQueryable<Role>>)x[1], (int)x[2], (int)x[3], (string)x[4]));

            IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.UserRepository.Returns(userRepository);
            unitOfWork.RoleRepository.Returns(roleRepository);

            UserService userService = new UserService(unitOfWork);

            // Act
            var result = userService.GetPagedUsersList(loggedInUserId: "1", search: "", searchAllWords: true);

            // Assert
            Assert.AreEqual(3, result.Users.Count());
            Assert.AreEqual("2", result.Users.ElementAt(0).Id);
            Assert.AreEqual("1", result.Users.ElementAt(1).Id);
            Assert.AreEqual("3", result.Users.ElementAt(2).Id);
            Assert.AreEqual(1, result.NumberOfPages);
            Assert.AreEqual(3, result.FoundItemsCount);
            Assert.AreEqual(3, result.TotalItemsCount);
        }

        [Test]
        public void GetPagedUsersList_WithSpacesAsSearchParamAndTrueSearchAllWordsParam_ReturnsAllUsers()
        {
            // Arrange
            IRepository<User, string> userRepository = Substitute.For<IRepository<User, string>>();
            userRepository.Get(Arg.Any<List<Expression<Func<User, bool>>>>(), Arg.Any<Func<IQueryable<User>, IOrderedQueryable<User>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getUsers((List<Expression<Func<User, bool>>>)x[0], (Func<IQueryable<User>, IOrderedQueryable<User>>)x[1], (int)x[2], (int)x[3], (string)x[4]));
            userRepository.GetById(Arg.Any<string>()).Returns(x => users.Where(u => u.Id == (string)x[0]).Single());
            userRepository.Count(Arg.Any<Expression<Func<User, bool>>[]>()).Returns(x => usersCount((Expression<Func<User, bool>>[])x[0]));

            IRepository<Role, string> roleRepository = Substitute.For<IRepository<Role, string>>();
            roleRepository.Get(Arg.Any<List<Expression<Func<Role, bool>>>>(), Arg.Any<Func<IQueryable<Role>, IOrderedQueryable<Role>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getRoles((List<Expression<Func<Role, bool>>>)x[0], (Func<IQueryable<Role>, IOrderedQueryable<Role>>)x[1], (int)x[2], (int)x[3], (string)x[4]));

            IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.UserRepository.Returns(userRepository);
            unitOfWork.RoleRepository.Returns(roleRepository);

            UserService userService = new UserService(unitOfWork);

            // Act
            var result = userService.GetPagedUsersList(loggedInUserId: "1", search: "    ", searchAllWords: true);

            // Assert
            Assert.AreEqual(3, result.Users.Count());
            Assert.AreEqual("2", result.Users.ElementAt(0).Id);
            Assert.AreEqual("1", result.Users.ElementAt(1).Id);
            Assert.AreEqual("3", result.Users.ElementAt(2).Id);
            Assert.AreEqual(1, result.NumberOfPages);
            Assert.AreEqual(3, result.FoundItemsCount);
            Assert.AreEqual(3, result.TotalItemsCount);
        }

        [Test]
        public void GetPagedUsersList_WithSpecificStringAsSearchParamAndTrueSearchAllWordsParam_ReturnsAllUsers()
        {
            // Arrange
            IRepository<User, string> userRepository = Substitute.For<IRepository<User, string>>();
            userRepository.Get(Arg.Any<List<Expression<Func<User, bool>>>>(), Arg.Any<Func<IQueryable<User>, IOrderedQueryable<User>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getUsers((List<Expression<Func<User, bool>>>)x[0], (Func<IQueryable<User>, IOrderedQueryable<User>>)x[1], (int)x[2], (int)x[3], (string)x[4]));
            userRepository.GetById(Arg.Any<string>()).Returns(x => users.Where(u => u.Id == (string)x[0]).Single());
            userRepository.Count(Arg.Any<Expression<Func<User, bool>>[]>()).Returns(x => usersCount((Expression<Func<User, bool>>[])x[0]));

            IRepository<Role, string> roleRepository = Substitute.For<IRepository<Role, string>>();
            roleRepository.Get(Arg.Any<List<Expression<Func<Role, bool>>>>(), Arg.Any<Func<IQueryable<Role>, IOrderedQueryable<Role>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getRoles((List<Expression<Func<Role, bool>>>)x[0], (Func<IQueryable<Role>, IOrderedQueryable<Role>>)x[1], (int)x[2], (int)x[3], (string)x[4]));

            IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.UserRepository.Returns(userRepository);
            unitOfWork.RoleRepository.Returns(roleRepository);

            UserService userService = new UserService(unitOfWork);

            // Act
            var result = userService.GetPagedUsersList(loggedInUserId: "1", search: "Sylvester Stallone sylvester.stallone@example.com 123-456-789 4323453 Microsoft Electricity", searchAllWords: true);

            // Assert
            Assert.AreEqual(1, result.Users.Count());
            Assert.AreEqual("1", result.Users.ElementAt(0).Id);
            Assert.AreEqual(1, result.NumberOfPages);
            Assert.AreEqual(1, result.FoundItemsCount);
            Assert.AreEqual(3, result.TotalItemsCount);
        }

        [Test]
        public void GetPagedUsersList_WithSpecificStringAsSearchParamAndFalseSearchAllWordsParam_ReturnsAllUsers()
        {
            // Arrange
            IRepository<User, string> userRepository = Substitute.For<IRepository<User, string>>();
            userRepository.Get(Arg.Any<List<Expression<Func<User, bool>>>>(), Arg.Any<Func<IQueryable<User>, IOrderedQueryable<User>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getUsers((List<Expression<Func<User, bool>>>)x[0], (Func<IQueryable<User>, IOrderedQueryable<User>>)x[1], (int)x[2], (int)x[3], (string)x[4]));
            userRepository.GetById(Arg.Any<string>()).Returns(x => users.Where(u => u.Id == (string)x[0]).Single());
            userRepository.Count(Arg.Any<Expression<Func<User, bool>>[]>()).Returns(x => usersCount((Expression<Func<User, bool>>[])x[0]));

            IRepository<Role, string> roleRepository = Substitute.For<IRepository<Role, string>>();
            roleRepository.Get(Arg.Any<List<Expression<Func<Role, bool>>>>(), Arg.Any<Func<IQueryable<Role>, IOrderedQueryable<Role>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getRoles((List<Expression<Func<Role, bool>>>)x[0], (Func<IQueryable<Role>, IOrderedQueryable<Role>>)x[1], (int)x[2], (int)x[3], (string)x[4]));

            IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.UserRepository.Returns(userRepository);
            unitOfWork.RoleRepository.Returns(roleRepository);

            UserService userService = new UserService(unitOfWork);

            // Act
            var result = userService.GetPagedUsersList(loggedInUserId: "1", search: "Sylvester Stallone sylvester.stallone@example.com 123-456-789 4323453 Microsoft Electricity", searchAllWords: false);

            // Assert
            Assert.AreEqual(2, result.Users.Count());
            Assert.AreEqual("1", result.Users.ElementAt(0).Id);
            Assert.AreEqual("3", result.Users.ElementAt(1).Id);
            Assert.AreEqual(1, result.NumberOfPages);
            Assert.AreEqual(2, result.FoundItemsCount);
            Assert.AreEqual(3, result.TotalItemsCount);
        }

        [Test]
        public void GetPagedUsersList_WithFirstNameAsSortByAndFalseAsDescSort_ReturnsAllUsersSortedByFirstName()
        {
            // Arrange
            IRepository<User, string> userRepository = Substitute.For<IRepository<User, string>>();
            userRepository.Get(Arg.Any<List<Expression<Func<User, bool>>>>(), Arg.Any<Func<IQueryable<User>, IOrderedQueryable<User>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getUsers((List<Expression<Func<User, bool>>>)x[0], (Func<IQueryable<User>, IOrderedQueryable<User>>)x[1], (int)x[2], (int)x[3], (string)x[4]));
            userRepository.GetById(Arg.Any<string>()).Returns(x => users.Where(u => u.Id == (string)x[0]).Single());
            userRepository.Count(Arg.Any<Expression<Func<User, bool>>[]>()).Returns(x => usersCount((Expression<Func<User, bool>>[])x[0]));

            IRepository<Role, string> roleRepository = Substitute.For<IRepository<Role, string>>();
            roleRepository.Get(Arg.Any<List<Expression<Func<Role, bool>>>>(), Arg.Any<Func<IQueryable<Role>, IOrderedQueryable<Role>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getRoles((List<Expression<Func<Role, bool>>>)x[0], (Func<IQueryable<Role>, IOrderedQueryable<Role>>)x[1], (int)x[2], (int)x[3], (string)x[4]));

            IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.UserRepository.Returns(userRepository);
            unitOfWork.RoleRepository.Returns(roleRepository);

            UserService userService = new UserService(unitOfWork);

            // Act
            var result = userService.GetPagedUsersList(loggedInUserId: "1", sortBy: "First name", descSort: false);

            // Assert
            Assert.AreEqual(3, result.Users.Count());
            Assert.AreEqual("2", result.Users.ElementAt(0).Id);
            Assert.AreEqual("3", result.Users.ElementAt(1).Id);
            Assert.AreEqual("1", result.Users.ElementAt(2).Id);
            Assert.AreEqual(1, result.NumberOfPages);
            Assert.AreEqual(3, result.FoundItemsCount);
            Assert.AreEqual(3, result.TotalItemsCount);
        }

        [Test]
        public void GetPagedUsersList_WithFirstNameAsSortByAndFalseAsDescSort_ReturnsAllUsersSortedByFirstNameDescending()
        {
            // Arrange
            IRepository<User, string> userRepository = Substitute.For<IRepository<User, string>>();
            userRepository.Get(Arg.Any<List<Expression<Func<User, bool>>>>(), Arg.Any<Func<IQueryable<User>, IOrderedQueryable<User>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getUsers((List<Expression<Func<User, bool>>>)x[0], (Func<IQueryable<User>, IOrderedQueryable<User>>)x[1], (int)x[2], (int)x[3], (string)x[4]));
            userRepository.GetById(Arg.Any<string>()).Returns(x => users.Where(u => u.Id == (string)x[0]).Single());
            userRepository.Count(Arg.Any<Expression<Func<User, bool>>[]>()).Returns(x => usersCount((Expression<Func<User, bool>>[])x[0]));

            IRepository<Role, string> roleRepository = Substitute.For<IRepository<Role, string>>();
            roleRepository.Get(Arg.Any<List<Expression<Func<Role, bool>>>>(), Arg.Any<Func<IQueryable<Role>, IOrderedQueryable<Role>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getRoles((List<Expression<Func<Role, bool>>>)x[0], (Func<IQueryable<Role>, IOrderedQueryable<Role>>)x[1], (int)x[2], (int)x[3], (string)x[4]));

            IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.UserRepository.Returns(userRepository);
            unitOfWork.RoleRepository.Returns(roleRepository);

            UserService userService = new UserService(unitOfWork);

            // Act
            var result = userService.GetPagedUsersList(loggedInUserId: "1", sortBy: "First name", descSort: true);

            // Assert
            Assert.AreEqual(3, result.Users.Count());
            Assert.AreEqual("1", result.Users.ElementAt(0).Id);
            Assert.AreEqual("3", result.Users.ElementAt(1).Id);
            Assert.AreEqual("2", result.Users.ElementAt(2).Id);
            Assert.AreEqual(1, result.NumberOfPages);
            Assert.AreEqual(3, result.FoundItemsCount);
            Assert.AreEqual(3, result.TotalItemsCount);
        }

        [Test]
        public void GetPagedUsersList_WithLastNameAsSortByAndFalseAsDescSort_ReturnsAllUsersSortedByLastName()
        {
            // Arrange
            IRepository<User, string> userRepository = Substitute.For<IRepository<User, string>>();
            userRepository.Get(Arg.Any<List<Expression<Func<User, bool>>>>(), Arg.Any<Func<IQueryable<User>, IOrderedQueryable<User>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getUsers((List<Expression<Func<User, bool>>>)x[0], (Func<IQueryable<User>, IOrderedQueryable<User>>)x[1], (int)x[2], (int)x[3], (string)x[4]));
            userRepository.GetById(Arg.Any<string>()).Returns(x => users.Where(u => u.Id == (string)x[0]).Single());
            userRepository.Count(Arg.Any<Expression<Func<User, bool>>[]>()).Returns(x => usersCount((Expression<Func<User, bool>>[])x[0]));

            IRepository<Role, string> roleRepository = Substitute.For<IRepository<Role, string>>();
            roleRepository.Get(Arg.Any<List<Expression<Func<Role, bool>>>>(), Arg.Any<Func<IQueryable<Role>, IOrderedQueryable<Role>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getRoles((List<Expression<Func<Role, bool>>>)x[0], (Func<IQueryable<Role>, IOrderedQueryable<Role>>)x[1], (int)x[2], (int)x[3], (string)x[4]));

            IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.UserRepository.Returns(userRepository);
            unitOfWork.RoleRepository.Returns(roleRepository);

            UserService userService = new UserService(unitOfWork);

            // Act
            var result = userService.GetPagedUsersList(loggedInUserId: "1", sortBy: "Last name", descSort: false);

            // Assert
            Assert.AreEqual(3, result.Users.Count());
            Assert.AreEqual("2", result.Users.ElementAt(0).Id);
            Assert.AreEqual("1", result.Users.ElementAt(1).Id);
            Assert.AreEqual("3", result.Users.ElementAt(2).Id);
            Assert.AreEqual(1, result.NumberOfPages);
            Assert.AreEqual(3, result.FoundItemsCount);
            Assert.AreEqual(3, result.TotalItemsCount);
        }

        [Test]
        public void GetPagedUsersList_WithLastNameAsSortByAndFalseAsDescSort_ReturnsAllUsersSortedByLastNameDescending()
        {
            // Arrange
            IRepository<User, string> userRepository = Substitute.For<IRepository<User, string>>();
            userRepository.Get(Arg.Any<List<Expression<Func<User, bool>>>>(), Arg.Any<Func<IQueryable<User>, IOrderedQueryable<User>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getUsers((List<Expression<Func<User, bool>>>)x[0], (Func<IQueryable<User>, IOrderedQueryable<User>>)x[1], (int)x[2], (int)x[3], (string)x[4]));
            userRepository.GetById(Arg.Any<string>()).Returns(x => users.Where(u => u.Id == (string)x[0]).Single());
            userRepository.Count(Arg.Any<Expression<Func<User, bool>>[]>()).Returns(x => usersCount((Expression<Func<User, bool>>[])x[0]));

            IRepository<Role, string> roleRepository = Substitute.For<IRepository<Role, string>>();
            roleRepository.Get(Arg.Any<List<Expression<Func<Role, bool>>>>(), Arg.Any<Func<IQueryable<Role>, IOrderedQueryable<Role>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getRoles((List<Expression<Func<Role, bool>>>)x[0], (Func<IQueryable<Role>, IOrderedQueryable<Role>>)x[1], (int)x[2], (int)x[3], (string)x[4]));

            IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.UserRepository.Returns(userRepository);
            unitOfWork.RoleRepository.Returns(roleRepository);

            UserService userService = new UserService(unitOfWork);

            // Act
            var result = userService.GetPagedUsersList(loggedInUserId: "1", sortBy: "Last name", descSort: true);

            // Assert
            Assert.AreEqual(3, result.Users.Count());
            Assert.AreEqual("3", result.Users.ElementAt(0).Id);
            Assert.AreEqual("1", result.Users.ElementAt(1).Id);
            Assert.AreEqual("2", result.Users.ElementAt(2).Id);
            Assert.AreEqual(1, result.NumberOfPages);
            Assert.AreEqual(3, result.FoundItemsCount);
            Assert.AreEqual(3, result.TotalItemsCount);
        }

        [Test]
        public void GetPagedUsersList_WithEmailAsSortByAndFalseAsDescSort_ReturnsAllUsersSortedByEmail()
        {
            // Arrange
            IRepository<User, string> userRepository = Substitute.For<IRepository<User, string>>();
            userRepository.Get(Arg.Any<List<Expression<Func<User, bool>>>>(), Arg.Any<Func<IQueryable<User>, IOrderedQueryable<User>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getUsers((List<Expression<Func<User, bool>>>)x[0], (Func<IQueryable<User>, IOrderedQueryable<User>>)x[1], (int)x[2], (int)x[3], (string)x[4]));
            userRepository.GetById(Arg.Any<string>()).Returns(x => users.Where(u => u.Id == (string)x[0]).Single());
            userRepository.Count(Arg.Any<Expression<Func<User, bool>>[]>()).Returns(x => usersCount((Expression<Func<User, bool>>[])x[0]));

            IRepository<Role, string> roleRepository = Substitute.For<IRepository<Role, string>>();
            roleRepository.Get(Arg.Any<List<Expression<Func<Role, bool>>>>(), Arg.Any<Func<IQueryable<Role>, IOrderedQueryable<Role>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getRoles((List<Expression<Func<Role, bool>>>)x[0], (Func<IQueryable<Role>, IOrderedQueryable<Role>>)x[1], (int)x[2], (int)x[3], (string)x[4]));

            IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.UserRepository.Returns(userRepository);
            unitOfWork.RoleRepository.Returns(roleRepository);

            UserService userService = new UserService(unitOfWork);

            // Act
            var result = userService.GetPagedUsersList(loggedInUserId: "1", sortBy: "Email", descSort: false);

            // Assert
            Assert.AreEqual(3, result.Users.Count());
            Assert.AreEqual("2", result.Users.ElementAt(0).Id);
            Assert.AreEqual("1", result.Users.ElementAt(1).Id);
            Assert.AreEqual("3", result.Users.ElementAt(2).Id);
            Assert.AreEqual(1, result.NumberOfPages);
            Assert.AreEqual(3, result.FoundItemsCount);
            Assert.AreEqual(3, result.TotalItemsCount);
        }

        [Test]
        public void GetPagedUsersList_WithEmailAsSortByAndFalseAsDescSort_ReturnsAllUsersSortedByEmailDescending()
        {
            // Arrange
            IRepository<User, string> userRepository = Substitute.For<IRepository<User, string>>();
            userRepository.Get(Arg.Any<List<Expression<Func<User, bool>>>>(), Arg.Any<Func<IQueryable<User>, IOrderedQueryable<User>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getUsers((List<Expression<Func<User, bool>>>)x[0], (Func<IQueryable<User>, IOrderedQueryable<User>>)x[1], (int)x[2], (int)x[3], (string)x[4]));
            userRepository.GetById(Arg.Any<string>()).Returns(x => users.Where(u => u.Id == (string)x[0]).Single());
            userRepository.Count(Arg.Any<Expression<Func<User, bool>>[]>()).Returns(x => usersCount((Expression<Func<User, bool>>[])x[0]));

            IRepository<Role, string> roleRepository = Substitute.For<IRepository<Role, string>>();
            roleRepository.Get(Arg.Any<List<Expression<Func<Role, bool>>>>(), Arg.Any<Func<IQueryable<Role>, IOrderedQueryable<Role>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getRoles((List<Expression<Func<Role, bool>>>)x[0], (Func<IQueryable<Role>, IOrderedQueryable<Role>>)x[1], (int)x[2], (int)x[3], (string)x[4]));

            IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.UserRepository.Returns(userRepository);
            unitOfWork.RoleRepository.Returns(roleRepository);

            UserService userService = new UserService(unitOfWork);

            // Act
            var result = userService.GetPagedUsersList(loggedInUserId: "1", sortBy: "Email", descSort: true);

            // Assert
            Assert.AreEqual(3, result.Users.Count());
            Assert.AreEqual("3", result.Users.ElementAt(0).Id);
            Assert.AreEqual("1", result.Users.ElementAt(1).Id);
            Assert.AreEqual("2", result.Users.ElementAt(2).Id);
            Assert.AreEqual(1, result.NumberOfPages);
            Assert.AreEqual(3, result.FoundItemsCount);
            Assert.AreEqual(3, result.TotalItemsCount);
        }

        [Test]
        public void GetPagedUsersList_WithPhoneAsSortByAndFalseAsDescSort_ReturnsAllUsersSortedByPhone()
        {
            // Arrange
            IRepository<User, string> userRepository = Substitute.For<IRepository<User, string>>();
            userRepository.Get(Arg.Any<List<Expression<Func<User, bool>>>>(), Arg.Any<Func<IQueryable<User>, IOrderedQueryable<User>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getUsers((List<Expression<Func<User, bool>>>)x[0], (Func<IQueryable<User>, IOrderedQueryable<User>>)x[1], (int)x[2], (int)x[3], (string)x[4]));
            userRepository.GetById(Arg.Any<string>()).Returns(x => users.Where(u => u.Id == (string)x[0]).Single());
            userRepository.Count(Arg.Any<Expression<Func<User, bool>>[]>()).Returns(x => usersCount((Expression<Func<User, bool>>[])x[0]));

            IRepository<Role, string> roleRepository = Substitute.For<IRepository<Role, string>>();
            roleRepository.Get(Arg.Any<List<Expression<Func<Role, bool>>>>(), Arg.Any<Func<IQueryable<Role>, IOrderedQueryable<Role>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getRoles((List<Expression<Func<Role, bool>>>)x[0], (Func<IQueryable<Role>, IOrderedQueryable<Role>>)x[1], (int)x[2], (int)x[3], (string)x[4]));

            IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.UserRepository.Returns(userRepository);
            unitOfWork.RoleRepository.Returns(roleRepository);

            UserService userService = new UserService(unitOfWork);

            // Act
            var result = userService.GetPagedUsersList(loggedInUserId: "1", sortBy: "Phone", descSort: false);

            // Assert
            Assert.AreEqual(3, result.Users.Count());
            Assert.AreEqual("1", result.Users.ElementAt(0).Id);
            Assert.AreEqual("3", result.Users.ElementAt(1).Id);
            Assert.AreEqual("2", result.Users.ElementAt(2).Id);
            Assert.AreEqual(1, result.NumberOfPages);
            Assert.AreEqual(3, result.FoundItemsCount);
            Assert.AreEqual(3, result.TotalItemsCount);
        }

        [Test]
        public void GetPagedUsersList_WithPhoneAsSortByAndFalseAsDescSort_ReturnsAllUsersSortedByPhoneDescending()
        {
            // Arrange
            IRepository<User, string> userRepository = Substitute.For<IRepository<User, string>>();
            userRepository.Get(Arg.Any<List<Expression<Func<User, bool>>>>(), Arg.Any<Func<IQueryable<User>, IOrderedQueryable<User>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getUsers((List<Expression<Func<User, bool>>>)x[0], (Func<IQueryable<User>, IOrderedQueryable<User>>)x[1], (int)x[2], (int)x[3], (string)x[4]));
            userRepository.GetById(Arg.Any<string>()).Returns(x => users.Where(u => u.Id == (string)x[0]).Single());
            userRepository.Count(Arg.Any<Expression<Func<User, bool>>[]>()).Returns(x => usersCount((Expression<Func<User, bool>>[])x[0]));

            IRepository<Role, string> roleRepository = Substitute.For<IRepository<Role, string>>();
            roleRepository.Get(Arg.Any<List<Expression<Func<Role, bool>>>>(), Arg.Any<Func<IQueryable<Role>, IOrderedQueryable<Role>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getRoles((List<Expression<Func<Role, bool>>>)x[0], (Func<IQueryable<Role>, IOrderedQueryable<Role>>)x[1], (int)x[2], (int)x[3], (string)x[4]));

            IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.UserRepository.Returns(userRepository);
            unitOfWork.RoleRepository.Returns(roleRepository);

            UserService userService = new UserService(unitOfWork);

            // Act
            var result = userService.GetPagedUsersList(loggedInUserId: "1", sortBy: "Phone", descSort: true);

            // Assert
            Assert.AreEqual(3, result.Users.Count());
            Assert.AreEqual("2", result.Users.ElementAt(0).Id);
            Assert.AreEqual("3", result.Users.ElementAt(1).Id);
            Assert.AreEqual("1", result.Users.ElementAt(2).Id);
            Assert.AreEqual(1, result.NumberOfPages);
            Assert.AreEqual(3, result.FoundItemsCount);
            Assert.AreEqual(3, result.TotalItemsCount);
        }

        [Test]
        public void GetPagedUsersList_WithMobilePhoneAsSortByAndFalseAsDescSort_ReturnsAllUsersSortedByMobilePhone()
        {
            // Arrange
            IRepository<User, string> userRepository = Substitute.For<IRepository<User, string>>();
            userRepository.Get(Arg.Any<List<Expression<Func<User, bool>>>>(), Arg.Any<Func<IQueryable<User>, IOrderedQueryable<User>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getUsers((List<Expression<Func<User, bool>>>)x[0], (Func<IQueryable<User>, IOrderedQueryable<User>>)x[1], (int)x[2], (int)x[3], (string)x[4]));
            userRepository.GetById(Arg.Any<string>()).Returns(x => users.Where(u => u.Id == (string)x[0]).Single());
            userRepository.Count(Arg.Any<Expression<Func<User, bool>>[]>()).Returns(x => usersCount((Expression<Func<User, bool>>[])x[0]));

            IRepository<Role, string> roleRepository = Substitute.For<IRepository<Role, string>>();
            roleRepository.Get(Arg.Any<List<Expression<Func<Role, bool>>>>(), Arg.Any<Func<IQueryable<Role>, IOrderedQueryable<Role>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getRoles((List<Expression<Func<Role, bool>>>)x[0], (Func<IQueryable<Role>, IOrderedQueryable<Role>>)x[1], (int)x[2], (int)x[3], (string)x[4]));

            IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.UserRepository.Returns(userRepository);
            unitOfWork.RoleRepository.Returns(roleRepository);

            UserService userService = new UserService(unitOfWork);

            // Act
            var result = userService.GetPagedUsersList(loggedInUserId: "1", sortBy: "Mobile phone", descSort: false);

            // Assert
            Assert.AreEqual(3, result.Users.Count());
            Assert.AreEqual("2", result.Users.ElementAt(0).Id);
            Assert.AreEqual("3", result.Users.ElementAt(1).Id);
            Assert.AreEqual("1", result.Users.ElementAt(2).Id);
            Assert.AreEqual(1, result.NumberOfPages);
            Assert.AreEqual(3, result.FoundItemsCount);
            Assert.AreEqual(3, result.TotalItemsCount);
        }

        [Test]
        public void GetPagedUsersList_WithMobilePhoneAsSortByAndFalseAsDescSort_ReturnsAllUsersSortedByMobilePhoneDescending()
        {
            // Arrange
            IRepository<User, string> userRepository = Substitute.For<IRepository<User, string>>();
            userRepository.Get(Arg.Any<List<Expression<Func<User, bool>>>>(), Arg.Any<Func<IQueryable<User>, IOrderedQueryable<User>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getUsers((List<Expression<Func<User, bool>>>)x[0], (Func<IQueryable<User>, IOrderedQueryable<User>>)x[1], (int)x[2], (int)x[3], (string)x[4]));
            userRepository.GetById(Arg.Any<string>()).Returns(x => users.Where(u => u.Id == (string)x[0]).Single());
            userRepository.Count(Arg.Any<Expression<Func<User, bool>>[]>()).Returns(x => usersCount((Expression<Func<User, bool>>[])x[0]));

            IRepository<Role, string> roleRepository = Substitute.For<IRepository<Role, string>>();
            roleRepository.Get(Arg.Any<List<Expression<Func<Role, bool>>>>(), Arg.Any<Func<IQueryable<Role>, IOrderedQueryable<Role>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getRoles((List<Expression<Func<Role, bool>>>)x[0], (Func<IQueryable<Role>, IOrderedQueryable<Role>>)x[1], (int)x[2], (int)x[3], (string)x[4]));

            IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.UserRepository.Returns(userRepository);
            unitOfWork.RoleRepository.Returns(roleRepository);

            UserService userService = new UserService(unitOfWork);

            // Act
            var result = userService.GetPagedUsersList(loggedInUserId: "1", sortBy: "Mobile phone", descSort: true);

            // Assert
            Assert.AreEqual(3, result.Users.Count());
            Assert.AreEqual("1", result.Users.ElementAt(0).Id);
            Assert.AreEqual("3", result.Users.ElementAt(1).Id);
            Assert.AreEqual("2", result.Users.ElementAt(2).Id);
            Assert.AreEqual(1, result.NumberOfPages);
            Assert.AreEqual(3, result.FoundItemsCount);
            Assert.AreEqual(3, result.TotalItemsCount);
        }

        [Test]
        public void GetPagedUsersList_WithCompanyAsSortByAndFalseAsDescSort_ReturnsAllUsersSortedByCompany()
        {
            // Arrange
            IRepository<User, string> userRepository = Substitute.For<IRepository<User, string>>();
            userRepository.Get(Arg.Any<List<Expression<Func<User, bool>>>>(), Arg.Any<Func<IQueryable<User>, IOrderedQueryable<User>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getUsers((List<Expression<Func<User, bool>>>)x[0], (Func<IQueryable<User>, IOrderedQueryable<User>>)x[1], (int)x[2], (int)x[3], (string)x[4]));
            userRepository.GetById(Arg.Any<string>()).Returns(x => users.Where(u => u.Id == (string)x[0]).Single());
            userRepository.Count(Arg.Any<Expression<Func<User, bool>>[]>()).Returns(x => usersCount((Expression<Func<User, bool>>[])x[0]));

            IRepository<Role, string> roleRepository = Substitute.For<IRepository<Role, string>>();
            roleRepository.Get(Arg.Any<List<Expression<Func<Role, bool>>>>(), Arg.Any<Func<IQueryable<Role>, IOrderedQueryable<Role>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getRoles((List<Expression<Func<Role, bool>>>)x[0], (Func<IQueryable<Role>, IOrderedQueryable<Role>>)x[1], (int)x[2], (int)x[3], (string)x[4]));

            IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.UserRepository.Returns(userRepository);
            unitOfWork.RoleRepository.Returns(roleRepository);

            UserService userService = new UserService(unitOfWork);

            // Act
            var result = userService.GetPagedUsersList(loggedInUserId: "1", sortBy: "Company", descSort: false);

            // Assert
            Assert.AreEqual(3, result.Users.Count());
            Assert.AreEqual("3", result.Users.ElementAt(0).Id);
            Assert.AreEqual("2", result.Users.ElementAt(1).Id);
            Assert.AreEqual("1", result.Users.ElementAt(2).Id);
            Assert.AreEqual(1, result.NumberOfPages);
            Assert.AreEqual(3, result.FoundItemsCount);
            Assert.AreEqual(3, result.TotalItemsCount);
        }

        [Test]
        public void GetPagedUsersList_WithCompanyAsSortByAndFalseAsDescSort_ReturnsAllUsersSortedByCompanyDescending()
        {
            // Arrange
            IRepository<User, string> userRepository = Substitute.For<IRepository<User, string>>();
            userRepository.Get(Arg.Any<List<Expression<Func<User, bool>>>>(), Arg.Any<Func<IQueryable<User>, IOrderedQueryable<User>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getUsers((List<Expression<Func<User, bool>>>)x[0], (Func<IQueryable<User>, IOrderedQueryable<User>>)x[1], (int)x[2], (int)x[3], (string)x[4]));
            userRepository.GetById(Arg.Any<string>()).Returns(x => users.Where(u => u.Id == (string)x[0]).Single());
            userRepository.Count(Arg.Any<Expression<Func<User, bool>>[]>()).Returns(x => usersCount((Expression<Func<User, bool>>[])x[0]));

            IRepository<Role, string> roleRepository = Substitute.For<IRepository<Role, string>>();
            roleRepository.Get(Arg.Any<List<Expression<Func<Role, bool>>>>(), Arg.Any<Func<IQueryable<Role>, IOrderedQueryable<Role>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getRoles((List<Expression<Func<Role, bool>>>)x[0], (Func<IQueryable<Role>, IOrderedQueryable<Role>>)x[1], (int)x[2], (int)x[3], (string)x[4]));

            IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.UserRepository.Returns(userRepository);
            unitOfWork.RoleRepository.Returns(roleRepository);

            UserService userService = new UserService(unitOfWork);

            // Act
            var result = userService.GetPagedUsersList(loggedInUserId: "1", sortBy: "Company", descSort: true);

            // Assert
            Assert.AreEqual(3, result.Users.Count());
            Assert.AreEqual("1", result.Users.ElementAt(0).Id);
            Assert.AreEqual("2", result.Users.ElementAt(1).Id);
            Assert.AreEqual("3", result.Users.ElementAt(2).Id);
            Assert.AreEqual(1, result.NumberOfPages);
            Assert.AreEqual(3, result.FoundItemsCount);
            Assert.AreEqual(3, result.TotalItemsCount);
        }

        [Test]
        public void GetPagedUsersList_WithDepartmentAsSortByAndFalseAsDescSort_ReturnsAllUsersSortedByDepartment()
        {
            // Arrange
            IRepository<User, string> userRepository = Substitute.For<IRepository<User, string>>();
            userRepository.Get(Arg.Any<List<Expression<Func<User, bool>>>>(), Arg.Any<Func<IQueryable<User>, IOrderedQueryable<User>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getUsers((List<Expression<Func<User, bool>>>)x[0], (Func<IQueryable<User>, IOrderedQueryable<User>>)x[1], (int)x[2], (int)x[3], (string)x[4]));
            userRepository.GetById(Arg.Any<string>()).Returns(x => users.Where(u => u.Id == (string)x[0]).Single());
            userRepository.Count(Arg.Any<Expression<Func<User, bool>>[]>()).Returns(x => usersCount((Expression<Func<User, bool>>[])x[0]));

            IRepository<Role, string> roleRepository = Substitute.For<IRepository<Role, string>>();
            roleRepository.Get(Arg.Any<List<Expression<Func<Role, bool>>>>(), Arg.Any<Func<IQueryable<Role>, IOrderedQueryable<Role>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getRoles((List<Expression<Func<Role, bool>>>)x[0], (Func<IQueryable<Role>, IOrderedQueryable<Role>>)x[1], (int)x[2], (int)x[3], (string)x[4]));

            IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.UserRepository.Returns(userRepository);
            unitOfWork.RoleRepository.Returns(roleRepository);

            UserService userService = new UserService(unitOfWork);

            // Act
            var result = userService.GetPagedUsersList(loggedInUserId: "1", sortBy: "Department", descSort: false);

            // Assert
            Assert.AreEqual(3, result.Users.Count());
            Assert.AreEqual("2", result.Users.ElementAt(0).Id);
            Assert.AreEqual("1", result.Users.ElementAt(1).Id);
            Assert.AreEqual("3", result.Users.ElementAt(2).Id);
            Assert.AreEqual(1, result.NumberOfPages);
            Assert.AreEqual(3, result.FoundItemsCount);
            Assert.AreEqual(3, result.TotalItemsCount);
        }

        [Test]
        public void GetPagedUsersList_WithDepartmentAsSortByAndFalseAsDescSort_ReturnsAllUsersSortedByDepartmentDescending()
        {
            // Arrange
            IRepository<User, string> userRepository = Substitute.For<IRepository<User, string>>();
            userRepository.Get(Arg.Any<List<Expression<Func<User, bool>>>>(), Arg.Any<Func<IQueryable<User>, IOrderedQueryable<User>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getUsers((List<Expression<Func<User, bool>>>)x[0], (Func<IQueryable<User>, IOrderedQueryable<User>>)x[1], (int)x[2], (int)x[3], (string)x[4]));
            userRepository.GetById(Arg.Any<string>()).Returns(x => users.Where(u => u.Id == (string)x[0]).Single());
            userRepository.Count(Arg.Any<Expression<Func<User, bool>>[]>()).Returns(x => usersCount((Expression<Func<User, bool>>[])x[0]));

            IRepository<Role, string> roleRepository = Substitute.For<IRepository<Role, string>>();
            roleRepository.Get(Arg.Any<List<Expression<Func<Role, bool>>>>(), Arg.Any<Func<IQueryable<Role>, IOrderedQueryable<Role>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getRoles((List<Expression<Func<Role, bool>>>)x[0], (Func<IQueryable<Role>, IOrderedQueryable<Role>>)x[1], (int)x[2], (int)x[3], (string)x[4]));

            IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.UserRepository.Returns(userRepository);
            unitOfWork.RoleRepository.Returns(roleRepository);

            UserService userService = new UserService(unitOfWork);

            // Act
            var result = userService.GetPagedUsersList(loggedInUserId: "1", sortBy: "Department", descSort: true);

            // Assert
            Assert.AreEqual(3, result.Users.Count());
            Assert.AreEqual("3", result.Users.ElementAt(0).Id);
            Assert.AreEqual("1", result.Users.ElementAt(1).Id);
            Assert.AreEqual("2", result.Users.ElementAt(2).Id);
            Assert.AreEqual(1, result.NumberOfPages);
            Assert.AreEqual(3, result.FoundItemsCount);
            Assert.AreEqual(3, result.TotalItemsCount);
        }

        [Test]
        public void GetPagedUsersList_WithRoleAsSortByAndFalseAsDescSort_ReturnsAllUsersSortedByRole()
        {
            // Arrange
            IRepository<User, string> userRepository = Substitute.For<IRepository<User, string>>();
            userRepository.Get(Arg.Any<List<Expression<Func<User, bool>>>>(), Arg.Any<Func<IQueryable<User>, IOrderedQueryable<User>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getUsers((List<Expression<Func<User, bool>>>)x[0], (Func<IQueryable<User>, IOrderedQueryable<User>>)x[1], (int)x[2], (int)x[3], (string)x[4]));
            userRepository.GetById(Arg.Any<string>()).Returns(x => users.Where(u => u.Id == (string)x[0]).Single());
            userRepository.Count(Arg.Any<Expression<Func<User, bool>>[]>()).Returns(x => usersCount((Expression<Func<User, bool>>[])x[0]));

            IRepository<Role, string> roleRepository = Substitute.For<IRepository<Role, string>>();
            roleRepository.Get(Arg.Any<List<Expression<Func<Role, bool>>>>(), Arg.Any<Func<IQueryable<Role>, IOrderedQueryable<Role>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getRoles((List<Expression<Func<Role, bool>>>)x[0], (Func<IQueryable<Role>, IOrderedQueryable<Role>>)x[1], (int)x[2], (int)x[3], (string)x[4]));

            IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.UserRepository.Returns(userRepository);
            unitOfWork.RoleRepository.Returns(roleRepository);

            UserService userService = new UserService(unitOfWork);

            // Act
            var result = userService.GetPagedUsersList(loggedInUserId: "1", sortBy: "Role", descSort: false);

            // Assert
            Assert.AreEqual(3, result.Users.Count());
            Assert.AreEqual("1", result.Users.ElementAt(0).Id);
            Assert.AreEqual("2", result.Users.ElementAt(1).Id);
            Assert.AreEqual("3", result.Users.ElementAt(2).Id);
            Assert.AreEqual(1, result.NumberOfPages);
            Assert.AreEqual(3, result.FoundItemsCount);
            Assert.AreEqual(3, result.TotalItemsCount);
        }

        [Test]
        public void GetPagedUsersList_WithRoleAsSortByAndFalseAsDescSort_ReturnsAllUsersSortedByRoleDescending()
        {
            // Arrange
            IRepository<User, string> userRepository = Substitute.For<IRepository<User, string>>();
            userRepository.Get(Arg.Any<List<Expression<Func<User, bool>>>>(), Arg.Any<Func<IQueryable<User>, IOrderedQueryable<User>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getUsers((List<Expression<Func<User, bool>>>)x[0], (Func<IQueryable<User>, IOrderedQueryable<User>>)x[1], (int)x[2], (int)x[3], (string)x[4]));
            userRepository.GetById(Arg.Any<string>()).Returns(x => users.Where(u => u.Id == (string)x[0]).Single());
            userRepository.Count(Arg.Any<Expression<Func<User, bool>>[]>()).Returns(x => usersCount((Expression<Func<User, bool>>[])x[0]));

            IRepository<Role, string> roleRepository = Substitute.For<IRepository<Role, string>>();
            roleRepository.Get(Arg.Any<List<Expression<Func<Role, bool>>>>(), Arg.Any<Func<IQueryable<Role>, IOrderedQueryable<Role>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getRoles((List<Expression<Func<Role, bool>>>)x[0], (Func<IQueryable<Role>, IOrderedQueryable<Role>>)x[1], (int)x[2], (int)x[3], (string)x[4]));

            IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.UserRepository.Returns(userRepository);
            unitOfWork.RoleRepository.Returns(roleRepository);

            UserService userService = new UserService(unitOfWork);

            // Act
            var result = userService.GetPagedUsersList(loggedInUserId: "1", sortBy: "Role", descSort: true);

            // Assert
            Assert.AreEqual(3, result.Users.Count());
            Assert.AreEqual("2", result.Users.ElementAt(0).Id);
            Assert.AreEqual("3", result.Users.ElementAt(1).Id);
            Assert.AreEqual("1", result.Users.ElementAt(2).Id);
            Assert.AreEqual(1, result.NumberOfPages);
            Assert.AreEqual(3, result.FoundItemsCount);
            Assert.AreEqual(3, result.TotalItemsCount);
        }

        [Test]
        public void GetPagedUsersList_WithLastActivityAsSortByAndFalseAsDescSort_ReturnsAllUsersSortedByLastActivity()
        {
            // Arrange
            IRepository<User, string> userRepository = Substitute.For<IRepository<User, string>>();
            userRepository.Get(Arg.Any<List<Expression<Func<User, bool>>>>(), Arg.Any<Func<IQueryable<User>, IOrderedQueryable<User>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getUsers((List<Expression<Func<User, bool>>>)x[0], (Func<IQueryable<User>, IOrderedQueryable<User>>)x[1], (int)x[2], (int)x[3], (string)x[4]));
            userRepository.GetById(Arg.Any<string>()).Returns(x => users.Where(u => u.Id == (string)x[0]).Single());
            userRepository.Count(Arg.Any<Expression<Func<User, bool>>[]>()).Returns(x => usersCount((Expression<Func<User, bool>>[])x[0]));

            IRepository<Role, string> roleRepository = Substitute.For<IRepository<Role, string>>();
            roleRepository.Get(Arg.Any<List<Expression<Func<Role, bool>>>>(), Arg.Any<Func<IQueryable<Role>, IOrderedQueryable<Role>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getRoles((List<Expression<Func<Role, bool>>>)x[0], (Func<IQueryable<Role>, IOrderedQueryable<Role>>)x[1], (int)x[2], (int)x[3], (string)x[4]));

            IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.UserRepository.Returns(userRepository);
            unitOfWork.RoleRepository.Returns(roleRepository);

            UserService userService = new UserService(unitOfWork);

            // Act
            var result = userService.GetPagedUsersList(loggedInUserId: "1", sortBy: "Last activity", descSort: false);

            // Assert
            Assert.AreEqual(3, result.Users.Count());
            Assert.AreEqual("3", result.Users.ElementAt(0).Id);
            Assert.AreEqual("2", result.Users.ElementAt(1).Id);
            Assert.AreEqual("1", result.Users.ElementAt(2).Id);
            Assert.AreEqual(1, result.NumberOfPages);
            Assert.AreEqual(3, result.FoundItemsCount);
            Assert.AreEqual(3, result.TotalItemsCount);
        }

        [Test]
        public void GetPagedUsersList_WithLastActivityAsSortByAndFalseAsDescSort_ReturnsAllUsersSortedByLastActivityDescending()
        {
            // Arrange
            IRepository<User, string> userRepository = Substitute.For<IRepository<User, string>>();
            userRepository.Get(Arg.Any<List<Expression<Func<User, bool>>>>(), Arg.Any<Func<IQueryable<User>, IOrderedQueryable<User>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getUsers((List<Expression<Func<User, bool>>>)x[0], (Func<IQueryable<User>, IOrderedQueryable<User>>)x[1], (int)x[2], (int)x[3], (string)x[4]));
            userRepository.GetById(Arg.Any<string>()).Returns(x => users.Where(u => u.Id == (string)x[0]).Single());
            userRepository.Count(Arg.Any<Expression<Func<User, bool>>[]>()).Returns(x => usersCount((Expression<Func<User, bool>>[])x[0]));

            IRepository<Role, string> roleRepository = Substitute.For<IRepository<Role, string>>();
            roleRepository.Get(Arg.Any<List<Expression<Func<Role, bool>>>>(), Arg.Any<Func<IQueryable<Role>, IOrderedQueryable<Role>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getRoles((List<Expression<Func<Role, bool>>>)x[0], (Func<IQueryable<Role>, IOrderedQueryable<Role>>)x[1], (int)x[2], (int)x[3], (string)x[4]));

            IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.UserRepository.Returns(userRepository);
            unitOfWork.RoleRepository.Returns(roleRepository);

            UserService userService = new UserService(unitOfWork);

            // Act
            var result = userService.GetPagedUsersList(loggedInUserId: "1", sortBy: "Last activity", descSort: true);

            // Assert
            Assert.AreEqual(3, result.Users.Count());
            Assert.AreEqual("1", result.Users.ElementAt(0).Id);
            Assert.AreEqual("2", result.Users.ElementAt(1).Id);
            Assert.AreEqual("3", result.Users.ElementAt(2).Id);
            Assert.AreEqual(1, result.NumberOfPages);
            Assert.AreEqual(3, result.FoundItemsCount);
            Assert.AreEqual(3, result.TotalItemsCount);
        }

        [Test]
        public void GetPagedUsersList_WithTicketsAsSortByAndFalseAsDescSort_ReturnsAllUsersSortedByTickets()
        {
            // Arrange
            IRepository<User, string> userRepository = Substitute.For<IRepository<User, string>>();
            userRepository.Get(Arg.Any<List<Expression<Func<User, bool>>>>(), Arg.Any<Func<IQueryable<User>, IOrderedQueryable<User>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getUsers((List<Expression<Func<User, bool>>>)x[0], (Func<IQueryable<User>, IOrderedQueryable<User>>)x[1], (int)x[2], (int)x[3], (string)x[4]));
            userRepository.GetById(Arg.Any<string>()).Returns(x => users.Where(u => u.Id == (string)x[0]).Single());
            userRepository.Count(Arg.Any<Expression<Func<User, bool>>[]>()).Returns(x => usersCount((Expression<Func<User, bool>>[])x[0]));

            IRepository<Role, string> roleRepository = Substitute.For<IRepository<Role, string>>();
            roleRepository.Get(Arg.Any<List<Expression<Func<Role, bool>>>>(), Arg.Any<Func<IQueryable<Role>, IOrderedQueryable<Role>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getRoles((List<Expression<Func<Role, bool>>>)x[0], (Func<IQueryable<Role>, IOrderedQueryable<Role>>)x[1], (int)x[2], (int)x[3], (string)x[4]));

            IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.UserRepository.Returns(userRepository);
            unitOfWork.RoleRepository.Returns(roleRepository);

            UserService userService = new UserService(unitOfWork);

            // Act
            var result = userService.GetPagedUsersList(loggedInUserId: "1", sortBy: "Tickets", descSort: false);

            // Assert
            Assert.AreEqual(3, result.Users.Count());
            Assert.AreEqual("2", result.Users.ElementAt(0).Id);
            Assert.AreEqual("3", result.Users.ElementAt(1).Id);
            Assert.AreEqual("1", result.Users.ElementAt(2).Id);
            Assert.AreEqual(1, result.NumberOfPages);
            Assert.AreEqual(3, result.FoundItemsCount);
            Assert.AreEqual(3, result.TotalItemsCount);
        }

        [Test]
        public void GetPagedUsersList_WithTicketsAsSortByAndFalseAsDescSort_ReturnsAllUsersSortedByTicketsDescending()
        {
            // Arrange
            IRepository<User, string> userRepository = Substitute.For<IRepository<User, string>>();
            userRepository.Get(Arg.Any<List<Expression<Func<User, bool>>>>(), Arg.Any<Func<IQueryable<User>, IOrderedQueryable<User>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getUsers((List<Expression<Func<User, bool>>>)x[0], (Func<IQueryable<User>, IOrderedQueryable<User>>)x[1], (int)x[2], (int)x[3], (string)x[4]));
            userRepository.GetById(Arg.Any<string>()).Returns(x => users.Where(u => u.Id == (string)x[0]).Single());
            userRepository.Count(Arg.Any<Expression<Func<User, bool>>[]>()).Returns(x => usersCount((Expression<Func<User, bool>>[])x[0]));

            IRepository<Role, string> roleRepository = Substitute.For<IRepository<Role, string>>();
            roleRepository.Get(Arg.Any<List<Expression<Func<Role, bool>>>>(), Arg.Any<Func<IQueryable<Role>, IOrderedQueryable<Role>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getRoles((List<Expression<Func<Role, bool>>>)x[0], (Func<IQueryable<Role>, IOrderedQueryable<Role>>)x[1], (int)x[2], (int)x[3], (string)x[4]));

            IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.UserRepository.Returns(userRepository);
            unitOfWork.RoleRepository.Returns(roleRepository);

            UserService userService = new UserService(unitOfWork);

            // Act
            var result = userService.GetPagedUsersList(loggedInUserId: "1", sortBy: "Tickets", descSort: true);

            // Assert
            Assert.AreEqual(3, result.Users.Count());
            Assert.AreEqual("1", result.Users.ElementAt(0).Id);
            Assert.AreEqual("3", result.Users.ElementAt(1).Id);
            Assert.AreEqual("2", result.Users.ElementAt(2).Id);
            Assert.AreEqual(1, result.NumberOfPages);
            Assert.AreEqual(3, result.FoundItemsCount);
            Assert.AreEqual(3, result.TotalItemsCount);
        }

        [TestCase("1", 1, 1, new string[] { "2" }, 3)]
        [TestCase("1", 2, 1, new string[] { "1" }, 3)]
        [TestCase("1", 3, 1, new string[] { "3" }, 3)]
        [TestCase("1", 4, 0, new string[] { }, 3)]
        [TestCase("2", 1, 2, new string[] { "2", "1" }, 2)]
        [TestCase("2", 2, 1, new string[] { "3" }, 2)]
        [TestCase("2", 3, 0, new string[] { }, 2)]
        [TestCase("3", 1, 3, new string[] { "2", "1", "3" }, 1)]
        [TestCase("3", 2, 0, new string[] { }, 1)]
        public void GetPagedUsersList_WithSelectedPageNumber_ReturnsUsersFromSelectedPage(string loggedInUserId, int page, int expectedUsersCount, string[] expectedUserIds, int expectedNumberOfPages)
        {
            // Arrange
            IRepository<User, string> userRepository = Substitute.For<IRepository<User, string>>();
            userRepository.Get(Arg.Any<List<Expression<Func<User, bool>>>>(), Arg.Any<Func<IQueryable<User>, IOrderedQueryable<User>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getUsers((List<Expression<Func<User, bool>>>)x[0], (Func<IQueryable<User>, IOrderedQueryable<User>>)x[1], (int)x[2], (int)x[3], (string)x[4]));
            userRepository.GetById(Arg.Any<string>()).Returns(x => users.Where(u => u.Id == (string)x[0]).Single());
            userRepository.Count(Arg.Any<Expression<Func<User, bool>>[]>()).Returns(x => usersCount((Expression<Func<User, bool>>[])x[0]));

            IRepository<Role, string> roleRepository = Substitute.For<IRepository<Role, string>>();
            roleRepository.Get(Arg.Any<List<Expression<Func<Role, bool>>>>(), Arg.Any<Func<IQueryable<Role>, IOrderedQueryable<Role>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getRoles((List<Expression<Func<Role, bool>>>)x[0], (Func<IQueryable<Role>, IOrderedQueryable<Role>>)x[1], (int)x[2], (int)x[3], (string)x[4]));

            IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.UserRepository.Returns(userRepository);
            unitOfWork.RoleRepository.Returns(roleRepository);

            UserService userService = new UserService(unitOfWork);

            // Act
            var result = userService.GetPagedUsersList(loggedInUserId: loggedInUserId, page: page);

            // Assert
            Assert.AreEqual(expectedUsersCount, result.Users.Count());
            Assert.AreEqual(expectedUserIds, result.Users.Select(u => u.Id).ToArray<string>());
            Assert.AreEqual(expectedNumberOfPages, result.NumberOfPages);
            //Assert.AreEqual(3, result.FoundItemsCount);
            Assert.AreEqual(3, result.TotalItemsCount);
        }

        public void GetPagedUsersList_WithSelectedPageNumberAndUsersPerPage_ReturnsUsersFromSelectedPage()
        {
            // Arrange
            IRepository<User, string> userRepository = Substitute.For<IRepository<User, string>>();
            userRepository.Get(Arg.Any<List<Expression<Func<User, bool>>>>(), Arg.Any<Func<IQueryable<User>, IOrderedQueryable<User>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getUsers((List<Expression<Func<User, bool>>>)x[0], (Func<IQueryable<User>, IOrderedQueryable<User>>)x[1], (int)x[2], (int)x[3], (string)x[4]));
            userRepository.GetById(Arg.Any<string>()).Returns(x => users.Where(u => u.Id == (string)x[0]).Single());
            userRepository.Count(Arg.Any<Expression<Func<User, bool>>[]>()).Returns(x => usersCount((Expression<Func<User, bool>>[])x[0]));

            IRepository<Role, string> roleRepository = Substitute.For<IRepository<Role, string>>();
            roleRepository.Get(Arg.Any<List<Expression<Func<Role, bool>>>>(), Arg.Any<Func<IQueryable<Role>, IOrderedQueryable<Role>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>()).Returns(x => getRoles((List<Expression<Func<Role, bool>>>)x[0], (Func<IQueryable<Role>, IOrderedQueryable<Role>>)x[1], (int)x[2], (int)x[3], (string)x[4]));

            IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.UserRepository.Returns(userRepository);
            unitOfWork.RoleRepository.Returns(roleRepository);

            UserService userService = new UserService(unitOfWork);

            // Act
            var result = userService.GetPagedUsersList("1", page: 2, usersPerPage: 2);

            // Assert
            Assert.AreEqual(1, result.Users.Count());
            Assert.AreEqual(new string[] { "3" }, result.Users.Select(u => u.Id).ToArray<string>());
            Assert.AreEqual(2, result.NumberOfPages);
            //Assert.AreEqual(3, result.FoundItemsCount);
            Assert.AreEqual(3, result.TotalItemsCount);
        }
    }
}
