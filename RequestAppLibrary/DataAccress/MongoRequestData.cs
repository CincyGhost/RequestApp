

namespace RequestAppLibrary.DataAccess;
public class MongoRequestData : IRequestData
{
   private readonly IMongoCollection<RequestModel> _requests;
   private readonly IDbConnection _db;
   private readonly IUserData _userData;
   private readonly IMemoryCache _cache;
   private const string CacheName = "RequestData";

   public MongoRequestData(IDbConnection db,
                              IUserData userData,
                              IMemoryCache cache)
   {
      _db = db;
      _userData = userData;
      _cache = cache;
      _requests = db.RequestCollection;
   }

   public async Task<List<RequestModel>> GetAllRequests()
   {
      var output = _cache.Get<List<RequestModel>>(CacheName);
      if (output == null)
      {
         var results = await _requests.FindAsync(r => r.Archived == false);
         output = results.ToList();

         _cache.Set(CacheName, output, TimeSpan.FromMinutes(1));
      }

      return output;
   }

   public async Task<List<RequestModel>> GetUsersRequests(string userId)
   {
      var output = _cache.Get<List<RequestModel>>(userId);
      if (output is null)
      {
         var results = await _requests.FindAsync(r => r.Author.Id == userId);
         output = results.ToList();

         _cache.Set(userId, output, TimeSpan.FromMinutes(1));
      }
      return output;
   }

   public async Task<List<RequestModel>> GetAllApprovedRequests()
   {
      var output = await GetAllRequests();
      return output.Where(r => r.ApprovedForRelease).ToList();

   }

   public async Task<RequestModel> GetRequest(string id)
   {
      var results = await _requests.FindAsync(r => r.Id == id);
      return results.FirstOrDefault();

   }

   public async Task<List<RequestModel>> GetAllRequestsWaitingForApproval()
   {
      var output = await GetAllRequests();
      return output
         .Where(r => r.ApprovedForRelease == false
         && r.Rejected == false).ToList();
   }

   public async Task UpdateRequest(RequestModel request)
   {
      await _requests.ReplaceOneAsync(s => s.Id == request.Id, request);
      _cache.Remove(CacheName);
   }

   public async Task UpvoteRequest(string requestId, string userId)
   {
      var client = _db.Client;

      using var session = await client.StartSessionAsync();

      session.StartTransaction();

      try
      {
         var db = client.GetDatabase(_db.DbName);
         var requestsInTransaction = db.GetCollection<RequestModel>(_db.RequestCollectionName);
         var request = (await requestsInTransaction.FindAsync(r => r.Id == requestId)).First();

         bool isUpvote = request.UserVotes.Add(userId);
         if (isUpvote == false)
         {
            request.UserVotes.Remove(userId);
         }

         await requestsInTransaction.ReplaceOneAsync(session, r => r.Id == requestId, request);
         var usersInTransaction = db.GetCollection<UserModel>(_db.UserCollectionName);
         var user = await _userData.GetUser(userId);

         if (isUpvote)
         {
            user.VotedOnRequests.Add(new BasicRequestModel(request));
         }
         else
         {
            var requestToRemove = user.VotedOnRequests.Where(r => r.Id == requestId).First();
            user.VotedOnRequests.Remove(new BasicRequestModel(request));
         }
         await usersInTransaction.ReplaceOneAsync(session, u => u.Id == userId, user);

         await session.CommitTransactionAsync();

         _cache.Remove(CacheName);
      }
      catch (Exception ex)
      {
         await session.AbortTransactionAsync();
         throw;
      }
   }

   public async Task CreateRequest(RequestModel request)
   {
      var client = _db.Client;

      using var session = await client.StartSessionAsync();

      session.StartTransaction();

      try
      {
         var db = client.GetDatabase(_db.DbName);
         var requestsInTransaction = db.GetCollection<RequestModel>(_db.RequestCollectionName);
         await requestsInTransaction.InsertOneAsync(session, request);

         var usersInTransaction = db.GetCollection<UserModel>(_db.UserCollectionName);
         var user = await _userData.GetUser(request.Author.Id);
         user.AuthoredRequests.Add(new BasicRequestModel(request));
         await usersInTransaction.ReplaceOneAsync(session, u => u.Id == user.Id, user);

         await session.CommitTransactionAsync();

      }
      catch (Exception ex)
      {
         await session.AbortTransactionAsync();
         throw;
      }
   }
}
