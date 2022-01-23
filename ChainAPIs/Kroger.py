import json
import requests

# Kroger Grocery chains
#   FRED MEYER STORES, FRED, FOODSCO
#   FOOD4LESS, DILLONS, QFC, PICK N SAVE
#   SMITHS, RALPHS, QUIK STOP, OWENS, METRO MARKET
#   KROGER, JAYC, GERBES, FRY, COPPS, CITYMARKET, BAKERS

# Item format return to front-end
#   {
#    location : 'address'
#    items: [
#               {
#                   item : 'name'
#                   price : {
#                               'regular' : 'basic price'
#                               'promo' : 'sale price'    
#                           }
#                   description: 'description'
#                   imange : 'url link'
#               },
#               {
#               ...Second Item    
#               }
#           ]
#   }

krogerGroceries = ['FRED MEYER STORES', 'FRED', 'FOODSCO', 
                   'FOOD4LESS', 'DILLONS', 'QFC', 'PICK N SAVE', 
                   'SMITHS', 'RALPHS', 'QUIK STOP', 'OWENS', 'METRO MARKET', 
                   'KROGER', 'JAYC', 'GERBES', 'FRY', 'COPPS', 'CITYMARKET', 'BAKERS']

def getToken():
    KrogerAuthentication = 'Basic cmVtaW5kcHJvbW90ZWRwcm9kdWN0YXBwbGljYXRpb24tMTgyZTM5NGM5OWVmZjM4YTBjODI1NjRmMjQ1NTkxMzAyNTIxMDI4NDIyNDI0Mzg1NDg0OlNmSjBsSnJXQ2FiNUFXSG9NQ2dkVldGbW5ETFVPYkluZDJnN3JIRE8='
    url = 'https://api.kroger.com/v1/connect/oauth2/token'
    headers = {'Content-Type' : 'application/x-www-form-urlencoded', 
                'Authorization' : KrogerAuthentication
    }
    data = {'grant_type': 'client_credentials', 
            'scope': 'product.compact'
    }
    r = requests.post(url, data=data, headers=headers)
    if (r.status_code >= 200 and r.status_code <= 299):
        return r.json()
    else:
        return {}

# This function will return all near by company own by Kroger
# Some of the company is not necessary
# Condition:
#   radius must be less than 100 mile
def getNearBy(token, zipcode, radius):
    
    # Early exit, out of bounce
    if (radius > 100):
        return {}
    url = 'https://api.kroger.com/v1/locations'
    authentication = 'Bearer ' + str(token)
    headers = {'Accept' : 'application/json', 
                'Authorization' : authentication
    }
    params = {
        'filter.zipCode.near' : str(zipcode),
        'filter.radiusInMiles' : str(radius),
    }
    r = requests.get(url, headers=headers,params=params)
    if (r.status_code >= 200 and r.status_code <= 299):
        return r.json()
    else:
        return {}

# This function will return all near by groceries store only
def getLocation(locationNearBy):
    searchLocations = list()
    if (bool(locationNearBy['data'])):
        for each in locationNearBy['data']:
            if (each['chain'] in krogerGroceries):
                searchLocations.append(each)
    return searchLocations

def searchHelper(token, locationID, itemName):
    url = 'https://api.kroger.com/v1/products'
    authentication = 'Bearer ' + str(token)
    headers = {'Accept' : 'application/json', 
                'Authorization' : authentication
    }
    params = {
        'filter.locationId' : str(locationID),
        'filter.term' : str(itemName),
    }
    r = requests.get(url, headers=headers,params=params)
    if (r.status_code >= 200 and r.status_code <= 299):
        return r.json()
    else:
        return {}
    
# This function will return the search item as json file from locationNearBy
def searchItem(token, locations, itemName):
    
    # Early exit, no location to search Item
    if (len(locations) < 0):
        return {}
    
    ## Location exist, starting search ##
    items = []
    for each in locations:
        rawItem = searchHelper(token, each['locationId'], itemName)
        if (bool(rawItem)):
            formatedItem = itemsToJSON(rawItem, each)
            items.append(formatedItem)
    return items

# This function will 
def itemsToJSON(item, location):
    pass

# main
def main():
    tokenDict = getToken()
    
    # Check if token valid
    if (bool(tokenDict)):
        token = tokenDict['access_token']
        zipcode = 98037
        radius = 100
        locationNearBy = getNearBy(token, zipcode, radius)
        
        # Check if location near by valid
        if (bool(locationNearBy)):
            locations = getLocation(locationNearBy)
            item = 'milk'
            listOfItems = searchItem(token, locations, item)
            print(str(len(listOfItems)))
        else:
            print("Something wrong with Zipcode/Radius")
    else:
        print("Invalid token to access Kroger API")
main()