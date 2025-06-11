using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace oop_lr5
{
    public class Horse
    {
        public string Name { get; set; }
        public double Speed { get; set; }
        public double Position { get; set; }
        public int FrameIndex { get; set; }
        public double Y { get; set; }
        public string ColorName { get; set; }
        public double Coefficient { get; set; }
        public int Money { get; set; }
        public SolidColorBrush ColorBrush { get; set; }
        public Image ImageControl { get; set; }
        public TextBlock NameLabel { get; set; }
        public bool Finished { get; set; } = false;
        public DateTime FinishTime { get; set; }
        public TimeSpan RaceTime { get; set; }
        public TimeSpan CurrentRaceTime { get; set; }
        public double DistanceRun { get; set; }
        public int DisplayPosition { get; set; }
    }
}
