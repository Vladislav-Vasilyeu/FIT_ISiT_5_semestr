using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DAL004
{
    public class Repository : IRepository
    {
        public static string JSONFileName { get; set; } = "Celebrities.json";
        private readonly string _basePath;
        private Celebrity[] _celebrities;
        private int _changes = 0;

        public string BasePath => _basePath;

        public static IRepository Create(string basePath)
        {
            return new Repository(basePath);
        }
        private Repository(string basePath)
        {
            _basePath = Path.Combine(Directory.GetCurrentDirectory(), basePath);
            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);

            }
            _changes = 0;
            LoadCelebrities();
        }
        private void LoadCelebrities()
        {
            string jsonFile = Path.Combine(_basePath, JSONFileName);
            if (!File.Exists(jsonFile))
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
        public int? addCelebrity(Celebrity celebrity)
        {
            if(_celebrities == null) return null;

            int newId;
            if (celebrity.Id == 0)
            {
                newId = _celebrities.Length > 0 ? _celebrities.Max(c => c.Id) + 1 : 1;
            }
            else
            {
                newId = celebrity.Id;
                if (_celebrities.Any(c => c.Id == newId)) return null;
            }

            var newCelebrity = celebrity with { Id = newId };
            _celebrities = _celebrities.Append(newCelebrity).ToArray();
            _changes++;
            return newId;
        }
        public bool delCelebrityById(int id)
        {
            if(_celebrities == null || !_celebrities.Any(c => c.Id == id)) return false;
            _celebrities = _celebrities.Where(c => c.Id != id).ToArray();
            _changes++;
            return true;
        }
        public int? updCelebrityById(int id, Celebrity celebrity)
        {
            if (_celebrities == null || !_celebrities.Any(c => c.Id == id)) return null;

            int newId = celebrity.Id != 0 ? celebrity.Id : id;  // Если 0, оставить старый

            if (newId != id && _celebrities.Any(c => c.Id == newId)) return null;  // Дубликат нового Id

            // Обновляем
            _celebrities = _celebrities.Select(c => c.Id == id ? celebrity with { Id = newId } : c).ToArray();
            _changes++;
            return newId;  // Возвращаем новый Id (или старый, если не изменился)
        }
        public int SaveChanges()
        {
            if (_celebrities == null || _changes == 0) return 0;

            string jsonFile = Path.Combine(_basePath, JSONFileName);
            string jsonContent = JsonSerializer.Serialize(_celebrities, new JsonSerializerOptions
            {
                WriteIndented = true  // Для читаемости JSON
            });
            File.WriteAllText(jsonFile, jsonContent);
            int savedChanges = _changes;
            _changes = 0;
            return savedChanges;
        }




        public void Dispose()
        {

            GC.SuppressFinalize(this);
        }
        
    }
}
