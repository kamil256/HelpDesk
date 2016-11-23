using Microsoft.AspNet.Identity.EntityFramework;

namespace HelpDesk.DAL.Entities
{
    public class Role : IdentityRole
    {
        public Role() : base() { }
        public Role(string name) : base(name) { }
    }
}