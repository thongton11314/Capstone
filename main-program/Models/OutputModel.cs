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
    public class OutputModel
    {
        public string chain { get; set; }
        public string locationId { get; set; } 
        public Dictionary<string, string> address { get; set; }
        public IList<ItemsModel> items { get; set; }
    }
}
