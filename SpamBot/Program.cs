using System.Net.Http.Json;
using System.Text;
using Newtonsoft.Json;

class Program
{
    private static readonly HttpClient client = new HttpClient();
    private static readonly string url = "https://localhost:7034/Contact/Submit"; // Port numaranıza göre değiştirin

    static async Task Main(string[] args)
    {
        Console.WriteLine("Spam Bot Başlatılıyor...");
        
        // Kaç adet spam mail göndermek istediğinizi belirleyin
        int spamCount = 1000;

        for (int i = 0; i < spamCount; i++)
        {
            try
            {
                var data = new
                {
                    Name = $"Test{i}",
                    Surname = $"User{i}",
                    Email = $"test{i}@test.com",
                    Phone = $"555000{i.ToString().PadLeft(4, '0')}",
                    Message = $"Bu bir test spam mesajıdır #{i}",
                    RecaptchaToken = "fake_token" // reCAPTCHA'yı bypass etmeye çalışıyoruz
                };

                var content = new StringContent(
                    JsonConvert.SerializeObject(data),
                    Encoding.UTF8,
                    "application/json");

                var response = await client.PostAsync(url, content);
                
                Console.WriteLine($"Spam #{i + 1} Sonuç: {response.StatusCode}");
                Console.WriteLine($"Yanıt: {await response.Content.ReadAsStringAsync()}");
                
                // Biraz bekleyelim ki sunucu aşırı yüklenmesin
                await Task.Delay(1000);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata oluştu: {ex.Message}");
            }
        }

        Console.WriteLine("Test tamamlandı. Çıkmak için bir tuşa basın...");
        Console.ReadKey();
    }
}