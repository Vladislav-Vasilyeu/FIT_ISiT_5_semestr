namespace DALMSQLXG
{
    public interface IRepository : GREPO.IRepository<WSRef, Comment> { }
    public class Repository : IRepository
    {
        private readonly Context _context;

        public Repository()
        {
            _context = new Context();
        }



        public static IRepository Create()
        {
            return new Repository();
        }

        public List<WSRef> getAllWSRef()
        {
            return _context.WSRefs.ToList();
        }

        public List<Comment> getAllComment()
        {
            return _context.Comments.ToList();
        }

        public Comment? GetCommentById(int Id)
        {
            return _context.Comments.FirstOrDefault(c => c.Id == Id);
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

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
