using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL_LES
{
    public interface IRepository : ICommon, ICelebrity, ILifeEvent { }

    public interface ICelebrity : ICelebrity<Celebrity> { }
    public interface ILifeEvent : ILifeEvent<LifeEvent> { }
    public interface ICommon : ICommon<Celebrity, LifeEvent> { }

    public interface ICommon<T1, T2>
    {
        List<T2> GetLifeEventsByCelebrityId(int celebrityId);
        T1 GetCelebrityByLifeEventId(int lifeevent);
    }
}
