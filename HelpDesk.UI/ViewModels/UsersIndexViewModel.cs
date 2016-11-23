﻿using HelpDesk.DAL.Entities;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HelpDesk.UI.ViewModels
{
    public class UsersIndexViewModel: ISortableViewModel
    {
        public string Search { get; set; }
        public string SortBy { get; set; } = "LastName";
        public bool DescSort { get; set; } = false;
        public int Page { get; set; } = 1;

        public IPagedList<User> Users { get; set; }
    }
}