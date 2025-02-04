using System.Net.Mail;
using System.Net;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

public class ContactController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public ContactController(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendMessageV2([FromBody] ContactModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Turnstile doğrulaması
            var token = Request.Form["cf-turnstile-response"].ToString();
            var isValid = await ValidateTurnstileToken(token);

            if (!isValid)
            {
                return BadRequest("Turnstile doğrulaması başarısız oldu.");
            }

            // E-posta gönderme
            try
            {
                using (var client = new SmtpClient("smtp.gmail.com", 587))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(
                        _configuration["EmailSettings:Email"],
                        _configuration["EmailSettings:Password"]
                    );

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_configuration["EmailSettings:Email"]),
                        Subject = "Yeni İletişim Formu Mesajı",
                        Body = $"Ad: {model.Name}\n" +
                               $"Soyad: {model.Surname}\n" +
                               $"E-posta: {model.Email}\n" +
                               $"Telefon: {model.Phone}\n" +
                               $"Mesaj: {model.Message}",
                        IsBodyHtml = false
                    };
                    mailMessage.To.Add("cagri363@gmail.com");

                    await client.SendMailAsync(mailMessage);
                }

                return Ok(new { message = "Mesajınız başarıyla gönderildi." });
            }
            catch
            {
                ModelState.AddModelError("", "E-posta gönderilirken bir hata oluştu.");
                return View("Index", model);
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Bir hata oluştu: " + ex.Message);
        }
    }

    private async Task<bool> ValidateTurnstileToken(string token)
    {
        var secret = _configuration["Turnstile:SecretKey"];
        var client = _httpClientFactory.CreateClient();

        var values = new Dictionary<string, string>
        {
            { "secret", secret },
            { "response", token }
        };

        var content = new FormUrlEncodedContent(values);
        var response = await client.PostAsync("https://challenges.cloudflare.com/turnstile/v0/siteverify", content);
        var result = await response.Content.ReadFromJsonAsync<TurnstileResponse>();

        return result?.Success ?? false;
    }
}

public class TurnstileResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("error-codes")]
    public string[] ErrorCodes { get; set; }
}