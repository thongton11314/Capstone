using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/**
* 
* 
* 
* 
*/
namespace program.Models
{
    public class UserInputModel
    {
        [Required]
        [StringLength(30)]
        [Display(Name = "Grocery Name")]
        public string ItemName { get; set; }

        [Required]
        [DataType(DataType.PostalCode)]
        [StringLength(5)]
        [Display(Name = "Zip Code")]
        public string ZipCode { get; set; }

        
    }
}
