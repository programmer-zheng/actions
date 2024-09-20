using System;

namespace DomainManageTool;

public class DomainRecord
{
    public string RecordName { get; set; }

    public string Type { get; set; }

    public string Line { get; set; }

    public string RecordValue { get; set; }

    public int TTL { get; set; }

    public string Comment { get; set; }

    public DateTime? LastModifyTime { get; set; }
}