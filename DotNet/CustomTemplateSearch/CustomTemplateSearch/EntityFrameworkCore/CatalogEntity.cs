using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CustomTemplateSearch.EntityFrameworkCore;

[Table("Catalog")]
public class CatalogEntity
{
    public int Id { get; set; }

    [DisplayName("类型")]
    [Column("Type")]
    public string Type { get; set; }

    [DisplayName("品牌")]
    public string Brand { get; set; }

    [DisplayName("名称")]
    public string Name { get; set; }

    [DisplayName("描述")]
    public string Description { get; set; }

    [DisplayName("价格")]
    [Precision(18, 2)]
    public decimal Price { get; set; }
}