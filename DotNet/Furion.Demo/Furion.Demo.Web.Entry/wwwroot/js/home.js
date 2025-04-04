document.addEventListener("DOMContentLoaded", function () {
    window.pointData = null;
    window.stationData = null;
    let eventSource = null;

    function renderData(parentDivId, data) {
        var parentDiv = document.getElementById(parentDivId);
        parentDiv.innerHTML = ''; // 清空之前的内容
        
        // 检查数据是否为空
        if (!data || data.length === 0) {
            var emptyMessage = document.createElement('div');
            emptyMessage.className = 'empty-data';
            emptyMessage.textContent = '暂无数据';
            parentDiv.appendChild(emptyMessage);
            return;
        }
        
        // 创建表格
        var table = document.createElement('table');
        table.className = 'data-table';
        
        // 创建表头
        var thead = document.createElement('thead');
        var headerRow = document.createElement('tr');
        
        // 根据数据类型设置不同的表头
        if (parentDivId === 'points') {
            headerRow.innerHTML = `
                <th>ID</th>
                <th>测点编号</th>
                <th>测点名称</th>
                <th>测点值</th>
                <th>安装位置</th>
                <th>状态</th>
            `;
        } else if (parentDivId === 'stations') {
            headerRow.innerHTML = `
                <th>ID</th>
                <th>分站号</th>
                <th>型号</th>
                <th>电池电压</th>
                <th>安装位置</th>
                <th>状态</th>
            `;
        }
        
        thead.appendChild(headerRow);
        table.appendChild(thead);
        
        // 创建表体
        var tbody = document.createElement('tbody');
        
        data.forEach(item => {
            var row = document.createElement('tr');
            
            // 根据数据类型设置不同的单元格内容
            if (parentDivId === 'points') {
                row.innerHTML = `
                    <td>${item.id || ''}</td>
                    <td>${item.pointNumber || ''}</td>
                    <td>${item.pointName || ''}</td>
                    <td>${item.pointValue || ''}</td>
                    <td>${item.installAddress || ''}</td>
                    <td class="status-cell ${item.status === '正常' ? 'status-normal' : 'status-abnormal'}">${item.status || ''}</td>
                `;
            } else if (parentDivId === 'stations') {
                row.innerHTML = `
                    <td>${item.id || ''}</td>
                    <td>${item.sno || ''}</td>
                    <td>${item.model || ''}</td>
                    <td>${item.batteryVoltage || ''}</td>
                    <td>${item.address || ''}</td>
                    <td class="status-cell ${item.status === '正常' ? 'status-normal' : 'status-abnormal'}">${item.status || ''}</td>
                `;
            }
            
            tbody.appendChild(row);
        });
        
        table.appendChild(tbody);
        parentDiv.appendChild(table);
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