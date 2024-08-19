using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MessagingApp.Models;
using Microsoft.AspNetCore.Authorization;
using MessagingApp.Data;
using System.Security.Claims;
using Firebase.Database.Query;
using System.Text;
using Newtonsoft.Json;

namespace MessagingApp.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly Database database = new();

    string userId = "";
    string photoUrl = "";

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public async Task<string> GetPhotoUrl()
    {
        try
        {
            userId = HttpContext.Session.GetString("UserId") ?? "";
            var user = await database.client.Child("users").Child(userId).OnceSingleAsync<Users>();
            photoUrl = user.PhotoUrl;
        }
        catch (Exception ex)
        {
            Console.WriteLine("GetPhotoUrl methodu hatası " + ex);
        }

        return photoUrl;
    }

    public async Task<IActionResult> Index()
    {
        userId = HttpContext.Session.GetString("UserId") ?? "";
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Logout", "Login");
        }

        TempData["Name"] = User.FindFirst(ClaimTypes.Name)?.Value + " " + User.FindFirst(ClaimTypes.Surname)?.Value;

        TempData["UserProfilePhoto"] = await GetPhotoUrl();

        return View();
    }

    public async Task<Dictionary<string, FriendList>> GetFriends()
    {
        Dictionary<string, FriendList> friendList = [];
        userId = HttpContext.Session.GetString("UserId") ?? "";
        var friendsResponse = await database.client.Child("friendList").Child(userId).OnceAsync<FriendList>();
        if (friendsResponse != null && friendsResponse.Count != 0)
        {
            foreach (var friend in friendsResponse)
            {
                if (!friendList.ContainsKey(friend.Key))
                {
                    friendList.Add(friend.Object.UserId, friend.Object);
                }
            }
        }

        return friendList;
    }

    public async Task<List<Message>> GetMessages()
    {
        List<Message> messages = [];
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
                        messages.Add(item.Value);
                        break;
                    }
                }
            }
        }

        return messages;
    }

    public async Task<Dictionary<string, Users>> GetUsers()
    {
        Dictionary<string, Users> usersList = [];
        var usersResponse = await database.client.Child("users").OnceAsync<Users>();
        if (usersResponse != null && usersResponse.Count != 0)
        {
            foreach (var user in usersResponse)
            {
                if (!usersList.ContainsKey(user.Key))
                {
                    usersList.Add(user.Key, user.Object);
                }
            }
        }
        return usersList;
    }

    [HttpGet]
    public async Task<IActionResult> LoadMessages()
    {
        userId = HttpContext.Session.GetString("UserId") ?? "";
        TempData["userId"] = userId;
        List<MessageInfo> messages = [];
        try
        {
            var messagesList = await GetMessages();
            var friendsList = await GetFriends();
            var usersList = await GetUsers();

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
            Console.WriteLine("Home Controller'da LoadMessages Fonksiyonunda Hata" + ex.Message);
        }

        messages = [.. messages.OrderByDescending(p => p.DateTime)];

        return PartialView("MessageBoxInfoPartial", messages);
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage()
    {
        List<Message> list = [];
        string message = "";
        string receId = "";
        using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
        {
            var jsonString = await reader.ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<dynamic>(jsonString);

            if (data != null)
            {
                message = data.messageText;
                receId = data.receiverId;
            }
        }

        try
        {
            userId = HttpContext.Session.GetString("UserId") ?? "";
            TempData["userId"] = userId;
            Message messageObject = new()
            {
                SenderId = userId,
                ReceiverId = receId,
                MessageText = message,
                DateTime = DateTime.Now.ToString("dd/MM/yyyy-HH:mm:ss"),
                Status = false
            };
            await database.client.Child("messageHistory").Child(userId).Child(receId).PostAsync(messageObject);
            messageObject.Status = true;
            await database.client.Child("messageHistory").Child(receId).Child(userId).PostAsync(messageObject);

            messageObject.DateTime = messageObject.DateTime.Substring(11, 5);
            list.Add(messageObject);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Home Controller'da SendMessage Fonksiyonunda Hata" + ex.Message);
        }

        return PartialView("Message", list);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
