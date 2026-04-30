using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

// Код выполнен студентом: Лазарев Артем

namespace AsyncHttpDemo
{
    class Program
    {
        private static readonly string[] Urls =
        {
            "https://jsonplaceholder.typicode.com/posts/1",
            "https://jsonplaceholder.typicode.com/comments/1",
            "https://jsonplaceholder.typicode.com/todos/1"
        };

        static async Task Main(string[] args)
        {
            Console.WriteLine("=== ВЕРСИЯ 2: Асинхронное выполнение с async/await ===");
            Console.WriteLine("Все запросы выполняются параллельно, поток не блокируется\n");

            var stopwatch = Stopwatch.StartNew();

            await ExecuteAsyncRequests();

            stopwatch.Stop();
            Console.WriteLine($"\n[ИТОГ] Общее время выполнения программы: {stopwatch.ElapsedMilliseconds} мс");

            Console.WriteLine("\n=== ПОЯСНЕНИЕ ===");
            Console.WriteLine("Асинхронный подход (async/await + Task.WhenAll):");
            Console.WriteLine("- Все три HTTP-запроса стартуют практически одновременно");
            Console.WriteLine("- Основной поток НЕ блокируется во время ожидания ответов от серверов");
            Console.WriteLine("- Во время ожидания сетевых операций поток может выполнять другую работу");
            Console.WriteLine("- Общее время равно времени САМОГО МЕДЛЕННОГО запроса, а не сумме времён");
            Console.WriteLine("- Task.WhenAll() — примитив синхронизации для ожидания набора задач");
            Console.WriteLine("- Отсутствуют гонки данных, так как каждая задача работает со своим URL");
        }

        static async Task ExecuteAsyncRequests()
        {
            using (var httpClient = new HttpClient())
            {
                var tasks = new List<Task<RequestResult>>();

                foreach (var url in Urls)
                {
                    tasks.Add(FetchJsonAsync(httpClient, url));
                }

                RequestResult[] results = await Task.WhenAll(tasks);

                Console.WriteLine("\n=== РЕЗУЛЬТАТЫ ЗАПРОСОВ ===\n");

                foreach (var result in results)
                {
                    if (result.IsSuccess)
                    {
                        Console.WriteLine($"✓ Запрос к: {result.Url}");
                        Console.WriteLine($"  Статус: УСПЕШНО");
                        Console.WriteLine($"  JSON-ответ: {FormatJson(result.Json)}");
                        Console.WriteLine();
                    }
                    else
                    {
                        Console.WriteLine($"✗ Запрос к: {result.Url}");
                        Console.WriteLine($"  Статус: ОШИБКА");
                        Console.WriteLine($"  Сообщение: {result.ErrorMessage}");
                        Console.WriteLine();
                    }
                }
            }
        }

        static async Task<RequestResult> FetchJsonAsync(HttpClient client, string url)
        {
            try
            {
                Console.WriteLine($"[ЗАПУЩЕН] Асинхронный запрос к: {url}");

                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"[ЗАВЕРШЁН] Запрос к {url} выполнен успешно");

                return RequestResult.Success(url, json);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[ОШИБКА] Запрос к {url}: {ex.Message}");
                return RequestResult.Failure(url, $"HTTP ошибка: {ex.Message}");
            }
            catch (TaskCanceledException)
            {
                return RequestResult.Failure(url, "Таймаут при выполнении запроса");
            }
            catch (Exception ex)
            {
                return RequestResult.Failure(url, $"Необработанное исключение: {ex.Message}");
            }
        }

        static string FormatJson(string json)
        {
            if (string.IsNullOrEmpty(json))
                return "<пустой ответ>";

            const int maxLength = 200;
            if (json.Length <= maxLength)
                return json;

            return json.Substring(0, maxLength) + "...";
        }
    }

    public class RequestResult
    {
        public string Url { get; }
        public bool IsSuccess { get; }
        public string Json { get; }
        public string ErrorMessage { get; }

        private RequestResult(string url, bool isSuccess, string json, string errorMessage)
        {
            Url = url;
            IsSuccess = isSuccess;
            Json = json;
            ErrorMessage = errorMessage;
        }

        public static RequestResult Success(string url, string json)
        {
            return new RequestResult(url, true, json, null);
        }

        public static RequestResult Failure(string url, string errorMessage)
        {
            return new RequestResult(url, false, null, errorMessage);
        }
    }
}