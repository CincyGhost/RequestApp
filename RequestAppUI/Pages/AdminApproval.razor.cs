

namespace RequestAppUI.Pages;

 public partial class AdminApproval
 {
     private List<RequestModel> submissions;
     private RequestModel editingModel;
     private string currentEditingTitle = "";
     private string editedTitle = "";
     private string currentEditingDescription = "";
     private string editedDescription = "";
     protected async override Task OnInitializedAsync()
     {
         submissions = await requestData.GetAllRequestsWaitingForApproval();
     }

     private async Task ApproveSubmission(RequestModel submission)
     {
         submission.ApprovedForRelease = true;
         submissions.Remove(submission);
         await requestData.UpdateRequest(submission);
     }

     private async Task RejectSubmission(RequestModel submission)
     {
         submission.Rejected = true;
         submissions.Remove(submission);
         await requestData.UpdateRequest(submission);
     }

     private void EditTitle(RequestModel model)
     {
         editingModel = model;
         editedTitle = model.Request;
         currentEditingTitle = model.Id;
         currentEditingDescription = "";
     }

     private async Task SaveTitle(RequestModel model)
     {
         currentEditingTitle = string.Empty;
         model.Request = editedTitle;
         await requestData.UpdateRequest(model);
     }

     private void ClosePage()
     {
         navManager.NavigateTo("/");
     }

     private void EditDescription(RequestModel model)
     {
         editingModel = model;
         editedDescription = model.Description;
         currentEditingTitle = "";
         currentEditingDescription = model.Id;
     }

     private async Task SaveDescription(RequestModel model)
     {
         currentEditingDescription = string.Empty;
         model.Description = editedDescription;
         await requestData.UpdateRequest(model);
     }
 }
