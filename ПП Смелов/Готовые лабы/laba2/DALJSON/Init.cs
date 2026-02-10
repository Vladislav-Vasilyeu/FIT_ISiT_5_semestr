using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DALJSON
{
    public class Init
    {
        public static void Execute()
        {
            using (IRepository repo = Repository.Create())
            {
                if (repo == null)
                {
                    Console.WriteLine("Ошибка: невозможно создать репозиторий.");
                    return;
                }

                if (repo.getAllComment().Count == 0)
                {
                    var ws1 = new WSRef() { Description = "Oracle, DMBS, PL/SQL", Url = @"https://www.oracle.com", Minus = 1, Plus = 3 };
                    repo.addWSRef(ws1);
                     
                    var ws2 = new WSRef() { Description = "Java, Jakarta, Java SE, J2EE", Url = @"https://jakarta.ee/", Minus = 2, Plus = 5 };
                    repo.addWSRef(ws2);
                    

                    repo.addComment(new Comment() { WSRefId = ws1.Id, CommText = "very useful link", Stamp = DateTime.Now });
                    repo.addComment(new Comment() { WSRefId = ws1.Id, CommText = "bad link", Stamp = DateTime.Now });
                    repo.addComment(new Comment() { WSRefId = ws2.Id, CommText = "deprecated information", Stamp = DateTime.Now });
                    
                }
            }
        }
    }
}
