def greet_users(names):
    """向列表中的每位用户发出简单的问候。"""
    names.reverse()
    print(f"反转的列表：{names}")
    for name in names:
        msg = f"Hello, {name.title()}!"
        print(msg)


usernames = ['hannah', 'ty', 'margot']
# 传递列表副本至函数中，原列表数据不变
greet_users(usernames[:])
print(f"原列表：{usernames}")

print("\n")


# 定义可变形参，只需要在前面加星号*
# 可变形参数需要在后面
def make_pizza(size, *toppings):
    """概述要制作的比萨。"""
    print(f"\nMaking a {size}-inch pizza with the following toppings:")
    for topping in toppings:
        print(f"- {topping}")


make_pizza(16, 'pepperoni')
make_pizza(12, 'mushrooms', 'green peppers', 'extra cheese')


# 形参**user_info中的两个星号让Python创建一个名为user_info的空字典，并将收到的所有名称值对都放到这个字典中
def build_profile(first, last, **user_info):
    """创建一个字典，其中包含我们知道的有关用户的一切。"""

    user_info['first_name'] = first
    user_info['last_name'] = last
    return user_info


user_profile = build_profile('albert', 'einstein',
                             location='princeton',
                             field='physics')
print(f"\n{user_profile}")


def make_car(brand, placeOfOrigin, **car):
    car["brand"] = brand
    car["placeOfOrigin"] = placeOfOrigin
    return car


car = make_car("斯巴鲁", "中国", color="白色")
print(f"\n{car}")
