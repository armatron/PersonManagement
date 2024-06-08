using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace PersonManagement.Models
{
    public class Address
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int PersonId { get; set; }
        [Required]
        public int CountryId { get; set; }
        [Required]
        [StringLength(200)]
        public string Street { get; set; }
        [Required]
        [StringLength(200)]
        public string City { get; set; }
        [Required]
        [StringLength(20)]
        public string ZIP { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual Person Person { get; set; }
        public virtual Country Country { get; set; }
    }
}
