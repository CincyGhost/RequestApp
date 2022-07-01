

namespace RequestAppUI.Pages;

 public partial class Profile
 {
     private UserModel loggedInUser;
     private List<RequestModel> submissions;
     private List<RequestModel> approved;
     private List<RequestModel> archived;
     private List<RequestModel> pending;
     private List<RequestModel> rejected;
     protected async override Task OnInitializedAsync()
     {
         loggedInUser = await authProvider.GetUserFromAuth(userData);
         var results = await requestData.GetUsersRequests(loggedInUser.Id);
         if (loggedInUser is not null && results is not null)
         {
             submissions = results.OrderByDescending(r => r.DateCreated).ToList();
             approved = submissions.Where(r => r.ApprovedForRelease && r.Archived == false & r.Rejected == false).ToList();
             archived = submissions.Where(r => r.Archived && r.Rejected == false).ToList();
             pending = submissions.Where(r => r.ApprovedForRelease == false && r.Rejected == false).ToList();
             rejected = submissions.Where(r => r.Rejected).ToList();
         }
     }

     private void ClosePage()
     {
         navManager.NavigateTo("/");
     }
 }
