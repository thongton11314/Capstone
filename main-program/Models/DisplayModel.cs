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
    public class DisplayModel
    {
        public Dictionary<string, double> itemsInfo { get; set; }
        public string storeAddress { get; set; }
        public string storeName { get; set; }

    }
}
