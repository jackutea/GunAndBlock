var LoginSendInfo = require("./LoginSendInfo");
var RoomInfo = require("./RoomInfo");
var RoleListSendInfo = require("./RoleListSendInfo");
var ServerSendInfo = require("./ServerSendInfo");
var ServerInfo = require("./ServerInfo");
var AccountState = require("./AccountState");
var RoleState = require("./RoleState");

class Factory {}

Factory.LoginSendInfo = LoginSendInfo,

Factory.RoomInfo = RoomInfo,

Factory.RoleListSendInfo = RoleListSendInfo,

Factory.ServerSendInfo = ServerSendInfo,

Factory.ServerInfo = ServerInfo,

Factory.AccountState = AccountState,

Factory.RoleState = RoleState

module.exports = Factory;