using System.ComponentModel.DataAnnotations;

public class ContactModel
{
    [Required(ErrorMessage = "Ad alanı zorunludur")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Soyad alanı zorunludur")]
    public string Surname { get; set; }

    [Required(ErrorMessage = "E-posta alanı zorunludur")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Telefon alanı zorunludur")]
    public string Phone { get; set; }

    [Required(ErrorMessage = "Mesaj alanı zorunludur")]
    public string Message { get; set; }

    public string RecaptchaToken { get; set; }
}