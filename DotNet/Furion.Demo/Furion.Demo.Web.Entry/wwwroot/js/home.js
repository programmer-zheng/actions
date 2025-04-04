document.addEventListener("DOMContentLoaded", function () {
    window.pointData = null;
    window.stationData = null;
    let eventSource = null;
    let lastHeartbeatTime = Date.now();
    let heartbeatCheckInterval = null;
    let reconnectAttempts = 0;
    const MAX_RECONNECT_ATTEMPTS = 10;
    const RECONNECT_DELAY = 5000; // 5秒

    // 连接状态管理
    function updateConnectionStatus(status) {
        const statusElement = document.getElementById('connectionStatus');
        statusElement.className = 'connection-status';
        
        switch(status) {
            case 'connected':
                statusElement.className += ' connection-connected';
                statusElement.textContent = '已连接';
                break;
            case 'disconnected':
                statusElement.className += ' connection-disconnected';
                statusElement.textContent = '已断开';
                break;
            case 'reconnecting':
                statusElement.className += ' connection-reconnecting';
                statusElement.textContent = '正在重连...';
                break;
        }
    }

    function renderData(data) {
        const pointsContainer = document.getElementById('points');
        const stationsContainer = document.getElementById('stations');
        
        // 渲染测点数据
        if (data.points && data.points.length > 0) {
            let pointsHtml = '<table class="data-table">';
            pointsHtml += '<thead><tr>';
            pointsHtml += '<th class="col-id">ID</th>';
            pointsHtml += '<th class="col-code">测点编号</th>';
            pointsHtml += '<th class="col-name">测点名称</th>';
            pointsHtml += '<th class="col-value">测点值</th>';
            pointsHtml += '<th class="col-address">安装位置</th>';
            pointsHtml += '<th class="col-status">状态</th>';
            pointsHtml += '</tr></thead><tbody>';
            
            data.points.forEach(point => {
                pointsHtml += '<tr>';
                pointsHtml += `<td class="col-id" data-full-text="${point.id}">${point.id}</td>`;
                pointsHtml += `<td class="col-code" data-full-text="${point.pointNumber}">${point.pointNumber}</td>`;
                pointsHtml += `<td class="col-name" data-full-text="${point.pointName}">${point.pointName}</td>`;
                pointsHtml += `<td class="col-value" data-full-text="${point.pointValue}">${point.pointValue}</td>`;
                pointsHtml += `<td class="col-address" data-full-text="${point.installAddress}">${point.installAddress}</td>`;
                pointsHtml += `<td class="col-status status-cell ${point.status === '正常' ? 'status-normal' : 'status-abnormal'}" data-full-text="${point.status}">${point.status}</td>`;
                pointsHtml += '</tr>';
            });
            
            pointsHtml += '</tbody></table>';
            pointsContainer.innerHTML = pointsHtml;
        } else {
            pointsContainer.innerHTML = '<div class="empty-data">暂无测点数据</div>';
        }
        
        // 渲染分站数据
        if (data.stations && data.stations.length > 0) {
            let stationsHtml = '<table class="data-table">';
            stationsHtml += '<thead><tr>';
            stationsHtml += '<th class="col-id">ID</th>';
            stationsHtml += '<th class="col-code">分站号</th>';
            stationsHtml += '<th class="col-name">型号</th>';
            stationsHtml += '<th class="col-value">电池电压</th>';
            stationsHtml += '<th class="col-address">安装位置</th>';
            stationsHtml += '<th class="col-status">状态</th>';
            stationsHtml += '</tr></thead><tbody>';
            
            data.stations.forEach(station => {
                stationsHtml += '<tr>';
                stationsHtml += `<td class="col-id" data-full-text="${station.id}">${station.id}</td>`;
                stationsHtml += `<td class="col-code" data-full-text="${station.sno}">${station.sno}</td>`;
                stationsHtml += `<td class="col-name" data-full-text="${station.model}">${station.model}</td>`;
                stationsHtml += `<td class="col-value" data-full-text="${station.batteryVoltage}">${station.batteryVoltage}</td>`;
                stationsHtml += `<td class="col-address" data-full-text="${station.address}">${station.address}</td>`;
                stationsHtml += `<td class="col-status status-cell ${station.status === '正常' ? 'status-normal' : 'status-abnormal'}" data-full-text="${station.status}">${station.status}</td>`;
                stationsHtml += '</tr>';
            });
            
            stationsHtml += '</tbody></table>';
            stationsContainer.innerHTML = stationsHtml;
        } else {
            stationsContainer.innerHTML = '<div class="empty-data">暂无分站数据</div>';
        }
    }

    function initEventSource() {
        if (eventSource) {
            eventSource.close();
        }

        // 清除之前的心跳检查定时器
        if (heartbeatCheckInterval) {
            clearInterval(heartbeatCheckInterval);
        }

        updateConnectionStatus('reconnecting');

        if (typeof (EventSource) !== "undefined") {
            eventSource = new EventSource("/api/monitor/server-send-event");
            
            // 连接建立时的处理
            eventSource.onopen = function(event) {
                console.log("SSE连接已建立");
                lastHeartbeatTime = Date.now();
                reconnectAttempts = 0;
                updateConnectionStatus('connected');
                
                // 设置心跳检查
                heartbeatCheckInterval = setInterval(checkHeartbeat, 60000); // 每分钟检查一次
            };
            
            // 接收消息的处理
            eventSource.onmessage = function (event) {
                console.log("收到SSE消息:", event.data);
                try {
                    var data = JSON.parse(event.data);
                    
                    // 处理连接成功消息
                    if (data.type === "connected") {
                        console.log("SSE连接已确认");
                        updateConnectionStatus('connected');
                        return;
                    }
                    
                    // 处理心跳消息
                    if (data.type === "heartbeat") {
                        console.log("收到SSE心跳");
                        lastHeartbeatTime = Date.now();
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
                        
                        // 构造正确的数据结构
                        const formattedData = {
                            points: window.pointData || [],
                            stations: window.stationData || []
                        };
                        renderData(formattedData);
                    }
                } catch (error) {
                    console.error("处理SSE消息时出错:", error, event.data);
                }
            };
            
            // 错误处理
            eventSource.onerror = function(event) {
                console.error("SSE连接错误:", event);
                clearInterval(heartbeatCheckInterval);
                updateConnectionStatus('disconnected');
                
                // 尝试重新连接
                if (reconnectAttempts < MAX_RECONNECT_ATTEMPTS) {
                    reconnectAttempts++;
                    console.log(`尝试重新连接 (${reconnectAttempts}/${MAX_RECONNECT_ATTEMPTS})...`);
                    setTimeout(initEventSource, RECONNECT_DELAY);
                } else {
                    console.error("达到最大重连次数，请刷新页面重试");
                    document.getElementById("updates").innerHTML = "连接已断开，请刷新页面重试";
                }
            };
        } else {
            document.getElementById("updates").innerHTML = "抱歉，您的浏览器不支持服务器发送事件...";
        }
    }
    
    // 检查心跳是否正常
    function checkHeartbeat() {
        const now = Date.now();
        const timeSinceLastHeartbeat = now - lastHeartbeatTime;
        
        // 如果超过90秒没有收到心跳，认为连接已断开
        if (timeSinceLastHeartbeat > 90000) {
            console.warn("超过90秒未收到心跳，连接可能已断开");
            clearInterval(heartbeatCheckInterval);
            updateConnectionStatus('disconnected');
            
            // 尝试重新连接
            if (reconnectAttempts < MAX_RECONNECT_ATTEMPTS) {
                reconnectAttempts++;
                console.log(`尝试重新连接 (${reconnectAttempts}/${MAX_RECONNECT_ATTEMPTS})...`);
                initEventSource();
            } else {
                console.error("达到最大重连次数，请刷新页面重试");
                document.getElementById("updates").innerHTML = "连接已断开，请刷新页面重试";
            }
        }
    }

    // 初始化数据
    fetch('/api/monitor/point-real-time-data')
        .then(response => response.json())
        .then(data => {
            window.pointData = data.data;
            // 构造正确的数据结构
            const formattedData = {
                points: data.data || [],
                stations: window.stationData || []
            };
            renderData(formattedData);
        })
        .catch(error => {
            console.error('获取测点数据出错:', error);
        });

    fetch('/api/monitor/station-data')
        .then(response => response.json())
        .then(data => {
            window.stationData = data.data;
            // 构造正确的数据结构
            const formattedData = {
                points: window.pointData || [],
                stations: data.data || []
            };
            renderData(formattedData);
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
    
    // 添加重新连接按钮事件处理
    document.getElementById('refreshConnection')?.addEventListener('click', function() {
        reconnectAttempts = 0; // 重置重连次数
        initEventSource();
    });
    
    // 页面关闭前清理资源
    window.addEventListener('beforeunload', function() {
        if (eventSource) {
            eventSource.close();
        }
        if (heartbeatCheckInterval) {
            clearInterval(heartbeatCheckInterval);
        }
    });
});