using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MessagingApp.Models;
using MessagingApp.Data;
using System.Text;
using Newtonsoft.Json;
using Firebase.Database.Query;

namespace MessagingApp.Controllers;

public class BaseController : Controller
{
    private readonly ILogger<BaseController> _logger;
    private readonly Database database = new();
    private string userId = "";

    public BaseController(ILogger<BaseController> logger)
    {
        _logger = logger;
    }

    public async Task<Dictionary<string, FriendList>> GetFriends()
    {
        Dictionary<string, FriendList> friendsList = [];
        userId = HttpContext.Session.GetString("UserId") ?? "";
        var friendsResponse = await database.client.Child("friendList").Child(userId).OnceAsync<FriendList>();
        if (friendsResponse != null && friendsResponse.Count != 0)
        {
            foreach (var friend in friendsResponse)
            {
                friendsList.Add(friend.Object.UserId, friend.Object);
            }
        }

        return friendsList;
    }

    public async Task<List<Message>> GetMessages()
    {
        List<Message> messagesList = [];
        HashSet<string> sendersId = [];
        userId = HttpContext.Session.GetString("UserId") ?? "";
        var messagesResponse = await database.client.Child("messageHistory").Child(userId).OnceAsync<Dictionary<string, Message>>();
        if (messagesResponse != null && messagesResponse.Count != 0)
        {
            foreach (var message in messagesResponse)
            {
                if (!sendersId.Contains(message.Key))
                {
                    foreach (var item in message.Object.Reverse())
                    {
                        messagesList.Add(item.Value);
                        break;
                    }
                }
            }
        }

        return messagesList;
    }

    public async Task<Dictionary<string, Users>> GetUsers()
    {
        Dictionary<string, Users> usersList = [];
        var usersResponse = await database.client.Child("users").OnceAsync<Users>();
        if (usersResponse != null && usersResponse.Count != 0)
        {
            foreach (var user in usersResponse)
            {
                usersList.Add(user.Key, user.Object);
            }
        }

        return usersList;
    }

    [HttpPost]
    public async Task<IActionResult> SearchFriend()
    {
        string phone = "";
        using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
        {
            var jsonString = await reader.ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<dynamic>(jsonString);

            if (data != null)
            {
                phone = data.phoneData;
            }
        }

        try
        {
            var usersList = await GetUsers();
            foreach (var user in usersList)
            {
                if (user.Value.PhoneNumber == phone)
                {
                    return Json(new { success = true, id = user.Key, name = user.Value.Name + " " + user.Value.SurName, photoUrl = user.Value.PhotoUrl });
                }
            }
            Console.WriteLine("Kullanıcı Bulunamadı.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Base Controller'da SearchFriend Fonksiyonunda Hata" + ex.Message);
        }

        return Json(new { success = false, message = phone + " Telefon Numaralı Kullanıcı Bulunamadı. Numarayı Doğru Yazdığınızdan Emin Olun ve Tekrar Deneyin!" });
    }

    [HttpPost]
    public async Task<IActionResult> AddFriend()
    {
        string nickname = "";
        string id = "";
        using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
        {
            var jsonString = await reader.ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<dynamic>(jsonString);

            if (data != null)
            {
                nickname = data.nicknameValue;
                id = data.friendId;
            }
        }

        try
        {
            var friendList = await GetFriends();
            if (friendList.TryGetValue(id, out var user))
            {
                return Json(new { success = false, message = "Bu Kişi İle Zaten Arkadaşsınız!" });
            }

            var response = await database.client.Child("friendList").Child(userId).PostAsync(new FriendList
            {
                UserId = id,
                Nickname = nickname
            });

            if (response != null)
            {
                return Json(new { success = true, message = "Arkadaş Başarılı Bir Şekilde Eklendi." });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Base Controller'da AddFriend Fonksiyonunda Hata" + ex.Message);
        }

        return Json(new { success = false, message = "Arkadaş Eklenemedi Daha Sonra Tekrar Deneyin!" });
    }

    [HttpGet]
    public async Task<IActionResult> ListFriend()
    {
        Dictionary<string, FriendList> friendList = [];
        HashSet<string> friends = [];
        try
        {
            var friendsList = await GetFriends();
            var usersList = await GetUsers();

            if (friendsList != null && friendsList.Count != 0)
            {
                foreach (var item in friendsList)
                {
                    if (usersList.TryGetValue(item.Value.UserId, out var user))
                    {
                        if (!friends.Contains(user.PhotoUrl))
                        {
                            var photoUrl = user.PhotoUrl;
                            friendList.Add(photoUrl, item.Value);
                            friends.Add(photoUrl);
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("Kullanıcının Arkadaş Listesi Yok.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Base Controller'da ListFriend Fonksiyonunda Hata" + ex.Message);
        }

        return PartialView("MessageBoxPartial", friendList);
    }

    [HttpPost]
    public async Task<IActionResult> OpenMessage()
    {
        string friendId = "";
        string photo = "";
        string nickname = "";
        using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
        {
            var jsonString = await reader.ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<dynamic>(jsonString);

            if (data != null)
            {
                friendId = data.friendId;
            }
        }

        List<MessageInfo> messages = [];
        userId = HttpContext.Session.GetString("UserId") ?? "";
        TempData["userId"] = userId;
        try
        {
            var messagesList = await GetMessages();
            var friendsList = await GetFriends();
            var usersList = await GetUsers();

            photo = usersList[friendId].PhotoUrl;
            nickname = friendsList[friendId].Nickname;

            foreach (var message in messagesList)
            {
                string tempPhotoUrl = "";
                string tempNickname = "";
                if (message.SenderId != userId)
                {
                    tempPhotoUrl = usersList[message.SenderId].PhotoUrl;
                    tempNickname = friendsList.TryGetValue(message.SenderId, out var value) ? value.Nickname
                    : $"{usersList[message.SenderId].PhoneNumber} ({usersList[message.SenderId].Name} {usersList[message.SenderId].SurName})";
                }
                else
                {
                    tempPhotoUrl = usersList[message.ReceiverId].PhotoUrl;
                    tempNickname = friendsList.TryGetValue(message.ReceiverId, out var value) ? value.Nickname
                    : $"{usersList[message.ReceiverId].PhoneNumber} ({usersList[message.ReceiverId].Name} {usersList[message.ReceiverId].SurName})";
                }

                messages.Add(new MessageInfo
                {
                    PhotoUrl = tempPhotoUrl,
                    SenderId = message.SenderId,
                    ReceiverId = message.ReceiverId,
                    Nickname = tempNickname,
                    MessageText = message.MessageText.Length > 30
                    ? string.Concat(message.MessageText.AsSpan(0, 30), "...")
                    : message.MessageText,
                    DateTime = message.DateTime.Substring(11, 5),
                    Status = message.Status
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Base Controller'da OpenMessage Fonksiyonunda Hata" + ex.Message);
        }

        messages = [.. messages.OrderByDescending(p => p.DateTime)];
        var specialItem = messages.FirstOrDefault(p => p.SenderId == friendId);
        if (specialItem == null)
        {
            specialItem = messages.FirstOrDefault(p => p.ReceiverId == friendId);
        }

        if (specialItem != null)
        {
            messages.Remove(specialItem);
            messages.Insert(0, specialItem);
        }
        else
        {
            messages.Insert(0, new MessageInfo
            {
                PhotoUrl = photo,
                SenderId = userId,
                ReceiverId = friendId,
                Nickname = nickname,
                MessageText = "",
                DateTime = DateTime.Now.ToString("HH:mm"),
                Status = false
            });
        }

        return PartialView("MessageBoxInfoPartial", messages);
    }

    [HttpPost]
    public async Task<IActionResult> ShowMessage()
    {
        string friendId = "";
        using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
        {
            var jsonString = await reader.ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<dynamic>(jsonString);

            if (data != null)
            {
                friendId = data.friendId;
            }
        }

        await SeenMessage(friendId);

        List<Message> messages = [];
        try
        {
            userId = HttpContext.Session.GetString("UserId") ?? "";
            TempData["userId"] = userId;
            var friendMessageResponse = await database.client.Child("messageHistory").Child(userId).Child(friendId).OnceAsync<Message>();

            if (friendMessageResponse != null && friendMessageResponse.Count != 0)
            {
                foreach (var message in friendMessageResponse)
                {
                    message.Object.DateTime = message.Object.DateTime.Substring(11, 5);
                    messages.Add(message.Object);
                }
            }
            else
            {
                Console.WriteLine("Bu Kullanıcı İle Mesajlaşması Bulunmamaktadır.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Base Controller'da ShowMessage Fonksiyonunda Hata" + ex.Message);
        }

        return PartialView("Message", messages);
    }

    public async Task SeenMessage(string friendId)
    {
        try
        {
            var userId = HttpContext.Session.GetString("UserId");
            var friendMessageResponse = await database.client.Child("messageHistory").Child(friendId).Child(userId).OnceAsync<Message>();

            if (friendMessageResponse != null && friendMessageResponse.Count != 0)
            {
                foreach (var message in friendMessageResponse)
                {
                    message.Object.Status = true;
                    await database.client.Child("messageHistory").Child(friendId).Child(userId).Child(message.Key).PutAsync(message.Object);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Base Controller'da SeenMessage Fonksiyonunda Hata" + ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddPhoto(IFormFile file)
    {
        try
        {
            if (file != null && file.Length > 0)
            {
                var userId = HttpContext.Session.GetString("UserId");
                var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                var stream = file.OpenReadStream();

                var url = await database.storage.Child("images").Child(userId).PutAsync(stream);
                var user = await database.client.Child("users").Child(userId).OnceSingleAsync<Users>();
                user.PhotoUrl = url;
                await database.client.Child("users").Child(userId).PutAsync(user);
            }
            else
            {
                return Json(new { success = false, message = "Eklenecek Resim Bulunamadı." });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Base Controller'da DeletePhoto Fonksiyonunda Hata" + ex.Message);
        }

        return Json(new { success = true, message = "Resim Başarılı Bir Şekilde Eklendi." });
    }

    [HttpGet]
    public async Task<IActionResult> DeletePhoto()
    {
        try
        {
            var userId = HttpContext.Session.GetString("UserId");
            try
            {
                await database.storage.Child("images").Child(userId).DeleteAsync();
                var user = await database.client.Child("users").Child(userId).OnceSingleAsync<Users>();
                user.PhotoUrl = "https://firebasestorage.googleapis.com/v0/b/messagingapp-863cc.appspot.com/o/defaultProfilePhoto.png?alt=media&token=9479ecb3-2a48-410d-9918-685aced34f3f";
                await database.client.Child("users").Child(userId).PutAsync(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Base Controller'da DeletePhoto Fonksiyonunda Hata" + ex.Message);
        }

        return Json(new { success = false, message = "Resim Başarılı Bir Şekilde Kaldırıldı." });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
