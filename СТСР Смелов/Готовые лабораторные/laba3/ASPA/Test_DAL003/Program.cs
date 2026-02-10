using DAL003;
internal class Program
{
    private static void Main(string[] args)
    {
      Repository.JSONFileName = "Celebrities.json";
        using (IRepository repository = Repository.Create("Celebrities"))
        {
            foreach (Celebrity celebrity in repository.getAllCelebrities())
            {
                Console.WriteLine($"Id = {celebrity.Id}, Firsname = {celebrity.Firstname}, " 
                    + $"Surname = {celebrity.Surname}, PhotoPath = {celebrity.PhotoPath} ");
            }
            Celebrity? celebrity1 = repository.getCelebrityById(1);
            if(celebrity1 != null)
            {
                Console.WriteLine($"Id = {celebrity1.Id}, Firsname = {celebrity1.Firstname}, " 
                    + $"Surname = {celebrity1.Surname}, PhotoPath = {celebrity1.PhotoPath} ");
            }
            Celebrity? celebrity3 = repository.getCelebrityById(3);
            if(celebrity3 != null)
            {
                Console.WriteLine($"Id = {celebrity3.Id}, Firsname = {celebrity3.Firstname}, " 
                    + $"Surname = {celebrity3.Surname}, PhotoPath = {celebrity3.PhotoPath} ");
            }
            Celebrity? selebrity7 = repository.getCelebrityById(7);
            if(selebrity7 != null)
            {
                Console.WriteLine($"Id = {selebrity7.Id}, Firsname = {selebrity7.Firstname}, " 
                    + $"Surname = {selebrity7.Surname}, PhotoPath = {selebrity7.PhotoPath} ");
            }
            Celebrity? celebrity222 = repository.getCelebrityById(222);
            if (celebrity222 != null)
            {
                Console.WriteLine($"Id = {celebrity222.Id}, Firsname = {celebrity222.Firstname}, "
                    + $"Surname = {celebrity222.Surname}, PhotoPath = {celebrity222.PhotoPath} ");
            }
            else Console.WriteLine("Not Found 222");

            foreach(Celebrity celebrity in repository.getCelebritiesBySurname("Chomsky"))
            {
                Console.WriteLine($"Id = {celebrity.Id}, Firsname = {celebrity.Firstname}, "
                   + $"Surname = {celebrity.Surname}, PhotoPath = {celebrity.PhotoPath} ");
            }
            foreach (Celebrity celebrity in repository.getCelebritiesBySurname("XXXX"))
            {
                Console.WriteLine($"Id = {celebrity.Id}, Firsname = {celebrity.Firstname}, "
                   + $"Surname = {celebrity.Surname}, PhotoPath = {celebrity.PhotoPath} ");
            }

            Console.WriteLine($"PhotoPathById = {repository.getPhotoPathById(4)}");
            Console.WriteLine($"PhotoPathById = {repository.getPhotoPathById(6)}");
            Console.WriteLine($"PhotoPathById = {repository.getPhotoPathById(222)}");
        }
    }
}