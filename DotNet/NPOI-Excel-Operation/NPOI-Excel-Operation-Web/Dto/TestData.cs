namespace NPOI_Excel_Operation_Web.Dto;

public class TestData
{
    public string OrderId { get; set; }

    public string OrderNo { get; set; }

    public decimal OrderAmount { get; set; }

    public string Currency { get; set; }

    public string OrderUserName { get; set; }

    public DateTime CreateTime { get; set; }

    public int Number { get; set; }
	
    public Dictionary<string, string> Dic { get; set; }
}