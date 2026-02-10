using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace DAL_LES
{
    public class LesRepository : IRepository
    {
        private readonly LesDbContext _context;
        private bool _disposed;

        public LesRepository(LesDbContext context)
        {
            _context = context;
            _disposed = false;
        }

        // ICelebrity<Celebrity>
        public List<Celebrity> GetAllCelebrities()
        {
            return _context.Celebrities.ToList();
        }

        public Celebrity? GetCelebrityById(int id)
        {
            return _context.Celebrities.Find(id);
        }

        public bool DelCelebrity(int id)
        {
            var celebrity = _context.Celebrities.Find(id);
            if (celebrity == null) return false;
            _context.Celebrities.Remove(celebrity);
            _context.SaveChanges();
            return true;
        }

        public bool AddCelebrity(Celebrity celebrity)
        {
            if (celebrity == null) return false;
            _context.Celebrities.Add(celebrity);
            _context.SaveChanges();
            return true;
        }

        public bool UpdCelebrity(int id, Celebrity celebrity)
        {
            var existing = _context.Celebrities.Find(id);
            if (existing == null) return false;
            existing.FullName = celebrity.FullName;
            existing.Nationality = celebrity.Nationality;
            existing.ReqPhotoPath = celebrity.ReqPhotoPath;
            _context.SaveChanges();
            return true;
        }

        // ILifeEvent<LifeEvent>
        public List<LifeEvent> GetAllLifeEvents()
        {
            return _context.LifeEvents.ToList();
        }

        public LifeEvent? GetLifeEventById(int id)
        {
            return _context.LifeEvents.Find(id);
        }

        public bool DelLifeEvent(int id)
        {
            var lifeEvent = _context.LifeEvents.Find(id);
            if (lifeEvent == null) return false;
            _context.LifeEvents.Remove(lifeEvent);
            _context.SaveChanges();
            return true;
        }

        public bool AddLifeEvent(LifeEvent lifeEvent)
        {
            if (lifeEvent == null) return false;
            _context.LifeEvents.Add(lifeEvent);
            _context.SaveChanges();
            return true;
        }

        public bool UpdLifeEvent(int id, LifeEvent lifeEvent)
        {
            var existing = _context.LifeEvents.Find(id);
            if (existing == null) return false;
            existing.CelebrityId = lifeEvent.CelebrityId;
            existing.Date = lifeEvent.Date;
            existing.Description = lifeEvent.Description;
            existing.ReqPhotoPath = lifeEvent.ReqPhotoPath;
            _context.SaveChanges();
            return true;
        }

        // ICommon<Celebrity, LifeEvent>
        public List<LifeEvent> GetLifeEventsByCelebrityId(int celebrityId)
        {
            return _context.LifeEvents
                .Where(e => e.CelebrityId == celebrityId)
                .ToList();
        }

        public Celebrity GetCelebrityByLifeEventId(int lifeEventId)
        {
            var lifeEvent = _context.LifeEvents
                .Include(e => e.Celebrity)
                .FirstOrDefault(e => e.Id == lifeEventId);
            return lifeEvent?.Celebrity;
        }

        // IDisposable
        public void Dispose()
        {
            if (!_disposed)
            {
                _context.Dispose();
                _disposed = true;
            }
        }
    }
}