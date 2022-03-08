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
    public class ItemsModel
    {
        public string productId { get; set; }
        public string[] categories { get; set; }
        public string description { get; set; }
        public string itemId { get; set; }
        public Dictionary<string, double> price { get; set; }
    }
}
