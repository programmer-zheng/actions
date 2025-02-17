using Opc.Ua;
using Opc.Ua.Client;

namespace OpcDemo;

public class OpcUaClient
{
    private Session session;
    private ApplicationConfiguration config;

    /// <summary>
    /// 连接OPC服务器
    /// </summary>
    /// <param name="serverUrl">OPC服务器地址</param>
    public void Connect(string serverUrl)
    {
        try
        {
            // 创建应用程序配置
            config = new ApplicationConfiguration
            {
                ApplicationName = "OPC Client Demo",
                ApplicationType = ApplicationType.Client,
                SecurityConfiguration = new SecurityConfiguration
                {
                    ApplicationCertificate = new CertificateIdentifier { StoreType = "X509Store", StorePath = "CurrentUser\\My", },
                    TrustedIssuerCertificates = new CertificateTrustList { StoreType = "Directory", StorePath = "opcua\\issuer" },
                    TrustedPeerCertificates = new CertificateTrustList { StoreType = "Directory", StorePath = "opcua\\trusted" },
                    RejectedCertificateStore = new CertificateTrustList { StoreType = "Directory", StorePath = "opcua\\rejected" }
                },
                TransportConfigurations = new TransportConfigurationCollection(),
                TransportQuotas = new TransportQuotas { OperationTimeout = 1500 },
                ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = 1500 },
                TraceConfiguration = new TraceConfiguration()
            };


            config.Validate(ApplicationType.Client).Wait();

            var endpointDescription = CoreClientUtils.SelectEndpoint(config, serverUrl, false, 1500);
            var endpointConfiguration = EndpointConfiguration.Create(config);
            var endpoint = new ConfiguredEndpoint(null, endpointDescription, endpointConfiguration);

            var identity = new UserIdentity();
            // 创建会话
            session = Session.Create(config, endpoint, false, config.ApplicationName, 1500, identity, null).Result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"连接失败: {ex.Message}");
        }
    }

    public void AddSubcription(string subscriptionName, string[] subNodeIds, Action<MonitoredItem, MonitoredItemNotificationEventArgs> callback)
    {
        if (subNodeIds?.Length == 0)
        {
            throw new ArgumentNullException(nameof(subNodeIds), "订阅节点不能为空");
        }

        var subscription = new Subscription(session.DefaultSubscription)
        {
            PublishingEnabled = true,
            PublishingInterval = 0,
            KeepAliveCount = uint.MaxValue,
            LifetimeCount = uint.MaxValue,
            DisplayName = config.ApplicationName,
            Priority = 100,
            MaxNotificationsPerPublish = uint.MaxValue,
        };
        for (int i = 0; i < subNodeIds.Length; i++)
        {
            try
            {
                var monitoredItem = new MonitoredItem
                {
                    StartNodeId = new NodeId(subNodeIds[i]),
                    DisplayName = subNodeIds[i],
                    AttributeId = Attributes.Value,
                    SamplingInterval = 1000, // 设置采样间隔，单位为毫秒
                    QueueSize = 10,
                    DiscardOldest = true
                };
                monitoredItem.Notification += (item, args) => callback(item, args);
                subscription.AddItem(monitoredItem);
            }
            catch (Exception e)
            {
                Console.WriteLine($"无法为{subNodeIds[i]}添加订阅");
            }
        }

        session.AddSubscription(subscription);
        subscription.Create();

        Console.WriteLine($"{new string('=', 10)} 添加节点订阅 {new string('=', 10)}");
    }

    public List<object> BatchReadWithParentNodeId(string parentNodeId)
    {
        if (!session.Connected)
        {
            throw new MethodAccessException("会话未建立");
        }

        var parentNode = new NodeId(parentNodeId);
        var browseDescription = new BrowseDescription()
        {
            NodeId = parentNode,
            BrowseDirection = BrowseDirection.Forward,
            IncludeSubtypes = true,
            NodeClassMask = (int)NodeClass.Variable | (int)NodeClass.Object,
            ResultMask = (uint)BrowseResultMask.All
        };
        var nodesToBrowse = new BrowseDescriptionCollection { browseDescription };
        session.Browse(null, null, 0, nodesToBrowse, out var results, out var diagnosticInfos);
        ClientBase.ValidateResponse(nodesToBrowse, nodesToBrowse);
        ClientBase.ValidateDiagnosticInfos(diagnosticInfos, nodesToBrowse);
        var nodesToRead = new ReadValueIdCollection();
        foreach (var item in results)
        {
            foreach (var element in item.References)
            {
                nodesToRead.Add(new ReadValueId { NodeId = element.NodeId.ToString(), AttributeId = Attributes.Value });
            }
        }

        session.Read(null, 0, TimestampsToReturn.Both, nodesToRead, out var valueCollection, out var diagnosticInfoCollection);
        ClientBase.ValidateResponse(valueCollection, nodesToRead);
        ClientBase.ValidateDiagnosticInfos(diagnosticInfoCollection, nodesToRead);
        var list = new List<object>();
        Console.WriteLine($"{new string('=', 10)} 批量读取节点值 {new string('=', 10)}");
        for (var i = 0; i < valueCollection.Count; i++)
        {
            DataValue dataValue = valueCollection[i];
            var nodeId = nodesToRead[i].NodeId;

            // 打印节点值
            Console.WriteLine($"{dataValue.ServerTimestamp:yyyy-MM-dd HH:mm:ss} -> NodeId: {nodeId}  Value: {dataValue.Value}");
            list.Add(new
            {
                Timestamp = $"{dataValue.ServerTimestamp:yyyy-MM-dd HH:mm:ss}",
                NodeId = nodeId,
                Value = dataValue.Value,
            });
        }

        Console.WriteLine();

        return list;
    }

    /// <summary>
    /// 读取单个节点的值
    /// </summary>
    /// <param name="nodeId"></param>
    /// <returns></returns>
    public object ReadNode(string nodeId)
    {
        try
        {
            if (!session.Connected)
            {
                throw new MethodAccessException("会话未建立");
            }

            // 创建一个读取请求
            ReadValueIdCollection nodesToRead = new ReadValueIdCollection
            {
                new ReadValueId
                {
                    NodeId = NodeId.Parse(nodeId),
                    AttributeId = Attributes.Value
                }
            };

            // 读取节点的值
            DataValueCollection results;
            DiagnosticInfoCollection diagnosticInfos;
            session.Read(null, 0, TimestampsToReturn.Both, nodesToRead, out results, out diagnosticInfos);

            // 检查读取结果
            ClientBase.ValidateResponse(results, nodesToRead);
            ClientBase.ValidateDiagnosticInfos(diagnosticInfos, nodesToRead);


            if (results[0].StatusCode == StatusCodes.Good)
            {
                return results[0].Value;
            }
            else
            {
                Console.WriteLine($"读取失败: {results[0].StatusCode}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"读取错误: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 断开连接 
    /// </summary>
    public void Disconnect()
    {
        if (session != null)
        {
            session.Close();
            session.Dispose();
            Console.WriteLine("已断开连接");
        }
    }
}