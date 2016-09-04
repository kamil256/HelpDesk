using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HelpDesk.Infrastructure.Abstract
{
    public interface IAuthProvider
    {
        bool Authenticate(string username, string password);
        void LogOut();
    }
}