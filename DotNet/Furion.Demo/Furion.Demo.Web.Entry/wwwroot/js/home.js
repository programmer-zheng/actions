document.addEventListener("DOMContentLoaded", function () {
    window.pointData = null;
    window.stationData = null;
    fetch('/api/monitor/point-real-time-data')
        .then(response => response.json())
        .then(data => {
            window.pointData = data.data;

        })
        .catch(error => {
            console.error('Error fetching API data:', error);
        });

    fetch('/api/monitor/station-data')
        .then(response => response.json())
        .then(data => {
            window.stationData = data.data;
        })
        .catch(error => {
            console.error('Error fetching API data:', error);
        });

    if (typeof (EventSource) !== "undefined") {
        var source = new EventSource("/api/monitor/server-send-event");
        source.onmessage = function (event) {
            var data = JSON.parse(event.data);
            var parentDivId = '';
            if (data.Key === 'Point') {
                parentDivId = 'points';
            } else if (data.Key === 'Station') {
                parentDivId = 'stations';
            }
            var newElement = document.createElement("div");
            newElement.innerHTML = "更新: " + data.Data;
            document.getElementById(parentDivId).appendChild(newElement);
        };
    } else {
        document.getElementById("updates").innerHTML = "抱歉，您的浏览器不支持服务器发送事件...";
    }

});