using DALJSONX;
using REPO;
internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Start");

        using (IRepository repo = RepositoryJSON.Create())
        {
            // Получаем все WSRef и выводим их
            repo.getAllWSRef().ForEach(wsRef => {
                Console.WriteLine($"WSRefs: {wsRef.Id}: {wsRef.Url}, {wsRef.Description}, {wsRef.Minus}, {wsRef.Plus}");
            });

            // Получаем все Comment и выводим их
            repo.getAllComment().ForEach(comment => {
                Console.WriteLine($"Comments {comment.Id}: {comment.CommText}, {comment.Stamp}, {comment.WSRefId}");
            });

            // Добавляем новый WSRef
            if (repo.addWSRef(new WSRef() { Url = "https://www.belstu.by/", Description = "БГТУ", Minus = 0, Plus = 0 }))
                Console.WriteLine("WSRefs: Add");
            else
                Console.WriteLine("WSRefs: Error Add");

            // Добавляем новый Comment
            if (repo.addComment(new Comment() { WSRefId = 3, CommText = "test", Stamp = DateTime.Now }))
                Console.WriteLine("Comments: Add");
            else
                Console.WriteLine("Comments: Error Add");

            Console.WriteLine("After addWSRef, addComment");

            // Выводим все WSRef после добавления
            repo.getAllWSRef().ForEach(wsRef => {
                Console.WriteLine($"WSRefs: {wsRef.Id}: {wsRef.Url}, {wsRef.Description}, {wsRef.Minus}, {wsRef.Plus}");
            });

            // Выводим все Comment после добавления
            repo.getAllComment().ForEach(comment => {
                Console.WriteLine($"Comments {comment.Id}: {comment.CommText}, {comment.Stamp}, {comment.WSRefId}");
            });
        }

        Console.WriteLine("Finish");
        Console.ReadLine();
    }
}
