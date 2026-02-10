
namespace DBLevel
{
    public interface IRepository: IDisposable
    {
        List<WSRef>    getAllWSRef();
        List<Comment>  getAllComment();
        bool           addWSRef(WSRef wsRef);
    }
    public class Repository : IRepository
    {
        Context context;    
        private Repository() 
        {
          this.context = new Context();     
        }   
        public static IRepository Create() {return new Repository();}
        public List<Comment> getAllComment()
        {
            List<Comment> rc = new List<Comment>(); 
            if (this.context.Comments != null)
            { 
                rc.AddRange(this.context.Comments); 
            }
            return  rc;
        }
        public List<WSRef> getAllWSRef()
        {
            List<WSRef> rc = new List<WSRef>(); 
            if(this.context.WSRefs != null)
            { 
             rc.AddRange(this.context.WSRefs);    
            }            return rc;
        }
        public bool addWSRef(WSRef wsref)
        {
              bool rc = false;     
              if (this.context.WSRefs != null)
              {
                context.Database.BeginTransaction();
                context.WSRefs.Add(wsref);
                rc = ( context.SaveChanges() > 0 );
                WSRef x = context.WSRefs.OrderByDescending(e=>e.Id).First();
                context.Database.CommitTransaction();
              }
              return rc;
        }
        //public List<WSRef> getWSRefByDescription(string text)
        //{
        //    List<WSRef> rc = new List<WSRef>();
        //    if (this.context.WSRefs != null)
        //    {
        //        rc.AddRange(this.context.WSRefs.Select(wsRef =>  (wsRef.Description != null)? wsRef.Description.Contains(text):false));


        //       // rc.AddRange(this.context.WSRefs.Find()
        //    }
        //    return rc;
        //}

        public void Dispose()
        {
            this.context.Dispose(); 
        }
    }


}
