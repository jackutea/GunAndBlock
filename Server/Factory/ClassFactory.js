var LoginSendInfo = require("./LoginSendInfo");
var RoomInfo = require("./RoomInfo");
var RoomSendInfo = require("./RoomSendInfo");
var RoleListSendInfo = require("./RoleListSendInfo");
var ServerSendInfo = require("./ServerSendInfo");
var ServerInfo = require("./ServerInfo");
var AccountState = require("./AccountState");
var RoleState = require("./RoleState");

var ClassFactory = {

    LoginSendInfo : LoginSendInfo,

    RoomInfo : RoomInfo,

    RoomSendInfo : RoomSendInfo,

    RoleListSendInfo : RoleListSendInfo,

    ServerSendInfo : ServerSendInfo,

    ServerInfo : ServerInfo,

    AccountState : AccountState,

    RoleState : RoleState

}

module.exports = ClassFactory;