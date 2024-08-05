namespace NPO_Excel_Operation;

public class BaseAddressInfo
{
    public string Name { get; set; }

    public List<BaseAddressInfo> Children { get; set; }

    public static List<BaseAddressInfo> GetSampleData()
    {
        return new List<BaseAddressInfo>
        {
            new BaseAddressInfo
            {
                Name = "北京市",
                Children = new List<BaseAddressInfo>
                {
                    new BaseAddressInfo
                    {
                        Name = "北京市",
                        Children = new List<BaseAddressInfo>
                        {
                            new BaseAddressInfo { Name = "东城区" },
                            new BaseAddressInfo { Name = "西城区" },
                            new BaseAddressInfo { Name = "朝阳区" },
                            new BaseAddressInfo { Name = "海淀区" }
                        }
                    }
                }
            },
            new BaseAddressInfo
            {
                Name = "上海市",
                Children = new List<BaseAddressInfo>
                {
                    new BaseAddressInfo
                    {
                        Name = "上海市",
                        Children = new List<BaseAddressInfo>
                        {
                            new BaseAddressInfo { Name = "黄浦区" },
                            new BaseAddressInfo { Name = "徐汇区" },
                            new BaseAddressInfo { Name = "长宁区" },
                            new BaseAddressInfo { Name = "静安区" }
                        }
                    }
                }
            },
            new BaseAddressInfo
            {
                Name = "广东省",
                Children = new List<BaseAddressInfo>
                {
                    new BaseAddressInfo
                    {
                        Name = "广州市",
                        Children = new List<BaseAddressInfo>
                        {
                            new BaseAddressInfo { Name = "越秀区" },
                            new BaseAddressInfo { Name = "海珠区" },
                            new BaseAddressInfo { Name = "天河区" }
                        }
                    },
                    new BaseAddressInfo
                    {
                        Name = "深圳市",
                        Children = new List<BaseAddressInfo>
                        {
                            new BaseAddressInfo { Name = "罗湖区" },
                            new BaseAddressInfo { Name = "福田区" },
                            new BaseAddressInfo { Name = "南山区" }
                        }
                    }
                }
            }
        };
    }
}