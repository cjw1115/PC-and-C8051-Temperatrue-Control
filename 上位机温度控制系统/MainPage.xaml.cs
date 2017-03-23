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
        public static MainPageVM VM;
        public static Polyline TempLine;
        public MainPage()
        {
            InitializeComponent();
            VM = this.DataContext as MainPageVM;
            TempLine = tempLine;
            this.canvas.SizeChanged += Canvas_SizeChanged;
            this.Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            initCanvans();
        }

        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            initCanvans();
        }
        void initCanvans()
        {
            var solidBrush = new SolidColorBrush(Colors.Blue);
            var height=canvas.ActualHeight / 100;
            for (int i = 0; i < 100; i++)
            {
                var line = new Polyline();
                line.Stroke = solidBrush;
                line.StrokeThickness = 0.3;

                line.Points = new PointCollection();
                for (int j = 0; j < 2; j++)
                {
                    line.Points.Add( new Point(0, height * i));
                    line.Points.Add(new Point(5, height * i));
                }
                this.canvas.Children.Add(line);
            }
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
            width = canvas.Width / MainPage.VM.TempCellectionCount ;
            foreach (var item in points)
            {
                collection.Add(new Point(count * width, canvas.ActualHeight- canvas.ActualHeight* item /100));
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
