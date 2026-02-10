using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL_LES;

namespace ServiceLocatorLibrary
{
    public interface ICashCelebrities
    {
        List<Celebrity> GetCachedCelebrities();
    }
}
