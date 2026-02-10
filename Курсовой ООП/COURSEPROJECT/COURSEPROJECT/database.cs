using Microsoft.Data.Sqlite;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO.Packaging;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;
using static COURSEPROJECT.AdminTable;
using System.Net;
using System.Reflection.Emit;
using System.Diagnostics;

namespace COURSEPROJECT
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        [Index(IsUnique = true)]
        [Column(TypeName = "nvarchar")]
        public string Name { get; set; }
        [Required]
        [StringLength(100)]
        [Column(TypeName = "nvarchar")]
        public string Password { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }
    [Table("Reviews")]
    public class Reviews
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("computer_id")]
        public int ComputerId { get; set; }

        [Required]
        [Range(0.0, 5.0)]
        [Column("Rating")]
        public double Rating { get; set; }

        [Required]
        [StringLength(500)]
        [Column("Comment")]
        public string Comment { get; set; }

        [Column("CreateAt")]
        public string CreateAt { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("ComputerId")]
        public virtual Computer Computer { get; set; }
    }
    public class Computer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Column(TypeName = "nvarchar")]
        public string Name { get; set; }


        [Required]
        [Column(TypeName = "nvarchar")]
        [MaxLength(500)]
        public string Description { get; set; }

        [Required]

        [StringLength(50)]
        [Column(TypeName = "nvarchar")]
        public string Cpu { get; set; }

        [Required]
        [Column(TypeName = "nvarchar")]
        [StringLength(50)]
        public string Gpu { get; set; }

        [Required]
        [Column(TypeName = "nvarchar")]
        [StringLength(50)]
        public string Ram { get; set; }

        [Required]
        [Column(TypeName = "nvarchar")]
        [StringLength(50)]
        public string Storage { get; set; }

        [Required]
        [Column(TypeName = "nvarchar")]
        [StringLength(50)]
        public string Monitor { get; set; }

        public double Rating { get; set; } = 0.0;

        [Required]
        public double PricePerHour { get; set; }

        [MaxLength]
        public byte[] GraphicData { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
    }
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("computer_id")]
        public int ComputerId { get; set; }

        [Required]
        [StringLength(20)]
        [Column(TypeName = "nvarchar")]
        public string Status { get; set; } = "active";

        [Column(TypeName = "nvarchar")]
        public string DateOrder { get; set; }

        [Required]
        [Column(TypeName = "nvarchar")]
        public string StartTime { get; set; }

        [Column(TypeName = "nvarchar")]
        public string EndTime { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("ComputerId")]
        public virtual Computer Computer { get; set; }
    }
    [Table("ApplicationGame")]
    public class ApplicationGame
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string IMG { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string URL { get; set; } = string.Empty;
    }
    public class AppDbContext : DbContext, IDisposable
    {
        public AppDbContext() : base("name=DBConnection")
        {
            Database.SetInitializer<AppDbContext>(null);
            // Отключаем lazy loading и proxy creation для избежания проблем с загрузкой связанных сущностей
            this.Configuration.LazyLoadingEnabled = false;
            this.Configuration.ProxyCreationEnabled = false;
        }
        public DbSet<Computer> Computers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Reviews> Reviews { get; set; }
        public DbSet<ApplicationGame> ApplicationGame { get; set; }
    }
    static class database
    {
        public static string connectionString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
        public static void InitDataBase()
        {
            using (var db = new AppDbContext())
            {
                db.Database.CreateIfNotExists();
                var connection = db.Database.Connection;
                MessageBox.Show($"Сервер: {connection.DataSource}\nБаза данных: {connection.Database}");
            }

            try
            {
                // Выполняем сидинг вне контекста сообщения пользователю
                SeedInitialData();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("SeedInitialData error: " + ex.Message);
            }
        }

        private static void SeedInitialData()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    // Админ
                    if (!db.Users.Any(u => u.Name == "admin"))
                    {
                        db.Users.Add(new User
                        {
                            Name = "admin",
                            Password = HashCode.HashPassword("admin")
                        });
                    }

                    // Тестовый пользователь
                    if (!db.Users.Any(u => u.Name == "testuser"))
                    {
                        db.Users.Add(new User
                        {
                            Name = "testuser",
                            Password = HashCode.HashPassword("password123")
                        });
                    }

                    // Демо-компьютеры
                    if (!db.Computers.Any())
                    {
                        db.Computers.Add(new Computer
                        {
                            Name = "Demo PC 1",
                            Description = "Demo gaming PC",
                            Cpu = "Intel Core i5",
                            Gpu = "GTX 1650",
                            Ram = "16",
                            Storage = "512GB SSD",
                            Monitor = "144",
                            Rating = 4.2,
                            PricePerHour = 5.0
                        });
                        db.Computers.Add(new Computer
                        {
                            Name = "Demo PC 2",
                            Description = "Demo workstation",
                            Cpu = "Intel Core i7",
                            Gpu = "RTX 2060",
                            Ram = "32",
                            Storage = "1TB SSD",
                            Monitor = "60",
                            Rating = 4.5,
                            PricePerHour = 7.5
                        });
                    }

                    // Демо-приложение
                    if (!db.ApplicationGame.Any())
                    {
                        db.ApplicationGame.Add(new ApplicationGame
                        {
                            Name = "Demo Game",
                            IMG = "images/demo.png",
                            Description = "Demo application",
                            URL = "" // оставьте пустым или укажите реальный .exe путь для теста
                        });
                    }

                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("SeedInitialData error: " + ex.Message);
            }
        }

        // --- async-friendly user creation (existing AddUserInDataBase left for compatibility) ---
        public static async Task AddUserInDataBase(string name, string password)
        {
            using (var db = new AppDbContext())
            {
                using (var tr = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (!db.Users.Any(u => u.Name == name))
                        {
                            var newHasgCode = HashCode.HashPassword(password);
                            var user = new User
                            {
                                Name = name,
                                Password = newHasgCode
                            };

                            db.Users.Add(user);
                            await db.SaveChangesAsync().ConfigureAwait(false);
                            tr.Commit();
                        }
                    }
                    catch (Exception ex)
                    {
                        tr.Rollback();
                        Debug.WriteLine("AddUserInDataBase error: " + ex.Message);
                        throw;
                    }
                }
            }
        }

        public static bool HaveThisUser(string user)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    return db.Users.Any(u => u.Name == user);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("HaveThisUser error: " + ex.Message);
                return false;
            }
        }

        public static string GetUserIdByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "0";

            try
            {
                using (var db = new AppDbContext())
                {
                    var id = db.Users.Where(u => u.Name == name).Select(u => (int?)u.Id).FirstOrDefault();
                    return (id ?? 0).ToString();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetUserIdByName error: " + ex.Message);
                return "0";
            }
        }

        public static string GetUserNameById(int id)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var name = db.Users.Where(x => x.Id == id).Select(x => x.Name).FirstOrDefault();
                    return name ?? "None";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetUserNameById error: " + ex.Message);
                return "None";
            }
        }

        public static bool CorrectEntrance(string user, string userpassword)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var users = db.Users.FirstOrDefault(x => x.Name == user);
                    if (users != null && HashCode.VerifyPassword(userpassword, users.Password))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("CorrectEntrance error: " + ex.Message);
                return false;
            }
        }

        public static List<string> GetInfoAboutUser(string userName)
        {
            var userInfo = new List<string>();

            if (string.IsNullOrWhiteSpace(userName))
            {
                return userInfo;
            }

            try
            {
                using (var db = new AppDbContext())
                {
                    var user = db.Users
                        .Where(u => u.Name.Equals(userName, StringComparison.OrdinalIgnoreCase))
                        .Select(u => new { u.Id, u.Name, u.Password })
                        .FirstOrDefault();

                    if (user != null)
                    {
                        userInfo.Add(user.Id.ToString());
                        userInfo.Add(user.Name);
                        userInfo.Add(user.Password);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetInfoAboutUser error: " + ex.Message);
            }

            return userInfo;
        }

        public static void SaveChanges(string name, string password)
        {
            using (var db = new AppDbContext())
            {
                using (var tr = db.Database.BeginTransaction())
                {
                    try
                    {
                        var user = db.Users.FirstOrDefault(x => x.Name == name);

                        if (user == null)
                        {
                            MessageBox.Show("Пользователь не найден");
                            return;
                        }

                        var newHashCode = HashCode.HashPassword(password);
                        db.Entry(user).Property(x => x.Password).IsModified = true;
                        user.Password = newHashCode;

                        db.SaveChanges();
                        tr.Commit();

                        MessageBox.Show("Пароль успешно изменён");
                    }
                    catch (Exception ex)
                    {
                        tr.Rollback();
                        Debug.WriteLine("SaveChanges error: " + ex.Message);
                        MessageBox.Show($"Ошибка: {ex.Message}");
                    }
                }
            }
        }

        // --- CREATE (existing async methods preserved) ---

        public static async Task AddElementInTableUsers(string name, string password)
        {
            using (var db = new AppDbContext())
            {
                using (var tr = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (!db.Users.Any(u => u.Name == name))
                        {
                            var newHasgCode = HashCode.HashPassword(password);
                            var user = new User
                            {
                                Name = name,
                                Password = newHasgCode
                            };

                            db.Users.Add(user);
                            await db.SaveChangesAsync().ConfigureAwait(false);
                            tr.Commit();
                        }
                    }
                    catch (Exception ex)
                    {
                        tr.Rollback();
                        Debug.WriteLine("AddElementInTableUsers error: " + ex.Message);
                        throw;
                    }
                }
            }
        }
        public static async Task AddElementInTableComputers(string name, string description, string cpu, string gpu, string ram, string storage, string monitor, float rating, float price, byte[] image)
        {
            using (var db = new AppDbContext())
            {
                using (var tr = db.Database.BeginTransaction())
                {
                    try
                    {
                        var computer = new Computer
                        {
                            Name = name,
                            Description = description,
                            Cpu = cpu,
                            Gpu = gpu,
                            Ram = ram,
                            Storage = storage,
                            Monitor = monitor,
                            Rating = rating,
                            PricePerHour = price,
                            GraphicData = image
                        };

                        db.Computers.Add(computer);
                        await db.SaveChangesAsync().ConfigureAwait(false);
                        tr.Commit();
                    }
                    catch (Exception ex)
                    {
                        tr.Rollback();
                        Debug.WriteLine("AddElementInTableComputers error: " + ex.Message);
                        throw;
                    }
                }
            }
        }
        public static async Task AddElementsInTableOrders(int user_id, int computer_id, string status, string date_order, string start_time, string end_time)
        {
            using (var db = new AppDbContext())
            {
                using (var tr = db.Database.BeginTransaction())
                {
                    try
                    {
                        var order = new Order
                        {
                            UserId = user_id,
                            ComputerId = computer_id,
                            Status = status,
                            DateOrder = date_order,
                            StartTime = start_time,
                            EndTime = end_time
                        };

                        db.Orders.Add(order);
                        await db.SaveChangesAsync().ConfigureAwait(false);
                        tr.Commit();
                    }
                    catch (Exception ex)
                    {
                        tr.Rollback();
                        Debug.WriteLine("AddElementsInTableOrders error: " + ex.Message);
                        throw;
                    }
                }
            }
        }
        public static async Task AddElementsInTableReviews(int user_id, int computer_id, float rating, string comment, string createAt)
        {
            using (var db = new AppDbContext())
            {
                try
                {
                    // Проверяем существование пользователя
                    var userExists = db.Users.Any(u => u.Id == user_id);
                    if (!userExists)
                    {
                        throw new InvalidOperationException($"User with ID {user_id} not found");
                    }

                    // Проверяем существование компьютера
                    var computerExists = db.Computers.Any(c => c.Id == computer_id);
                    if (!computerExists)
                    {
                        throw new InvalidOperationException($"Computer with ID {computer_id} not found");
                    }

                    // Используем прямой SQL запрос для вставки, чтобы избежать проблем с отслеживанием сущностей
                    // Используем параметризованный запрос для безопасности
                    string sql = @"INSERT INTO Reviews (user_id, computer_id, Rating, Comment, CreateAt) 
                                   VALUES (@p0, @p1, @p2, @p3, @p4)";
                    
                    object[] parameters = new object[]
                    {
                        new SqlParameter("@p0", user_id),
                        new SqlParameter("@p1", computer_id),
                        new SqlParameter("@p2", rating),
                        new SqlParameter("@p3", comment ?? string.Empty),
                        new SqlParameter("@p4", createAt ?? string.Empty)
                    };
                    
                    await db.Database.ExecuteSqlCommandAsync(sql, parameters).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("AddElementsInTableReviews error: " + ex.Message);
                    if (ex.InnerException != null)
                    {
                        Debug.WriteLine("Inner exception: " + ex.InnerException.Message);
                        Debug.WriteLine("Inner exception stack trace: " + ex.InnerException.StackTrace);
                        
                        // Проверяем вложенные исключения
                        var innerEx = ex.InnerException;
                        int depth = 0;
                        while (innerEx != null && depth < 5)
                        {
                            Debug.WriteLine($"Nested inner exception (depth {depth}): " + innerEx.Message);
                            innerEx = innerEx.InnerException;
                            depth++;
                        }
                    }
                    throw;
                }
            }
        }
        public static async Task AddElementsInTableApplicationGame(string name, string img, string description, string url)
        {
            using (var db = new AppDbContext())
            {
                using (var tr = db.Database.BeginTransaction())
                {
                    try
                    {
                        var app = new ApplicationGame
                        {
                            Name = name,
                            IMG = img,
                            Description = description,
                            URL = url
                        };

                        db.ApplicationGame.Add(app);
                        await db.SaveChangesAsync().ConfigureAwait(false);
                        tr.Commit();
                    }
                    catch (Exception ex)
                    {
                        tr.Rollback();
                        Debug.WriteLine("AddElementsInTableApplicationGame error: " + ex.Message);
                        throw;
                    }
                }
            }
        }

        // --- READ (synchronous wrappers kept for compatibility) ---

        public static List<User> ReadElementsInTableUsers()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    return db.Users.ToList();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ReadElementsInTableUsers error: " + ex.Message);
                return new List<User>();
            }
        }

        public static List<Computer> ReadElementsInTableComputers()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    return db.Computers.ToList();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ReadElementsInTableComputers error: " + ex.Message);
                return new List<Computer>();
            }
        }

        public static List<Order> ReadElementsInTableOrders()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    return db.Orders.ToList();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ReadElementsInTableOrders error: " + ex.Message);
                return new List<Order>();
            }
        }

        public static List<Reviews> ReadElementsInTableReviews()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    return db.Reviews.ToList();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ReadElementsInTableReviews error: " + ex.Message);
                return new List<Reviews>();
            }
        }

        public static List<ApplicationGame> ReadElementsInTableApplicationGame()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    return db.ApplicationGame.ToList();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ReadElementsInTableApplicationGame error: " + ex.Message);
                return new List<ApplicationGame>();
            }
        }

        // --- ASYNC READ helpers (preferred for UI) ---

        public static async Task<List<User>> ReadElementsInTableUsersAsync()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    return await db.Users.ToListAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ReadElementsInTableUsersAsync error: " + ex.Message);
                return new List<User>();
            }
        }

        public static async Task<List<Computer>> ReadElementsInTableComputersAsync()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    return await db.Computers.ToListAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ReadElementsInTableComputersAsync error: " + ex.Message);
                return new List<Computer>();
            }
        }

        public static async Task<List<Order>> ReadElementsInTableOrdersAsync()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    return await db.Orders.ToListAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ReadElementsInTableOrdersAsync error: " + ex.Message);
                return new List<Order>();
            }
        }

        public static async Task<List<Reviews>> ReadElementsInTableReviewsAsync()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    return await db.Reviews.ToListAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ReadElementsInTableReviewsAsync error: " + ex.Message);
                return new List<Reviews>();
            }
        }

        public static async Task<List<ApplicationGame>> ReadElementsInTableApplicationGameAsync()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    return await db.ApplicationGame.ToListAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ReadElementsInTableApplicationGameAsync error: " + ex.Message);
                return new List<ApplicationGame>();
            }
        }

        // --- UPDATE (async existing) ---

        public static async Task UpdateElementInTableUsers(int id, string name, string password)
        {
            if (HashCode.IsHash(password))
            {
                MessageBox.Show(
                    Lang.lang == "en"
            ? "The password field contains a hash. Please enter a new password, not an existing hash."
            : "В поле пароля обнаружен хэш. Введите новый пароль, а не существующий хэш."
                );
                return;
            }
            using (var db = new AppDbContext())
            {
                using (var tr = db.Database.BeginTransaction())
                {
                    try
                    {
                        var user = db.Users.Where(x => x.Id == id).FirstOrDefault();
                        if (user == null) return;
                        user.Name = name;
                        user.Password = HashCode.HashPassword(password);
                        await db.SaveChangesAsync().ConfigureAwait(false);
                        tr.Commit();
                    }
                    catch (Exception ex)
                    {
                        tr.Rollback();
                        Debug.WriteLine("UpdateElementInTableUsers error: " + ex.Message);
                        throw;
                    }
                }
            }
        }
        public static async Task UpdateElementInTableComputers(int id, string name, string description, string cpu, string gpu, string ram, string storage, string monitor, float rating, float price, byte[] image)
        {
            using (var db = new AppDbContext())
            {
                using (var tr = db.Database.BeginTransaction())
                {
                    try
                    {
                        var computer = db.Computers.Where(x => x.Id == id).FirstOrDefault();
                        if (computer == null) return;
                        computer.Name = name;
                        computer.Description = description;
                        computer.Cpu = cpu;
                        computer.Gpu = gpu;
                        computer.Ram = ram;
                        computer.Storage = storage;
                        computer.Monitor = monitor;
                        computer.Rating = rating;
                        computer.PricePerHour = price;
                        computer.GraphicData = image;
                        await db.SaveChangesAsync().ConfigureAwait(false);
                        tr.Commit();
                    }
                    catch (Exception ex)
                    {
                        tr.Rollback();
                        Debug.WriteLine("UpdateElementInTableComputers error: " + ex.Message);
                        throw;
                    }
                }
            }
        }
        public static async Task UpdateElementInTableOrders(int id, int user_id, int computer_id, string status, string date_order, string start_time, string end_time)
        {
            using (var db = new AppDbContext())
            {
                using (var tr = db.Database.BeginTransaction())
                {
                    try
                    {
                        var order = db.Orders.Where(x => x.Id == id).FirstOrDefault();
                        if (order == null) return;
                        order.UserId = user_id;
                        order.ComputerId = computer_id;
                        order.Status = status;
                        order.DateOrder = date_order;
                        order.StartTime = start_time;
                        order.EndTime = end_time;
                        await db.SaveChangesAsync().ConfigureAwait(false);
                        tr.Commit();
                    }
                    catch (Exception ex)
                    {
                        tr.Rollback();
                        Debug.WriteLine("UpdateElementInTableOrders error: " + ex.Message);
                        throw;
                    }
                }
            }
        }
        public static async Task UpdateElementInTableReviews(int id, int user_id, int computer_id, float rating, string comment, string createAt)
        {
            using (var db = new AppDbContext())
            {
                try
                {
                    var review = db.Reviews.Where(x => x.Id == id).FirstOrDefault();
                    if (review == null) return;
                    review.UserId = user_id;
                    review.ComputerId = computer_id;
                    review.Rating = rating;
                    review.Comment = comment;
                    review.CreateAt = createAt;
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("UpdateElementInTableReviews error: " + ex.Message);
                    throw;
                }
            }
        }
        public static async Task UpdateElementInTableApplicationGame(int id, string name, string img, string description, string url)
        {
            using (var db = new AppDbContext())
            {
                try
                {
                    var app = db.ApplicationGame.Where(x => x.Id == id).FirstOrDefault();
                    if (app == null) return;
                    app.Name = name;
                    app.IMG = img;
                    app.Description = description;
                    app.URL = url;
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("UpdateElementInTableApplicationGame error: " + ex.Message);
                    throw;
                }
            }
        }

        // --- DELETE (async existing) ---

        public static async Task DeleteElementInTableUsers(int id)
        {
            using (var db = new AppDbContext())
            {
                using (var tr = db.Database.BeginTransaction())
                {
                    try
                    {
                        var user = db.Users.FirstOrDefault(x => x.Id == id);
                        if (user != null)
                        {
                            var rewToDel = db.Reviews.Where(x => x.UserId == id);
                            if (rewToDel != null)
                            {
                                foreach (var el in rewToDel)
                                {
                                    db.Reviews.Remove(el);
                                }
                            }
                            db.Users.Remove(user);
                        }
                        await db.SaveChangesAsync().ConfigureAwait(false);
                        tr.Commit();
                    }
                    catch (Exception ex)
                    {
                        tr.Rollback();
                        Debug.WriteLine("DeleteElementInTableUsers error: " + ex.Message);
                        throw;
                    }
                }
            }
        }
        public static async Task DeleteElementInTableComputers(int id)
        {
            using (var db = new AppDbContext())
            {
                using (var tr = db.Database.BeginTransaction())
                {
                    try
                    {
                        var computer = db.Computers.FirstOrDefault(x => x.Id == id);
                        if (computer != null)
                        {
                            var reviewsToDelete = db.Reviews.Where(x => x.ComputerId == id);
                            if (reviewsToDelete != null)
                            {
                                foreach (var review in reviewsToDelete)
                                {
                                    db.Reviews.Remove(review);
                                }
                            }

                            var ordersToDelete = db.Orders.Where(x => x.ComputerId == id);
                            if (ordersToDelete != null)
                            {
                                foreach (var order in ordersToDelete)
                                {
                                    db.Orders.Remove(order);
                                }
                            }

                            db.Computers.Remove(computer);
                        }
                        await db.SaveChangesAsync().ConfigureAwait(false);
                        tr.Commit();
                    }
                    catch (Exception ex)
                    {
                        tr.Rollback();
                        Debug.WriteLine("DeleteElementInTableComputers error: " + ex.Message);
                        throw;
                    }
                }
            }
        }
        public static async Task DeleteElementInTableOrders(int id)
        {
            using (var db = new AppDbContext())
            {
                using (var tr = db.Database.BeginTransaction())
                {
                    try
                    {
                        var order = db.Orders.FirstOrDefault(x => x.Id == id);
                        if (order != null)
                        {
                            db.Orders.Remove(order);
                        }
                        await db.SaveChangesAsync().ConfigureAwait(false);
                        tr.Commit();
                    }
                    catch (Exception ex)
                    {
                        tr.Rollback();
                        Debug.WriteLine("DeleteElementInTableOrders error: " + ex.Message);
                        throw;
                    }
                }
            }
        }
        public static async Task DeleteElementInTableReviews(int id)
        {
            using (var db = new AppDbContext())
            {
                using (var tr = db.Database.BeginTransaction())
                {
                    try
                    {
                        var review = db.Reviews.FirstOrDefault(x => x.Id == id);
                        if (review != null)
                        {
                            db.Reviews.Remove(review);
                        }
                        await db.SaveChangesAsync().ConfigureAwait(false);
                        tr.Commit();
                    }
                    catch (Exception ex)
                    {
                        tr.Rollback();
                        Debug.WriteLine("DeleteElementInTableReviews error: " + ex.Message);
                        throw;
                    }
                }
            }
        }
        public static async Task DeleteElementInTableApplicationGame(int id)
        {
            using (var db = new AppDbContext())
            {
                using (var tr = db.Database.BeginTransaction())
                {
                    try
                    {
                        var app = db.ApplicationGame.FirstOrDefault(x => x.Id == id);
                        if (app != null)
                        {
                            db.ApplicationGame.Remove(app);
                        }
                        await db.SaveChangesAsync().ConfigureAwait(false);
                        tr.Commit();
                    }
                    catch (Exception ex)
                    {
                        tr.Rollback();
                        Debug.WriteLine("DeleteElementInTableApplicationGame error: " + ex.Message);
                        throw;
                    }
                }
            }
        }

        // --- DataTable helpers (unchanged logic, но упрощены транзакции) ---

        public static DataTable GetComputersTable(string filter = "default")
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    IQueryable<Computer> query = db.Computers;

                    if (filter == "Мощные процессоры")
                    {
                        query = query.Where(c => c.Cpu.Contains("Core i7") ||
                                               c.Cpu.Contains("Core i5") ||
                                               c.Cpu.Contains("Core i9") ||
                                               c.Cpu.Contains("Ryzen 5") ||
                                               c.Cpu.Contains("Ryzen 7") ||
                                               c.Cpu.Contains("Ryzen 9"));
                    }
                    else if (filter == "Мощные видеокарты")
                    {
                        query = query.Where(c => c.Gpu.Contains("GTX") ||
                                               c.Gpu.Contains("RTX") ||
                                               c.Gpu.Contains("RX"));
                    }
                    else if (filter == "Дорогие")
                    {
                        query = query.Where(c => c.PricePerHour >= 1000);
                    }

                    var computers = query.ToList();

                    DataTable table = new DataTable("Computers");

                    table.Columns.Add("Id", typeof(int));
                    table.Columns.Add("Name", typeof(string));
                    table.Columns.Add("Description", typeof(string));
                    table.Columns.Add("Cpu", typeof(string));
                    table.Columns.Add("Gpu", typeof(string));
                    table.Columns.Add("Ram", typeof(string));
                    table.Columns.Add("Storage", typeof(string));
                    table.Columns.Add("Monitor", typeof(string));
                    table.Columns.Add("Rating", typeof(float));
                    table.Columns.Add("PricePerHour", typeof(float));
                    table.Columns.Add("GraphicData", typeof(byte[]));

                    foreach (var computer in computers)
                    {
                        table.Rows.Add(
                            computer.Id,
                            computer.Name,
                            computer.Description,
                            computer.Cpu,
                            computer.Gpu,
                            computer.Ram,
                            computer.Storage,
                            computer.Monitor,
                            computer.Rating,
                            computer.PricePerHour,
                            computer.GraphicData ?? (object)DBNull.Value
                        );
                    }

                    return table;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetComputersTable error: " + ex.Message);
                return new DataTable("Computers");
            }
        }

        public static DataTable GetComputersTableWhere(string text, string filter = "default")
        {
            try
            {
                text = string.Empty; // защита от null
                string[] filters = text.Split(',');
                string cpuFilter = filters.Length > 0 ? filters[0].Trim() : "";
                string gpuFilter = filters.Length > 1 ? filters[1].Trim() : "";

                using (var db = new AppDbContext())
                {
                    IQueryable<Computer> query = db.Computers;

                    if (!string.IsNullOrEmpty(cpuFilter))
                    {
                        query = query.Where(c => c.Cpu.Contains(cpuFilter));
                    }

                    if (!string.IsNullOrEmpty(gpuFilter))
                    {
                        query = query.Where(c => c.Gpu.Contains(gpuFilter));
                    }
                    if (filter == "Мощные процессоры" || filter == "Powerful processors")
                    {
                        query = query.Where(c => c.Cpu.Contains("Core i7") ||
                                               c.Cpu.Contains("Core i5") ||
                                               c.Cpu.Contains("Core i9") ||
                                               c.Cpu.Contains("Ryzen 5") ||
                                               c.Cpu.Contains("Ryzen 7") ||
                                               c.Cpu.Contains("Ryzen 9"));
                    }
                    else if (filter == "Мощные видеокарты" || filter == "Powerful graphics cards")
                    {
                        query = query.Where(c => c.Gpu.Contains("GTX") ||
                                               c.Gpu.Contains("RTX") ||
                                               c.Gpu.Contains("RX"));
                    }
                    else if (filter == "Дорогие" || filter == "Expensive")
                    {
                        // согласованный порог с GetComputersTable
                        query = query.Where(c => c.PricePerHour >= 1000);
                    }

                    var computers = query.ToList();

                    DataTable table = new DataTable("Computers");

                    table.Columns.Add("Id", typeof(int));
                    table.Columns.Add("Name", typeof(string));
                    table.Columns.Add("Description", typeof(string));
                    table.Columns.Add("Cpu", typeof(string));
                    table.Columns.Add("Gpu", typeof(string));
                    table.Columns.Add("Ram", typeof(string));
                    table.Columns.Add("Storage", typeof(string));
                    table.Columns.Add("Monitor", typeof(string));
                    table.Columns.Add("Rating", typeof(float));
                    table.Columns.Add("PricePerHour", typeof(float));
                    table.Columns.Add("GraphicData", typeof(byte[]));

                    foreach (var computer in computers)
                    {
                        table.Rows.Add(
                            computer.Id,
                            computer.Name,
                            computer.Description,
                            computer.Cpu,
                            computer.Gpu,
                            computer.Ram,
                            computer.Storage,
                            computer.Monitor,
                            computer.Rating,
                            computer.PricePerHour,
                            computer.GraphicData ?? (object)DBNull.Value
                        );
                    }

                    return table;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetComputersTableWhere error: " + ex.Message);
                return new DataTable("Computers");
            }
        }

        public static DataTable GetUsersTable()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var users = db.Users.ToList();

                    DataTable table = new DataTable("Users");

                    table.Columns.Add("Id", typeof(int));
                    table.Columns.Add("Name", typeof(string));
                    table.Columns.Add("Password", typeof(string));

                    foreach (var user in users)
                    {
                        DataRow row = table.NewRow();
                        row["Id"] = user.Id;
                        row["Name"] = user.Name;
                        row["Password"] = user.Password;
                        table.Rows.Add(row);
                    }

                    return table;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetUsersTable error: " + ex.Message);
                return new DataTable("Users");
            }
        }

        public static DataTable GetUsersTableWhere(string text)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var users = db.Users.ToList();

                    DataTable table = new DataTable("Users");

                    table.Columns.Add("Id", typeof(int));
                    table.Columns.Add("Name", typeof(string));
                    table.Columns.Add("Password", typeof(string));

                    foreach (var user in users)
                    {
                        if (user.Name.Contains(text) && text.Length > 0)
                        {
                            DataRow row = table.NewRow();
                            row["Id"] = user.Id;
                            row["Name"] = user.Name;
                            row["Password"] = user.Password;
                            table.Rows.Add(row);
                        }
                        else if (text.Length == 0)
                        {
                            DataRow row = table.NewRow();
                            row["Id"] = user.Id;
                            row["Name"] = user.Name;
                            row["Password"] = user.Password;
                            table.Rows.Add(row);
                        }
                    }
                    return table;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetUsersTableWhere error: " + ex.Message);
                return new DataTable("Users");
            }
        }

        public static DataTable GetOrdersTable(string filter = "default")
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var query = db.Orders.AsQueryable();

                    if (filter == "Завершенные" || filter == "Completed")
                        query = query.Where(o => o.Status == "completed");
                    else if (filter == "Активные" || filter == "Active")
                        query = query.Where(o => o.Status == "active");
                    else if (filter == "Отмененные" || filter == "Cancelled")
                        query = query.Where(o => o.Status == "cancelled");

                    var orders = query.ToList();

                    DataTable table = new DataTable("Orders");


                    table.Columns.Add("Id", typeof(int));
                    table.Columns.Add("UserId", typeof(int));
                    table.Columns.Add("ComputerId", typeof(int));
                    table.Columns.Add("Status", typeof(string));
                    table.Columns.Add("DateOrder", typeof(string));
                    table.Columns.Add("StartTime", typeof(string));
                    table.Columns.Add("EndTime", typeof(string));


                    foreach (var order in orders)
                    {
                        table.Rows.Add(
                            order.Id,
                            order.UserId,
                            order.ComputerId,
                            order.Status,
                            order.DateOrder,
                            order.StartTime,
                            order.EndTime

                        );
                    }

                    return table;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetOrdersTable error: " + ex.Message);
                return new DataTable("Orders");
            }
        }

        public static DataTable GetReviewsTable()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var reviews = db.Reviews.ToList();
                    Debug.WriteLine($"GetReviewsTable: Found {reviews.Count} reviews in database");

                    DataTable table = new DataTable("Reviews");

                    table.Columns.Add("Id", typeof(int));
                    table.Columns.Add("UserId", typeof(int));
                    table.Columns.Add("ComputerId", typeof(int));
                    table.Columns.Add("Rating", typeof(float));
                    table.Columns.Add("Comment", typeof(string));
                    table.Columns.Add("CreateAt", typeof(string));

                    foreach (var review in reviews)
                    {
                        try
                        {
                            DataRow row = table.NewRow();
                            row["Id"] = review.Id;
                            row["UserId"] = review.UserId;
                            row["ComputerId"] = review.ComputerId;
                            row["Rating"] = review.Rating;
                            row["Comment"] = review.Comment ?? string.Empty;
                            row["CreateAt"] = review.CreateAt ?? string.Empty;
                            table.Rows.Add(row);
                        }
                        catch (Exception rowEx)
                        {
                            Debug.WriteLine($"Error adding review row (Id: {review.Id}): {rowEx.Message}");
                        }
                    }

                    Debug.WriteLine($"GetReviewsTable: Created DataTable with {table.Rows.Count} rows");
                    return table;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetReviewsTable error: " + ex.Message);
                if (ex.InnerException != null)
                {
                    Debug.WriteLine("Inner exception: " + ex.InnerException.Message);
                }
                return new DataTable("Reviews");
            }
        }

        public static DataTable GetApplicationGameTable()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var apps = db.ApplicationGame.ToList();

                    DataTable table = new DataTable("ApplicationGame");

                    table.Columns.Add("Id", typeof(int));
                    table.Columns.Add("Name", typeof(string));
                    table.Columns.Add("IMG", typeof(string));
                    table.Columns.Add("Description", typeof(string));
                    table.Columns.Add("URL", typeof(string));

                    foreach (var app in apps)
                    {
                        DataRow row = table.NewRow();
                        row["Id"] = app.Id;
                        row["Name"] = app.Name;
                        row["IMG"] = app.IMG;
                        row["Description"] = app.Description;
                        row["URL"] = app.URL;
                        table.Rows.Add(row);
                    }

                    return table;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetApplicationGameTable error: " + ex.Message);
                return new DataTable("ApplicationGame");
            }
        }
    }
}
