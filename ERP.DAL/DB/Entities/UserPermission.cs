using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZATCA.DAL.DB.Entities
{
    public class UsZATCAermission : IBaseEntity
    {
        [NotMapped]
        public int Id { get; set; }

        public bool IsActive { get; set; } = true;

        public int UserId { get; set; }

        public int PermissionId { get; set; }


        [ForeignKey("UserId")]
        public SystemUser User { get; set; }

    }
}
