using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CapstoneApp.Models
{
    public class UserInputViewModel
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

    public class DisplayViewModel
    {
        public Dictionary<string, double> itemsInfo { get; set; }
        public string storeAddress { get; set; }
        public string storeName { get; set; }

    }

    public class OutputModel
    {
        public string chain { get; set; }
        public string locationId { get; set; }
        public Dictionary<string, string> address { get; set; }
        public IList<ItemsModel> items { get; set; }
    }

    public class ItemsModel
    {
        public string productId { get; set; }
        public string[] categories { get; set; }
        public string description { get; set; }
        public string itemId { get; set; }
        public Dictionary<string, double> price { get; set; }
    }
}
