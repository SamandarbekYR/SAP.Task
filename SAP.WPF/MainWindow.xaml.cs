using LiveCharts;
using LiveCharts.Wpf;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SAP.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            PopulateChartData();
            Items();
        }
        // C# tilida
        public void PopulateChartData()
        {
            List<DateTime> dates = new List<DateTime>() {
                           DateTime.ParseExact("2022-24-10", "yyyy-dd-MM", CultureInfo.InvariantCulture),
                           DateTime.ParseExact("2022-25-10", "yyyy-dd-MM", CultureInfo.InvariantCulture),
                           DateTime.ParseExact("2022-26-10", "yyyy-dd-MM", CultureInfo.InvariantCulture)
};

            var chart = chart1;

            var xAxis = chart.AxisX.First(); 

        
            xAxis.Labels = dates.Select(date => date.ToString("yyyy-MM-dd")).ToList();
        }

        private void btnShutDown(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnNormal(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else WindowState = WindowState.Maximized;
        }

        private void btnMinimized(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        public void Items()
        {
            List<Double> item = new List<Double>();

            List<float> list = new List<float> { 2.3f, 2.5f, 2.5f,2.8f,3f,2.2f };


            foreach (var i in list)
            {
                seller_post_dg.Values.Add(Convert.ToDouble(i));
            }
        }

        private void Button_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Button? button = sender as Button;
            if (button != null)
            {
                button.Background = Brushes.Cyan;
            }
        }

        private void Button_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            Button? button = sender as Button;
            if (button != null)
            {
                button.Background = Brushes.White;
            }
        }
    }
}