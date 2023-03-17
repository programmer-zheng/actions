cars = ['bmw', 'audi', 'toyota', 'subaru']

print("原始列表:")
print(cars)

sortedCars = sorted(cars)
print("\n排序后的列表:")
print(sortedCars)

print("\n循环列表并对首字母大写：")
for car in sortedCars:
    if car == "bmw":
        print(car.upper())
    else:
        print(car.title())

sortedCars.reverse()
print("\n列表反转:")
print(sortedCars)

print("\n列表长度:", len(sortedCars))

print("\n列表中最后一个元素:", sortedCars[-1])

print("\n生成数字列表：")
# 默认从0开始
numbers = list(range(6))
print(numbers)

print("\n生成数字列表2：")
# 指定起始数字
numbers = list(range(1, 6))
print(numbers)

print("\n步进式生成数字列表")
# 从2开始，到11结束（不包含11），每次步进3
even_numbers = list(range(2, 11, 3))
print(even_numbers)

print("\n计算平方")
squares = []
for value in even_numbers:
    squares.append(value ** 2)

print(squares)
print("\n数字总和")
print(sum(squares))
print(sum(squares))

print("\n最小数")
print(min(squares))

print("\n最大数")
print(max(squares))

print("\n简洁语法生成数字列表: [value**2 for value in range(2, 11, 3)]")
squares = [value ** 2 for value in range(2, 11, 3)]
print(squares)
