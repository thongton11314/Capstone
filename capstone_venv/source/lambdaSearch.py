import json
from kroger import Kroger

def lambda_search(event=None, context=None):
    k = Kroger()
    items = k.search(98036, 10, 'egg')
    if (len(items) <= 0):
            return {
                'statusCode': 200,
                'headers': {'Content-Type': 'application/json'},
                'body' : json.dumps('[]')
                }
    return {
            'statusCode': 200,
            'headers': {'Content-Type': 'application/json'},
            'body': items
        }
