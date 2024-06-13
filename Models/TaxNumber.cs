using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace PersonManagement.Models
{
    public class TaxNumber
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public int PersonId { get; set; }
        [Required]
        public int CountryId { get; set; }
        [Required, StringLength(50)]
        public string Number { get; set; }
        public virtual Person Person { get; set; }
        public virtual Country Country { get; set; }
    }
}
