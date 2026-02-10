
using GREPO;
using DALMSQLXG;
using DALJSONXG;


class Program
{

    private static void Main(string[] args)
    {
        Console.WriteLine("Init");
        //DALMSQLXG.Init.Execute();

        Console.WriteLine("Start");
        Console.WriteLine("");

        Console.WriteLine("\nTest DALMSQLX");
        using (DALMSQLXG.IRepository repo = DALMSQLXG.Repository.Create())
        {
            Console.WriteLine("------>Start \n");
            Console.WriteLine("\n------>AfterExecute");

            repo.getAllComment().ForEach(comment => {
                Console.WriteLine($"{comment.Id}: {comment.CommText}, {comment.Stamp}, {comment.WSRefId} ");
            });

            repo.getAllWSRef().ForEach(wsRef => {
                Console.WriteLine($"{wsRef.Id}: {wsRef.Url}, {wsRef.Description}, {wsRef.Minus}, {wsRef.Plus}");
            });

            repo.addWSRef(new DALMSQLXG.WSRef { Url = "google.com", Minus = 0, Plus = 0 });
            repo.addComment(new DALMSQLXG.Comment { WSRefId = 4, Stamp = DateTime.UtcNow, CommText = "Текст изменен" });
            repo.addComment(new DALMSQLXG.Comment { WSRefId = 5, Stamp = DateTime.UtcNow, CommText = "Добавлен новый комментарий к Id 5" });
            repo.addComment(new DALMSQLXG.Comment { WSRefId = 2028, Stamp = DateTime.UtcNow, CommText = "Комментарий к 2038 1 вариант" });
            repo.addComment(new DALMSQLXG.Comment { WSRefId = 2028, Stamp = DateTime.UtcNow, CommText = "Комментарий к 2038 2 вариант" });



            Console.WriteLine("-----------------------------------------------------------------");


            repo.getAllComment().ForEach(comment => {
                Console.WriteLine($"{comment.Id}: {comment.CommText}, {comment.Stamp}, {comment.WSRefId} ");
            });

            repo.getAllWSRef().ForEach(wsRef => {
                Console.WriteLine($"{wsRef.Id}: {wsRef.Url}, {wsRef.Description}, {wsRef.Minus}, {wsRef.Plus}");
            });
            
        }

        Console.WriteLine("");
        Console.WriteLine("");
        Console.WriteLine("------------------------------------------------");
        Console.WriteLine("");
        Console.WriteLine("");

        Console.WriteLine("Test DALJSONX");
        //DALJSONXG.Init.Execute();
        using (DALJSONXG.IRepository repo = DALJSONXG.Repository.Create("WSRef.json"))
        {
            Console.WriteLine("All WSRef:");
            repo.getAllWSRef().ForEach(wsRef => {
                Console.WriteLine($"WSRef: {wsRef.Id}: {wsRef.Url}, {wsRef.Description}, {wsRef.Minus}, {wsRef.Plus}");
            });

            Console.WriteLine("All Comments:");
            repo.getAllComment().ForEach(comment => {
                Console.WriteLine($"Comment {comment.Id}: {comment.CommText}, {comment.Stamp}, {comment.WSRefId}");
            });

            repo.addWSRef(new DALJSONXG.WSRef { Url = "belstu.by", Minus = 0, Plus = 0 });
            repo.addComment(new DALJSONXG.Comment { WSRefId = 2, Stamp = DateTime.UtcNow, CommText = "Текст изменен" });
            repo.addComment(new DALJSONXG.Comment { WSRefId = 3, Stamp = DateTime.UtcNow, CommText = "Новый комментарий" });
            repo.addComment(new DALJSONXG.Comment { WSRefId = 3, Stamp = DateTime.UtcNow, CommText = "Новый комментарий2" });




            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("After Additions:");
            Console.WriteLine("");
            Console.WriteLine("");

            repo.getAllWSRef().ForEach(wsRef => {
                Console.WriteLine($"WSRef: {wsRef.Id}: {wsRef.Url}, {wsRef.Description}, {wsRef.Minus}, {wsRef.Plus}");
            });

            repo.getAllComment().ForEach(comment => {
                Console.WriteLine($"Comment {comment.Id}: {comment.CommText}, {comment.Stamp}, {comment.WSRefId}");
            });
            repo.Dispose();
        }

        Console.WriteLine("Finish");
        Console.ReadLine();
    }
}
