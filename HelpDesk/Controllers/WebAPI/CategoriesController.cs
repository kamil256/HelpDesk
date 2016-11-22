using HelpDesk.DAL.Abstract;
using HelpDesk.DAL.Concrete;
using HelpDesk.Models.Categories;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace HelpDesk.Controllers.WebAPI
{
    public class CategoriesController : ApiController
    {
        private IUnitOfWork unitOfWork;

        public CategoriesController()
        {
            unitOfWork = new UnitOfWork();
        }

        [OverrideAuthorization]
        [HttpGet]
        public IEnumerable<CategoryDTO> GetCategories()
        {
            return unitOfWork.CategoryRepository.Get(filters: null, orderBy: o => o.OrderBy(c => c.Order)).Select(c => new CategoryDTO { CategoryId = c.CategoryID, Name = c.Name });
        }
    }
}
