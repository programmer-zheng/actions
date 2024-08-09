namespace NPOI_Excel_Operation_Web.Dto;

public class DynamicGroupPropertyOperationDto
{
    public DynamicGroupPropertyOperationDto(DynamicGroupLinqOperatorEnum operatorEnum, string propertyName) : this(operatorEnum, propertyName, null)
    {

    }
    public DynamicGroupPropertyOperationDto(DynamicGroupLinqOperatorEnum operatorEnum, string propertyName, string distinctPropertyName)
    {
        PropertyName = propertyName;
        Operate = operatorEnum;
        DistinctByPropertyName = distinctPropertyName;
    }
    /// <summary>
    /// 属性名称
    /// </summary>
    public string PropertyName { get; set; }

    /// <summary>
    /// 操作
    /// </summary>
    public DynamicGroupLinqOperatorEnum Operate { get; set; }

    /// <summary>
    /// 去重属性名称
    /// </summary>
    public string DistinctByPropertyName { get; set; }
}