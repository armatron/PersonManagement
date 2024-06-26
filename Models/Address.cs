﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace PersonManagement.Models
{
    public class Address
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required(ErrorMessage = "The Person field is required.")]
        public int PersonId { get; set; }
        [Required(ErrorMessage = "The Country field is required.")]
        public int CountryId { get; set; }
        [Required, StringLength(200)]
        public string Street { get; set; }
        [Required, StringLength(200)]
        public string City { get; set; }
        [Required, StringLength(20)]
        public string ZIP { get; set; }
        public virtual Person Person { get; set; }
        public virtual Country Country { get; set; }
    }
}
