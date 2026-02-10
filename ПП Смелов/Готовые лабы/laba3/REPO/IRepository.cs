using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace REPO
{
    public interface IRepository : IDisposable
    {
        List<WSRef> getAllWSRef();
        List<Comment> getAllComment();
        bool addWSRef(WSRef wsRef);
        bool addComment(Comment comment);
    }
}
