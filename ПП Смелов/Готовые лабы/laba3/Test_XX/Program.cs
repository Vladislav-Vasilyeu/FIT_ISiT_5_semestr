using DALMSQLX;
using DALJSONX;
using REPO;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Testing MS SQL and JSON implementations");

        // Test for DAL_MSQLX
        Console.WriteLine("\nTesting DAL_MSQLX:");
        using (IRepository repoMSQL = RepositoryMSSQL.Create())  // Репозиторий для MS SQL
        {
            TestRepositoryFunctions(repoMSQL);
        }

        // Test for DAL_JSONX
        Console.WriteLine("\nTesting DAL_JSONX:");
        using (IRepository repoJSON = RepositoryJSON.Create())  // Репозиторий для JSON
        {
            TestRepositoryFunctions(repoJSON);
        }

        Console.WriteLine("Test completed");
        Console.ReadLine();
    }

    private static void TestRepositoryFunctions(IRepository repo)
    {
        // Вывод всех ссылок
        repo.getAllWSRef().ForEach(wsRef => {
            Console.WriteLine($"WSRefs: {wsRef.Id}: {wsRef.Url}, {wsRef.Description}, {wsRef.Minus}, {wsRef.Plus}");
        });

        // Вывод всех комментариев
        repo.getAllComment().ForEach(comment => {
            Console.WriteLine($"Comments {comment.Id}: {comment.CommText}, {comment.Stamp}, {comment.WSRefId} ");
        });

        // Добавление новой ссылки
        if (repo.addWSRef(new WSRef() { Url = "https://example.com", Description = "Example Site", Minus = 0, Plus = 1 }))
            Console.WriteLine("WSRefs: Add");
        else
            Console.WriteLine("WSRefs: Error Add");

        // Добавление нового комментария
        if (repo.addComment(new Comment() { WSRefId = 1, CommText = "This is a test comment", Stamp = DateTime.Now }))
            Console.WriteLine("Comments: Add");
        else
            Console.WriteLine("Comments: Error Add");
    }
}
