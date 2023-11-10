using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZATCA.DAL.DB.Entities
{
    public class OnBoardingCSID : IBaseEntity
    {
        public int Id { get; set; }
        public bool IsActive { get; set; } = true;

        public int CompanyId { get; set; }
        [NotMapped]
        public Company Company { get; set; }
        [NotMapped]
        public string OTP { get; set; }

        public string UserName { get; set; }
        public string Secret { get; set; }
        public DateTime StartedDate { get; set; }
        public DateTime ExpiredDate { get; set;}


    }
}
