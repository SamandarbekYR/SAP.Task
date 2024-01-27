using SAP.API.Entities.Rates;
using SAP.API.Services.Service;
using System.Net.Http;
using System.Net.Http.Json;
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
        public RateService _rate;
        public MainWindow()
        {
            InitializeComponent();
            //  PopulateChartData();
        }
        //        // C# tilida
        //        public void PopulateChartData()
        //        {
        //            List<DateTime> dates = new List<DateTime>() {
        //                           DateTime.ParseExact("2022-24-10", "yyyy-dd-MM", CultureInfo.InvariantCulture),
        //                           DateTime.ParseExact("2022-25-10", "yyyy-dd-MM", CultureInfo.InvariantCulture),
        //                           DateTime.ParseExact("2022-26-10", "yyyy-dd-MM", CultureInfo.InvariantCulture)
        //};

        //            var chart = chart1;

        //            var xAxis = chart.AxisX.First();


        //            xAxis.Labels = dates.Select(date => date.ToString("yyyy-MM-dd")).ToList();
        //        }

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

        private async Task<List<Rate>> GetAll(int Cur_ID, DateTime startDate, DateTime endDate)
        {
            try
            {
                List<Rate> list = new List<Rate>();
                using (HttpClient client = new HttpClient())
                {
                    // API url manzili
                    string apiUrl = $"https://localhost:7083/api/Get?id={Cur_ID}" +
                                    $"&startDate={startDate:yyyy-MM-dd}" +
                                    $"&endDate={endDate:yyyy-MM-dd}";

                    // GET so'rovini yuborish
                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    // Natijani tekshirish
                    if (response.IsSuccessStatusCode)
                    {
                        // Ma'lumotni JSON formatidan o'qib olamiz
                        List<Rate>? result = await response.Content.ReadFromJsonAsync<List<Rate>>()!;
                        if (result != null)
                            return result;
                    }
                    else
                    {
                        Console.WriteLine($"HTTP xato kodi: {response.StatusCode}");
                    }
                    return list;
                }
            }
            catch (Exception ex)
            {
                return new List<Rate>();
            }
        }

        private async Task btnSendRequest1()
        {
            try
            {
                DateTime fromDate;
                DateTime toDate;

                if (DateTime.TryParse(FromDate.SelectedDate.ToString(), out fromDate) &&
                    DateTime.TryParse(ToDate.SelectedDate.ToString(), out toDate))
                {
                    //bu sanadan kichik sanada ma'lumotlar yo'q
                    string minDate = "2021-07-09";
                    DateTime MinDate = DateTime.Parse(minDate);

                    if (ToDate.SelectedDate >= MinDate)
                    {
                        if (CMBSelectValyuta.SelectedItem is ComboBoxItem selectedItem)
                        {
                            int Cur_ID = Convert.ToInt32(selectedItem.TabIndex);
                            DateTime toDate1 = DateTime.Parse(ToDate.SelectedDate.ToString()!);
                            DateTime fromDate1 = DateTime.Parse(FromDate.SelectedDate.ToString()!);

                            List<Rate> info = await GetAll(Cur_ID, toDate1, fromDate1);

                            List<DateTime> dates = new List<DateTime>();

                            foreach (Rate rate in info)
                            {
                                dates.Add(rate.Date);
                            }

                            var chart = chart1;
                            var xAxis = chart.AxisX.First();
                            xAxis.Labels = dates.Select(date => date.ToString("yyyy-MM-dd")).ToList();

                            foreach (var i in info)
                            {
                                seller_post_dg.Values.Add(Convert.ToDouble(i.Cur_OfficialRate));
                            }
                        };
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }


        private async void btnSendRequest(object sender, RoutedEventArgs e)
        {
            await btnSendRequest1();
        }
    }
}
