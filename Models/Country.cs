using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace PersonManagement.Models
{
    public class Country
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required, StringLength(200)]
        public string Name { get; set; }
        public virtual ICollection<Address> Address { get; set; }
        public virtual ICollection<TaxNumber> TaxNumber { get; set; }
    }
}
