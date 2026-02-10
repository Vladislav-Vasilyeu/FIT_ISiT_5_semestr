using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL_LES;

namespace ServiceLocatorLibrary
{
    public class CashCelebrities : ICashCelebrities, IDisposable
    {
        private readonly IRepository _repository;
        private readonly TimeSpan _cacheDuration;
        private List<Celebrity> _cachedCelebrities;
        private DateTime _lastUpdated;

        public CashCelebrities(IRepository repository, TimeSpan cacheDuration)
        {
            _repository = repository;
            _cacheDuration = cacheDuration;
            _cachedCelebrities = new List<Celebrity>();
            _lastUpdated = DateTime.MinValue;
        }

        public List<Celebrity> GetCachedCelebrities()
        {
            if (DateTime.Now - _lastUpdated > _cacheDuration || !_cachedCelebrities.Any())
            {
                _cachedCelebrities = _repository.GetAllCelebrities();
                _lastUpdated = DateTime.Now;
            }
            return _cachedCelebrities;
        }

        public void Dispose()
        {
            
        }
    }
}
