﻿using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SAP.API.Services.Interfaces;
using System.Net.Http.Json;
using System.Security.Cryptography.Xml;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
            _redis = redis;
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

                    // 2 ta listni birlashtirganimizda duplicate lar paydo bo'lishi mumkun
                    // shuning uchun duplicatelarni o'chiramiz
                    List<Rate> uniqueDataList = addList
                                                     .GroupBy(obj => new { obj.Cur_ID, obj.Date })
                                                     .GroupBy(group => group.Key.Date)  // Guruhlarni ikkita bosqichda guruhlash
                                                     .SelectMany(group => group.Select(g => g.First()))
                                                     .ToList();


                    // List ni jsonga formatlash
                    var json1 = JsonConvert.SerializeObject(uniqueDataList, Newtonsoft.Json.Formatting.Indented);
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
                _logger.LogWarning($"Cachega qo'shishda xatolik yuz berdi {ex}");

                return "Xatolik yuz berdi";
            }
        }

        public async Task<List<Rate>> GetAll(int Cur_ID, DateTime startDate, DateTime endDate)
        {
            try
            {
                var data1 = await _redis.GetString(CACHE_KEY);
                if (!string.IsNullOrWhiteSpace(data1) && data1 != null)
                {
                    List<Rate> entityList = JsonConvert.DeserializeObject<List<Rate>>(data1)!
                                                .Where(c => c.Cur_ID == Cur_ID)
                                                .ToList();
                    if(entityList is null || entityList.Count() == 0)
                    {
                         await AddCacheAsync(Cur_ID, startDate, endDate.AddDays(1));
                         return await GetAllInfoAsync(Cur_ID, startDate, endDate);
                    }
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
                                       .Where(c => c.Cur_ID == Cur_ID && (c.Date.AddDays(1)> startDate && c.Date < endDate.AddDays(1)))
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
