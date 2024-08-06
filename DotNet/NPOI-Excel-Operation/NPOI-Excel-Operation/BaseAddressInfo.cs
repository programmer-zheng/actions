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
                Name = "江苏省",
                Children = new List<BaseAddressInfo>
                {
                    new BaseAddressInfo
                    {
                        Name = "南京市",
                        Children = new List<BaseAddressInfo>
                        {
                            new BaseAddressInfo { Name = "浦口区" },
                            new BaseAddressInfo { Name = "建邺区" },
                            new BaseAddressInfo { Name = "鼓楼区" },
                        }
                    },
                    new BaseAddressInfo
                    {
                    Name = "苏州市",
                    Children = new List<BaseAddressInfo>
                    {
                        new BaseAddressInfo { Name = "姑苏区" },
                        new BaseAddressInfo { Name = "吴中区" },
                        new BaseAddressInfo { Name = "虎丘区" },
                    }
                }
                }
            },
            new BaseAddressInfo
            {
                Name = "安徽省",
                Children = new List<BaseAddressInfo>
                {
                    new BaseAddressInfo
                    {
                        Name = "合肥市",
                        Children = new List<BaseAddressInfo>
                        {
                            new BaseAddressInfo { Name = "肥东县" },
                            new BaseAddressInfo { Name = "肥西县" },
                        }
                    },
                    new BaseAddressInfo
                    {
                        Name = "宿州市",
                        Children = new List<BaseAddressInfo>
                        {
                            new BaseAddressInfo { Name = "埇桥区" },
                            new BaseAddressInfo { Name = "砀山县" },
                            new BaseAddressInfo { Name = "萧县" },
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