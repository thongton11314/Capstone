
using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Xml.Linq;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;

using program.Data;
using program.Models;

/**
 * 
 * 
 * 
 * 
 */
namespace program.Controllers
{
    public class MainController : Controller
    {
        
        private readonly programContext _context;//
        private readonly string _gmap_api_key = "your key here";//
        private string _api_end_point = "https://k085mfa57l.execute-api.us-west-2.amazonaws.com/capstone/?zipcode={0}&radius={1}&item=%20%22{2}%22";//
        private List<OutputModel> _raw_data = new List<OutputModel>();//

        /**
         * 
         * 
         * 
         * 
         */
        public MainController(programContext context)
        {
            _context = context;
        }

        /**
         * 
         * 
         * 
         * 
         */
        public IActionResult Index()
        {
            return View();
        }

        /**
        * POST: Main/Navigate
        * To protect from overposting attacks, enable the specific properties you want to bind to.
        * For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        * 
        */
        [HttpGet]
        public IActionResult Navigate(string address, string chain)
        {
            string requestUri = string.Format("https://maps.googleapis.com/maps/api/geocode/xml?key={1}&address={0}&sensor=false", Uri.EscapeDataString(address), _gmap_api_key);
            WebRequest request = WebRequest.Create(requestUri);
            WebResponse response = request.GetResponse();
            XDocument xdoc = XDocument.Load(response.GetResponseStream());

            XElement result = xdoc.Element("GeocodeResponse").Element("result");
            XElement locationElement = result.Element("geometry").Element("location");
            XElement lat = locationElement.Element("lat");
            XElement lng = locationElement.Element("lng");

            ViewData["address"] = address;
            ViewData["store"] = chain;
            ViewData["lat"] = lat.Value;
            ViewData["long"] = lng.Value;
            ViewData["key"] = _gmap_api_key;
            return View("Navigate");
        }

        /**
         * POST: Main/Search
         * To protect from overposting attacks, enable the specific properties you want to bind to.
         * For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
         * 
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Search(UserInputModel input)
        {
            if (ModelState.IsValid)
            {
                //create api end-point url
                _api_end_point = String.Format(_api_end_point, input.ZipCode, 10, input.ItemName);

                //download JSON from api end-point
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                //get the JSON string from URL.
                HttpClient client = new HttpClient();
                HttpResponseMessage response = client.GetAsync(_api_end_point).Result;

                //deserialize JSON string & store result in a list for further processing
                _raw_data = JsonConvert.DeserializeObject<List<OutputModel>>(response.Content.ReadAsStringAsync().Result);

                //process retrieved for displaying purposes
                processData();
            }
            //display to Views/SearchResult for testing purposes
            return View("SearchResult");
        }

        /**
         * 
         * 
         * 
         * 
         */
        private void processData()
        {
            Dictionary<string, Dictionary<string, double>> cleanedData = new Dictionary<string, Dictionary<string, double>>();
            for (int store = 0; store < _raw_data.Count; store++)
            {
                string address = _raw_data[store].address["addressLine1"] + " " +
                    _raw_data[store].address["city"] + " " + _raw_data[store].address["state"];
                cleanedData[address] = new Dictionary<string, double>();

                for(int item = 0; item < _raw_data[store].items.Count; item++)
                {
                    string description = _raw_data[store].items[item].description;
                    double price = Double.IsNaN(_raw_data[store].items[item].price["promo"]) == false ? _raw_data[store].items[item].price["regular"] : -1.0;
                    cleanedData[address][description] = price;
                }
            }

            int count = 1;
            foreach (var key in cleanedData.Keys)
            {
                ViewData[count + "."] = new DisplayModel()
                {
                    storeName = _raw_data[count - 1].chain,
                    storeAddress = key,
                    itemsInfo = cleanedData[key].OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value)
                };
                count++;
            }
        }
    }
}
