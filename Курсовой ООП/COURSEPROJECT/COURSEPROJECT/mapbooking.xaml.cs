using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace COURSEPROJECT
{
    static public class StatesBooking
    {
        static public List<string> elem = new List<string>();
        static public int id = -1;

        static public void AddElem(string item)
        {
            elem.Add(item);
            id = elem.Count;
        }
        static public void PrevState(Canvas ClubMap)
        {
            if (id > 0)
            {
                var computerGrids = ClubMap.Children.OfType<Grid>().ToList();
                foreach (var grid in computerGrids)
                {
                    var rectangles = grid.Children.OfType<Rectangle>().ToList();
                    var textBlocks = grid.Children.OfType<TextBlock>().ToList();
                    if (textBlocks[0].Text == elem[id])
                    {
                        rectangles[0].Fill = (Brush)new BrushConverter().ConvertFrom("#FF695AC3");
                        id--;
                    }
                }
            }
        }
        static public void NextState(Canvas ClubMap)
        {
            if (id < elem.Count - 1)
            {
                var computerGrids = ClubMap.Children.OfType<Grid>().ToList();
                foreach (var grid in computerGrids)
                {
                    var rectangles = grid.Children.OfType<Rectangle>().ToList();
                    var textBlocks = grid.Children.OfType<TextBlock>().ToList();
                    if (textBlocks[0].Text == elem[id])
                    {
                        rectangles[0].Fill = (Brush)new BrushConverter().ConvertFrom("#FF62c259");
                        id++;
                    }
                }
            }
        }
    }

    public partial class mapbooking : Window
    {
        public DispatcherTimer timer;
        private string _usercompid;
        public mapbooking(string usercompid = null)
        {
            InitializeComponent();
            _usercompid = usercompid;
            try
            {
                this.Cursor = new Cursor(System.Windows.Application.GetResourceStream(new Uri("/images/cursor.cur", UriKind.Relative)).Stream);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Lang.lang == "en"
                   ? $"Cursor loading error: {ex.Message}"
                   : $"Ошибка загрузки курсора: {ex.Message}");
            }
            
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(.1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            GenerateComputers(_usercompid);
        }
        private void GenerateComputers(string usercompid = null)
        {
            var positions = new[]
            {
        new Point(67, 64),
        new Point(150, 64),
        new Point(236, 64),
        new Point(317, 64),
        new Point(400, 64),
        new Point(584, 64),
        new Point(666, 64),
        new Point(67, 240),
        new Point(150, 240),
        new Point(236, 240),
        new Point(317, 240),
        new Point(400, 240),
        new Point(584, 240),
        new Point(666, 240),
        new Point(400, 471),
        new Point(495, 471),
        new Point(584, 471),
        new Point(666, 471)
    };

            List<Computer> computers = database.ReadElementsInTableComputers()
                .OrderBy(c => c.Id)  
                .ToList();

            string user = Global.CurrentUser;
            string userid = database.GetUserIdByName(user);
            List<Order> userOrders = new List<Order>();
            List<Order> allOrders = database.ReadElementsInTableOrders();

            if (!string.IsNullOrEmpty(userid))
            {
                userOrders = allOrders
                    .Where(x => x.UserId == int.Parse(userid))
                    .ToList();
            }

            for (int i = 0; i < positions.Length && i < computers.Count; i++)
            {
                var computer = computers[i];
                var pos = positions[i];

                var grid = new Grid
                {
                    Width = 60,
                    Height = 60,
                    Tag = computer.Id  
                };

                Shape element = new Rectangle
                {
                    Fill = (Brush)new BrushConverter().ConvertFrom("#FF695AC3"),
                    Stroke = (Brush)new BrushConverter().ConvertFrom("#FF000000"),
                    StrokeThickness = 2
                };

                element.MouseDown += (sender, e) =>
                {
                    var window = System.Windows.Application.Current.Windows.OfType<Item>().FirstOrDefault();
                    if (window == null)
                    {
                        window = new Item(computer);
                        window.Show();
                    }
                    else
                    {
                        window.Activate();
                        if (window.WindowState == WindowState.Minimized)
                            window.WindowState = WindowState.Normal;
                    }
                };

                var textBlock = new TextBlock
                {
                    Text = computer.Id.ToString(),
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };


                var userActiveOrder = userOrders.FirstOrDefault(o =>
                    o.ComputerId == computer.Id &&
                    o.Status == "active");

                if (userActiveOrder != null)
                {
                    bool isTime = IsCurrentTimeInRange(
                        userActiveOrder.StartTime,
                        userActiveOrder.EndTime,
                        DateTime.Parse(userActiveOrder.DateOrder));

                    element.Fill = isTime
                        ? (Brush)new BrushConverter().ConvertFrom("#1ea855")  
                        : (Brush)new BrushConverter().ConvertFrom("#d63176"); 
                }
                else
                {

                    var otherActiveOrder = allOrders.FirstOrDefault(o =>
                        o.ComputerId == computer.Id &&
                        o.Status == "active" &&
                        o.UserId.ToString() != userid);

                    if (otherActiveOrder != null)
                    {
                        bool isTime = IsCurrentTimeInRange(
                            otherActiveOrder.StartTime,
                            otherActiveOrder.EndTime,
                            DateTime.Parse(otherActiveOrder.DateOrder));

                        if (isTime)
                        {
                            element.Fill = (Brush)new BrushConverter().ConvertFrom("#b2b51b");
                        }
                    }
                }

 
                if (usercompid != null && computer.Name == usercompid)
                {
                    element.Fill = (Brush)new BrushConverter().ConvertFrom("#25baba");
                }

                grid.Children.Add(element);
                grid.Children.Add(textBlock);

                Canvas.SetLeft(grid, pos.X);
                Canvas.SetTop(grid, pos.Y);
                ClubMap.Children.Add(grid);
            }
        }
        public bool IsCurrentTimeInRange(string startTimeStr, string endTimeStr, DateTime dateOrder)
        {
            DateTime currentDateTime = DateTime.Now;
            DateTime startDateTime = dateOrder.Date + TimeSpan.Parse(startTimeStr);
            DateTime endDateTime = dateOrder.Date + TimeSpan.Parse(endTimeStr);

            if (TimeSpan.Parse(startTimeStr) > TimeSpan.Parse(endTimeStr))
            {
                endDateTime = endDateTime.AddDays(1);
            }

            return currentDateTime >= startDateTime && currentDateTime <= endDateTime;
        }
    }
}
