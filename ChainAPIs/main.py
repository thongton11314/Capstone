from Kroger import Kroger
import requests



k = Kroger()
items = k.search(98036, 10, 'egg')
print(items)
