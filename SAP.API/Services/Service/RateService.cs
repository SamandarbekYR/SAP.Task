using Newtonsoft.Json;
using SAP.API.Entities.Rates;
using SAP.API.Services.Interfaces;

namespace SAP.API.Services.Service
{
    public class RateService : IRate
    {
        private const string CACHE_KEY = "testkey";
        private IRedis _redis;

        public RateService(IRedis redis)
        {
            this._redis = redis;
        }

        public async Task<string> AddCacheAsync(int Cur_ID, DateTime startDate, DateTime endDate)
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

            return json;
        }

        public async Task<List<Rate>> GetAll(int Cur_ID, string startDate, string endDate)
        {
            var data1 = await _redis.GetString(CACHE_KEY);
            DateTime startdate = DateTime.Parse(startDate);
            DateTime enddate = DateTime.Parse(endDate);
            if (data1 != null)
            {
                List<Rate> entityList = JsonConvert.DeserializeObject<List<Rate>>(data1)!
                                            .Where(c => c.Cur_ID == Cur_ID)
                                            .ToList();
                if (entityList != null)
                {
                    DateTime minDate = entityList.Min(date => date.Date);
                    DateTime maxDate = entityList.Max(date => date.Date);

                    if (minDate > startdate)
                    {
                        await AddCacheAsync(Cur_ID, startdate, minDate)!;
                    }
                    if (maxDate < enddate)
                    {
                        await AddCacheAsync(Cur_ID, maxDate, enddate.AddDays(1))!;
                    }
                    return await GetAllInfoAsync(Cur_ID, startdate, enddate);
                }
                else
                {
                    await AddCacheAsync(Cur_ID, startdate, enddate.AddDays(1))!;
                    return await GetAllInfoAsync(Cur_ID, startdate, enddate);
                }
            }

            await AddCacheAsync(Cur_ID, startdate, enddate.AddDays(1));

            return await GetAllInfoAsync(Cur_ID, startdate, enddate);
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
                return new List<Rate>();
            }
        }
    }
}
