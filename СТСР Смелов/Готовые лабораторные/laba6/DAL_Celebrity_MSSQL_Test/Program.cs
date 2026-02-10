using DAL_Celebrity_MSSQL;
internal class Program
{
    private static void Main(string[] args)
    {
        string CS = @"Server=VladPC;Database=Life Events of  Celebrities;Trusted_Connection=True;TrustServerCertificate=True;";

        Init init = new Init(CS);
        Init.Execute(delete: true, create: true);

        Func<Celebrity, string> printC = (c) => $"Id = {c.Id}, FullName = {c.FullName}, Nationality = {c.Nationality}, ReqPhotoPath = {c.ReqPhotoPath}";
        Func<Lifeevent, string> printL = (l) => $"Id = {l.Id}, CelebrityId = {l.CelebrityId}, Date = {l.Date}, Description = {l.Description},  ReqPhotoPath = {l.ReqPhotoPath}";
        Func<string, string> puri = (string f) => $"{f}";

        using(IRepository repo = Repository.Create(CS))
        {
            {
                Console.WriteLine("--- GetAllCelebrities() ---");
                repo.GetAllCelebrities().ForEach(c => Console.WriteLine(printC(c)));
            }

           { 
                Console.WriteLine("--- GetAllLifeEvents() ---");
                repo.GetAllLifeevents().ForEach(l => Console.WriteLine(printL(l)));
           }

            {
                Console.WriteLine("--- AddCelebrity() ---");
                Celebrity c = new Celebrity
                {
                    FullName = "Albert Einstein",
                    Nationality = "DE",
                    ReqPhotoPath = puri("Einstein.jpg")
                };
                Console.WriteLine(repo.AddCelebrity(c)
                    ? $"OK: AddCelebrity: {printC(c)}"
                    : $"ERROR: AddCelebrity: {printC(c)}");
            }
            {
                Console.WriteLine("--- AddCelebrity() ---");
                Celebrity c = new Celebrity
                {
                    FullName = "Samuel Huntington",
                    Nationality = "US",
                    ReqPhotoPath = puri("Huntington.jpg")
                };
                Console.WriteLine(repo.AddCelebrity(c)
                    ? $"OK: AddCelebrity: {printC(c)}"
                    : $"ERROR: AddCelebrity: {printC(c)}");
            }
            {
                Console.WriteLine("--- DelCelebrity() ---");
                int einsteinId = repo.GetCelebrityIdByName("Einstein");
                if (einsteinId > 0)
                {
                    Console.WriteLine(repo.DelCelebrity(einsteinId)
                        ? $"OK: Deleted celebrity with ID: {einsteinId}"
                        : $"ERROR: Failed to delete celebrity with ID: {einsteinId}");
                }
                else
                {
                    Console.WriteLine("ERROR: GetCelebrityIdByName: Einstein not found");
                }

            }
            {
                Console.WriteLine("--- UpdCelebrity() ---");
                int huntingtonId = repo.GetCelebrityIdByName("Huntington");
                if (huntingtonId > 0)
                {
                    var existing = repo.GetCelebrityById(huntingtonId);
                    if (existing != null)
                    {
                        existing.FullName = "Samuel Phillips Huntington";
                        existing.Nationality = "UK";
                        Console.WriteLine(repo.UpdCelebrity(huntingtonId, existing)
                            ? $"OK: UpdatedCelebrity:12, {printC(existing)}"
                            : $"ERROR: Failed to update celebrity: {printC(existing)}");
                    }
                    else
                    {
                        Console.WriteLine($"ERROR: GetCelebrityById: {huntingtonId} not found");
                    }
                }
                else
                {
                    Console.WriteLine("ERROR: GetCelebrityIdByName: Huntington not found");
                }
                if (huntingtonId > 0)
                {
                    var existing = repo.GetCelebrityById(huntingtonId);
                    if (existing != null)
                    {
                        Console.WriteLine($"OK: GetCelebrityById, {printC(existing)}");
                    }
                    else
                    {
                        Console.WriteLine($"ERROR: Failed to get celebrity Huntington");
                    }
                }
                else
                {
                    Console.WriteLine($"ERROR: Failed to get celebrity Huntington");
                }
            }
            // -----
            {
                Console.WriteLine("--- AddLifeEvent() ---");
                int newCelebrityId = repo.GetCelebrityIdByName("Huntington");
                if (newCelebrityId > 0)
                {
                    var lifeEvent = new Lifeevent
                    {
                        CelebrityId = newCelebrityId,
                        Date = new DateTime(1927, 04, 18),
                        Description = "Дата рождения",
                        ReqPhotoPath = null
                    };
                    Console.WriteLine(repo.AddLifeevent(lifeEvent)
                        ? $"OK: AddedLifeEvent:{printL(lifeEvent)}"
                        : $"ERROR: Failed to add life event");
                }
                else
                {
                    Console.WriteLine("ERROR: GetCelebrityIdByName: Huntington not found");
                }
                if (newCelebrityId > 0)
                {
                    var lifeEvent = new Lifeevent
                    {
                        CelebrityId = newCelebrityId,
                        Date = new DateTime(1927, 04, 18),
                        Description = "Дата рождения",
                        ReqPhotoPath = null
                    };
                    Console.WriteLine(repo.AddLifeevent(lifeEvent)
                        ? $"OK: AddedLifeEvent:{printL(lifeEvent)}"
                        : $"ERROR: Failed to add life event");
                }
                else
                {
                    Console.WriteLine("ERROR: GetCelebrityIdByName: Huntington not found");
                }
                if (newCelebrityId > 0)
                {
                    var lifeEvent = new Lifeevent
                    {
                        CelebrityId = newCelebrityId,
                        Date = new DateTime(2008, 12, 24),
                        Description = "Дата рождения",
                        ReqPhotoPath = null
                    };
                    Console.WriteLine(repo.AddLifeevent(lifeEvent)
                        ? $"OK: AddedLifeEvent:{printL(lifeEvent)}"
                        : $"ERROR: Failed to add life event");
                }
                else
                {
                    Console.WriteLine("ERROR: GetCelebrityIdByName: Huntington not found");
                }
            }
            // -----
            {
                {
                    Console.WriteLine("--- DelLifeEvent() ---");
                    int id = 22;
                    if (repo.DelLifeevent(id))
                        Console.WriteLine($"OK: DelLifeEvent: {id}");
                    else
                        Console.WriteLine($"ERROR: DelLifeEvent: {id}");
                }
            }

            {
                Console.WriteLine("--- UpdLifeEvent() ---");
                int id = 23;
                Lifeevent? l1;
                if ((l1 = repo.GetLifeeventById(id)) != null)
                {
                    l1.Description = "Дата смерти";
                    if (repo.UpdLifeevent(id, l1))
                        Console.WriteLine($"OK: UpdLifeEvent {id}, {printL(l1)}");
                    else
                        Console.WriteLine($"ERROR: UpdLifeEvent {id}, {printL(l1)}");
                }
            }

            {
                Console.WriteLine("--- GetLifeEventsByCelebrityId ---");
                int id = repo.GetCelebrityIdByName("Huntington");
                if (id > 0)
                {
                    Celebrity? c = repo.GetCelebrityById(id);
                    if (c != null)
                        repo.GetLifeeventsByCelebrityId(c.Id).ForEach(l =>
                            Console.WriteLine($"OK: GetLifeEventsByCelebrityId, {id}, {printL(l)}"));
                    else
                        Console.WriteLine($"ERROR: GetLifeEventsByCelebrityId: {id}");
                }
                else
                    Console.WriteLine("ERROR: GetCelebrityIdByName");
            }

            {
                Console.WriteLine("--- GetCelebrityByLifeEventId ---");
                int id = 23;
                Celebrity? c = repo.GetCelebrityByLifeeventId(id);
                if (c != null)
                    Console.WriteLine($"OK: {printC(c)}");
                else
                    Console.WriteLine($"ERROR: GetCelebrityByLifeEventId, {id}");
            }

            Console.WriteLine("--->");
            Console.ReadKey();
        }

    }
}