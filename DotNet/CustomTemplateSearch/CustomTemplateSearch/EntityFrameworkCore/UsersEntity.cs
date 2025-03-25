using System.ComponentModel;

namespace CustomTemplateSearch.EntityFrameworkCore;

public class Users : BaseEntity
{
    [DisplayName("用户ID")]
    public long Id { get; set; }


    [DisplayName("用户名")]
    public string UserName { get; set; }

    [DisplayName("性别")]
    public Gender Gender { get; set; }
}