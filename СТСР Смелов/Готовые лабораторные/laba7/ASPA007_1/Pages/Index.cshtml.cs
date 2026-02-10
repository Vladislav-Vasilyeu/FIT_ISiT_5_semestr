using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DAL_Celebrity_MSSQL;
using System.Data.Common;
using DAL_Celebrity_MSSQL;
using DAL_Celebrity;
namespace ASPA007_1.Pages;

public class IndexModel : PageModel
{
    public List<Celebrity> cs = new List<Celebrity>();
   
    public IndexModel()
    {
        
    }

    public void OnGet()
    {
        using (var db = new Repository(AppConfig.ConnectionString)) {
            cs = db.GetAllCelebrities();
        }
    }
}
