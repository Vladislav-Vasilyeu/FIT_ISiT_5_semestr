using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL_LES
{
    public interface ICelebrity<T> : IDisposable
    {
        List<T> GetAllCelebrities();
        T? GetCelebrityById(int Id);
        bool DelCelebrity(int id);
        bool AddCelebrity(T celebrity);
        bool UpdCelebrity(int id, T celebrity);
    }

    public interface ILifeEvent<T> : IDisposable
    {
        List<T> GetAllLifeEvents();
        T? GetLifeEventById(int Id);
        bool DelLifeEvent(int id);
        bool AddLifeEvent(T lifeevent);
        bool UpdLifeEvent(int id, T lifeevent);
    }

    public class Celebrity 
    {
        public Celebrity() { this.FullName = ""; this.Nationality = "XX"; }
        public int Id { get; set; }
        public string? FullName {  get; set; }
        public string Nationality { get; set; }
        public string? ReqPhotoPath { get; set; }
        public virtual List<LifeEvent> LifeEvents { get; set; } = new List<LifeEvent>();
    }

    public class LifeEvent 
    {
        public LifeEvent() { this.Description = ""; }
        public int Id { get; set; }
        public int CelebrityId { get; set; }
        public DateTime Date { get; set; }
        public string? Description { get; set; }
        public string? ReqPhotoPath { get; set; }
        public virtual Celebrity? Celebrity { get; set; }
    }
}
