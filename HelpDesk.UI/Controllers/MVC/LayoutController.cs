using HelpDesk.UI.ViewModels.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

namespace HelpDesk.UI.Controllers.MVC
{
    public class LayoutController : Controller
    {
        [ChildActionOnly]
        public PartialViewResult Footer()
        {
            XDocument xml;
            try
            {
                xml = XDocument.Load(Server.MapPath("~/App_Data/Settings.xml"));
            }
            catch
            {
                xml = null;
            }

            XElement settings = xml != null ? xml.Element("Settings") : null;
            XElement companyInfo = settings != null ? settings.Element("CompanyInfo") : null;
            XElement address = companyInfo != null ? companyInfo.Element("Address") : null;
            XElement phone = companyInfo != null ? companyInfo.Element("Phone") : null;
            XElement email = companyInfo != null ? companyInfo.Element("Email") : null;

            CompanyInfoViewModel model = new CompanyInfoViewModel
            {
                Address = address != null ? address.Value : "not configured",
                Phone = phone != null ? phone.Value : "not configured",
                Email = email != null ? email.Value : "not configured"
            };
            return PartialView("_Footer", model);
        }
    }
}