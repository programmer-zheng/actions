document.addEventListener("DOMContentLoaded", function () {
    window.pointData = null;
    window.stationData = null;
    let eventSource = null;

    function renderData(parentDivId, data) {
        var parentDiv = document.getElementById(parentDivId);
        parentDiv.innerHTML = ''; // 清空之前的内容
        data.forEach(item => {
            var newElement = document.createElement("div");
            newElement.innerHTML = "数据: " + JSON.stringify(item); // 假设数据有 Data 属性
            parentDiv.appendChild(newElement);
        });
    }

    function initEventSource() {
        if (eventSource) {
            eventSource.close();
        }

        if (typeof (EventSource) !== "undefined") {
            eventSource = new EventSource("/api/monitor/server-send-event");
            
            // 连接建立时的处理
            eventSource.onopen = function(event) {
                console.log("SSE连接已建立");
            };
            
            // 接收消息的处理
            eventSource.onmessage = function (event) {
                console.log("收到SSE消息:", event.data);
                try {
                    var data = JSON.parse(event.data);
                    
                    // 处理连接成功消息
                    if (data.type === "connected") {
                        console.log("SSE连接已确认");
                        return;
                    }
                    
                    var parentDivId = '';
                    var dataArray = null;
                    var receivedDataArray = null;
                    
                    if (data.data) {
                        receivedDataArray = Array.isArray(data.data) ? data.data : [data.data];
                    }
                    
                    if (data.key === 'Point') {
                        parentDivId = 'points';
                        dataArray = window.pointData || [];
                    } else if (data.key === 'Station') {
                        parentDivId = 'stations';
                        dataArray = window.stationData || [];
                    }
                    
                    if (dataArray && receivedDataArray) {
                        receivedDataArray.forEach(receivedItem => {
                            var existingItem = dataArray.find(item => item.id == receivedItem.id);
                            if (existingItem) {
                                // 替换现有数据
                                Object.assign(existingItem, receivedItem);
                            } else {
                                // 追加新数据
                                dataArray.push(receivedItem);
                            }
                        });
                        
                        // 更新全局变量
                        if (data.key === 'Point') {
                            window.pointData = dataArray;
                        } else if (data.key === 'Station') {
                            window.stationData = dataArray;
                        }
                        
                        renderData(parentDivId, dataArray);
                    }
                } catch (error) {
                    console.error("处理SSE消息时出错:", error, event.data);
                }
            };
            
            // 错误处理
            eventSource.onerror = function(event) {
                console.error("SSE连接错误:", event);
                // 尝试重新连接
                setTimeout(initEventSource, 5000);
            };
        } else {
            document.getElementById("updates").innerHTML = "抱歉，您的浏览器不支持服务器发送事件...";
        }
    }

    // 初始化数据
    fetch('/api/monitor/point-real-time-data')
        .then(response => response.json())
        .then(data => {
            window.pointData = data.data;
            renderData('points', window.pointData);
        })
        .catch(error => {
            console.error('获取测点数据出错:', error);
        });

    fetch('/api/monitor/station-data')
        .then(response => response.json())
        .then(data => {
            window.stationData = data.data;
            renderData('stations', window.stationData);
        })
        .catch(error => {
            console.error('获取分站数据出错:', error);
        });

    // 初始化SSE连接
    initEventSource();
    
    // 添加测试按钮事件处理
    document.getElementById('testPointChange')?.addEventListener('click', function() {
        fetch('/api/monitor/point-change')
            .then(response => console.log('测点数据变更请求已发送'))
            .catch(error => console.error('测点数据变更请求出错:', error));
    });
    
    document.getElementById('testStationChange')?.addEventListener('click', function() {
        fetch('/api/monitor/station-change')
            .then(response => console.log('分站数据变更请求已发送'))
            .catch(error => console.error('分站数据变更请求出错:', error));
    });
});