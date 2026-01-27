using System.Diagnostics;

namespace HttpLoadTest {
    internal class Program {
        // ===== 設定値 =====
        private const string TargetUrl = "http://localhost:5048/";  // 接続先URL
        private const int ParallelCount = 10;  // 並列実行数
        private const int TotalRequests = 200;  // 総リクエスト数
        private const string JwtToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkxvYWRUZXN0IiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";  // ダミーJWT
        // ==================

        static async Task Main(string[] args) {
            Console.WriteLine("=== HTTP 負荷テスト開始 ===");
            Console.WriteLine($"接続先: {TargetUrl}");
            Console.WriteLine($"並列数: {ParallelCount}");
            Console.WriteLine($"総リクエスト数: {TotalRequests}");
            Console.WriteLine();

            var results = new List<TestResult>();
            var lockObject = new object();

            // Bearer認証付きHTTPクライアントの作成
            using var httpClient = new HttpClient() {
                Timeout = TimeSpan.FromSeconds(30)
            };
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", JwtToken);

            var stopwatch = Stopwatch.StartNew();

            var lastProgressTime = DateTime.Now;

            // 並列実行オプション（実行前に設定）
            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = ParallelCount };

            // 並列実行（awaitで全リクエストの完了を待機）
            await Parallel.ForEachAsync(
                Enumerable.Range(1, TotalRequests),
                parallelOptions,
                async (requestNumber, cancellationToken) => {
                    var result = await ExecuteRequest(httpClient, requestNumber);

                    lock (lockObject) {
                        results.Add(result);

                        // 進捗表示（約1秒おき、または最終リクエスト）
                        var now = DateTime.Now;
                        if ((now - lastProgressTime).TotalSeconds >= 1.0 || results.Count == TotalRequests) {
                            Console.WriteLine($"進捗: {results.Count}/{TotalRequests} リクエスト完了");
                            lastProgressTime = now;
                        }
                    }
                });

            stopwatch.Stop();

            // 結果集計
            Console.WriteLine();
            Console.WriteLine("=== テスト結果 ===");
            Console.WriteLine($"総実行時間: {stopwatch.ElapsedMilliseconds}ms ({stopwatch.Elapsed.TotalSeconds:F2}秒)");
            Console.WriteLine($"成功: {results.Count(r => r.Success)}");
            Console.WriteLine($"失敗: {results.Count(r => !r.Success)}");

            if (results.Any(r => r.Success)) {
                var successResults = results.Where(r => r.Success).ToList();
                Console.WriteLine($"平均レスポンス時間: {successResults.Average(r => r.ElapsedMilliseconds):F2}ms");
                Console.WriteLine($"最小レスポンス時間: {successResults.Min(r => r.ElapsedMilliseconds)}ms");
                Console.WriteLine($"最大レスポンス時間: {successResults.Max(r => r.ElapsedMilliseconds)}ms");
            }

            // エラーがある場合は表示
            var errors = results.Where(r => !r.Success).ToList();
            if (errors.Any()) {
                Console.WriteLine();
                Console.WriteLine("=== エラー詳細 ===");
                foreach (var error in errors.Take(5)) {
                    Console.WriteLine($"リクエスト#{error.RequestNumber}: {error.ErrorMessage}");
                }
                if (errors.Count > 5) {
                    Console.WriteLine($"...他 {errors.Count - 5} 件のエラー");
                }
            }

            Console.WriteLine();
            Console.WriteLine("テスト完了。Enterキーで終了...");
            Console.ReadLine();
        }

        private static async Task<TestResult> ExecuteRequest(HttpClient httpClient, int requestNumber) {
            var stopwatch = Stopwatch.StartNew();
            var result = new TestResult { RequestNumber = requestNumber };

            try {
                var response = await httpClient.GetAsync(TargetUrl);
                stopwatch.Stop();

                result.Success = response.IsSuccessStatusCode;
                result.StatusCode = (int)response.StatusCode;
                result.ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;

                if (!response.IsSuccessStatusCode) {
                    result.ErrorMessage = $"HTTP {result.StatusCode}";
                }
            }
            catch (Exception ex) {
                stopwatch.Stop();
                result.Success = false;
                result.ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;
                result.ErrorMessage = ex.Message;
            }

            return result;
        }
    }

    internal class TestResult {
        public int RequestNumber { get; set; }
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public long ElapsedMilliseconds { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
