

namespace RequestAppUI.Pages;

 public partial class Index
 {
     private UserModel loggedInUser;
     private List<RequestModel> requests;
     private List<CategoryModel> categories;
     private List<StatusModel> statuses;
     private RequestModel archivingRequest;
     private string selectedCategory = "All";
     private string selectedStatus = "All";
     private string searchText = "";
     private bool isSortedByNew = true;
     private bool showCategories = false;
     private bool showStatuses = false;
     protected async override Task OnInitializedAsync()
     {
         categories = await categoryData.GetAllCategories();
         statuses = await statusData.GetAllStatuses();
         await LoadAndVerifyUser();
     }

     private async Task ArchiveRequest()
     {
         archivingRequest.Archived = true;
         await requestData.UpdateRequest(archivingRequest);
         requests.Remove(archivingRequest);
         archivingRequest = null;
         await FilterRequests();
     }

     private void LoadCreatePage()
     {
         if (loggedInUser is not null)
         {
             navManager.NavigateTo("/Create");
         }
         else
         {
             navManager.NavigateTo("/MicrosoftIdentity/Account/SignIn", true);
         }
     }

     private async Task LoadAndVerifyUser()
     {
         var authState = await authProvider.GetAuthenticationStateAsync();
         string objectId = authState.User.Claims.FirstOrDefault(c => c.Type.Contains("objectidentifier"))?.Value;
         if (string.IsNullOrWhiteSpace(objectId) == false)
         {
             loggedInUser = await userData.GetUserFromAuthentication(objectId) ?? new();
             string firstName = authState.User.Claims.FirstOrDefault(c => c.Type.Contains("givenname"))?.Value;
             string lastName = authState.User.Claims.FirstOrDefault(c => c.Type.Contains("surname"))?.Value;
             string displayName = authState.User.Claims.FirstOrDefault(c => c.Type.Equals("name"))?.Value;
             string emailAddress = authState.User.Claims.FirstOrDefault(c => c.Type.Contains("email"))?.Value;
             bool isDirty = false;
             if (objectId.Equals(loggedInUser.ObjectIdentifier) == false)
             {
                 isDirty = true;
                 loggedInUser.ObjectIdentifier = objectId;
             }

             if (firstName.Equals(loggedInUser.FirstName) == false)
             {
                 isDirty = true;
                 loggedInUser.FirstName = firstName;
             }

             if (lastName.Equals(loggedInUser.LastName) == false)
             {
                 isDirty = true;
                 loggedInUser.LastName = lastName;
             }

             if (displayName.Equals(loggedInUser.DisplayName) == false)
             {
                 isDirty = true;
                 loggedInUser.DisplayName = displayName;
             }

             if (emailAddress.Equals(loggedInUser.EmailAddress) == false)
             {
                 isDirty = true;
                 loggedInUser.EmailAddress = emailAddress;
             }

             if (isDirty)
             {
                 if (string.IsNullOrWhiteSpace(loggedInUser.Id))
                 {
                     await userData.CreateUser(loggedInUser);
                 }
                 else
                 {
                     await userData.UpdateUser(loggedInUser);
                 }
             }
         }
     }

     protected async override Task OnAfterRenderAsync(bool firstRender)
     {
         if (firstRender)
         {
             await LoadFilterState();
             await FilterRequests();
             StateHasChanged();
         }
     }

     private async Task LoadFilterState()
     {
         var stringResults = await sessionStorage.GetAsync<string>(nameof(selectedCategory));
         selectedCategory = stringResults.Success ? stringResults.Value : "All";
         stringResults = await sessionStorage.GetAsync<string>(nameof(selectedStatus));
         selectedStatus = stringResults.Success ? stringResults.Value : "All";
         stringResults = await sessionStorage.GetAsync<string>(nameof(searchText));
         searchText = stringResults.Success ? stringResults.Value : "";
         var boolResults = await sessionStorage.GetAsync<bool>(nameof(isSortedByNew));
         isSortedByNew = boolResults.Success ? boolResults.Value : true;
     }

     private async Task SaveFilterState()
     {
         await sessionStorage.SetAsync(nameof(selectedCategory), selectedCategory);
         await sessionStorage.SetAsync(nameof(selectedStatus), selectedStatus);
         await sessionStorage.SetAsync(nameof(searchText), searchText);
         await sessionStorage.SetAsync(nameof(isSortedByNew), isSortedByNew);
     }

     private async Task FilterRequests()
     {
         var output = await requestData.GetAllApprovedRequests();
         if (selectedCategory != "All")
         {
             output = output.Where(r => r.Category?.CategoryName == selectedCategory).ToList();
         }

         if (selectedStatus != "All")
         {
             output = output.Where(r => r.RequestStatus?.StatusName == selectedStatus).ToList();
         }

         // For search text on actual request and search text on description
         if (string.IsNullOrWhiteSpace(searchText) == false)
         {
             output = output.Where(r => r.Request.Contains(searchText,
                StringComparison.InvariantCultureIgnoreCase) || r.Description.Contains(searchText,
                StringComparison.InvariantCultureIgnoreCase)).ToList();
         }

         if (isSortedByNew)
         {
             output = output.OrderByDescending(r => r.DateCreated).ToList();
         }
         else
         {
             output = output.OrderByDescending(r => r.UserVotes.Count).ThenByDescending(r => r.DateCreated).ToList();
         }

         requests = output;
         await SaveFilterState();
     }

     private async Task OrderByNew(bool isNew)
     {
         isSortedByNew = isNew;
         await FilterRequests();
     }

     private async Task OnSearchInput(string searchInput)
     {
         searchText = searchInput;
         await FilterRequests();
     }

     private async Task OnCategoryClick(string category = "All")
     {
         selectedCategory = category;
         showCategories = false;
         await FilterRequests();
     }

     private async Task OnStatusClick(string status = "All")
     {
         selectedStatus = status;
         showStatuses = false;
         await FilterRequests();
     }

     private async Task VoteUp(RequestModel request)
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
             if (isSortedByNew == false)
             {
                 requests = requests.OrderByDescending(r => r.UserVotes.Count).ThenByDescending(r => r.DateCreated).ToList();
             }
         }
         else
         {
             navManager.NavigateTo("/MicrosoftIdentity/Account/SignIn", true);
         }
     }

     private string GetUpvoteTopText(RequestModel request)
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

     private string GetUpvoteBottomText(RequestModel request)
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

     private void OpenDetails(RequestModel request)
     {
         navManager.NavigateTo($"/Details/{request.Id}");
     }

     private string SortedByNewClass(bool isNew)
     {
         if (isNew == isSortedByNew)
         {
             return "sorted-selected";
         }
         else
         {
             return "";
         }
     }

     private string GetVoteClass(RequestModel request)
     {
         if (request.UserVotes is null || request.UserVotes.Count == 0)
         {
             return "request-entry-no-votes";
         }
         else if (request.UserVotes.Contains(loggedInUser?.Id))
         {
             return "request-entry-voted";
         }
         else
         {
             return "request-entry-not-voted";
         }
     }

     private string GetRequestStatusClass(RequestModel request)
     {
         if (request is null || request.RequestStatus is null)
         {
             return "request-entry-status-none";
         }

         string output = request.RequestStatus.StatusName switch
         {
             "Completed" => "request-entry-status-completed",
             "Watching" => "request-entry-status-watching",
             "Upcoming" => "request-entry-status-upcoming",
             "Dismissed" => "request-entry-status-dismissed",
             _ => "request-entry-status-none",
         };
         return output;
     }

     private string GetSelectedCategory(string category = "All")
     {
         if (category == selectedCategory)
         {
             return "selected-category";
         }
         else
         {
             return "";
         }
     }

     private string GetSelectedStatus(string status = "All")
     {
         if (status == selectedStatus)
         {
             return "selected-status";
         }
         else
         {
             return "";
         }
     }
 }
