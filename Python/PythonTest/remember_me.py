import json


def get_stored_username():
    """从文件中获取用户名字"""
    filename = "username.json"
    try:
        with open(filename) as file:
            username = json.load(file)
    except FileNotFoundError:
        return None
    else:
        return username


def get_new_username():
    """询问用户名字"""
    username = input(f"What's your name?")
    filename = "username.json"
    with open(filename, 'w') as file:
        json.dump(username, file)
    return username


def great_user():
    """问候用户，并指出名字"""
    username = get_stored_username()

    if username:
        print(f"Welcome back,{username}")
    else:
        username = get_new_username()
        print(f"We'll remember you,when you come back,{username}")


great_user()
