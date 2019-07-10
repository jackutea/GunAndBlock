const MongoClient = require("mongodb").MongoClient;
// const url = "mongodb://root:Adol0609@47.104.169.23:27017/?authSource=admin"

// const url = "mongodb://root:Adol0609@localhost:27017/?authSource=admin"
const url = "mongodb://localhost:27017/"

var option = {
    reconnectTries : 10,
    auto_reconnect : true,
    poolSize : 40,
    connectTimeoutMS : 500,
    useNewUrlParser : true,
}

function MongoPool () {}

var pool;

function initPool (callback) {
    MongoClient.connect(url, option, (err, db) => {
        if (err) throw err;
        pool = db;
        if (callback && typeof(callback) == "function") {
            callback(pool)
        }
    });
    return MongoPool
}

MongoPool.initPool = initPool;

function getInstance (callback) {
    if (!pool) {
        initPool(callback)
    } else {
        if (callback && typeof(callback) == "function") {
            callback(pool);
        }
    }
}

MongoPool.getInstance = getInstance;

module.exports = MongoPool;