

namespace RequestAppLibrary.Models;
public class BasicRequestModel
{

   [BsonRepresentation(BsonType.ObjectId)]

   public string Id { get; set; }

   public string Request { get; set; }

   public BasicRequestModel()
   {

   }

   public BasicRequestModel(RequestModel request)
   {
      Id = request.Id;
      Request = request.Request;

   }
}
