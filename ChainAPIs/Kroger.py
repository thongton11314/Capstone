import json
import requests

# Kroger Grocery chains
#   FRED MEYER STORES, FRED, FOODSCO
#   FOOD4LESS, DILLONS, QFC, PICK N SAVE
#   SMITHS, RALPHS, QUIK STOP, OWENS, METRO MARKET
#   KROGER, JAYC, GERBES, FRY, COPPS, CITYMARKET, BAKERS

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

def getLocation(token, zipcode, radius):
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
    
def getCompany(locations):
    if (bool(locations['data'])):
        for each in locations['data']:
            if (each['chain'] in krogerGroceries):
                print('LocationID: ' + each['locationId'])
                print('Chain: ' + each['chain'])
                print('Address: ' + str(each['address']))
                print()

# main
def main():
    tokenDict = getToken()
    
    # Check if return token
    if (bool(tokenDict)):
        token = tokenDict['access_token']
        zipcode = 98117
        radius = 50
        locations = getLocation(token, zipcode, radius)
        getCompany(locations)
    else:
        print("Invalid token to access Kroger API")
main()