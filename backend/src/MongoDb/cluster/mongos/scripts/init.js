var dbName = "appDb";
var namespace = dbName + ".AuctionsReadModel";

db.auth("auctionhouse", "Test-1234")

sh.addShard("n1/db-node1:27018");
sh.addShard("n2/db-node2:27018");
sh.addShard("n3/db-node3:27018");
sh.enableSharding(dbName);
sh.shardCollection(namespace, {"Category.Category0Id": 1, "Category.Category1Id": 1, "Category.Category2Id": 1})
load('/scripts/categories.js');
for (const cat of categories) {
    for (const subCat of cat["sub-categories"]){
        sh.splitAt(namespace, {"Category.Category0Id": cat.id, "Category.Category1Id": subCat.id, "Category.Category2Id": 0});
        var lastSub2Cat = subCat["sub-sub-categories"][subCat["sub-sub-categories"].length - 1];
        sh.splitAt(namespace, {"Category.Category0Id": cat.id, "Category.Category1Id": subCat.id, "Category.Category2Id": lastSub2Cat.id});
        print("shard split [" + cat.id + " " + subCat.id + " 0 ] --> [" + cat.id + " " + subCat.id + " " + lastSub2Cat.id + " ]" );
    }
}
