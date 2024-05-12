using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class MeterReading
    {
        [Required]
        public int AccountId { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime MeterReadingDateTime { get; set; }

        [Required]
         [Range(0, 99999, ErrorMessage = "Meter reading value must be between 0 and 99999.")]
        public int MeterReadingValue { get; set; }
        
        [ForeignKey("AccountId")]
        public Account? Account { get; set; }

        
    }
}