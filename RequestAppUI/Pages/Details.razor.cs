using Microsoft.AspNetCore.Components;

namespace RequestAppUI.Pages;

 public partial class Details
 {
     [Parameter]
     public string Id { get; set; }

     private RequestModel request;
     private UserModel loggedInUser;
     private List<StatusModel> statuses;
     private string settingStatus = "";
     private string urlText = "";
     protected async override Task OnInitializedAsync()
     {
         request = await requestData.GetRequest(Id);
         loggedInUser = await authProvider.GetUserFromAuth(userData);
         statuses = await statusData.GetAllStatuses();
     }

     private async Task CompleteSetStatus()
     {
         switch (settingStatus)
         {
             case "completed":
                 if (string.IsNullOrWhiteSpace(urlText))
                 {
                     return;
                 }

                 request.RequestStatus = statuses.Where(r => r.StatusName.ToLower() == settingStatus.ToLower()).First();
                 request.OwnerNotes = $"Good choice, this is a great idea for a program. There is a resource for it here: <a href='{urlText}' target='_blank'>{urlText}</a>";
                 break;
             case "watching":
                 request.RequestStatus = statuses.Where(r => r.StatusName.ToLower() == settingStatus.ToLower()).First();
                 request.OwnerNotes = "This request is getting some attention...If more people vote we may implement this.";
                 break;
             case "upcoming":
                 request.RequestStatus = statuses.Where(r => r.StatusName.ToLower() == settingStatus.ToLower()).First();
                 request.OwnerNotes = "Good idea! We are currently working on this.";
                 break;
             case "dismissed":
                 request.RequestStatus = statuses.Where(r => r.StatusName.ToLower() == settingStatus.ToLower()).First();
                 request.OwnerNotes = "For one reason or another, we will not be implementing this idea.";
                 break;
             default:
                 return;
         }

         settingStatus = null;
         await requestData.UpdateRequest(request);
     }

     private void ClosePage()
     {
         navManager.NavigateTo("/");
     }

     private string GetUpvoteTopText()
     {
         if (request.UserVotes?.Count > 0)
         {
             return request.UserVotes.Count.ToString("00");
         }
         else
         {
             if (request.Author.Id == loggedInUser?.Id)
             {
                 return "Awaiting";
             }
             else
             {
                 return "Click To";
             }
         }
     }

     private string GetUpvoteBottomText()
     {
         if (request.UserVotes?.Count > 1)
         {
             return "Upvotes";
         }
         else
         {
             return "Upvote";
         }
     }

     private async Task VoteUp()
     {
         if (loggedInUser is not null)
         {
             if (request.Author.Id == loggedInUser.Id)
             {
                 // Can't vote on your own request
                 return;
             }

             if (request.UserVotes.Add(loggedInUser.Id) == false)
             {
                 request.UserVotes.Remove(loggedInUser.Id);
             }

             await requestData.UpvoteRequest(request.Id, loggedInUser.Id);
         }
         else
         {
             navManager.NavigateTo("/MicrosoftIdentity/AccountSignIn", true);
         }
     }

     private string GetVoteClass()
     {
         if (request.UserVotes is null || request.UserVotes.Count == 0)
         {
             return "request-detail-no-votes";
         }
         else if (request.UserVotes.Contains(loggedInUser?.Id))
         {
             return "request-detail-voted";
         }
         else
         {
             return "request-detail-not-voted";
         }
     }

     private string GetStatusClass()
     {
         if (request is null || request.RequestStatus is null)
         {
             return "request-detail-status-none";
         }

         string output = request.RequestStatus.StatusName switch
         {
             "Completed" => "request-detail-status-completed",
             "Watching" => "request-detail-status-watching", //Was "Watching"
             "Upcoming" => "request-detail-status-upcoming",
             "Dismissed" => "request-detail-status-dismissed", //Was "Dismissed"
             _ => "request-detail-status-none",
         };
         return output;
     }
 }
