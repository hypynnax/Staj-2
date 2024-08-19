document.addEventListener("DOMContentLoaded", function () {
    $.ajax({
        url: 'Home/LoadMessages',
        type: 'GET',
        success: function (result) {
            $('#messages-box-container').html(result);
        },
    });
});

var sendMessageInput = document.getElementById("sendMessageInput");
var sendButton = document.getElementsByName("send")[0];

sendButton.addEventListener('click', function () {
    $.ajax({
        url: 'Home/SendMessage',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ messageText: sendMessageInput.value, receiverId: sendButton.id }),
        success: function (response) {
            sendMessageInput.value = "";
            $('#message-box').append(response);
        },
    });
});

sendMessageInput.addEventListener('keydown', function (event) {
    if (event.key === "Enter") {
        sendButton.click();
    }
});
