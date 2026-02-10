using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DAL003
{
    public class Repository : IRepository
    {
        public static string JSONFileName { get; set; } = "Celebrities.json";
        private readonly string _basePath;
        private Celebrity[] _celebrities;

        public string BasePath => _basePath;

        public static IRepository Create(string basePath)
        {
            return new Repository(basePath);
        }
        private Repository(string basePath)
        {
            _basePath = Path.Combine(Directory.GetCurrentDirectory(), basePath);
            if(!Directory.Exists(_basePath))
            {
               Directory.CreateDirectory(_basePath);
                
            }
            LoadCelebrities();
        }
        private void LoadCelebrities()
        {
            string jsonFile = Path.Combine(_basePath, JSONFileName);
            if(!File.Exists(jsonFile))
                throw new FileNotFoundException("JSON file not found", jsonFile);

            string jsonContent = File.ReadAllText(jsonFile);
            _celebrities = JsonSerializer.Deserialize<Celebrity[]>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? Array.Empty<Celebrity>();
        }
        public Celebrity[] getAllCelebrities() => _celebrities;
        public Celebrity? getCelebrityById(int id)
        {
            return getAllCelebrities().FirstOrDefault(c => c.Id == id);
        }
        public Celebrity[] getCelebritiesBySurname(string surname)
        {
            return getAllCelebrities().Where(c => c.Surname.Equals(surname, StringComparison.OrdinalIgnoreCase)).ToArray();
        }
        public string? getPhotoPathById(int id)
        {
            var celebrity = getCelebrityById(id);
            return celebrity?.PhotoPath;
        }
        public void Dispose()
        {
            
            GC.SuppressFinalize(this);
        }
    }
}
