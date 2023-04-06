import unittest
from name_function import get_formatted_name


class NamesTestCase(unittest.TestCase):
    """测试name_function.py。"""

    # 测试方法须以 test_ 开头
    def test_first_last_name(self):
        """能够正确地处理像Janis Joplin这样的姓名吗？"""
        formatted_name = get_formatted_name('janis', 'joplin')
        self.assertEqual(formatted_name, 'Janis Joplin')

    def test_middle_name(self):
        formatted_name = get_formatted_name('zheng', 'bo', 'chu')
        self.assertEqual(formatted_name, "Zheng Chu Bo")


if __name__ == '__main__':
    unittest.main()
