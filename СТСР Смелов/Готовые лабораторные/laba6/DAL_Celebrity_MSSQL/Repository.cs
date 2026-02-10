using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL_Celebrity_MSSQL
{
    public class Repository : IRepository
    {
        Context context;
        public Repository() 
        { 
            this.context = new Context();
        }
        public Repository(string connectionstring)
        { 
            this.context = new Context(connectionstring); 
        }
        public static IRepository Create() 
        { 
            return new Repository(); 
        }
        public static IRepository Create(string connectionstring)
        { 
            return new Repository(connectionstring); 
        }
        public List<Celebrity> GetAllCelebrities() 
        {
            return this.context.Celebrities.ToList<Celebrity>(); 
        }
        public Celebrity?  GetCelebrityById(int Id)
        {
            return this.context.Celebrities.FirstOrDefault(c => c.Id == Id);
        }
        public bool AddCelebrity(Celebrity celebrity)
        {
            try
            {
                this.context.Celebrities.Add(celebrity);
                this.context.SaveChanges();
                return true;
            }
            catch { return false; }
        }
        public bool DelCelebrity(int id)
        {
            try
            {
                Celebrity? el = this.context.Celebrities.Find(id);
                if (el == null) return false;
                this.context.Celebrities.Remove(el);
                this.context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        public bool UpdCelebrity(int id, Celebrity celebrity)
        {
            try
            {
                Celebrity? el = this.context.Celebrities.Find(id);
                if (el == null) return false;
                el.FullName = celebrity.FullName;
                el.Nationality = celebrity.Nationality;
                el.ReqPhotoPath = celebrity.ReqPhotoPath;
                this.context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }


        }
        public List<Lifeevent> GetAllLifeevents()
        {
            return this.context.Lifeevents.ToList<Lifeevent>();
        }

        public Lifeevent? GetLifeeventById(int id)
        {
            return this.context.Lifeevents.FirstOrDefault(x => x.Id == id);
        }

        public bool AddLifeevent(Lifeevent lifeEvent)
        {
            try
            {
                //lifeEvent.Id = this.context.Lifeevents.OrderBy(x => x.Id).LastOrDefault().Id + 1;
                if (lifeEvent == null) return false;
                this.context.Lifeevents.Add(lifeEvent);
                this.context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        public bool DelLifeevent(int id)
        {
            try
            {
                Lifeevent? el = this.context.Lifeevents.Find(id);
                if (el == null) return false;
                this.context.Lifeevents.Remove(el);
                this.context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public bool UpdLifeevent(int id, Lifeevent lifeEvent)
        {
            try
            {
                Lifeevent? el = this.context.Lifeevents.Find(id);
                if (el == null) return false;
                el.CelebrityId = lifeEvent.CelebrityId;
                el.Date = lifeEvent.Date;
                el.Description = lifeEvent.Description;
                el.ReqPhotoPath = lifeEvent.ReqPhotoPath;
                this.context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        public List<Lifeevent> GetLifeeventsByCelebrityId(int celebrityId)
        {
            if (celebrityId <= 0)
                throw new ArgumentException("Неверный id", nameof(celebrityId));
            return this.context.Lifeevents
                .Where(x => x.CelebrityId == celebrityId)
                .ToList();
        }

        public Celebrity? GetCelebrityByLifeeventId(int lifeEventId)
        {
            if (lifeEventId <= 0)
                throw new ArgumentException("Неверный id", nameof(lifeEventId));

            var lifeEvent = this.context.Lifeevents
                .FirstOrDefault(le => le.Id == lifeEventId);

            if (lifeEvent == null || lifeEvent.CelebrityId <= 0)
                return null;

            return this.context.Celebrities
                .FirstOrDefault(c => c.Id == lifeEvent.CelebrityId);
        }

        public int GetCelebrityIdByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Имя не может быть пустым", nameof(name));

            var celebrity = this.context.Celebrities
                .FirstOrDefault(c => c.FullName.Contains(name));
            return celebrity?.Id ?? -1;
        }
        public void Dispose()
        {

        }
    }
}
