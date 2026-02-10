using System;
using Microsoft.EntityFrameworkCore;
using DAL_LES;

namespace Test_DAL_LES
{
    class Program
    {
        static void Main(string[] args)
        {
            // Строка подключения к SQL Server
            string connectionString = "Server=localhost;Database=LES_Database;Trusted_Connection=True;TrustServerCertificate=True;";

            // Настройка EF Core
            var optionsBuilder = new DbContextOptionsBuilder<LesDbContext>();
            optionsBuilder.UseSqlServer(connectionString);
            using var context = new LesDbContext(optionsBuilder.Options);

            // Убедимся, что база данных создана
            context.Database.EnsureCreated();

            using var repository = new LesRepository(context);

            // Тестирование ICelebrity
            Console.WriteLine("Testing ICelebrity:");
            var celebrity = new Celebrity
            {
                FullName = "John Doe",
                Nationality = "US",
                ReqPhotoPath = "john_doe.jpg"
            };
            Console.WriteLine($"AddCelebrity: {repository.AddCelebrity(celebrity)}");
            Console.WriteLine($"Added Celebrity: ID={celebrity.Id}, FullName={celebrity.FullName}, Nationality={celebrity.Nationality}, ReqPhotoPath={celebrity.ReqPhotoPath}");

            var retrievedCelebrity = repository.GetCelebrityById(celebrity.Id);
            Console.WriteLine($"Retrieved Celebrity: ID={retrievedCelebrity?.Id}, FullName={retrievedCelebrity?.FullName}, Nationality={retrievedCelebrity?.Nationality}, ReqPhotoPath={retrievedCelebrity?.ReqPhotoPath}");

            var updatedCelebrity = new Celebrity { FullName = "John Smith", Nationality = "UK", ReqPhotoPath = "john_smith.jpg" };
            Console.WriteLine($"UpdCelebrity: {repository.UpdCelebrity(celebrity.Id, updatedCelebrity)}");

            Console.WriteLine("All Celebrities:");
            foreach (var celeb in repository.GetAllCelebrities())
            {
                Console.WriteLine($"ID={celeb.Id}, FullName={celeb.FullName}, Nationality={celeb.Nationality}, ReqPhotoPath={celeb.ReqPhotoPath}");
            }

            // Тестирование ILifeEvent
            Console.WriteLine("\nTesting ILifeEvent:");
            var lifeEvent = new LifeEvent
            {
                CelebrityId = celebrity.Id,
                Date = new DateTime(2020, 1, 1),
                Description = "Received an award",
                ReqPhotoPath = "award.jpg"
            };
            Console.WriteLine($"AddLifeEvent: {repository.AddLifeEvent(lifeEvent)}");
            Console.WriteLine($"Added LifeEvent: ID={lifeEvent.Id}, CelebrityId={lifeEvent.CelebrityId}, Date={lifeEvent.Date.ToShortDateString()}, Description={lifeEvent.Description}, ReqPhotoPath={lifeEvent.ReqPhotoPath}");

            var retrievedLifeEvent = repository.GetLifeEventById(lifeEvent.Id);
            Console.WriteLine($"Retrieved LifeEvent: ID={retrievedLifeEvent?.Id}, CelebrityId={retrievedLifeEvent?.CelebrityId}, Date={retrievedLifeEvent?.Date.ToShortDateString()}, Description={retrievedLifeEvent?.Description}, ReqPhotoPath={retrievedLifeEvent?.ReqPhotoPath}");

            var updatedLifeEvent = new LifeEvent { CelebrityId = celebrity.Id, Date = new DateTime(2021, 1, 1), Description = "Published a book", ReqPhotoPath = "book.jpg" };
            Console.WriteLine($"UpdLifeEvent: {repository.UpdLifeEvent(lifeEvent.Id, updatedLifeEvent)}");

            // Тестирование ICommon
            Console.WriteLine("\nTesting ICommon:");
            Console.WriteLine($"LifeEvents for Celebrity ID={celebrity.Id}:");
            foreach (var evt in repository.GetLifeEventsByCelebrityId(celebrity.Id))
            {
                Console.WriteLine($"ID={evt.Id}, CelebrityId={evt.CelebrityId}, Date={evt.Date.ToShortDateString()}, Description={evt.Description}, ReqPhotoPath={evt.ReqPhotoPath}");
            }

            var celebrityByEvent = repository.GetCelebrityByLifeEventId(lifeEvent.Id);
            Console.WriteLine($"Celebrity by LifeEvent ID={lifeEvent.Id}: ID={celebrityByEvent?.Id}, FullName={celebrityByEvent?.FullName}, Nationality={celebrityByEvent?.Nationality}");

            // Тестирование удаления
            Console.WriteLine("\nTesting Deletion:");
            Console.WriteLine($"DelLifeEvent: {repository.DelLifeEvent(lifeEvent.Id)}");
            Console.WriteLine($"DelCelebrity: {repository.DelCelebrity(celebrity.Id)}");
        }
    }
}