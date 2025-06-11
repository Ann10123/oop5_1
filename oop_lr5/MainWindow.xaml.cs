using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Threading;
using System.Security.Policy;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

namespace oop_lr5
{
    public partial class MainWindow : Window
    {
        private const int FrameCount = 12;
        private const string ImagePath = @"C:\\Users\\06028\\source\\repos\\oop_lr5\\oop_lr5\\Images\\";
        private const int TrackWidth = 1300;

        private bool betPlaced = false;

        private bool raceFinished = false;
        private List<Horse> horses = new List<Horse>();
        private DispatcherTimer timer;
        private bool isRaceFinished = false;
        private Random rand = new Random();
        private HashSet<int> horsesBetOn = new HashSet<int>();
        private ObservableCollection<HorseInfo> horseInfoList = new ObservableCollection<HorseInfo>();

        private int balance = 10000;
        private int betAmount = 20;
        private int selectedHorseIndex = 0;

        private DateTime raceStartTime;

        public MainWindow()
        {
            InitializeComponent();

            LoadBackground();

            if (HorseCountSelector.SelectedIndex == -1)
                HorseCountSelector.SelectedIndex = 0; // За замовчуванням 2 коні

            InitHorses(HorseCountSelector.SelectedIndex + 2);
            UpdateUI();
        }
        private void LoadBackground()
        {
            string trackPath = System.IO.Path.Combine(ImagePath, @"Background\\Track.png");
            if (System.IO.File.Exists(trackPath))
            {
                var trackImage = new BitmapImage(new Uri(trackPath, UriKind.Absolute));
                RaceCanvas.Background = new ImageBrush(trackImage)
                {
                    Stretch = Stretch.Fill,
                    Viewport = new Rect(0, 0, 1200, 1000),
                    ViewportUnits = BrushMappingMode.Absolute
                };
            }
        }
        private void InitHorses(int count)
        {
            RaceCanvas.Children.Clear();
            horses.Clear();

            for (int i = 0; i < count; i++)
            {
                double coeff = Math.Round(1.1 + rand.NextDouble() * 0.9, 2);

                string[] horseNames = { "Lucky", "Ranger", "Willow", "Tucker", "Storm", "Thunder", "Shadow", "Blaze", "Comet", "Spirit" };
                Color colors = Color.FromRgb(
                    (byte)rand.Next(0, 256),
                    (byte)rand.Next(0, 256),
                    (byte)rand.Next(0, 256));
                SolidColorBrush randomBrush = new SolidColorBrush(colors);

                Horse horse = new Horse
                {
                    Name = horseNames[i % horseNames.Length],
                    Speed = rand.Next(5, 11),
                    Position = 0,
                    FrameIndex = 0,
                    Y = 220 + i * 30,
                    ColorName = colors.ToString(),
                    Coefficient = coeff,
                    Money = 0,
                    ColorBrush = randomBrush,
                    ImageControl = new Image { Width = 64, Height = 64 }
                };

                TextBlock nameLabel = new TextBlock
                {
                    Text = horse.Name,
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.Bold,
                    Visibility = Visibility.Hidden,
                    Padding = new Thickness(2),
                    FontSize = 12
                };

                horse.NameLabel = nameLabel;
                RaceCanvas.Children.Add(horse.NameLabel);
                Canvas.SetTop(horse.NameLabel, horse.Y - 20);
                Canvas.SetLeft(horse.NameLabel, horse.Position);

                horse.ImageControl.MouseEnter += (s, e) => nameLabel.Visibility = Visibility.Visible;
                horse.ImageControl.MouseLeave += (s, e) => nameLabel.Visibility = Visibility.Hidden;

                horses.Add(horse);
                RaceCanvas.Children.Add(horse.ImageControl);
                Canvas.SetTop(horse.ImageControl, horse.Y);
                Canvas.SetLeft(horse.ImageControl, horse.Position);
                UpdateHorseImage(horse);
            }

            HorseSelector.ItemsSource = horses.Select((h, i) => $"{i + 1}. {h.Name}");
            HorseSelector.SelectedIndex = 0;
            selectedHorseIndex = 0;

            horseInfoList.Clear();
            foreach (var h in horses)
            {
                horseInfoList.Add(new HorseInfo
                {
                    Name = h.Name,
                    Color = h.ColorBrush,
                    Speed = (int)h.Speed,
                    Position = 0,
                    Time = "00:00.000",
                    Coefficient = h.Coefficient,
                    Money = Math.Round(h.Coefficient * betAmount, 2)
                });
            }

            DetailsGrid.ItemsSource = horseInfoList;
        }
        private void StartAnimation()
        {
            timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
            raceStartTime = DateTime.Now;
            timer.Tick += Timer_Tick;
            timer.Start();
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            bool allFinished = true;

            foreach (var horse in horses)
            {
                if (horse.Finished)
                {
                    horse.CurrentRaceTime = horse.RaceTime;
                    continue;
                }

                double randomFactor = 0.5 + rand.NextDouble() * 0.4;
                horse.Position += horse.Speed * randomFactor;
                Canvas.SetLeft(horse.ImageControl, horse.Position);
                Canvas.SetLeft(horse.NameLabel, horse.Position);
                horse.DistanceRun += horse.Speed;

                horse.FrameIndex = (horse.FrameIndex + 1) % FrameCount;
                UpdateHorseImage(horse);

                horse.CurrentRaceTime = DateTime.Now - raceStartTime;

                if (horse.Position >= TrackWidth - 100)
                {
                    horse.Finished = true;
                    horse.FinishTime = DateTime.Now;
                    horse.RaceTime = horse.FinishTime - raceStartTime;
                    horse.CurrentRaceTime = horse.RaceTime;
                }
                else
                {
                    allFinished = false;
                }
            }

            RaceScrollViewer.ScrollToHorizontalOffset(horses.Max(h => h.Position) - 200);

            if (!raceFinished && horses.All(h => h.Finished))
            {
                raceFinished = true;
                isRaceFinished = true;
                timer.Stop();
                HorseSelector.IsEnabled = true;
                UpdateDetailsGrid();

                var winner = horses.OrderBy(h => h.RaceTime).First();

                if (horses.IndexOf(winner) == selectedHorseIndex)
                {
                    double win = betAmount * winner.Coefficient;
                    balance += (int)win;
                    winner.Money = (int)win;

                    MessageBox.Show($"🏆 Переміг {winner.Name}! Ви виграли {win}$!");
                }
                else
                {
                    MessageBox.Show($"🐍 Переміг {winner.Name}. Ви програли {betAmount}$.");
                }
                UpdateUI();
            }
            HorseSelector.IsEnabled = true;
            // Оновити UI
            UpdateDetailsGrid();
        }
        public void UpdateHorseImage(Horse horse)
        {
            string frameFile = $"WithOutBorder_00{horse.FrameIndex:D2}.png";
            string maskFile = $"mask_00{horse.FrameIndex:D2}.png";

            var frameBitmap = new BitmapImage(new Uri(System.IO.Path.Combine(ImagePath, "Horses", frameFile)));
            var maskBitmap = new BitmapImage(new Uri(System.IO.Path.Combine(ImagePath, "HorsesMask", maskFile)));

            var visual = new DrawingVisual();
            using (var dc = visual.RenderOpen())
            {
                dc.DrawImage(frameBitmap, new Rect(0, 0, 64, 64));
                dc.PushOpacityMask(new ImageBrush(maskBitmap));
                dc.DrawRectangle(horse.ColorBrush, null, new Rect(0, 0, 64, 64));
            }

            var rtb = new RenderTargetBitmap(64, 64, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(visual);
            horse.ImageControl.Source = rtb;
        }
        public void UpdateUI()
        {
            BalanceText.Text = $"{balance}$";
            BetAmountText.Text = $"{betAmount}$";
        }
        public void UpdateDetailsGrid()
        {
            var sortedHorses = horses
            .OrderBy(h => h.Finished ? 0 : 1)
            .ThenBy(h => h.Finished ? h.RaceTime.TotalMilliseconds : -h.Position)
            .ToList();

            for (int i = 0; i < sortedHorses.Count; i++)
            {
                sortedHorses[i].DisplayPosition = i + 1;
            }

            foreach (var horse in horses)
            {
                var info = horseInfoList.FirstOrDefault(h => h.Name == horse.Name);
                if (info != null)
                {
                    info.Position = horse.DisplayPosition;
                    info.Time = horse.CurrentRaceTime.ToString(@"mm\:ss\.fff");
                    info.Money = Math.Round(horse.Coefficient * betAmount, 2);
                }
            }

            // Пересортування View (виводу в таблиці)
            ICollectionView view = CollectionViewSource.GetDefaultView(DetailsGrid.ItemsSource);
            if (view != null)
            {
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new SortDescription("Position", ListSortDirection.Ascending));
                view.Refresh();
            }
        }
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (!betPlaced)
            {
                MessageBox.Show("Ставку не зроблено.");
                return;
            }
            horsesBetOn.Clear();
            betPlaced = false;
            raceFinished = false;
            HorseSelector.IsEnabled = false;
            isRaceFinished = false;
            StartAnimation();
        }
        private void DecreaseBet_Click(object sender, RoutedEventArgs e)
        {
            if (betAmount > 10)
            {
                betAmount -= 5;
                UpdateUI();
            }
        }
        private void IncreaseBet_Click(object sender, RoutedEventArgs e)
        {
            if (betAmount + 10 <= balance)
            {
                betAmount += 5;
                UpdateUI();
            }
        }
        private void PlaceBet_Click(object sender, RoutedEventArgs e)
        {
            if (HorseSelector.SelectedIndex < 0)
            {
                MessageBox.Show("Виберіть коня для ставки.");
                return;
            }
            if (horsesBetOn.Contains(HorseSelector.SelectedIndex))
            {
                MessageBox.Show("Ставку на цього коня вже зроблено! Оберіть іншого.");
                return;
            }

            if (betAmount > balance)
            {
                MessageBox.Show("Недостатньо коштів.");
                return;
            }

            selectedHorseIndex = HorseSelector.SelectedIndex;
            balance -= betAmount;
            MessageBox.Show($"Ставка {betAmount}$ на {horses[selectedHorseIndex].Name} прийнята.");
            UpdateUI();

            betPlaced = true;
            horsesBetOn.Add(selectedHorseIndex);

            bet.IsEnabled = false;
            HorseSelector.IsEnabled = false;
        }
        private void ConfirmHorseCount_Click(object sender, RoutedEventArgs e)
        {
            if (HorseCountSelector.SelectedItem is ComboBoxItem selectedItem &&
         int.TryParse(selectedItem.Content.ToString(), out int count))
            {
                InitHorses(count);

                UpdateUI();
                UpdateDetailsGrid();

                HorseSelector.IsEnabled = true;
                bet.IsEnabled = true;
            }
        }
    }
}
