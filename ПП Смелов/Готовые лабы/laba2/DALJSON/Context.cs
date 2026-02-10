using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace DALJSON
{
    public class JSONContext : IDisposable
    {
        private readonly string _fileName;
        public List<WSRef> WSRefs { get; private set; }

        public List<Comment> Comments => WSRefs.SelectMany(wsref => wsref.Comments).ToList();

        private JSONContext(string fileName)
        {
            _fileName = fileName;
            WSRefs = Load();
        }

        public static JSONContext Create(string fileName) => new JSONContext(fileName);

        private List<WSRef> Load()
        {
            if (!File.Exists(_fileName))
            {
                File.WriteAllText(_fileName, "[]"); // Создаём пустой JSON-массив
                return new List<WSRef>();
            }

            try
            {
                string json = File.ReadAllText(_fileName);
                return JsonSerializer.Deserialize<List<WSRef>>(json) ?? new List<WSRef>();
            }
            catch
            {
                return new List<WSRef>(); // Защита от ошибок десериализации
            }
        }

        public int SaveChanges()
        {
            try
            {
                string json = JsonSerializer.Serialize(WSRefs, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_fileName, json);
                return 1;
            }
            catch
            {
                return 0;
            }
        }

        private int MaxWSRefsId() => WSRefs.Any() ? WSRefs.Max(wsref => wsref.Id) : 0;

        public bool addWSRef(WSRef wsref)
        {
            wsref.Id = MaxWSRefsId() + 1;
            WSRefs.Add(wsref);
            return SaveChanges() > 0;
        }

        private int MaxCommentsId() =>
            WSRefs.SelectMany(wsref => wsref.Comments)
                 .DefaultIfEmpty(new Comment { Id = 0 }) // Безопасная проверка
                 .Max(comment => comment.Id);

        public bool addComment(Comment comment)
        {
            var wsref = WSRefs.FirstOrDefault(w => w.Id == comment.WSRefId);
            if (wsref == null) return false;

            comment.Id = MaxCommentsId() + 1;
            wsref.Comments.Add(comment);
            return SaveChanges() > 0;
        }

        public void Dispose() { /* Ничего не нужно освобождать, FileStream не используется постоянно */ }
    }
}
