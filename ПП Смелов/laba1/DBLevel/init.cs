using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLevel
{
    public class Init
    {
        public static void Execute()
        {
            Context context = new Context();
            if (context.WSRefs != null && context.Comments != null)
            {
                context.Database.EnsureCreated();
                if(!context.WSRefs.Any())
                {
                    List<WSRef> refs = new List<WSRef>();
                    {
                        new WSRef() { Description = "Oracle", Url = @"", Minus = 0, Plus = 0 };
                        new WSRef() { Description = "Java", Url = "", Minus = 0, Plus = 0 };
                        new WSRef() { Description = "JavaScript", Url = "", Minus = 0, Plus = 0 };

                    };
                    context.WSRefs.AddRange(refs);
                    context.SaveChanges();
                    List<Comment> comments = new List<Comment>();
                    foreach (WSRef wsRef in refs)
                    {
                        comments.Add(new Comment() { WSrefId = wsRef.Id, Stamp = DateTime.Now, Commtext = wsRef.Id.ToString() + "-Comment1" });
                        comments.Add(new Comment() { WSrefId = wsRef.Id, Stamp = DateTime.Now, Commtext = wsRef.Id.ToString() + "-Comment2" });
                    }
                    context.Comments.AddRange(comments);
                    context.SaveChanges();
                }
            }
        }
    }
}
