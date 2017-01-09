using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HelpDesk.UI.Infrastructure.Abstract
{
    public interface IEmailSender
    {
        void SendEmail(string receiverEmail, string subject, string content);
    }
}