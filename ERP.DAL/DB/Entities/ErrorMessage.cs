using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZATCA.DAL.DB.Entities
{
    public class ErrorMessage:IBaseEntity
    {
        public int Id { get; set; }
        public bool IsActive { get; set; } = true;
        public string Type { get; set; }

        public string Code { get; set; }

        public string Category { get; set; }

        public string Message { get; set; }

        public string Status { get; set; }
    }
}
