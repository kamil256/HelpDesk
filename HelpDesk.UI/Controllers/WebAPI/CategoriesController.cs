using HelpDesk.DAL.Abstract;
using HelpDesk.DAL.Concrete;
using HelpDesk.UI.ViewModels.Categories;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace HelpDesk.UI.Controllers.WebAPI
{
    [Authorize(Roles = "Admin")]
    public class CategoriesController : ApiController
    {
        private IUnitOfWork unitOfWork;

        public CategoriesController()
        {
            this.unitOfWork = new UnitOfWork();
        }

        [HttpGet]
        [OverrideAuthorization]
        [Authorize]
        public IEnumerable<CategoryDTO> GetCategories()
        {
            return unitOfWork.CategoryRepository.Get(orderBy: q => q.OrderBy(c => c.Order)).Select(c => new CategoryDTO
            {
                CategoryId = c.CategoryId,
                Name = c.Name
            });
        }
    }
}
