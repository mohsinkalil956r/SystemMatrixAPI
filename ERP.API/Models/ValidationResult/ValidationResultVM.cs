using ZATCA.API.Models.ErrorMessage;
using ZATCA.API.Models.InfoMessage;
using ZATCA.API.Models.WarnngMessage;
using ZATCA.DAL.DB.Entities;

namespace ZATCA.API.Models.ValidationResult
{
    public class ValidationResultVM
    {

        public int Id { get; set; }
        public bool IsActive { get; set; } = true;

       // public int InfoMessageId { get; set; }

       // public int WarnngMessageId { get; set; }
       // public int ErrorMessageId { get; set; }

    }
}
