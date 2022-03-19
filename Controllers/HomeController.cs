using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Xml.Linq;
using Newtonsoft.Json;
using System.Web.Mvc;

using CapstoneApp.Models;

/**
 * Capstone Project 2022
 * Part of the Microsoft ASP.NET Framework MVC 5
 * This class deals with Home services such as getting user input and displaying results (front-end)
 */
namespace CapstoneApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly string _gmap_api_key = System.Web.Configuration.WebConfigurationManager.AppSettings["gmapsKey"];//for Google map services
        private string _api_end_point = System.Web.Configuration.WebConfigurationManager.AppSettings["apiEndpoint"];//for grocery retrieving task
        private List<OutputModel> _raw_data;//for retrieved data for system/internal processing

        //
        //GET: /Home/Index
        //default/home view when user accesses the site
        public ActionResult Index()
        {
            return View();
        }

        //
        //GET: /Home/About
        public ActionResult About()
        {
            return View();
        }

        /**
        * POST: /Main/Search
        * To protect from overposting attacks, enable the specific properties you want to bind to.
        * For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Search(UserInputViewModel input)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("Index");
                }

                //connect and retrieve data from API
                ConnectAPI(input.ZipCode, 10, input.ItemName);

                //at this point, if no data is available, throw some error
                if (_raw_data == null)
                {
                    return View("Error");
                }

                //transfer data to Views/SearchResult
                ViewBag.storesList = ProcessData();

                //display results to Views/SearchResult
                return View("SearchResult");
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        //
        //GET: /Main/Navigate
        [HttpGet]
        public ActionResult Navigate(string address, string chain)
        {
            try
            {
                string requestUri = string.Format("https://maps.googleapis.com/maps/api/geocode/xml?key={1}&address={0}&sensor=false", Uri.EscapeDataString(address), _gmap_api_key);
                WebRequest request = WebRequest.Create(requestUri);
                WebResponse response = request.GetResponse();
                XDocument xdoc = XDocument.Load(response.GetResponseStream());

                XElement result = xdoc.Element("GeocodeResponse").Element("result");
                XElement locationElement = result.Element("geometry").Element("location");
                XElement lat = locationElement.Element("lat");
                XElement lng = locationElement.Element("lng");

                ViewBag.address = address;
                ViewBag.store = chain;
                ViewBag.lat = lat.Value;
                ViewBag.lng = lng.Value;
                ViewBag.key = _gmap_api_key;
                return View("Navigate");
            }
            catch(Exception)
            {
                return View("Error");
            }
        }

        //
        //for internal calls and processing only.
        //use to retrieve item(s) info from logged in user's grocery list.
        //info include cheapest price, and it's store.
        //return a dictionary of all stores, each store has a dictionary of all items
        //in user's grocery list, each item has item name: item price
        [HttpPost]
        [ValidateAntiForgeryToken]
        public Dictionary<string, Dictionary<string, double>> GetItemsInfo(string zipCode, IList<string> items)
        {
            try
            {
                //output result
                Dictionary<string, Dictionary<string, double>> output = new Dictionary<string, Dictionary<string, double>>();
                int count = 0;
                IList<DisplayViewModel> list;
                KeyValuePair<string, double> cheapest;
                foreach (var item in items)
                {
                    count++;
                    //connect and retrieve data from API
                    ConnectAPI(zipCode, 10, item);

                    //further data processing before storing resuls in output
                    list = ProcessData();
                    foreach (var store in list)
                    {
                        string storeNameAndAddress = store.storeName + "(" + store.storeAddress + ")";
                        if (!output.ContainsKey(storeNameAndAddress))
                        {
                            output.Add(storeNameAndAddress, new Dictionary<string, double>());
                        }
                        cheapest = store.itemsInfo.First();
                        //cheapest.Key is cheapest item name /or description, value is cheapest item's price
                        output[storeNameAndAddress][cheapest.Key] = cheapest.Value;
                    }
                }
                return output;
            }
            catch (Exception)
            {
                return null;
            }
        }

        //
        //for internal data processing 
        private void ConnectAPI(string zipCode, int radius, string itemName)
        {
            try
            {
                //create api end-point url
                string url = String.Format(_api_end_point, zipCode, radius, itemName);

                //some security stuff
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                //get the JSON string from URL.
                HttpClient client = new HttpClient();
                HttpResponseMessage response = client.GetAsync(url).Result;

                //deserialize JSON string & store result in a list for further processing
                _raw_data = JsonConvert.DeserializeObject<List<OutputModel>>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception)
            {
                return;
            }
        }

        //
        //for internal data processing 
        private IList<DisplayViewModel> ProcessData()
        {
            try
            {
                Dictionary<string, Dictionary<string, double>> cleanedData = new Dictionary<string, Dictionary<string, double>>();
                for (int store = 0; store < _raw_data.Count; store++)
                {
                    string address = _raw_data[store].address["addressLine1"] + " " +
                        _raw_data[store].address["city"] + " " + _raw_data[store].address["state"];
                    cleanedData[address] = new Dictionary<string, double>();

                    for (int item = 0; item < _raw_data[store].items.Count; item++)
                    {
                        string description = _raw_data[store].items[item].description;
                        double price = Double.IsNaN(_raw_data[store].items[item].price["promo"]) == false ? _raw_data[store].items[item].price["regular"] : -1.0;
                        cleanedData[address][description] = price; // {address : {description: price}}
                    }
                }

                int count = 1;
                IList<DisplayViewModel> list = new List<DisplayViewModel>();
                foreach (var key in cleanedData.Keys)
                {
                    list.Add(new DisplayViewModel()
                    {
                        storeName = _raw_data[count - 1].chain,
                        storeAddress = key,
                        itemsInfo = cleanedData[key].OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value)
                    });
                    count++;
                }

                return list;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
