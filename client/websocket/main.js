var vm = new Vue({
    el: "#app",
    data: {
        sendTo: "All",
        message: "",
        username: prompt("请输入用户名", ""),
        userList: null,
        messageList: null,
        websocket: null
    },
    methods: {
        openWebSocket: function () {
            var isSupport = 'WebSocket' in window;
            if (!isSupport) {
                var message = self.formatMessage(new Date(), "系统消息", "当前浏览器不支持WebSocket");
                self.messageList = self.messageList == null ? message : self.messageList + "<br/>" + message;
                return;
            }

            this.websocket = new WebSocket("ws://localhost:8000/ws?username=" + this.username);
            let self = this;

            //链接错误得回调方法
            this.websocket.onerror = function () {
                var message = self.formatMessage(new Date(), "系统消息", "WebSocket连接发生错误");
                self.messageList = self.messageList == null ? message : self.messageList + "<br/>" + message;
            };

            //连接成功建立的回调方法
            this.websocket.onopen = function () {
                var message = self.formatMessage(new Date(), "系统消息", "WebSocket连接成功");
                self.messageList = self.messageList == null ? message : self.messageList + "<br/>" + message;
                self.sendEvent("Joined");
            }

            //接收到消息的回调方法
            this.websocket.onmessage = function (event) {
                var entity = JSON.parse(event.data);
                if (entity.Type == "Event") {
                    var eventData = JSON.parse(entity.Message);
                    self.userList = eventData.Data.filter(function (data) {
                        return data != self.username;
                    });
                } else {
                    var message = self.formatMessage(entity.SendTime, entity.Sender, entity.Message);
                    self.messageList = self.messageList == null ? message : self.messageList + "<br/>" + message;
                }
            }

            //连接关闭的回调方法
            this.websocket.onclose = function () {
                var message = self.formatMessage(new Date(), "系统消息", "WebSocket连接关闭");
                self.messageList = self.messageList == null ? message : self.messageList + "<br/>" + message;
            }

            //监听窗口关闭事件
            window.onbeforeunload = function () {
                if (self.websocket == null) {
                    reurn;
                }

                self.closeWebSocket();
            }
        },

        closeWebSocket: function () {
            if (this.websocket == null) {
                reurn;
            }

            this.sendEvent("Leaved");
            this.websocket.close();
        },

        sendMessage: function () {
            if (this.websocket == null) {
                reurn;
            }

            if (this.message == null || this.message == "") {
                alert("不允许发送空消息");
                return;
            }
            this.websocket.send(JSON.stringify({
                "sender": this.username,
                "receiver": this.sendTo,
                "message": this.message,
                "sendTime": new Date().toISOString(),
            }));

            var message = this.formatMessage(new Date(), this.username, this.message);
            this.messageList = this.messageList == null ? message : this.messageList + "<br/>" + message;
            this.message = null;
        },

        sendEvent: function (eventName) {
            if (this.websocket == null) {
                return;
            }

            var eventData = JSON.stringify({
                "event": eventName,
                "data": null
            });

            this.websocket.send(JSON.stringify({
                "type": "event",
                "sender": this.username,
                "message": eventData,
                "sendTime": new Date().toISOString(),
            }));
        },

        formatMessage: function (sendTime, sender, message) {
            return moment(sendTime).format("YYYY-MM-DD hh:mm:ss") + " - " + sender + " : " + message;
        },

        clearMessageList: function () {
            this.messageList = null;
        }
    }
});
