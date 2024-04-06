using SqlSugar;

namespace SqlSugarDemo.Models;

[SugarTable("t_student")] //当和数据库名称不一样可以设置表别名 指定表明
public class Student
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    [SugarColumn(ColumnName = "StudentName")]
    public string Name { get; set; }

    [Navigate(NavigateType.OneToMany, nameof(Book.SID), nameof(Id))]
    public List<Book> Books { get; set; }
}

public class Book
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    /// <summary>
    /// 学生ID
    /// </summary>
    public int SID { get; set; }

    public string BookName { get; set; }
}