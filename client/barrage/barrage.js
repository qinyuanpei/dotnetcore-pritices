/*!
** by zhangxinxu(.com)
** 与HTML5 video视频真实交互的弹幕效果
** http://www.zhangxinxu.com/wordpress/?p=6386
** MIT License
** 保留版权申明
*/

var defaults = {
	opacity: 100,
	fontSize: 24,
	speed: 2,
	range: [0, 1],
	color: 'white',
};

/* 
 * 初始化配置
 * @param opts
 * @param keys
 */
var initConfig = function (opts, keys) {
	var config = {};
	for (var key in keys) {
		if (opts[key]) {
			config[key] = opts[key];
		} else {
			config[key] = keys[key];
		}
	}

	return config;
};

/* 
 * 计算RGB数值
 * @param color
 */
var rgb = function (color) {
	var div = document.createElement('div');
	div.style.backgroundColor = color;
	document.body.appendChild(div);
	var rgbColor = window.getComputedStyle(div).backgroundColor;
	document.body.removeChild(div);
	return rgbColor;
}

/*
 * 测量文本宽度
 * @param text
 */
measureText = function (text) {
	var span = document.createElement('span');
	span.style.position = 'absolute';
	span.style.whiteSpace = 'nowrap';
	span.style.font = 'bold ' + fontSize + 'px "microsoft yahei", sans-serif';
	span.innerText = text;
	span.textContent = text;
	document.body.appendChild(span);
	var width = span.clientWidth;
	document.body.removeChild(span);
	return width;
}



var CanvasBarrage = function (canvas, video, options, data) {
	if (!canvas || !video) {
		return;
	}

	/* 构造参数 */
	this.opts = initConfig(options, defaults);

	var data = self.data

	/* 初始化Canvas */
	var context = canvas.getContext('2d');
	canvas.width = canvas.clientWidth;
	canvas.height = canvas.clientHeight;

	// 存储实例
	var store = {};

	// 暂停与否
	var isPause = true;
	// 播放时长
	var time = video.currentTime;

	// 字号大小
	var fontSize = 28;



	/* 
	* 弹幕实体定义
	* @param text
	* @param opts
	*/
	var Barrage = function (text, opts) {
		var self = this;
		self.text = text;
		var opts = initConfig(opts, defaults)

		this.init = function () {
			if (opts.speed !== 0) {
				self.speed = opts.speed + self.text.length / 100;
			}

			self.color = rgb(opts.color);
			self.range = opts.range;
			self.opacity = opts.opacity / 100;
			self.width = measureText(self.text);
			self.fontSize = opts.fontSize;

			self.x = canvas.width;
			if (self.speed == 0) {
				self.x = (self.x - self.width) / 2;
			}

			self.y = self.range[0] * canvas.height
				+ (self.range[1] - self.range[0]) * canvas.height * Math.random();
			if (self.y < self.fontSize) {
				self.y = fontSize;
			} else if (self.y > canvas.height - self.fontSize) {
				self.y = canvas.height - self.fontSize;
			}

			self.actualX = canvas.width;
		}
	};

	this.draw = function () {
		// 根据此时x位置绘制文本
		context.shadowColor = 'rgba(0,0,0,' + this.opacity + ')';
		context.shadowBlur = 2;
		context.font = this.fontSize + 'px "microsoft yahei", sans-serif';
		if (/rgb\(/.test(this.color)) {
			context.fillStyle = 'rgba(' + this.color.split('(')[1].split(')')[0] + ',' + this.opacity + ')';
		} else {
			context.fillStyle = this.color;
		}
		// 填色
		context.fillText(this.value, this.x, this.y);
	};
};

data.forEach(function (obj, index) {
	store[index] = new Barrage(obj);
});

// 绘制弹幕文本
var draw = function () {
	for (var index in store) {
		var barrage = store[index];

		if (barrage && !barrage.disabled && time >= barrage.time) {
			if (!barrage.inited) {
				barrage.init();
				barrage.inited = true;
			}
			barrage.x -= barrage.moveX;
			if (barrage.moveX == 0) {
				// 不动的弹幕
				barrage.actualX -= self.speed;
			} else {
				barrage.actualX = barrage.x;
			}
			// 移出屏幕
			if (barrage.actualX < -1 * barrage.width) {
				// 下面这行给speed为0的弹幕
				barrage.x = barrage.actualX;
				// 该弹幕不运动
				barrage.disabled = true;
			}
			// 根据新位置绘制圆圈圈
			barrage.draw();
		}
	}
};

// 画布渲染
var render = function () {
	// 更新已经播放时间
	time = video.currentTime;
	// 清除画布
	context.clearRect(0, 0, canvas.width, canvas.height);

	// 绘制画布
	draw();



	// 继续渲染
	if (isPause == false) {
		requestAnimationFrame(render);
	}
};

// 视频处理
video.addEventListener('play', function () {
	isPause = false;
	render();
});
video.addEventListener('pause', function () {
	isPause = true;
});
video.addEventListener('seeked', function () {
	// 跳转播放需要清屏
	self.reset();
});


// 添加数据的方法 
self.add = function (obj) {
	store[Object.keys(store).length] = new Barrage(obj);
};

// 重置
this.reset = function () {
	time = video.currentTime;
	// 画布清除
	context.clearRect(0, 0, canvas.width, canvas.height);

	for (var index in store) {
		var barrage = store[index];
		if (barrage) {
			// 状态变化
			barrage.disabled = false;
			// 根据时间判断哪些可以走起
			if (time < barrage.time) {
				// 视频时间小于播放时间
				// barrage.disabled = true;
				barrage.inited = null;
			} else {
				// 视频时间大于播放时间
				barrage.disabled = true;
			}
		}
	}
};
};