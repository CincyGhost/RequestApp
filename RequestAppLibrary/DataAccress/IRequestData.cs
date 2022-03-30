
namespace RequestAppLibrary.DataAccess;

public interface IRequestData
{
   Task CreateRequest(RequestModel request);
   Task<List<RequestModel>> GetAllApprovedRequests();
   Task<List<RequestModel>> GetAllRequests();
   Task<List<RequestModel>> GetAllRequestsWaitingForApproval();
   Task<RequestModel> GetRequest(string id);
   Task<List<RequestModel>> GetUsersRequests(string userId);
   Task UpdateRequest(RequestModel request);
   Task UpvoteRequest(string requestId, string userId);
}