import json
import requests

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
#        items: [
#                   {
#                       item : 'name'
#                       price : {
#                                   'regular' : 'basic price'
#                                   'promo' : 'sale price'    
#                               }
#                       description: 'description'
#                       imange : 'url link'
#                  },
#                  {
#                   ...Second Item    
#                  }
#               ]
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
        return r.json()['access_token']
    else:
        return ""

# This function will return all near by company own by Kroger
# Some of the company is not necessary
# Condition:
#   radius must be less than 100 mile
def getGroceries(zipcode, radius):
    
    # Early exit, out of bounce
    if (radius > 100):
        return {}
    token = getToken()
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
        if (bool(nearBy)):
            return getGroceriesHelper(nearBy)
        else:
            return {}
    else:
        return {}

# This function will return all near by groceries store only. This function is invoke in getGroceries()
def getGroceriesHelper(locationNearBy):
    searchLocations = list()
    if (bool(locationNearBy['data'])):
        for each in locationNearBy['data']:
            if (each['chain'] in krogerGroceries):
                searchLocations.append(each)
    return searchLocations


# Resource: https://developer.kroger.com/reference/#operation/productGet
def getItemHelper(locationID, itemName):
    token = getToken()
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
        return r.json()['data']
    else:
        return {}

# This function will store list of item in to current address json
# This function will be invoke in getItems
# RawItems contain 0 or more items
# Resouce: https://developer.kroger.com/reference/#operation/productGet 
def buildDataHelper(rawItemsData, location):

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
            'categories' : each['categories'],
            'description' : each['description'],
            'productId' : each['productId'],
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
def getItemsInStore(locations, itemName):
    
    # Early exit, no location to search Item
    if (len(locations) < 0):
        return {}
    
    ## Location exist, starting search ##
    result = []
    for location in locations:
        rawItemsData = getItemHelper(location['locationId'], itemName)
        if (bool(rawItemsData)):
            data = buildDataHelper(rawItemsData, location)
            result.append(data)
    return result

# main
def main():    
    # Check if token valid
    zipcode = 98037
    radius = 100
    item = 'milk'
    groceries = getGroceries(zipcode, radius)

    # Check if location near by valid
    if (bool(groceries)):       
        data = getItemsInStore(groceries, item)
        print(data[0])

main()