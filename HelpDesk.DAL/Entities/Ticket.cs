﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HelpDesk.DAL.Entities
{
    public class Ticket : ICloneable
    {
        public int TicketId { get; set; }
        public DateTime CreateDate { get; set; }
        public string CreatorId { get; set; }
        public string RequesterId { get; set; }
        public string AssignedUserId { get; set; }        

        [Required]
        public string Status { get; set; }
        public int? CategoryId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }
        public string Solution { get; set; }

        public virtual User Creator { get; set; }
        public virtual User Requester { get; set; }
        public virtual User AssignedUser { get; set; }
        public virtual Category Category { get; set; }

        public object Clone()
        {
            return new Ticket
            {
                TicketId = this.TicketId,
                CreateDate = this.CreateDate,
                CreatorId = this.CreatorId,
                RequesterId = this.RequesterId,
                AssignedUserId = this.AssignedUserId,                
                Status = this.Status,
                CategoryId = this.CategoryId,
                Title = this.Title,
                Content = this.Content,
                Solution = this.Solution
            };
        }

        public class TicketsComparer : IEqualityComparer<Ticket>
        {
            public bool Equals(Ticket x, Ticket y)
            {
                return x.TicketId == y.TicketId;
            }

            public int GetHashCode(Ticket obj)
            {
                return obj.TicketId;
            }
        }
    }
}