with open('README.md', encoding='utf-8') as file_object:
    contents = file_object.readlines()

print("\n读取文件内容")
print(f"\n行数：{len(contents)}")
for line in contents:
    print(line.strip())

with open('programming.txt', mode='w', encoding='utf-8') as file_object:
    for number in list(range(0, 300)):
        number += 1
        if number == 1:
            file_object.write("开始循环写入数据")
        file_object.write(f"\n{str(number)}")
