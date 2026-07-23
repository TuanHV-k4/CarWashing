using System;
using System.Collections.Generic;

namespace DataAccessLayer.Entity
{
    public class Role
    {
        public Role()
        {
            RoleID = Guid.NewGuid();
            Users = new HashSet<User>();
        }

        public Guid RoleID { get; set; }
        public string RoleName { get; set; } = null!;

        public virtual ICollection<User> Users { get; set; }
    }
}
