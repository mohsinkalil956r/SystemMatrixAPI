using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZATCA.DAL.DB.Entities
{
    public class Permission : IBaseEntity
    {
        public string Name { get; set;}
        public string Description { get; set;}

        public List<SystemUser> Users { get; set; } = new();

        public int Id { get; set; }
        public bool IsActive { get; set; } = true;

    } 
}
