document.addEventListener("DOMContentLoaded", function () {
    window.pointData = null;
    window.stationData = null;

    function renderData(parentDivId, data) {
        var parentDiv = document.getElementById(parentDivId);
        parentDiv.innerHTML = ''; // 清空之前的内容
        data.forEach(item => {
            var newElement = document.createElement("div");
            newElement.innerHTML = "数据: " + JSON.stringify(item); // 假设数据有 Data 属性
            parentDiv.appendChild(newElement);
        });
    }

    fetch('/api/monitor/point-real-time-data')
        .then(response => response.json())
        .then(data => {
            window.pointData = data.data;
            renderData('points', window.pointData);
        })
        .catch(error => {
            console.error('Error fetching API data:', error);
        });

    fetch('/api/monitor/station-data')
        .then(response => response.json())
        .then(data => {
            window.stationData = data.data;
            renderData('stations', window.stationData);
        })
        .catch(error => {
            console.error('Error fetching API data:', error);
        });

    if (typeof (EventSource) !== "undefined") {
        var source = new EventSource("/api/monitor/server-send-event");
        source.onmessage = function (event) {
            var data = JSON.parse(event.data);
            var parentDivId = '';
            var dataArray = null;
            var receivedDataArray = null;
            if (data.data) {
                receivedDataArray = data.data;
            }
            if (data.key === 'Point') {
                parentDivId = 'points';
                dataArray = window.pointData;
            } else if (data.key === 'Station') {
                parentDivId = 'stations';
                dataArray = window.stationData;
            }
            if (dataArray) {

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
                renderData(parentDivId, dataArray);
            }
        };
    } else {
        document.getElementById("updates").innerHTML = "抱歉，您的浏览器不支持服务器发送事件...";
    }

});