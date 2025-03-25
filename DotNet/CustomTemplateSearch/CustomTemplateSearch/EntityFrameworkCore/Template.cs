namespace CustomTemplateSearch.EntityFrameworkCore;

[TemplateIgnore]
public class Template : BaseEntity
{
    public int Id { get; set; }


    public string TemplateName { get; set; }

    public string TableName { get; set; }
}