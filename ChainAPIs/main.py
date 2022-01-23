from ChainAPIs.Kroger import Kroger

k = Kroger()
items = k.search(98036, 10, 'milk')
print(items)