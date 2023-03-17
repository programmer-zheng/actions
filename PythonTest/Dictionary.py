# Python的字典以大括号声明，键的左右可以用单引号，也可以用双引号
alien_0 = {"color": "green", "points": 5}
print(alien_0)

# 使用中括号加上键名获取字典值
if "color" in alien_0.keys():
    print(alien_0['color'])

if "points" in alien_0.keys():
    print(alien_0['points'])

if "abc" not in alien_0.keys():
    print("字典中不包含键”abc“")

# 修改字典值
alien_0["color"] = "yellow"

alien_0['x_position'] = 0
alien_0['y_position'] = 25
print(alien_0)

favorite_languages = {
    'jen': 'python',
    'sarah': 'c',
    'edward': 'ruby',
    'phil': 'python',
}

language = favorite_languages['sarah'].title()
print(f"Sarah's favorite language is {language}.")

# 遍历字典
for key, value in favorite_languages.items():
    print(f"{key} likes {value}")

# 去重，将值转换为集合
print("The following languages have been mentioned:")
for language in set(favorite_languages.values()):
    print(language.title())

# 存储所点比萨的信息。
pizza = {
    'crust': 'thick',
    'toppings': ['mushrooms', 'extra cheese'],
}

# 概述所点的比萨。
print(f"You ordered a {pizza['crust']}-crust pizza "
      "with the following toppings:")

for topping in pizza['toppings']:
    print(f"\t{topping}")
