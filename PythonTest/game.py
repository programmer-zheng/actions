import random

count = countNumber = 5

randomNumber = random.randint(1, 10)
while count > 0:
    temp = input("请输入 1-10 之间的数，猜猜你输入的数字是否和随机数相等：")
    guessNumber = int(temp)
    if guessNumber == randomNumber:
        print("恭喜你，猜对了~")
        break
    else:
        if guessNumber > randomNumber:
            print("大啦")
        else:
            print("小了")
    count = count - 1
    if count == 0:
        print(f"已经猜了{countNumber}次，没机会了，正确答案为：{randomNumber}")
