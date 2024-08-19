using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MessagingApp.Models;
using MessagingApp.Data;
using Firebase.Database.Query;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Globalization;
using System.Net.Mail;
using System.Net;

namespace MessagingApp.Controllers;

public class LoginController : Controller
{
    private readonly ILogger<LoginController> _logger;
    private readonly Database database = new();
    private string message = "";
    private bool check = false;


    public LoginController(ILogger<LoginController> logger)
    {
        _logger = logger;
    }

    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string phone_number, string password)
    {
        try
        {
            var response = await database.client.Child("users").OnceAsync<Users>();

            if (response != null && response.Count != 0)
            {
                var userSnapshot = response.FirstOrDefault(snapshot => snapshot.Object.PhoneNumber == phone_number && snapshot.Object.Password == password);

                if (userSnapshot != null)
                {
                    var userId = userSnapshot.Key;
                    HttpContext.Session.SetString("UserId", userId);
                    var claims = new List<Claim>{
                        new Claim(ClaimTypes.Name, userSnapshot.Object.Name),
                        new Claim(ClaimTypes.Surname, userSnapshot.Object.SurName),
                        new Claim(ClaimTypes.MobilePhone, phone_number)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties();

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    Console.WriteLine("Kullanıcı Bulunamadı.");
                    message = "Kullanıcı Bulunamadı.";
                }
            }
            else
            {
                Console.WriteLine("İnternet Bağlantı Hatası! Serverdan Yanıt Boş Geldi.");
                message = "İnternet bağlantınızı kontrol edin. Lütfen daha sonra tekrar deneyin.";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Login Controller'da Login Fonksiyonunda Hata" + ex.Message);
            message = "Beklenmedik bir hata lütfen tekrar deneyin.";
        }

        TempData["Message"] = message;
        TempData["Status"] = check;
        TempData["RedirectUrl"] = "";

        return View();
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(string name, string surname, string email, string phone_number, string new_password, string confirm_password)
    {
        try
        {
            bool hasUser = false;
            var response = await database.client.Child("users").OnceAsync<Users>();

            if (response != null && response.Count != 0)
            {
                var usersList = response.Where(sb => sb.Object.PhoneNumber == phone_number).ToList();

                if (usersList.Count > 0)
                {
                    hasUser = true;
                }
            }
            else
            {
                Console.WriteLine("İnternet Bağlantı Hatası! Serverdan Yanıt Boş Geldi.");
                message = "İnternet bağlantınızı kontrol edin. Lütfen daha sonra tekrar deneyin.";
            }

            if (!hasUser)
            {
                if (new_password.Equals(confirm_password))
                {
                    if (new_password.Length > 6 && new_password.Length <= 15)
                    {
                        Users user = new()
                        {
                            PhotoUrl = "",
                            Name = new CultureInfo("tr-TR", false).TextInfo.ToTitleCase(name),
                            SurName = surname.ToUpper(),
                            Email = email,
                            PhoneNumber = phone_number,
                            Password = new_password
                        };

                        var userResponse = await database.client.Child("users").PostAsync(user);
                        try
                        {
                            using var fileStream = new FileStream("wwwroot/img/defaultProfilePhoto.png", FileMode.Open);
                            var url = await database.storage.Child("images").Child(userResponse.Key).PutAsync(fileStream);
                            user.PhotoUrl = url;
                            await database.client.Child("users").Child(userResponse.Key).PutAsync(user);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }

                        if (userResponse != null)
                        {
                            Console.WriteLine("Kullanıcı Kaydı Başarılı");
                            message = "Kullanıcı Kaydı Başarılı";
                            check = true;
                            TempData["RedirectUrl"] = Url.Action("Login", "Login");
                        }
                        else
                        {
                            message = "Kullanıcı Kaydı Başarısız";
                            Console.WriteLine("Kullanıcı Kaydı Başarısız");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Yeni şifre 6 karekterden uzun ve en fazla 15 karekterden oluşmalıdır.");
                        message = "Yeni şifre 6 karekterden uzun ve en fazla 15 karekterden oluşmalıdır.";
                    }
                }
                else
                {
                    Console.WriteLine("Girilen yeni şifreler aynı olmalıdır.");
                    message = "Girilen yeni şifreler aynı olmalıdır.";
                }
            }
            else
            {
                Console.WriteLine("Numara kullanılıyor.");
                message = "Girilen telefon numarası kullanımda.";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Login Controller'da Register Fonksiyonunda Hata" + ex.Message);
            message = "Beklenmedik bir hata lütfen tekrar deneyin.";
        }

        TempData["Message"] = message;
        TempData["Status"] = check;
        if (!check)
            TempData["RedirectUrl"] = "";

        return View();
    }

    [HttpGet]
    public IActionResult SendMail()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SendMail(string phone_number, string email)
    {
        try
        {
            var response = await database.client.Child("users").OnceAsync<Users>();
            if (response != null && response.Count != 0)
            {
                var userSnapshot = response.FirstOrDefault(snapshot => snapshot.Object.PhoneNumber == phone_number && snapshot.Object.Email == email);

                if (userSnapshot != null)
                {
                    var code = new Random().Next(100000, 1000000).ToString("D6");
                    TempData["code"] = code;
                    SmtpClient smtpClient = new SmtpClient("smtp.gmail.com")
                    {
                        Port = 587,
                        Credentials = new NetworkCredential("nktkargo@gmail.com", "kgsv plpm oheu ocnz"),
                        EnableSsl = true,
                    };

                    MailMessage mailMessage = new MailMessage
                    {
                        From = new MailAddress("nktkargo@gmail.com", userSnapshot.Object.Name + " " + userSnapshot.Object.SurName + " Şire Yenileme"),
                        Subject = "Talep Edilen Şifre Yenileme Codu",
                        Body = "Şifre Yenileme İçin Kullanılacak Tek Seferlik Şifre Yenileme Kodu : " + code,
                        IsBodyHtml = true,
                    };

                    mailMessage.To.Add(email);

                    smtpClient.Send(mailMessage);

                    Console.WriteLine("Kod Başarılı Bir Şekilde Gönderildi.");

                    message = "Kod Gönderildi.";
                    check = true;

                    var claims = new List<Claim>{
                        new Claim(ClaimTypes.Name, userSnapshot.Object.Name),
                        new Claim(ClaimTypes.Surname, userSnapshot.Object.SurName),
                        new Claim(ClaimTypes.MobilePhone, phone_number)
                    };
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties();
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
                    TempData["RedirectUrl"] = Url.Action("ForgetPassword", "Login");
                }
                else
                {
                    Console.WriteLine("Kullanıcı Bulunamadı.");
                    message = "Kullanıcı Bulunamadı.";
                }
            }
            else
            {
                Console.WriteLine("İnternet Bağlantı Hatası! Serverdan Yanıt Boş Geldi.");
                message = "İnternet bağlantınızı kontrol edin. Lütfen daha sonra tekrar deneyin.";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Login Controller'da Login Fonksiyonunda Hata" + ex.Message);
            message = "Beklenmedik bir hata lütfen tekrar deneyin.";
        }

        TempData["Message"] = message;
        TempData["Status"] = check;
        if (!check)
            TempData["RedirectUrl"] = "";

        return View();
    }

    [HttpGet]
    public IActionResult ForgetPassword()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ForgetPassword(string confirm_code, string new_password, string confirm_password)
    {
        var temp = TempData["code"] as string;
        try
        {
            var response = await database.client.Child("users").OnceAsync<Users>();
            if (response != null && response.Count != 0)
            {
                var userSnapshot = response.FirstOrDefault(snapshot => snapshot.Object.PhoneNumber == User.FindFirst(ClaimTypes.MobilePhone)?.Value);

                if (userSnapshot != null)
                {
                    if (temp == confirm_code)
                    {
                        if (new_password.Equals(confirm_password) && new_password.Length > 6 && new_password.Length <= 15)
                        {
                            if (new_password != userSnapshot.Object.Password)
                            {
                                userSnapshot.Object.Password = new_password;
                                await database.client.Child("users").Child(userSnapshot.Key).PutAsync(userSnapshot.Object);
                                Console.WriteLine("Şifre Başarılı Bir Şekilde Değiştirildi.");
                                message = "Şire başarılı bir şekilde değiştirildi.";
                                check = true;
                                TempData["RedirectUrl"] = Url.Action("Login", "Login");
                            }
                            else
                            {
                                Console.WriteLine("Yeni Şifre Eskisi ile aynı olamaz.");
                                message = "Yeni Şifre Eskisi ile aynı olamaz.";
                            }
                        }
                        else
                        {
                            Console.WriteLine("Yeni Şifreyi aynı girmeniz gerekiyor. Yeni şifre 6 karekterden uzun ve en fazla 15 karekterden oluşmalıdır.");
                            message = "Yeni Şifreyi aynı girmeniz gerekiyor. Yeni şifre 6 karekterden uzun ve en fazla 15 karekterden oluşmalıdır.";
                        }
                    }
                    else
                    {
                        Console.WriteLine("Yanlış Yenileme Kodu");
                        message = "Yanlış Yenileme Kodu";
                    }
                }
            }
            else
            {
                Console.WriteLine("İnternet Bağlantı Hatası! Serverdan Yanıt Boş Geldi.");
                message = "İnternet bağlantınızı kontrol edin. Lütfen daha sonra tekrar deneyin.";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Login Controller'da Login Fonksiyonunda Hata" + ex.Message);
            message = "Beklenmedik bir hata lütfen tekrar deneyin.";
        }

        TempData["Message"] = message;
        TempData["Status"] = check;
        TempData["code"] = temp;
        if (!check)
            TempData["RedirectUrl"] = "";

        return View();
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.Session.Clear();
        HttpContext.Session.Remove("UserId");
        HttpContext.Response.Cookies.Delete("Message.Auth");
        return RedirectToAction("Index", "Home");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
