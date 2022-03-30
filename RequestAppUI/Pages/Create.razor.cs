using RequestAppUI.Models;

namespace RequestAppUI.Pages;

 public partial class Create
 {
     private CreateRequestModel request = new();
     private List<CategoryModel> categories;
     private UserModel loggedInUser;
     protected async override Task OnInitializedAsync()
     {
         categories = await categoryData.GetAllCategories();
         loggedInUser = await authProvider.GetUserFromAuth(userData);
     }

     private void ClosePage()
     {
         navManager.NavigateTo("/");
     }

     private async Task CreateRequest()
     {
         RequestModel r = new();
         r.Request = request.Request;
         r.Description = request.Description;
         r.Author = new BasicUserModel(loggedInUser);
         r.Category = categories.Where(c => c.Id == request.CategoryId).FirstOrDefault();
         if (r.Category is null)
         {
             request.CategoryId = "";
             return;
         }

         await requestData.CreateRequest(r);
         request = new();
         ClosePage();
     }
 }
