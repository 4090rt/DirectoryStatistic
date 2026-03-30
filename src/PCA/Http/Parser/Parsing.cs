using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DirectoryStatistic.Http.Parser
{
    public class Parsing
    {
        private readonly ILogger _logger;

        public Parsing(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<List<T>> ParsingForHttp<T>(Stream stream)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                // Читаем JSON как строку для определения типа (объект или массив)
                using var reader = new StreamReader(stream, Encoding.UTF8);
                var json = await reader.ReadToEndAsync();

                // Проверяем, начинается ли JSON с '[' (массив) или '{' (объект)
                var trimmedJson = json.Trim();
                if (trimmedJson.StartsWith("["))
                {
                    // Это массив
                    var result = await JsonSerializer.DeserializeAsync<List<T>>(new MemoryStream(Encoding.UTF8.GetBytes(json)), options);
                    if (result != null)
                    {
                        return result;
                    }
                }
                else if (trimmedJson.StartsWith("{"))
                {
                    // Это одиночный объект - оборачиваем в список
                    var item = await JsonSerializer.DeserializeAsync<T>(new MemoryStream(Encoding.UTF8.GetBytes(json)), options);
                    if (item != null)
                    {
                        return new List<T> { item };
                    }
                }

                _logger.LogError("Результат парсинга пуст или имеет неверный формат!");
                throw new Exception("Ошибка парсинга: неверный формат JSON");
            }
            catch (JsonException ex)
            {
                _logger.LogError("Возникло исключение при работе с JSON: " + ex.Message);
                throw new Exception("Исключение парсинга. Ошибка Json", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение: " + ex.Message);
                throw new Exception("Исключение при парсинге", ex);
            }
        }
    }
}
