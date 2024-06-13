using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace PersonManagement.Models
{
    public enum Gender
    {
        Male = 1,
        Female = 2
    }
    public class Person
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required, StringLength(20)]
        public string IdentityNumber { get; set;  }
        public Gender? Gender { get; set; }
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
        [StringLength(500), DataType(DataType.MultilineText)]
        public string? Description { get; set; }
        public virtual ICollection<Address> Address { get; set; }
        public virtual ICollection<TaxNumber> TaxNumber { get; set; }
       
    }
}
