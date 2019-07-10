var CuteUDP = require("./CuteUDP/CuteUDP");
var cuteEvent = require("./cuteEvent");

var cuteUDP = new CuteUDP("127.0.0.1", 10000, 11000);

CuteUDP.prototype.instance.on("login", ()=>{console.log("lgdd")});

// console.log(cuteUDP);