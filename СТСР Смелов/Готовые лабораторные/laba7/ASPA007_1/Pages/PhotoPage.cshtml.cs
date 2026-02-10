using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DAL_Celebrity;
using DAL_Celebrity_MSSQL;
namespace ASPA007_1.Pages
{
    public class PhotoPageModel : PageModel
    {
        public Celebrity celebrity { get; set; }
        public void OnGet(int id)
        {
            using (var db = new Repository(AppConfig.ConnectionString)) {
                celebrity = db.GetCelebrityById(id);
            } 
        }
    }
}
