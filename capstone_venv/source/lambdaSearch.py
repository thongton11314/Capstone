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
    
    ##### check valid context #####
    if (len(event) != 3):
        return toRestAPI(status=200, body=json.dumps('Invalid parameters. Exactly 3 parameters'));
    
    # Valid context
    else:
        
        # Check if valid zipcode string
        if (len(str(event['zipcode'])) > 6):
            return toRestAPI(status=200, body=json.dumps('Zipcode is out of range. Only 5 digits'));
        
        # Check if valid radius
        if (event['radius'] > 100):
            return toRestAPI(status=200, body=json.dumps('Radius should be less than 100'));
        
        # Check if valid items string
        if (not event['item'] or event['item'].isspace()):
            return toRestAPI(status=200, body=json.dumps('Input item invalid'));
    
    ##### Start searching #####
    k = Kroger()
    items = k.search(event['zipcode'], event['radius'], event['item'])
    
    # Found Nothing
    if (len(items) <= 0):
        return toRestAPI(status=200, body=json.dumps('[]'))
    
    # Found items
    return  toRestAPI(status=200, body=items)
