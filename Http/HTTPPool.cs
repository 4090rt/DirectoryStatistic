using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryStatistic
{
    public class HTTPPool
    {
        public readonly Stack<HttpClient> _pool = new Stack<HttpClient>();

        public HttpClient httpClientOpen()
        {
            try
            {
                lock (_pool)
                {
                    if (_pool.Count > 0)
                    {
                        _pool.Pop();
                    }
                    return new HttpClient();
                }
            }
            catch(Exception ex) 
            {
                Console.WriteLine("Ошибка получения пула объекта" + ex.Message);
                return null;
            }
        }

        public void httpClientClosed(HttpClient client)
        { 
            client.DefaultRequestHeaders.Clear();
            client.CancelPendingRequests();
            try
            {
                lock (_pool)
                {
                    if (_pool.Count < 10)
                    {
                        _pool.Push(client);
                    }
                    else
                    {
                        client.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка возвращения в  пул объекта" + ex.Message);
            }
        }
    }
}
