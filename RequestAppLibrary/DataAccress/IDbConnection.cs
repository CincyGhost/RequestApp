using MongoDB.Driver;

namespace RequestAppLibrary.DataAccess;

public interface IDbConnection
{
   IMongoCollection<CategoryModel> CategoryCollection { get; }
   string CategoryCollectionName { get; }
   MongoClient Client { get; }
   string DbName { get; }
   IMongoCollection<StatusModel> StatusCollection { get; }
   string StatusCollectionName { get; }
   IMongoCollection<RequestModel> RequestCollection { get; }
   string RequestCollectionName { get; }
   IMongoCollection<UserModel> UserCollection { get; }
   string UserCollectionName { get; }
}