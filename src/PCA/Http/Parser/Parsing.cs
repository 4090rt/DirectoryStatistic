using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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

                var result = await JsonSerializer.DeserializeAsync<List<T>>(stream, options);

                if (result != null)
                {
                    return result;
                }
                else
                {
                    _logger.LogError("Результат парсинг пуст!");
                    throw new Exception("Ошибка парсинга");
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError("Возникло исключение при работе с JSON" + ex.Message + ex.StackTrace);
                throw new Exception("Исключениепарсинга. Ошибка Json");
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение" + ex.Message + ex.StackTrace);
                throw new Exception("Исключение при парсинге");
            }
        }
    }
}
