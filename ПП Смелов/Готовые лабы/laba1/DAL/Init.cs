using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public static class Init
    {
        public static void SeedDatabase()
        {
            using (var context = new Context())
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var wsRef1 = new WSRef
                {
                    Url = "https://example.com",
                    Description = "Example website",
                    Plus = 10,
                    Minus = 2
                };
                var wsRef2 = new WSRef
                {
                    Url = "https://another-example.com",
                    Description = "Another useful site",
                    Plus = 5,
                    Minus = 1
                };

                context.WSRefs.AddRange(wsRef1, wsRef2);
                context.SaveChanges();

                var comment1 = new Comment
                {
                    WSRefId = wsRef1.Id,
                    CommText = "Great site!"
                };

                var comment2 = new Comment
                {
                    WSRefId = wsRef2.Id,
                    CommText = "Very informative."
                };

                context.Comments.AddRange(comment1, comment2);
                context.SaveChanges();
            }

            Console.WriteLine("База данных успешно инициализирована!");
        }
    }
}
