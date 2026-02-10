using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using REPO;

namespace DALMSQLX
{
    public class RepositoryMSSQL : IRepository
    {
        private readonly Context _context;

        public RepositoryMSSQL()
        {
            _context = new Context();
        }
        public List<WSRef> getAllWSRef()
        {
            return _context.WSRefs.Include(c => c.Comments).ToList();
        }
        public List<Comment> getAllComment()
        {
            return _context.Comments.Include(c => c.WSRef).ToList();
        }
        public bool addWSRef(WSRef wsRef)
        {
            _context.WSRefs.Add(wsRef);
            return _context.SaveChanges() > 0;
        }
        public bool addComment(Comment comment)
        {
            _context.Comments.Add(comment);
            return _context.SaveChanges() > 0;
        }
        public static IRepository Create()
        {
            return new RepositoryMSSQL();
        }
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
