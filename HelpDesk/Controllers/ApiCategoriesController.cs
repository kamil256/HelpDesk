﻿using HelpDesk.DAL;
using HelpDesk.Entities;
using HelpDesk.Models.Categories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace HelpDesk.Controllers
{
    public class ApiCategoriesController : ApiController
    {
        private IUnitOfWork unitOfWork;
        private HelpDeskContext context;

        public ApiCategoriesController()
        {
            unitOfWork = new UnitOfWork();
            context = new HelpDeskContext();
        }

        [OverrideAuthorization]
        [HttpGet]
        public IEnumerable<CategoryDTO> GetCategories()
        {
            return unitOfWork.CategoryRepository.GetAll(filters: null, orderBy: o => o.OrderBy(c => c.Order)).Select(c => new CategoryDTO { CategoryId = c.CategoryID, Name = c.Name });
        }
    }
}