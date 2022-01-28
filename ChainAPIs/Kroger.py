import json
from Libs.requests import requests

# Kroger Grocery chains
#   FRED MEYER STORES, FRED, FOODSCO
#   FOOD4LESS, DILLONS, QFC, PICK N SAVE
#   SMITHS, RALPHS, QUIK STOP, OWENS, METRO MARKET
#   KROGER, JAYC, GERBES, FRY, COPPS, CITYMARKET, BAKERS

# Item format return to front-end
#   {
#        'locationId' : 'locationId'
#        'address' : {'addressLine1', 
#                    'city', 
#                    'state', 
#                    'zipcode', 
#                    'county'
#                }
#        'chain' :  'Name'
#        'items' : [
#                   {
#                       'productId' : 'productId',
#                       'categories' : 'categories',
#                       'description' : 'description',
#                       'itemId' : 'itemID',
#                       'price' : {
#                           'regular' : 'regular'
#                           'promo' : 'promo'
#                       }
#                   }
#                   {
#                   ...Second Item    
#                   }
#               ]
#   }
class Kroger:
    def __init__(self):
        self.__krogerGroceries = ['FRED MEYER STORES', 'FRED', 'FOODSCO', 
                    'FOOD4LESS', 'DILLONS', 'QFC', 'PICK N SAVE', 
                    'SMITHS', 'RALPHS', 'QUIK STOP', 'OWENS', 'METRO MARKET', 
                    'KROGER', 'JAYC', 'GERBES', 'FRY', 'COPPS', 'CITYMARKET', 'BAKERS']

    def __getToken(self):
        KrogerAuthentication = 'Basic cmVtaW5kcHJvbW90ZWRwcm9kdWN0YXBwbGljYXRpb24tMTgyZTM5NGM5OWVmZjM4YTBjODI1NjRmMjQ1NTkxMzAyNTIxMDI4NDIyNDI0Mzg1NDg0OlNmSjBsSnJXQ2FiNUFXSG9NQ2dkVldGbW5ETFVPYkluZDJnN3JIRE8='
        url = 'https://api.kroger.com/v1/connect/oauth2/token'
        headers = {'Content-Type' : 'application/x-www-form-urlencoded', 
                    'Authorization' : KrogerAuthentication
        }
        data = {'grant_type': 'client_credentials', 
                'scope': 'product.compact'
        }
        r = requests.post(url=url, data=data, headers=headers)
        if (r.status_code >= 200 and r.status_code <= 299):
            return r.json()['access_token']
        else:
            return ""

    # This function will return all near by company own by Kroger
    # Some of the company is not necessary
    # Condition:
    #   radius must be less than 100 mile
    def __getGroceries(self, zipcode, radius):
        
        # Early exit, out of bounce
        if (radius > 100):
            return {}
        token = self.__getToken()
        url = 'https://api.kroger.com/v1/locations'
        authentication = 'Bearer ' + token
        headers = {'Accept' : 'application/json', 
                    'Authorization' : authentication
        }
        params = {
            'filter.zipCode.near' : str(zipcode),
            'filter.radiusInMiles' : str(radius),
        }
        r = requests.get(url, headers=headers,params=params)
        if (r.status_code >= 200 and r.status_code <= 299):
            nearBy = r.json()
            if (bool(nearBy)):                              # check if json file
                if ('data' in nearBy):                      # check if key('data') exist
                    return self.__getGroceriesHelper(nearBy)  # check
        else:
            return {}

    # This function will return all near by groceries store only. This function is invoke in getGroceries()
    def __getGroceriesHelper(self, locationNearBy):
        searchLocations = []
        if (bool(locationNearBy['data'])):
            for each in locationNearBy['data']:
                if (each['chain'] in self.__krogerGroceries):
                    searchLocations.append(each)
        return searchLocations


    # Resource: https://developer.kroger.com/reference/#operation/productGet
    def __getItemHelper(self, locationID, itemName):
        token = self.__getToken()
        url = 'https://api.kroger.com/v1/products'
        authentication = 'Bearer ' + token
        headers = {'Accept' : 'application/json', 
                    'Authorization' : authentication
        }
        params = {
            'filter.locationId' : str(locationID),
            'filter.term' : str(itemName),
        }
        r = requests.get(url, headers=headers,params=params)
        if (r.status_code >= 200 and r.status_code <= 299):
            itemData = r.json()
            if (bool(itemData)):            # check if json file
                if ('data' in itemData):    # check if key('data') exist
                    return itemData['data']
        else:
            return {}

    # This function will store list of item in to current address json
    # This function will be invoke in getItems
    # RawItems contain 0 or more items
    # Resouce: https://developer.kroger.com/reference/#operation/productGet 
    def __buildDataHelper(self, rawItemsData, location):
        listOfItems = []
        temp = ''
        for each in rawItemsData:
            itemID = ''
            itemPrice = {}
            for itemDetail in each['items']:
                itemID = itemDetail['itemId']
                itemPrice = {
                    "regular": itemDetail['price']['regular'],
                    "promo": itemDetail['price']['promo']
                }
            temp = {
                'productId' : each['productId'],
                'categories' : each['categories'],
                'description' : each['description'],
                'itemId' : itemID,
                'price' : itemPrice
            }
            listOfItems.append(temp)

        data = {
            'locationId' : location['locationId'],
            'address' : location['address'],
            'chain' : location['chain'],
            'items' : listOfItems
        }
        return data
    
    # This function will return the search item as json file from locationNearBy
    def __getItemsInStore(self, locations, itemName):
        
        # Early exit, no location to search Item
        if (len(locations) < 0):
            return {}
        
        ## Location exist, starting search ##
        result = []
        for location in locations:
            rawItemsData = self.__getItemHelper(location['locationId'], itemName)
            if (bool(rawItemsData)):
                data = self.__buildDataHelper(rawItemsData=rawItemsData, location=location)
                result.append(data)
        return result

    def search(self, zipcode, radius, item):    
        groceries = self.__getGroceries(zipcode=zipcode, radius=radius)

        # Check if location near by valid
        if (bool(groceries)):       
            data = self.__getItemsInStore(locations=groceries, itemName=item)
            return data
        else:
            return []

