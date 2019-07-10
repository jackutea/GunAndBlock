
var mongoPool = require("./mongoPool");

var dataBaseName = "troubledhuman";

var MongoDB = {

    conn : function(){},

    createCollection : function(){},

    drop : function(){},

    insertOne : function(){},

    insertMany : function(){},

    find : function(){},

    findOne : function(){},

    updateOne : function(){},

    updateMany : function(){},

    deleteOne : function(){},

    deleteMany : function(){}

}

MongoDB.createCollection = function (collectionName, callback) {

    mongoPool.getInstance ( (db) => {

        var dataBase = db.db(dataBaseName)

        dataBase.createCollection(collectionName, function(err, result){

            if (err) throw err;

            callback(err, result)

        })
    })
}

MongoDB.drop = function(collectionName, callback){

    mongoPool.getInstance ( (db) => {

        var dataBase = db.db(dataBaseName)

        dataBase.collection(collectionName).drop(function(err, result){

            if (err) throw err;

            callback(err, result)

        })
    })
}

MongoDB.insertOne = function(collectionName, obj, callback){

    mongoPool.getInstance ( (db) => {

        var dataBase = db.db(dataBaseName);

        dataBase.collection(collectionName).insertOne(obj, function(err, result){

            if (err) throw err;

            callback(err, result)

        })
    })
}

MongoDB.insertMany = function(collectionName, objArray, callback){

    mongoPool.getInstance ( (db) => {

        var dataBase = db.db(dataBaseName);

        dataBase.collection(collectionName).insertMany(objArray, function(err, result){

            if (err) throw err;

            callback(err, result)

        })
    })
}

MongoDB.find = function(collectionName, obj, callback){

    mongoPool.getInstance ( (db) => {

        var dataBase = db.db(dataBaseName);

        dataBase.collection(collectionName).find(obj).toArray(function(err, result){

            if (err) {

                console.log(err)

            }

            callback(err, result)

        })
    })
}

MongoDB.findOne = function(collectionName, obj, callback){

    mongoPool.getInstance ( (db) => {

        var dataBase = db.db(dataBaseName);

        dataBase.collection(collectionName).findOne(obj, function(err, result){

            if (err) {

                console.log(err)

            }

            callback(err, result)

        })
    })
}

MongoDB.updateOne = function(collectionName, oldObj, newObj, callback){

    mongoPool.getInstance ( (db) => {

        var dataBase = db.db(dataBaseName);

        let upObj = {$set : newObj};

        dataBase.collection(collectionName).updateOne(oldObj, upObj, function(err, result){

            if (err) throw err;

            callback(err, result)

        })
    })
}

MongoDB.updateMany = function (collectionName, oldObj, newObj, callback){

    mongoPool.getInstance ( (db) => {

        var dataBase = db.db(dataBaseName);

        let upObj = {$set : newObj};

        dataBase.collection(collectionName).updateOne(oldObj, upObj, function(err, result){

            if (err) throw err;

            callback(err, result)

        })
    })
}

MongoDB.deleteOne = function (collectionName, obj, callback){

    mongoPool.getInstance ( (db) => {

        var dataBase = db.db(dataBaseName);

        dataBase.collection(collectionName).deleteOne(obj, function(err, result){

            if (err) throw err;

            callback(err, result)

        })
    })
}

MongoDB.deleteMany = function (collectionName, obj, callback) {

    mongoPool.getInstance ( (db) => {

        var dataBase = db.db(dataBaseName);

        dataBase.collection(collectionName).deleteOne(obj, function(err, result){

            if (err) throw err;

            callback(err, result)

        })
    })
}

module.exports = MongoDB;