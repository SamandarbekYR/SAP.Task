using LiveCharts.Wpf;
using MahApps.Metro.Controls;
using SAP.API.Services;
using SAP.Service.Services.Service;
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
        public MainWindow()
        {
            InitializeComponent();
            AppOpen();
            // MessageBox.Show($"{DateTime.UtcNow.AddHours(5):yyyy-MM-dd}");
        }
        public void AppOpen()
        {
            FileWorker _fileWorker = new FileWorker();
            _fileWorker.FileToCache();

            ToDate.SelectedDate = Properties.Settings.Default.StartDate;
            FromDate.SelectedDate = Properties.Settings.Default.EndDate;
            CMBSelectValyuta.SelectedIndex = Properties.Settings.Default.Valyuta;
            var state = Properties.Settings.Default.WindowS;
            WindowState = state == 1 ? WindowState.Maximized : WindowState.Normal;
        }
        private async void btnShutDown(object sender, RoutedEventArgs e)
        {
            await AppShutDown();
            Application.Current.Shutdown();
        }
        public async Task AppShutDown()
        {
            try
            {
                if (FromDate.SelectedDate is not null)
                    Properties.Settings.Default.EndDate = FromDate.SelectedDate.Value;
                if (ToDate.SelectedDate is not null)
                    Properties.Settings.Default.StartDate = ToDate.SelectedDate.Value;
                if (WindowState == WindowState.Normal)
                {
                    Properties.Settings.Default.WindowS = 2;
                }
                else
                {
                    Properties.Settings.Default.WindowS = 1;
                }
                if (CMBSelectValyuta.SelectedItem != null)
                {
                    if (CMBSelectValyuta.SelectedItem is ComboBoxItem selectedItem)
                    {
                        int tabIndex = selectedItem.TabIndex;
                        Properties.Settings.Default.Valyuta = tabIndex == 431 ? 0 :
                                                              tabIndex == 451 ? 1 : 2;
                    }
                    Properties.Settings.Default.Save();
                    // Dastur boshida
                    FileWorker _fileWorker = new FileWorker();

                    await _fileWorker.CacheToFile();

                }
            }
            catch (Exception ex)
            {
                // Istisno bilan ishlash
                MessageBox.Show($"Xatolik: {ex.Message}");
            }
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
                        else
                        {
                            MessageBox.Show("Siz bergan vaqt oraliqda ma'lumotlar topilmadi");
                        }
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
                MessageBox.Show($"Malumot kiritishda xatolik yuz berdi {ex}");
                return new List<Rate>();
            }
        }

        private LiveCharts.Wpf.LineSeries GetSeller_post_dg()
        {
            return seller_post_dg;
        }

        private async Task btnSendRequest1(LiveCharts.Wpf.LineSeries seller_post_dg)
        {
            btnSendRequestName.IsEnabled = false;
            seller_post_dg.Values.Clear();

            try
            {
                if (DateTime.TryParse(FromDate.SelectedDate?.ToString(), out DateTime fromDate) &&
                    DateTime.TryParse(ToDate.SelectedDate?.ToString(), out DateTime toDate))
                {
                    string maxD = $"{DateTime.UtcNow.AddHours(5):yyyy-MM-dd}";
                    // Minimum date condition
                    DateTime minDate = DateTime.Parse("2021-07-09");

                    if (toDate >= minDate && toDate <= fromDate && fromDate <= DateTime.Parse(maxD))
                    {
                        if (CMBSelectValyuta.SelectedItem is ComboBoxItem selectedItem)
                        {
                            int Cur_ID = Convert.ToInt32(selectedItem.TabIndex);

                            List<Rate> info = await GetAll(Cur_ID, toDate, fromDate);

                            List<DateTime> dates = info.Select(rate => rate.Date).ToList();

                            await Task.Run(() =>
                            {
                                Axis xAxis = null;
                                chart1.BeginInvoke(() =>
                                {
                                    xAxis = chart1.AxisX.First();
                                    xAxis.Labels = dates.Select(date => date.ToString("yyyy-MM-dd")).ToList();

                                    foreach (var i in info)
                                    {
                                        seller_post_dg.Values.Add(Convert.ToDouble(i.Cur_OfficialRate));
                                    }
                                });

                                // Minimum value condition
                                //if (seller_post_dg.Values.Any())
                                //{
                                //    double minValue = seller_post_dg.Values.Min();
                                //    var minPoint = seller_post_dg.Points.FirstOrDefault(p => p.Y == minValue);
                                //    if (minPoint != null)
                                //        minPoint.Stroke = Brushes.Green;
                                //}
                            });
                            SolidColorBrush DangerBrush = new SolidColorBrush(Colors.Red);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Kun tanlashda xatolik yuz berdi bu kunda ma'lumotlar yo'q", "Information",
                                   MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Siz noto'g'ri ma'lumot kiritdingiz", "Xatolik",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Xatolik yuz berdi: {ex.Message}");
            }
            finally
            {
                btnSendRequestName.IsEnabled = true;
            }
        }
        private async void btnSendRequest(object sender, RoutedEventArgs e)
        {
            await btnSendRequest1(GetSeller_post_dg());
        }
    }
}
