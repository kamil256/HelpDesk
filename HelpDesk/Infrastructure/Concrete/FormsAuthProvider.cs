using HelpDesk.DAL;
using HelpDesk.Infrastructure.Abstract;
using HelpDesk.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using static HelpDesk.Infrastructure.Utilities;

namespace HelpDesk.Infrastructure.Concrete
{
    public class FormsAuthProvider : IAuthProvider
    {
        private IUnitOfWork unitOfWork;

        public FormsAuthProvider(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public bool Authenticate(string email, string password)
        {
            User user = unitOfWork.UserRepository.GetAll(filter: u => u.Email.ToLower() == email.ToLower()).SingleOrDefault();
            if (user != null && HashPassword(password, user.Salt) == user.HashedPassword)
            {
                FormsAuthentication.SetAuthCookie(user.Email, false);
                return true;
            }
            return false;
        }

        public void LogOut()
        {
            FormsAuthentication.SignOut();
        }
    }
}