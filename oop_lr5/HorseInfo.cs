using System.ComponentModel;
using System.Windows.Media;

namespace oop_lr5
{
    public class HorseInfo : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public SolidColorBrush Color { get; set; }

        private int position;
        public int Position
        {
            get => position;
            set { position = value; OnPropertyChanged(nameof(Position)); }
        }
        private string time;
        public string Time
        {
            get => time;
            set { time = value; OnPropertyChanged(nameof(Time)); }
        }
        public double Coefficient { get; set; }

        private double money;
        public double Money
        {
            get => money;
            set { money = value; OnPropertyChanged(nameof(Money)); }
        }
        public int Speed { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

