using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZATCA.DAL.DB.Entities
{
    public class UserRole: IdentityUserRole<int>, IBaseEntity
    {
        public SystemUser User { get; set; }
        public Role Role { get; set; }

        [NotMapped]
        public int Id { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
