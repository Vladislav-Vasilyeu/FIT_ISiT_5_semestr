(function (cjs, an) {

var p; // shortcut to reference prototypes
var lib={};var ss={};var img={};
lib.ssMetadata = [];


(lib.AnMovieClip = function(){
	this.actionFrames = [];
	this.ignorePause = false;
	this.currentSoundStreamInMovieclip;
	this.soundStreamDuration = new Map();
	this.streamSoundSymbolsList = [];

	this.gotoAndPlayForStreamSoundSync = function(positionOrLabel){
		cjs.MovieClip.prototype.gotoAndPlay.call(this,positionOrLabel);
	}
	this.gotoAndPlay = function(positionOrLabel){
		this.clearAllSoundStreams();
		var pos = this.timeline.resolve(positionOrLabel);
		if (pos != null) { this.startStreamSoundsForTargetedFrame(pos); }
		cjs.MovieClip.prototype.gotoAndPlay.call(this,positionOrLabel);
	}
	this.play = function(){
		this.clearAllSoundStreams();
		this.startStreamSoundsForTargetedFrame(this.currentFrame);
		cjs.MovieClip.prototype.play.call(this);
	}
	this.gotoAndStop = function(positionOrLabel){
		cjs.MovieClip.prototype.gotoAndStop.call(this,positionOrLabel);
		this.clearAllSoundStreams();
	}
	this.stop = function(){
		cjs.MovieClip.prototype.stop.call(this);
		this.clearAllSoundStreams();
	}
	this.startStreamSoundsForTargetedFrame = function(targetFrame){
		for(var index=0; index<this.streamSoundSymbolsList.length; index++){
			if(index <= targetFrame && this.streamSoundSymbolsList[index] != undefined){
				for(var i=0; i<this.streamSoundSymbolsList[index].length; i++){
					var sound = this.streamSoundSymbolsList[index][i];
					if(sound.endFrame > targetFrame){
						var targetPosition = Math.abs((((targetFrame - sound.startFrame)/lib.properties.fps) * 1000));
						var instance = playSound(sound.id);
						var remainingLoop = 0;
						if(sound.offset){
							targetPosition = targetPosition + sound.offset;
						}
						else if(sound.loop > 1){
							var loop = targetPosition /instance.duration;
							remainingLoop = Math.floor(sound.loop - loop);
							if(targetPosition == 0){ remainingLoop -= 1; }
							targetPosition = targetPosition % instance.duration;
						}
						instance.loop = remainingLoop;
						instance.position = Math.round(targetPosition);
						this.InsertIntoSoundStreamData(instance, sound.startFrame, sound.endFrame, sound.loop , sound.offset);
					}
				}
			}
		}
	}
	this.InsertIntoSoundStreamData = function(soundInstance, startIndex, endIndex, loopValue, offsetValue){ 
 		this.soundStreamDuration.set({instance:soundInstance}, {start: startIndex, end:endIndex, loop:loopValue, offset:offsetValue});
	}
	this.clearAllSoundStreams = function(){
		this.soundStreamDuration.forEach(function(value,key){
			key.instance.stop();
		});
 		this.soundStreamDuration.clear();
		this.currentSoundStreamInMovieclip = undefined;
	}
	this.stopSoundStreams = function(currentFrame){
		if(this.soundStreamDuration.size > 0){
			var _this = this;
			this.soundStreamDuration.forEach(function(value,key,arr){
				if((value.end) == currentFrame){
					key.instance.stop();
					if(_this.currentSoundStreamInMovieclip == key) { _this.currentSoundStreamInMovieclip = undefined; }
					arr.delete(key);
				}
			});
		}
	}

	this.computeCurrentSoundStreamInstance = function(currentFrame){
		if(this.currentSoundStreamInMovieclip == undefined){
			var _this = this;
			if(this.soundStreamDuration.size > 0){
				var maxDuration = 0;
				this.soundStreamDuration.forEach(function(value,key){
					if(value.end > maxDuration){
						maxDuration = value.end;
						_this.currentSoundStreamInMovieclip = key;
					}
				});
			}
		}
	}
	this.getDesiredFrame = function(currentFrame, calculatedDesiredFrame){
		for(var frameIndex in this.actionFrames){
			if((frameIndex > currentFrame) && (frameIndex < calculatedDesiredFrame)){
				return frameIndex;
			}
		}
		return calculatedDesiredFrame;
	}

	this.syncStreamSounds = function(){
		this.stopSoundStreams(this.currentFrame);
		this.computeCurrentSoundStreamInstance(this.currentFrame);
		if(this.currentSoundStreamInMovieclip != undefined){
			var soundInstance = this.currentSoundStreamInMovieclip.instance;
			if(soundInstance.position != 0){
				var soundValue = this.soundStreamDuration.get(this.currentSoundStreamInMovieclip);
				var soundPosition = (soundValue.offset?(soundInstance.position - soundValue.offset): soundInstance.position);
				var calculatedDesiredFrame = (soundValue.start)+((soundPosition/1000) * lib.properties.fps);
				if(soundValue.loop > 1){
					calculatedDesiredFrame +=(((((soundValue.loop - soundInstance.loop -1)*soundInstance.duration)) / 1000) * lib.properties.fps);
				}
				calculatedDesiredFrame = Math.floor(calculatedDesiredFrame);
				var deltaFrame = calculatedDesiredFrame - this.currentFrame;
				if((deltaFrame >= 0) && this.ignorePause){
					cjs.MovieClip.prototype.play.call(this);
					this.ignorePause = false;
				}
				else if(deltaFrame >= 2){
					this.gotoAndPlayForStreamSoundSync(this.getDesiredFrame(this.currentFrame,calculatedDesiredFrame));
				}
				else if(deltaFrame <= -2){
					cjs.MovieClip.prototype.stop.call(this);
					this.ignorePause = true;
				}
			}
		}
	}
}).prototype = p = new cjs.MovieClip();
// symbols:



(lib.Stop = function(mode,startPosition,loop,reversed) {
if (loop == null) { loop = true; }
if (reversed == null) { reversed = false; }
	var props = new Object();
	props.mode = mode;
	props.startPosition = startPosition;
	props.labels = {};
	props.loop = loop;
	props.reversed = reversed;
	cjs.MovieClip.apply(this,[props]);

	// Слой_1
	this.shape = new cjs.Shape();
	this.shape.graphics.f().s("#000000").ss(1,1,1).p("AnznzIPnAAIAAPnIvnAAg");
	this.shape.setTransform(1,0.05);

	this.shape_1 = new cjs.Shape();
	this.shape_1.graphics.f("#DB0000").s().p("AnzH0IAAvnIPnAAIAAPng");
	this.shape_1.setTransform(1,0.05);

	this.shape_2 = new cjs.Shape();
	this.shape_2.graphics.f("#FF0000").s().p("AnzH0IAAvnIPnAAIAAPng");
	this.shape_2.setTransform(1,0.05);

	this.shape_3 = new cjs.Shape();
	this.shape_3.graphics.f("#890000").s().p("AnzH0IAAvnIPnAAIAAPng");
	this.shape_3.setTransform(1,10.05);

	this.timeline.addTween(cjs.Tween.get({}).to({state:[{t:this.shape_1},{t:this.shape,p:{y:0.05}}]}).to({state:[{t:this.shape_2},{t:this.shape,p:{y:0.05}}]},1).to({state:[{t:this.shape_3,p:{y:10.05}},{t:this.shape,p:{y:10.05}}]},1).to({state:[{t:this.shape_3,p:{y:0.05}},{t:this.shape,p:{y:0.05}}]},1).wait(1));

	this._renderFirstFrame();

}).prototype = p = new cjs.MovieClip();
p.nominalBounds = new cjs.Rectangle(-50,-50.9,102,112);


(lib.Start = function(mode,startPosition,loop,reversed) {
if (loop == null) { loop = true; }
if (reversed == null) { reversed = false; }
	var props = new Object();
	props.mode = mode;
	props.startPosition = startPosition;
	props.labels = {};
	props.loop = loop;
	props.reversed = reversed;
	cjs.MovieClip.apply(this,[props]);

	// Слой_1
	this.shape = new cjs.Shape();
	this.shape.graphics.f().s("#000000").ss(1,1,1).p("An9nzIP7IlIv7HCg");
	this.shape.setTransform(1,-0.95);

	this.shape_1 = new cjs.Shape();
	this.shape_1.graphics.f("#00BF24").s().p("An9nzIP7IlIv7HCg");
	this.shape_1.setTransform(1,-0.95);

	this.shape_2 = new cjs.Shape();
	this.shape_2.graphics.f("#00FF00").s().p("An9nzIP7IlIv7HCg");
	this.shape_2.setTransform(1,-0.95);

	this.shape_3 = new cjs.Shape();
	this.shape_3.graphics.f("#008000").s().p("An9nzIP7IlIv7HCg");
	this.shape_3.setTransform(1,10.05);

	this.shape_4 = new cjs.Shape();
	this.shape_4.graphics.f().s("#000000").ss(1,1,1).p("AnznzIPnAAIAAPnIvnAAg");
	this.shape_4.setTransform(-1,-1.95);

	this.shape_5 = new cjs.Shape();
	this.shape_5.graphics.f("#00BF24").s().p("AnzH0IAAvnIPnAAIAAPng");
	this.shape_5.setTransform(-1,-1.95);

	this.timeline.addTween(cjs.Tween.get({}).to({state:[{t:this.shape_1},{t:this.shape,p:{y:-0.95}}]}).to({state:[{t:this.shape_2},{t:this.shape,p:{y:-0.95}}]},1).to({state:[{t:this.shape_3},{t:this.shape,p:{y:10.05}}]},1).to({state:[{t:this.shape_5},{t:this.shape_4}]},1).wait(1));

	this._renderFirstFrame();

}).prototype = p = new cjs.MovieClip();
p.nominalBounds = new cjs.Rectangle(-52,-52.9,105,114);


(lib.Reset = function(mode,startPosition,loop,reversed) {
if (loop == null) { loop = true; }
if (reversed == null) { reversed = false; }
	var props = new Object();
	props.mode = mode;
	props.startPosition = startPosition;
	props.labels = {};
	props.loop = loop;
	props.reversed = reversed;
	cjs.MovieClip.apply(this,[props]);

	// Слой_1
	this.shape = new cjs.Shape();
	this.shape.graphics.f("#0000B1").s().p("AvLCMIHWocIAYGgQQ+gJAnp+IFEAAQkXP3yIh5IAQFxg");
	this.shape.setTransform(-0.95,-2.8);

	this.shape_1 = new cjs.Shape();
	this.shape_1.graphics.f("#0003FF").s().p("AvLCMIHWocIAYGgQQ+gJAnp+IFEAAQkXP3yIh5IAQFxg");
	this.shape_1.setTransform(-0.95,-2.8);

	this.shape_2 = new cjs.Shape();
	this.shape_2.graphics.f("#000079").s().p("AvLCMIHWocIAYGgQQ+gJAnp+IFEAAQkXP3yIh5IAQFxg");
	this.shape_2.setTransform(-0.95,9.2);

	this.shape_3 = new cjs.Shape();
	this.shape_3.graphics.f().s("#000000").ss(1,1,1).p("AvnqJIfPAAIAAUTI/PAAg");
	this.shape_3.setTransform(-0.8,-0.6);

	this.shape_4 = new cjs.Shape();
	this.shape_4.graphics.f("#000079").s().p("AvnKKIAA0TIfPAAIAAUTg");
	this.shape_4.setTransform(-0.8,-0.6);

	this.timeline.addTween(cjs.Tween.get({}).to({state:[{t:this.shape}]}).to({state:[{t:this.shape_1}]},1).to({state:[{t:this.shape_2}]},1).to({state:[{t:this.shape_4},{t:this.shape_3}]},1).wait(1));

	this._renderFirstFrame();

}).prototype = p = new cjs.MovieClip();
p.nominalBounds = new cjs.Rectangle(-101.8,-66.6,202,139);


(lib.Нога_жука = function(mode,startPosition,loop,reversed) {
if (loop == null) { loop = true; }
if (reversed == null) { reversed = false; }
	var props = new Object();
	props.mode = mode;
	props.startPosition = startPosition;
	props.labels = {};
	props.loop = loop;
	props.reversed = reversed;
	cjs.MovieClip.apply(this,[props]);

	// Слой_1
	this.shape = new cjs.Shape();
	this.shape.graphics.f().s("#000000").ss(19,1,1).p("AAAx4MAAAAjx");
	this.shape.setTransform(-2,112.525);

	this.shape_1 = new cjs.Shape();
	this.shape_1.graphics.f().s("#000000").ss(19,1,1).p("Ai1Q4MAFrghv");
	this.shape_1.setTransform(-20.175,106);

	this.shape_2 = new cjs.Shape();
	this.shape_2.graphics.f().s("#000000").ss(19,1,1).p("AlrP2ILX/r");
	this.shape_2.setTransform(-38.375,99.475);

	this.shape_3 = new cjs.Shape();
	this.shape_3.graphics.f().s("#000000").ss(19,1,1).p("AohO1IRD9p");
	this.shape_3.setTransform(-56.55,92.95);

	this.shape_4 = new cjs.Shape();
	this.shape_4.graphics.f().s("#000000").ss(19,1,1).p("ALYtzI2vbn");
	this.shape_4.setTransform(-74.75,86.425);

	this.shape_5 = new cjs.Shape();
	this.shape_5.graphics.f().s("#000000").ss(19,1,1).p("ApHOoISP9P");
	this.shape_5.setTransform(-60.4,91.65);

	this.shape_6 = new cjs.Shape();
	this.shape_6.graphics.f().s("#000000").ss(19,1,1).p("Am3PcINv+3");
	this.shape_6.setTransform(-46.025,96.85);

	this.shape_7 = new cjs.Shape();
	this.shape_7.graphics.f().s("#000000").ss(19,1,1).p("AkoQQMAJQggf");
	this.shape_7.setTransform(-31.7,102.075);

	this.shape_8 = new cjs.Shape();
	this.shape_8.graphics.f().s("#000000").ss(19,1,1).p("AiYREMAExgiH");
	this.shape_8.setTransform(-17.325,107.275);

	this.shape_9 = new cjs.Shape();
	this.shape_9.graphics.f().s("#000000").ss(19,1,1).p("AAJx4MgARAjx");
	this.shape_9.setTransform(-2.975,112.5);

	this.timeline.addTween(cjs.Tween.get({}).to({state:[{t:this.shape}]}).to({state:[{t:this.shape_1}]},1).to({state:[{t:this.shape_2}]},1).to({state:[{t:this.shape_3}]},1).to({state:[{t:this.shape_4}]},1).to({state:[{t:this.shape_5}]},1).to({state:[{t:this.shape_6}]},1).to({state:[{t:this.shape_7}]},1).to({state:[{t:this.shape_8}]},1).to({state:[{t:this.shape_9}]},1).wait(1));

	this._renderFirstFrame();

}).prototype = p = new cjs.MovieClip();
p.nominalBounds = new cjs.Rectangle(-157,-11.4,164.5,247.9);


(lib.Жук_тело = function(mode,startPosition,loop,reversed) {
if (loop == null) { loop = true; }
if (reversed == null) { reversed = false; }
	var props = new Object();
	props.mode = mode;
	props.startPosition = startPosition;
	props.labels = {};
	props.loop = loop;
	props.reversed = reversed;
	cjs.MovieClip.apply(this,[props]);

	// Слой_1
	this.shape = new cjs.Shape();
	this.shape.graphics.f().s("#000000").ss(19,1,1).p("A3HHCIROFzIelKTA4rh3IKvmZIY4u3");
	this.shape.setTransform(1051.9,147.975);

	this.shape_1 = new cjs.Shape();
	this.shape_1.graphics.f("#963600").s().p("Eg5DATAQ3on3gBrJQABrHXon4UAXpgH4AhaAAAUAhcAAAAXoAH4QHhChFJC1QK/GEAAHlQAAD7i8DiIxPl0IRPF0QlZGdvUFGUgXoAH4ghcAAAUghaAAAgXpgH4gEA6+gHQIKwmZgEBNxAHdg");
	this.shape_1.setTransform(516.45,182.45);

	this.timeline.addTween(cjs.Tween.get({}).to({state:[{t:this.shape_1},{t:this.shape}]}).wait(1));

	this._renderFirstFrame();

}).prototype = p = new cjs.MovieClip();
p.nominalBounds = new cjs.Rectangle(0,-9.5,1219.4,364);


(lib.Жук_живой = function(mode,startPosition,loop,reversed) {
if (loop == null) { loop = true; }
if (reversed == null) { reversed = false; }
	var props = new Object();
	props.mode = mode;
	props.startPosition = startPosition;
	props.labels = {};
	props.loop = loop;
	props.reversed = reversed;
	cjs.MovieClip.apply(this,[props]);

	// Слой_1
	this.instance = new lib.Жук_тело("synched",0);
	this.instance.setTransform(609.7,321.5,1,1,0,0,0,609.7,172.5);

	this.instance_1 = new lib.Нога_жука("synched",0);
	this.instance_1.setTransform(280.7,114.5,1,1,180,0,0,-74.8,112.5);

	this.instance_2 = new lib.Нога_жука("synched",3);
	this.instance_2.setTransform(581.35,114.5,1,1,180,0,0,-74.8,112.5);

	this.instance_3 = new lib.Нога_жука("synched",6);
	this.instance_3.setTransform(829.3,114.5,1,1,180,0,0,-74.8,112.5);

	this.instance_4 = new lib.Нога_жука("synched",1);
	this.instance_4.setTransform(135.1,544.1,1,1,0,0,0,-74.8,112.5);

	this.instance_5 = new lib.Нога_жука("synched",7);
	this.instance_5.setTransform(683.7,544.1,1,1,0,0,0,-74.8,112.5);

	this.instance_6 = new lib.Нога_жука("synched",4);
	this.instance_6.setTransform(435.75,544.1,1,1,0,0,0,-74.8,112.5);

	this.timeline.addTween(cjs.Tween.get({}).to({state:[{t:this.instance_6},{t:this.instance_5},{t:this.instance_4},{t:this.instance_3},{t:this.instance_2},{t:this.instance_1},{t:this.instance}]}).wait(10));

	this._renderFirstFrame();

}).prototype = p = new cjs.MovieClip();
p.nominalBounds = new cjs.Rectangle(0,-9.5,1219.4,677.6);


// stage content:
(lib.Из_жизни_жуков_HTML5Canvas = function(mode,startPosition,loop,reversed) {
if (loop == null) { loop = true; }
if (reversed == null) { reversed = false; }
	var props = new Object();
	props.mode = mode;
	props.startPosition = startPosition;
	props.labels = {};
	props.loop = loop;
	props.reversed = reversed;
	cjs.MovieClip.apply(this,[props]);

	this.actionFrames = [0,7,34,37,77];
	this.streamSoundSymbolsList[0] = [{id:"run",startFrame:0,endFrame:77,loop:0,offset:0}];
	this.streamSoundSymbolsList[7] = [{id:"run",startFrame:7,endFrame:81,loop:1,offset:0}];
	this.streamSoundSymbolsList[34] = [{id:"boom",startFrame:34,endFrame:51,loop:1,offset:0}];
	this.streamSoundSymbolsList[37] = [{id:"bigboom",startFrame:37,endFrame:47,loop:1,offset:0}];
	this.streamSoundSymbolsList[77] = [{id:"run",startFrame:77,endFrame:81,loop:1,offset:0}];
	// timeline functions:
	this.frame_0 = function() {
		this.clearAllSoundStreams();
		 
		var soundInstance = playSound("run",-1);
		this.InsertIntoSoundStreamData(soundInstance,0,77,0);
		this.stop();  // Пауза
		
		this.btnStart.addEventListener("click", onPlay.bind(this));
		this.btnStop.addEventListener("click", onStop.bind(this));
		this.btnReset.addEventListener("click", onReset.bind(this));
		
		function onPlay() {
		    this.play();
		}
		
		function onStop() {
		    this.stop();
		}
		
		function onReset() {
		    this.gotoAndStop(0);  // Кадр 0 = 1
		}
	}
	this.frame_7 = function() {
		var soundInstance = playSound("run",0);
		this.InsertIntoSoundStreamData(soundInstance,7,81,1);
	}
	this.frame_34 = function() {
		var soundInstance = playSound("boom",0);
		this.InsertIntoSoundStreamData(soundInstance,34,51,1);
	}
	this.frame_37 = function() {
		var soundInstance = playSound("bigboom",0);
		this.InsertIntoSoundStreamData(soundInstance,37,47,1);
	}
	this.frame_77 = function() {
		var soundInstance = playSound("run",0);
		this.InsertIntoSoundStreamData(soundInstance,77,81,1);
	}

	// actions tween:
	this.timeline.addTween(cjs.Tween.get(this).call(this.frame_0).wait(7).call(this.frame_7).wait(27).call(this.frame_34).wait(3).call(this.frame_37).wait(40).call(this.frame_77).wait(4));

	// Кнопки_управления
	this.btnReset = new lib.Reset();
	this.btnReset.name = "btnReset";
	this.btnReset.setTransform(1101.4,1006.2);
	new cjs.ButtonHelper(this.btnReset, 0, 1, 2, false, new lib.Reset(), 3);

	this.btnStop = new lib.Stop();
	this.btnStop.name = "btnStop";
	this.btnStop.setTransform(876.3,1005.6);
	new cjs.ButtonHelper(this.btnStop, 0, 1, 2, false, new lib.Stop(), 3);

	this.btnStart = new lib.Start();
	this.btnStart.name = "btnStart";
	this.btnStart.setTransform(714.3,1006.6);
	new cjs.ButtonHelper(this.btnStart, 0, 1, 2, false, new lib.Start(), 3);

	this.timeline.addTween(cjs.Tween.get({}).to({state:[{t:this.btnStart},{t:this.btnStop},{t:this.btnReset}]}).wait(81));

	// Жук_маленький
	this.instance = new lib.Жук_живой();
	this.instance.setTransform(1500.5,-31.8,0.0707,0.0874,113.8,0,0,609.6,329.7);
	this.instance._off = true;

	this.timeline.addTween(cjs.Tween.get(this.instance).wait(12).to({_off:false},0).to({regX:609.9,regY:329.2,rotation:107.1505,guide:{path:[1500.5,-31.7,1495.9,-25.7,1491.3,-19.6,1486.9,-12.4,1481.4,12,1475.5,38.1,1475.5,54.6,1475.5,80,1481.1,98.9,1484.8,111.4,1493.4,127,1502.9,144.3,1505.7,151.9,1511.3,167.3,1511.3,186.6,1511.3,205.8,1501.5,224.3,1493.8,238.9,1478.4,255.4,1476.6,257.3,1451.4,282.5,1436,297.8,1429.8,307.8,1421.5,321,1418.6,342.8,1416.3,359.9,1416.9,386.9,1417.3,402.5,1418.7,434.7,1419.6,462.8,1417.8,480.5,1415.7,501.6,1401.5,525.4], orient:'fixed'}},28).to({rotation:0,skewX:72.8495,skewY:-107.14},5).to({rotation:107.1505,skewX:0,skewY:0},5).to({regX:611.1,regY:328.6,rotation:99.0283,guide:{path:[1401.5,525.5,1397,533,1391.4,540.7,1382.4,553,1367.7,569.7,1350.2,589.3,1341.3,599.4,1307.9,637.2,1292.8,664.2,1271,703.1,1271,742.5,1271,760.8,1282.4,787.5,1291,807.9,1307.9,836.4,1317.4,852.4,1334.8,880.6,1348,902.6,1350.3,911.5,1363.9,964.8,1370.1,1016.9,1377.5,1078.8,1372.3,1124], orient:'fixed'}},30).wait(1));

	// Жуки
	this.instance_1 = new lib.Жук_живой();
	this.instance_1.setTransform(1752,137.8,0.1752,0.158,150.0025,0,0,610.6,329.4);

	this.instance_2 = new lib.Жук_живой();
	this.instance_2.setTransform(240.8,973.85,0.1686,0.2196,-14.9901,0,0,610.8,329.2);

	this.instance_3 = new lib.Жук_живой();
	this.instance_3.setTransform(1033.1,165.6,0.0768,0.1083,75.0005,0,0,611.1,328.8);

	this.instance_4 = new lib.Жук_живой();
	this.instance_4.setTransform(1607.1,963.9,0.1234,0.1588,0,-153.201,-150.0092,608.9,329.1);

	this.instance_5 = new lib.Жук_живой();
	this.instance_5.setTransform(215.35,88,0.0768,0.077,0,0,0,609.9,329.2);

	this.timeline.addTween(cjs.Tween.get({}).to({state:[{t:this.instance_5},{t:this.instance_4},{t:this.instance_3},{t:this.instance_2},{t:this.instance_1}]}).wait(81));

	// Жук
	this.instance_6 = new lib.Жук_живой();
	this.instance_6.setTransform(-107.7,578.95,0.1794,0.2052,0,0,0,609.6,329.9);

	this.timeline.addTween(cjs.Tween.get(this.instance_6).to({regX:609.9,regY:329.4,scaleX:0.1465,scaleY:0.1456,rotation:79.28,guide:{path:[-107.6,578.9,-25.7,583.2,24.8,570.9,88.5,555.3,117.1,511.1,124.7,499.5,145.1,462.6,162.1,431.7,174.4,415.7,192.4,392.1,213.1,380.3,237.7,366.2,269.4,366.2,283.7,366.2,291.9,386.2,299.1,403.8,302.2,438.8,304.7,466.2,305.3,513.3,305.7,540.5,305.9,598.6,306.4,650.2,308.7,684,311.8,728.6,318.6,758.5,335.3,831.1,378.7,831.1,430.2,831.1,462.6,806.7,491.4,784.9,506.7,742.1,519.4,706.6,524.4,650.9,527.3,618.9,529.3,546.3,531.2,476.3,534.7,441.8,540.3,386.3,554,350.6,570.4,307.9,600.7,286.1,634.7,261.6,688.2,261.6,703.8,261.6,726.7,300.6,745.7,333,767.7,388.7,785.8,434.2,800.9,482.4,814.6,525.7,816.1,538.6,818.2,556.9,824,593.6,832,643.7,840.6,685.4,853.4,747.6,864.9,778.7,876.4,809.8,886.6,809.8,887.5,809.8,916.2,798.7,950.6,785.4,981.7,772.4,1078.1,732.1,1090,717.2,1100.4,704.4,1106.9,689.1,1112.6,675.9,1116.8,657.1,1119.3,645.7,1124,618.1,1129.2,588.1,1133.1,569.6,1136.2,554.2,1136.3,501,1136.4,433.3,1138.2,405.3,1141.5,349.5,1152.8,320.1,1167,283.3,1194.7,283.3,1280.7,283.3,1336.9,365.6,1360.3,399.9,1379.8,449.8,1387.5,469.7,1395,493.4], orient:'fixed'}},40).to({regX:610.6,rotation:67.9945,guide:{path:[1395,493.4,1387.4,469.8,1379.7,449.9,1361.3,402.4,1339.3,369.1], orient:'fixed'}},3).to({regX:609.9,rotation:79.28,guide:{path:[1339.3,369.1,1361.3,402.4,1379.7,449.9,1387.4,469.8,1395,493.4], orient:'fixed'}},2).to({regX:609.8,scaleX:0.1309,scaleY:0.1173,rotation:0,guide:{path:[1394.9,493.4,1402.8,518,1410.4,546.6,1418.1,575.5,1433.9,643.3,1447.4,700.9,1455.4,727.6,1467.3,767.8,1479.7,787.2,1494.1,809.9,1512,809.9,1562,809.9,1597.4,732.1,1625.9,669.5,1643.9,558.6,1656.8,479.2,1663.7,379.2,1665.8,349.2,1667.4,315.2,1668.8,284.8,1668.9,283.4,1688.9,283.4,1708.8,283.4,1745.9,305.8,1765.4,341.7,1781.3,370.9,1792,423.2,1795.3,439.3,1801.2,473.9,1807.8,511.7,1811.7,531.8,1826.2,606.7,1847.6,671.2,1859.2,661.9,1876.9,640.7,1908.1,603.4,1908.4,603,1927.9,581.1,1945,568.5,1966.8,552.6,1989.3,547.9], orient:'fixed'}},23).to({_off:true},1).wait(12));

	this._renderFirstFrame();

}).prototype = p = new lib.AnMovieClip();
p.nominalBounds = new cjs.Rectangle(742.9,462.7,1326.2999999999997,706.5);
// library properties:
lib.properties = {
	id: '3C407B6F6CF3B34484E50DFC54112A6C',
	width: 1920,
	height: 1080,
	fps: 30,
	color: "#FFFFFF",
	opacity: 1.00,
	manifest: [
		{src:"sounds/bigboom.mp3?1759900840927", id:"bigboom"},
		{src:"sounds/boom.mp3?1759900840927", id:"boom"},
		{src:"sounds/run.mp3?1759900840927", id:"run"}
	],
	preloads: []
};



// bootstrap callback support:

(lib.Stage = function(canvas) {
	createjs.Stage.call(this, canvas);
}).prototype = p = new createjs.Stage();

p.setAutoPlay = function(autoPlay) {
	this.tickEnabled = autoPlay;
}
p.play = function() { this.tickEnabled = true; this.getChildAt(0).gotoAndPlay(this.getTimelinePosition()) }
p.stop = function(ms) { if(ms) this.seek(ms); this.tickEnabled = false; }
p.seek = function(ms) { this.tickEnabled = true; this.getChildAt(0).gotoAndStop(lib.properties.fps * ms / 1000); }
p.getDuration = function() { return this.getChildAt(0).totalFrames / lib.properties.fps * 1000; }

p.getTimelinePosition = function() { return this.getChildAt(0).currentFrame / lib.properties.fps * 1000; }

an.bootcompsLoaded = an.bootcompsLoaded || [];
if(!an.bootstrapListeners) {
	an.bootstrapListeners=[];
}

an.bootstrapCallback=function(fnCallback) {
	an.bootstrapListeners.push(fnCallback);
	if(an.bootcompsLoaded.length > 0) {
		for(var i=0; i<an.bootcompsLoaded.length; ++i) {
			fnCallback(an.bootcompsLoaded[i]);
		}
	}
};

an.compositions = an.compositions || {};
an.compositions['3C407B6F6CF3B34484E50DFC54112A6C'] = {
	getStage: function() { return exportRoot.stage; },
	getLibrary: function() { return lib; },
	getSpriteSheet: function() { return ss; },
	getImages: function() { return img; }
};

an.compositionLoaded = function(id) {
	an.bootcompsLoaded.push(id);
	for(var j=0; j<an.bootstrapListeners.length; j++) {
		an.bootstrapListeners[j](id);
	}
}

an.getComposition = function(id) {
	return an.compositions[id];
}


an.makeResponsive = function(isResp, respDim, isScale, scaleType, domContainers) {		
	var lastW, lastH, lastS=1;		
	window.addEventListener('resize', resizeCanvas);		
	resizeCanvas();		
	function resizeCanvas() {			
		var w = lib.properties.width, h = lib.properties.height;			
		var iw = window.innerWidth, ih=window.innerHeight;			
		var pRatio = window.devicePixelRatio || 1, xRatio=iw/w, yRatio=ih/h, sRatio=1;			
		if(isResp) {                
			if((respDim=='width'&&lastW==iw) || (respDim=='height'&&lastH==ih)) {                    
				sRatio = lastS;                
			}				
			else if(!isScale) {					
				if(iw<w || ih<h)						
					sRatio = Math.min(xRatio, yRatio);				
			}				
			else if(scaleType==1) {					
				sRatio = Math.min(xRatio, yRatio);				
			}				
			else if(scaleType==2) {					
				sRatio = Math.max(xRatio, yRatio);				
			}			
		}
		domContainers[0].width = w * pRatio * sRatio;			
		domContainers[0].height = h * pRatio * sRatio;
		domContainers.forEach(function(container) {				
			container.style.width = w * sRatio + 'px';				
			container.style.height = h * sRatio + 'px';			
		});
		stage.scaleX = pRatio*sRatio;			
		stage.scaleY = pRatio*sRatio;
		lastW = iw; lastH = ih; lastS = sRatio;            
		stage.tickOnUpdate = false;            
		stage.update();            
		stage.tickOnUpdate = true;		
	}
}
an.handleSoundStreamOnTick = function(event) {
	if(!event.paused){
		var stageChild = stage.getChildAt(0);
		if(!stageChild.paused || stageChild.ignorePause){
			stageChild.syncStreamSounds();
		}
	}
}
an.handleFilterCache = function(event) {
	if(!event.paused){
		var target = event.target;
		if(target){
			if(target.filterCacheList){
				for(var index = 0; index < target.filterCacheList.length ; index++){
					var cacheInst = target.filterCacheList[index];
					if((cacheInst.startFrame <= target.currentFrame) && (target.currentFrame <= cacheInst.endFrame)){
						cacheInst.instance.cache(cacheInst.x, cacheInst.y, cacheInst.w, cacheInst.h);
					}
				}
			}
		}
	}
}


})(createjs = createjs||{}, AdobeAn = AdobeAn||{});
var createjs, AdobeAn;