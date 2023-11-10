using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZATCA.DAL.DB.Entities
{
    public class Role : IdentityRole<int>, IBaseEntity
    {
        public override int Id { get; set; }
        public bool IsActive { get; set; } = true;
        public List<SystemUser> Users { get; set; } = new();

    }
}
