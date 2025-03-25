using System.ComponentModel;

namespace CustomTemplateSearch.EntityFrameworkCore;

public enum Gender
{
    [Description("男")]
    Male,

    [Description("女")]
    Female
}