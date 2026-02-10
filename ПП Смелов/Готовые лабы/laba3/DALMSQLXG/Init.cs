using DALMSQLX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DALMSQLXG
{
    public class Init
    {
        public static void Execute()
        {
            Context context = new Context();

            if (context.WSRefs != null && context.Comments != null)
            {
                context.Database.EnsureCreated();
                if (!context.WSRefs.Any())
                {
                    List<WSRef> refs = new List<WSRef>();
                    {
                        new WSRef() { Description = "Oracle", Url = @"https://www.oracle.com", Minus = 0, Plus = 0 };
                        new WSRef() { Description = "Java", Url = "https://www.jakarta.ee", Minus = 0, Plus = 0 };
                        new WSRef() { Description = "JavaScript", Url = "", Minus = 0, Plus = 0 };

                    };
                    context.WSRefs.AddRange(refs);
                    context.SaveChanges();
                    List<Comment> comments = new List<Comment>();
                    foreach (WSRef wSRef in refs)
                    {
                        comments.Add(new Comment() { WSRefId = wSRef.Id, Stamp = DateTime.Now, CommText = wSRef.Id.ToString() + "-Comment1" });
                        comments.Add(new Comment() { WSRefId = wSRef.Id, Stamp = DateTime.Now, CommText = wSRef.Id.ToString() + "-Comment2" });


                    }
                    context.Comments.AddRange(comments);
                    context.SaveChanges();
                }
            }
        }
    }
}
