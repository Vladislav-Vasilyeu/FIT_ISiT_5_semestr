using DALMSQLX;
using REPO;
using System;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Test Start");

        // Создание репозитория для работы с данными
        using (IRepository repo = RepositoryMSSQL.Create())
        {
            // Тестирование: добавление WSRef
            Console.WriteLine("Добавление нового WSRef...");
            WSRef wsRef = new WSRef { Url = "https://www.example.com", Description = "Example Link", Minus = 0, Plus = 5 };
            bool wsRefAdded = repo.addWSRef(wsRef);
            Console.WriteLine(wsRefAdded ? "WSRef добавлен успешно" : "Ошибка при добавлении WSRef");

            // Тестирование: добавление Comment
            Console.WriteLine("Добавление нового комментария...");
            Comment comment = new Comment { WSRefId = wsRef.Id, CommText = "Nice website!", Stamp = DateTime.Now };
            bool commentAdded = repo.addComment(comment);
            Console.WriteLine(commentAdded ? "Комментарий добавлен успешно" : "Ошибка при добавлении комментария");

            // Тестирование: извлечение всех WSRef
            Console.WriteLine("\nВывод всех WSRef:");
            var allWSRefs = repo.getAllWSRef();
            foreach (var ws in allWSRefs)
            {
                Console.WriteLine($"ID: {ws.Id}, URL: {ws.Url}, Description: {ws.Description}, Minus: {ws.Minus}, Plus: {ws.Plus}");
            }

            // Тестирование: извлечение всех комментариев
            Console.WriteLine("\nВывод всех комментариев:");
            var allComments = repo.getAllComment();
            foreach (var com in allComments)
            {
                Console.WriteLine($"ID: {com.Id}, WSRefId: {com.WSRefId}, Comment: {com.CommText}, Date: {com.Stamp}");
            }

            // Проверка добавления нового WSRef и Comment
            WSRef newWSRef = new WSRef { Url = "https://www.anotherexample.com", Description = "Another Example", Minus = 1, Plus = 3 };
            bool newWsRefAdded = repo.addWSRef(newWSRef);
            Console.WriteLine(newWsRefAdded ? "Новый WSRef добавлен успешно" : "Ошибка при добавлении нового WSRef");

            // Добавляем комментарий к новому WSRef
            Comment newComment = new Comment { WSRefId = newWSRef.Id, CommText = "Great example!", Stamp = DateTime.Now };
            bool newCommentAdded = repo.addComment(newComment);
            Console.WriteLine(newCommentAdded ? "Новый комментарий добавлен успешно" : "Ошибка при добавлении нового комментария");
        }

        Console.WriteLine("Test Finished");
        Console.ReadLine();
    }
}
