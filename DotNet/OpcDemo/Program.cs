namespace OpcDemo;

class Program
{
    static void Main(string[] args)
    {
        // 测试从 OPC UA Simulation Server 读取数据
        var client = new OpcUaClient();
        var serverUrl = "opc.tcp://127.0.0.1:53530/OPCUA/SimulationServer";
        serverUrl = "opc.tcp://192.168.188.35:53530/OPCUA/SimulationServer";
        client.Connect(serverUrl);

        // 读取节点（示例节点ID，需替换为实际节点）
        var value = client.ReadNode("ns=3;i=1002");
        Console.WriteLine($"读取到的值: {value}");

        // 批量读取节点
        var parentNodeId = "ns=3;s=85/0:Simulation";
        var list = client.BatchReadWithParentNodeId(parentNodeId);

        var subscriptionNodes = new string[]
        {
            "ns=3;i=1001",
            "ns=3;i=1005"
        };
        client.AddSubcription("订阅", subscriptionNodes, (monitoredItem, e) =>
        {
            foreach (var notification in monitoredItem.DequeueValues())
            {
                Console.WriteLine($"{notification.ServerTimestamp:yyyy-MM-dd HHmm:ss} 节点 {monitoredItem.DisplayName} 的值更新为: {notification.Value}");
            }
        });

        Console.ReadKey();

        client.Disconnect();
    }
}