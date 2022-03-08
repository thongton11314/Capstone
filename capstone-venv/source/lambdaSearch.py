import json
from kroger import Kroger

# Use to create restAPI
def toRestAPI(status, body, headers=None):
    return {
        'statusCode' : status,
        'headers' : {'Content-Type': 'application/json'},
        'body' : body
    }
    
# event is JSON object that map in from query parameters
def lambda_search(event, context):
    
    # Check if contains zipcode, radius, item name
    if (len(event['queryStringParameters']) != 3):
        return toRestAPI(status=200, body=json.dumps('Must input: zipcode, radius, item'))
    
    # Mapping data
    data = {
        'zipcode' : int(event['queryStringParameters']['zipcode']),
        'radius' : int(event['queryStringParameters']['radius']),
        'item' : event['queryStringParameters']['item']
    }
    
    ##### check valid event(JSON) #####
    # Check if valid zipcode string
    if (len(str(data['zipcode'])) > 6):
        return toRestAPI(status=200, body=json.dumps('Zipcode is out of range. Only 5 digits'))
        
    # Check if valid radius
    if (int(data['radius']) > 100):
        return toRestAPI(status=200, body=json.dumps('Radius should be less than 100'))
        
    # Check if valid items string
    if (not data['item'] or data['item'].isspace()):
        return toRestAPI(status=200, body=json.dumps('Input item invalid'))

    
    ##### Start searching #####
    k = Kroger()
    items = k.search(data['zipcode'], data['radius'], data['item'])
    
    # Found Nothing
    if (len(items) <= 0):
        return toRestAPI(status=200, body=json.dumps('Item not found!'))
    
    # Found items
    return  toRestAPI(status=200, body=json.dumps(items))
