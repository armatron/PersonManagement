using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace PersonManagement.Models
{
    public class Person
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(20)]
        public string IdentityNumber { get; set;  }
        [Required]
        [StringLength(200)]
        public string FirstName { get; set; }
        [Required]
        [StringLength(200)]
        public string LastName { get; set; }
        [StringLength(200)]
        public string? Email { get; set; }
        [StringLength(200)]
        public string? Phone { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual ICollection<Address> Address { get; set; }
        public virtual ICollection<TaxNumber> TaxNumber { get; set; }
    }
}
