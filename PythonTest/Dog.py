class Dog:
    """狗"""

    def __init__(self, name):
        """构造函数，初始化狗狗名称和年龄"""
        # python中的属性不需要预先定义
        self.name = name
        # 指定属性默认值，后续可直接修改属性值
        self.age = 0

    def sit(self):
        """命令狗狗坐下"""
        print(f"{self.name}收到命令，已经坐下")

    def roll_over(self):
        """命令狗狗打滚"""
        print(f"{self.name}收到命令，开始打滚")

    def eating(self):
        print(f"{self.name}喜欢吃骨头")


class ChineseDog(Dog):
    def __init__(self, name):
        super().__init__(name)

    def eating(self):
        print(f"中华田园犬{self.name}不挑食")
