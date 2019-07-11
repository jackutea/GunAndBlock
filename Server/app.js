var CuteUDP = require("./CuteUDP/CuteUDP");
var cuteEvent = require("./cuteEvent");

var cuteUDP = new CuteUDP("127.0.0.1", 10000, 11000);

cuteUDP.on("login", cuteEvent.login);

// console.log(cuteUDP);