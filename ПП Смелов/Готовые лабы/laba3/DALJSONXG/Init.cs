using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DALJSONXG
{
    public class Init
    {
        public static void Execute()
        {
            using (IRepository repo = Repository.Create())
            {
                if (repo.getAllComment().Count == 0)
                {
                    repo.addWSRef(new WSRef() { Description = "Oracle, DMBS, PL/SQL", Url = @"https://www.oracle.com", Minus = 1, Plus = 3 });
                    repo.addComment(new Comment() { WSRefId = 1, CommText = "very useful link", Stamp = DateTime.Now });
                    repo.addComment(new Comment() { WSRefId = 1, CommText = "bad link", Stamp = DateTime.Now });
                    repo.addWSRef(new WSRef() { Description = "Java, Jakarta, Java SE, J2EE", Url = @"https://jakarta.ee/", Minus = 2, Plus = 5 });
                    repo.addComment(new Comment() { WSRefId = 2, CommText = "deprecated information", Stamp = DateTime.Now });
                }
            }
        }
    }
}
