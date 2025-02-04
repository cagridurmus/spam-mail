using System.Net.Mail;
using System.Net;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Mvc;

public class ContactController : Controller
{
    private readonly IConfiguration _configuration;

    public ContactController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Submit(ContactModel model)
    {
        //if (!ModelState.IsValid)
        //{
        //    return View("Index", model);
        //}

        // reCAPTCHA doğrulama
        var recaptchaSuccess = await VerifyRecaptcha(model.RecaptchaToken);
        if (!recaptchaSuccess)
        {
            ModelState.AddModelError("", "Güvenlik doğrulaması başarısız oldu. Lütfen tekrar deneyin.");
            return View("Index", model);
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

            TempData["Success"] = "Mesajınız başarıyla gönderildi.";
            return RedirectToAction("Index");
        }
        catch
        {
            ModelState.AddModelError("", "E-posta gönderilirken bir hata oluştu.");
            return View("Index", model);
        }
    }

    private async Task<bool> VerifyRecaptcha(string token)
    {
        using (var client = new HttpClient())
        {
            var response = await client.GetStringAsync(
                $"https://www.google.com/recaptcha/api/siteverify?" +
                $"secret={_configuration["RecaptchaSettings:SecretKey"]}&" +
                $"response={token}"
            );

            var jsonResponse = JObject.Parse(response);
            return jsonResponse.Value<bool>("success") && 
                   jsonResponse.Value<double>("score") >= 0.5;
        }
    }
}