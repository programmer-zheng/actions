namespace CustomTemplateSearch.EntityFrameworkCore;

public abstract class BaseEntity
{
    public bool IsDeleted { get; set; }

    public DateTime? DeletetionTime { get; set; }

    public DateTime CreationTime { get; set; } = DateTime.Now;
}