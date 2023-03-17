import random

count = 5

while count > 0:
    temp = input("请输入 1-10 之间的数，猜猜你输入的数字是否和随机数相等：")
    guessNumber = int(temp)
    randomNumber = random.randint(1, 10)
    if guessNumber != randomNumber:
        print(f"猜错啦~,正确数字为：{randomNumber}")
    else:
        print("恭喜你，猜对了~")
        break
    count = count - 1
    if count == 0:
        print(f"已经猜了{count}次，没机会了")
