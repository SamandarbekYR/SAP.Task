using Newtonsoft.Json;
using SAP.API.Entities.Rates;
using SAP.API.Services.Interfaces;

namespace SAP.API.Services.Service
{
    public class RateService : IRate
    {
        private const string CACHE_KEY = "testkey";
        private ILogger<RateService> _logger;
        private IRedis _redis;


        public RateService(IRedis redis, ILogger<RateService> logger)
        {
            _logger = logger;
            _redis = redis ?? throw new ArgumentNullException(nameof(redis));

            //lifetime.ApplicationStopping.Register(async () => await OnStopping());
        }


        // Bu hozircha ishlatilmayapti To'g'irlaydigon joylari bor
        private async Task OnStopping()
        {
            try
            {
                Console.WriteLine("Stop ishlamoqdamanlfjoefhriug3rpog5rhgh59g99ih");
                string filePath = "C:\\Users\\Samandar\\Desktop\\SAP.Task\\SAP.API\\Files\\Cache\\Cache.json"; // Fayl manzili
                var data = await _redis.GetString(CACHE_KEY);

                if (data != null)
                {
                    await File.WriteAllTextAsync(filePath, data);
                    await _redis.DeleteAsync(CACHE_KEY);
                    Console.WriteLine("Stop ishlamoqdamanlfjoefhriug3rpog5rhgh59g99ih");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling file: {ex.Message}");
            }
        }


        public async Task<string> AddCacheAsync(int Cur_ID, DateTime startDate, DateTime endDate)
        {
            try
            {
                List<Rate> list = new List<Rate>();
                List<Rate> entityList = new List<Rate>();

                var data1 = await _redis.GetString(CACHE_KEY);
                await _redis.DeleteAsync(CACHE_KEY);

                if (data1 is not null)
                {
                    entityList = JsonConvert.DeserializeObject<List<Rate>>(data1!)!;
                }

                using (HttpClient client = new HttpClient())
                {
                    // Barcha sanalarni olish uchun sikl (loop)
                    while (startDate < endDate)
                    {
                        // API url manzili
                        string apiUrl = $"https://api.nbrb.by/exrates/rates/{Cur_ID}?ondate={startDate:yyyy-MM-dd}";

                        // GET so'rovini yuborish
                        HttpResponseMessage response = await client.GetAsync(apiUrl);

                        // Natijani tekshirish
                        if (response.IsSuccessStatusCode)
                        {
                            // Ma'lumotni JSON formatidan o'qib olamiz
                            Rate? result = await response.Content.ReadFromJsonAsync<Rate>();

                            if (result != null)
                            {
                                // Rate obyektini listga qo'shamiz
                                list.Add(result);
                            }
                        }
                        else
                        {
                            Console.WriteLine($"HTTP xato kodi: {response.StatusCode}");
                        }

                        // Keyingi kunga o'tkazish
                        startDate = startDate.AddDays(1);
                    }
                }

                if (entityList.Any())
                {
                    // Oldingi cache bilan yangi qo'shayotgan cache listimizni bir biriga qo'shish
                    List<Rate> addList = entityList.Concat(list).ToList();

                    // List ni jsonga formatlash
                    var json1 = JsonConvert.SerializeObject(addList, Newtonsoft.Json.Formatting.Indented);
                    await _redis.SetAsync(CACHE_KEY, json1);

                    return json1;
                }

                // List ni jsonga formatlash
                var json = JsonConvert.SerializeObject(list, Newtonsoft.Json.Formatting.Indented);
                // RedisCache ga malumot qo'shish
                await _redis.SetAsync(CACHE_KEY, json);

                _logger.LogInformation($"Yangi malumotlar cachega saqlandi {json}");
                return json;

            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Xatolik yuz berdi {ex}");

                return "Xatolik yuz berdi";
            }
        }

        public async Task<List<Rate>> GetAll(int Cur_ID, DateTime startDate, DateTime endDate)
        {

            try
            {
                var data1 = await _redis.GetString(CACHE_KEY);
                if (string.IsNullOrWhiteSpace(data1) && data1 != null)
                {
                    List<Rate> entityList = JsonConvert.DeserializeObject<List<Rate>>(data1)!
                                                .Where(c => c.Cur_ID == Cur_ID)
                                                .ToList();
                    if (entityList != null)
                    {
                        DateTime minDate = entityList.Min(date => date.Date);
                        DateTime maxDate = entityList.Max(date => date.Date);

                        if (minDate > startDate)
                        {
                            await AddCacheAsync(Cur_ID, startDate, minDate)!;
                        }
                        if (maxDate < endDate)
                        {
                            await AddCacheAsync(Cur_ID, maxDate, endDate.AddDays(1))!;
                        }
                        return await GetAllInfoAsync(Cur_ID, startDate, endDate);
                    }
                    else
                    {
                        await AddCacheAsync(Cur_ID, startDate, endDate.AddDays(1))!;
                        return await GetAllInfoAsync(Cur_ID, startDate, endDate);
                    }
                }

                await AddCacheAsync(Cur_ID, startDate, endDate.AddDays(1));

                _logger.LogInformation("Cache dan malumot olinmoqda");
                return await GetAllInfoAsync(Cur_ID, startDate, endDate);
            }
            catch (Exception ex)
            {

                _logger.LogInformation($"Cache dan malumot olishda xatolik yuz berdi {ex}");

                return new List<Rate>();
            }
        }

        public async Task<List<Rate>> GetAllInfoAsync(int Cur_ID, DateTime startDate, DateTime endDate)
        {
            try
            {
                var data1 = await _redis.GetString(CACHE_KEY);
                List<Rate> entityList = JsonConvert.DeserializeObject<List<Rate>>(data1!)!
                                       .Where(c => c.Cur_ID == Cur_ID && (c.Date >= startDate || c.Date <= endDate))
                                       .ToList();
                return entityList;
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Cache dan malumot olishda xatolik yuz berdi {ex}");
                return new List<Rate>();
            }
        }
    }
}
