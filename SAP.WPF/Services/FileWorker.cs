using Newtonsoft.Json;
using SAP.API.Services;
using SAP.WPF;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows;

namespace SAP.Service.Services.Service
{
    public class FileWorker : IFileWorker
    {
        public const string path = "..//..//..//Files//Cache.json";
        public const string key = "testkey";

        public async Task CacheToFile()
        {
            try
            {
                List<Rate> list = new List<Rate>();
                using (HttpClient client = new HttpClient())
                {
                    // API url manzili
                    string apiUrl = $"https://localhost:7083/api/get/cache-to-file?key={key}";

                    // GET so'rovini yuborish
                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    // Natijani tekshirish
                    if (response.IsSuccessStatusCode)
                    {
                        // Ma'lumotni JSON formatidan o'qib olamiz
                        List<Rate>? result = await response.Content.ReadFromJsonAsync<List<Rate>>()!;
                        if (result != null)
                        {
                            await File.WriteAllTextAsync(path, JsonConvert.SerializeObject(result,Formatting.Indented));
                        }
                    }
                    else
                    {
                        Console.WriteLine($"HTTP xato kodi: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Malumot kiritishda xatolik yuz berdi {ex}");
            }
        }

        public async void FileToCache()
        {
            string apiUrl = $"https://localhost:7083/api/get/file-to-cache?key={key}";

            using (HttpClient client = new HttpClient())
            {
                // Request headers
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/plain"));
                client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                // Request data directly from the file
                StringContent content = new StringContent(File.ReadAllText(path), System.Text.Encoding.UTF8, "application/json");

                // Send POST request
                HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Malumotlar saqlandi");
                }
                else
                {
                    // Handle error
                    Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                }
            }
        }

    }
}
