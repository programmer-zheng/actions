from dog import Dog, ChineseDog as CDog

# 使用as，将导入的类起个别名，使用时直接使用别名
# import dog

my_dog = Dog("球球")
# 直接修改属性值
my_dog.age = 3
print(f"狗狗名字叫{my_dog.name},已经{my_dog.age}岁了")
my_dog.sit()
my_dog.roll_over()
my_dog.eating()

chinese_dog = CDog("黑子")
chinese_dog.age = 4
chinese_dog.eating()

