using DAL_Celebrity;
using DAL_Celebrity_MSSQL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.FileProviders;
using System;

namespace ASPA007_1.Pages
{
    public class _0Model : PageModel
    {
        public bool isCorrect = false;

        public string _name;
        public string _nation;
        public string _filePath;
        public void OnGet()
        {

        }
        public void OnPostRender(string name, string nation, IFormFile photo)
        {
            if (name != null && name.Length > 0 &&
                nation != null && nation.Length > 0 &&
                photo != null && photo.Length > 0
                ) {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "photo");
                Directory.CreateDirectory(uploadsFolder);
                string filePath = Path.Combine(uploadsFolder, photo.FileName);
                string safeFileName = Path.GetFileName(photo.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    photo.CopyTo(stream);
                }
                _name = name;
                _nation = nation;
                _filePath = $"/photo/{safeFileName}";
                TempData["FilePath"] = _filePath;
                isCorrect = true;
            }
        }
        public void OnPostSave(string namesave, string nationsave) {
            _filePath = TempData["FilePath"]?.ToString();
            if (namesave != null && namesave.Length > 0 &&
                nationsave != null && nationsave.Length > 0 &&
                _filePath != null && _filePath.Length > 0
                )
            {
                using (var db = new Repository(AppConfig.ConnectionString))
                {
                    Celebrity cel = new Celebrity();
                    cel.FullName = namesave;
                    cel.Nationality = nationsave;
                    cel.ReqPhotoPath = Path.GetFileName(_filePath);
                    if (!db.AddCelebrity(cel))
                    {
                        ModelState.AddModelError("", "Не удалось сохранить данные");
                    }
                }
                _name = "";
                _nation = "";
                _filePath = "";
                isCorrect = false;
            }
        }

        public IActionResult OnPostCancel() {
            _filePath = TempData["FilePath"]?.ToString();
            if (!string.IsNullOrEmpty(_filePath))
            {
                string fullPath = Path.Combine(Directory.GetCurrentDirectory(),
                                            "wwwroot",
                                            _filePath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }
            _name = "";
            _nation = "";
            _filePath = "";
            isCorrect = false;
            return RedirectToPage();
        }
    }
}
