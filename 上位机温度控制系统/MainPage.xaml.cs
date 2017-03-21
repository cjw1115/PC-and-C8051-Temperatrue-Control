using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
namespace 上位机温度控制系统
{
    /// <summary>
    /// MainPage.xaml 的交互逻辑
    /// </summary>
    public partial class MainPage : Window
    {
        public static Polyline TempLine;
        public MainPage()
        {
            InitializeComponent();
            TempLine = tempLine;
            this.canvas.SizeChanged += Canvas_SizeChanged;

        }

        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            initCanvans();
        }
        void initCanvans()
        {
        }
        //默认显示100个点

        
        public static void Notify(string message)
        {
            MessageBox.Show(message);
        }
    }
   public class CanvaseExpand : Canvas
    {
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
        }
        private static double width = 400 / 100d;
        public static readonly DependencyProperty CustomPointsProperty = DependencyProperty.RegisterAttached("CustomPoints", typeof(List<double>), typeof(Canvas), new PropertyMetadata((o, e) =>
        {
            var canvas = o as Canvas;
            PointCollection collection = new PointCollection();
            var points = e.NewValue as List<double>;
            int count = 0;
            if (points == null || points.Count == 0)
                return;
            foreach (var item in points)
            {
                collection.Add(new Point(count * width, 200 - item*2));
                count++;
            }
            MainPage.TempLine.Points = collection;
        }));
        public List<double> CustomPoints
        {
            get { return (List<double>)this.GetValue(CustomPointsProperty); }
            set { SetValue(CustomPointsProperty, value); }
        }

        //public static void SetCustomPoints(DependencyObject sender, List<double> value)
        //{
        //    sender.SetValue(CustomPointsProperty, value);
        //}
        //public static List<double> GetCustomPoints(DependencyObject sender)
        //{
        //    return (List<double>)sender.GetValue(CustomPointsProperty);
        //}
    }
}
