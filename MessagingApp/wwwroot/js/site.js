const sidebarContainer = document.getElementById('sidebarContainer');
const sidebarMessage = document.getElementById("messages-box-wrapper");
const MessageContainer = document.getElementById("message-container");
const sidebars = [
    {
        name: "AddFrined",
        icon: document.getElementById('addFriendIcon'),
        container: document.getElementById('addFriendContainer'),
        button: document.getElementById('addFriendButton')
    },
    {
        name: "SendMessage",
        icon: document.getElementById('addMessageIcon'),
        container: document.getElementById('addMessageContainer'),
        button: document.getElementById('addMessageButton')
    },
    {
        name: "AddPhoto",
        icon: document.getElementById('addPhotoIcon'),
        container: document.getElementById('addPhotoContainer'),
        button: document.getElementById('addPhotoButton')
    }
];

let currentOpenSidebar = null;

sidebars.forEach(sidebar => {
    sidebar.icon.addEventListener('click', function () {
        if (currentOpenSidebar && currentOpenSidebar !== sidebar) {
            currentOpenSidebar.container.style.display = "none";
            currentOpenSidebar.icon.classList.remove("change");
        }

        if (currentOpenSidebar === sidebar) {
            sidebar.container.style.display = "none";
            if (sidebar.name === "AddFrined") {
                sidebar.icon.classList.remove("change");
            }
            currentOpenSidebar = null;
            sidebarContainer.style.width = "70px";
            sidebarMessage.style.width = "30%";
            MessageContainer.style.width = "70%";
        } else {
            sidebar.container.style.display = "flex";
            if (sidebar.name === "AddFrined") {
                sidebar.icon.classList.add("change");
            }

            currentOpenSidebar = sidebar;
            sidebarContainer.style.width = "475px";
            sidebarMessage.style.width = "40%";
            MessageContainer.style.width = "60%";
        }

        if (sidebar.name == "SendMessage") {
            $.ajax({
                url: 'Base/ListFriend',
                type: 'GET',
                contentType: 'application/json',
                success: function (response) {
                    $('#box').html(response);
                },
            });
        }
    });

    sidebar.button.addEventListener("click", function () {
        if (sidebar.name == "AddFrined") {
            var nickname = document.getElementById("nickname").value;
            $.ajax({
                url: 'Base/AddFriend',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({ nicknameValue: nickname, friendId: findId }),
                success: function (result) {
                    if (result.success) {
                        var messageBox = document.getElementById("addFriendMessageBox");
                        var message = document.getElementById("addFriendMessage");
                        messageBox.style.display = "flex";
                        messageBox.classList.add("success");
                        message.innerText = result.message;
                        setTimeout(() => {
                            messageBox.style.display = "none";
                            messageBox.classList.remove("success");
                            message.innerText = "";
                            setTimeout(() => {
                                sidebar.container.style.display = "none";
                                sidebar.icon.classList.remove("change");
                                currentOpenSidebar = null;
                                sidebarContainer.style.width = "70px";
                                sidebarMessage.style.width = "30%";
                                MessageContainer.style.width = "70%";
                                document.getElementById("addPersonPhoto").src = "https://firebasestorage.googleapis.com/v0/b/messagingapp-863cc.appspot.com/o/defaultProfilePhoto.png?alt=media&token=9479ecb3-2a48-410d-9918-685aced34f3f";
                                document.getElementById("searchFriendName").innerText = "";
                                document.getElementById("nickname").value = "";
                                document.getElementById("phone_number").value = "";
                            }, 1000);
                        }, 3000);
                    } else {
                        var messageBox = document.getElementById("addFriendMessageBox");
                        var message = document.getElementById("addFriendMessage");
                        messageBox.style.display = "flex";
                        messageBox.classList.add("error");
                        message.innerText = result.message;
                        setTimeout(() => {
                            messageBox.style.display = "none";
                            messageBox.classList.remove("error");
                            message.innerText = "";
                        }, 3000);
                    }
                },
                error: function (result) {
                    var messageBox = document.getElementById("addFriendMessageBox");
                    var message = document.getElementById("addFriendMessage");
                    messageBox.style.display = "flex";
                    messageBox.classList.add("error");
                    message.innerText = result.message;
                    setTimeout(() => {
                        messageBox.style.display = "none";
                        messageBox.classList.remove("error");
                        message.innerText = "";
                    }, 3000);
                }
            });
            nickname.value = "";
        }

        if (sidebar.name == "SendMessage") {
            sidebar.button.id = "addMessageButton";
            sidebar.button.disabled = true;
            sidebar.button.style.backgroundColor = "#00800088";
            sidebar.container.style.display = "none";
            sidebar.icon.classList.remove("change");
            currentOpenSidebar = null;
            sidebarContainer.style.width = "70px";
            sidebarMessage.style.width = "30%";
            MessageContainer.style.width = "70%";
        }

        if (sidebar.name == "AddPhoto") {
            var fileInput = document.getElementById('file');
            var file = fileInput.files[0];
            if (file) {
                var formData = new FormData();
                formData.append('file', file);
                $.ajax({
                    url: 'Base/AddPhoto',
                    type: 'POST',
                    processData: false,
                    contentType: false,
                    data: formData,
                    success: function (result) {
                        if (result.success) {
                            var messageBox = document.getElementById("addPhotoMessageBox");
                            var message = document.getElementById("addPhotoMessage");
                            messageBox.style.display = "flex";
                            messageBox.classList.add("success");
                            message.innerText = result.message;
                            setTimeout(() => {
                                messageBox.style.display = "none";
                                messageBox.classList.remove("success");
                                message.innerText = "";
                                setTimeout(() => {
                                    sidebar.container.style.display = "none";
                                    sidebar.icon.classList.remove("change");
                                    currentOpenSidebar = null;
                                    sidebarContainer.style.width = "70px";
                                    sidebarMessage.style.width = "30%";
                                    MessageContainer.style.width = "70%";
                                }, 1000);
                            }, 3000);
                        } else {
                            var messageBox = document.getElementById("addPhotoMessageBox");
                            var message = document.getElementById("addPhotoMessage");
                            messageBox.style.display = "flex";
                            messageBox.classList.add("error");
                            message.innerText = result.message;
                            setTimeout(() => {
                                messageBox.style.display = "none";
                                messageBox.classList.remove("error");
                                message.innerText = "";
                                setTimeout(() => {
                                    sidebar.container.style.display = "none";
                                    sidebar.icon.classList.remove("change");
                                    currentOpenSidebar = null;
                                    sidebarContainer.style.width = "70px";
                                    sidebarMessage.style.width = "30%";
                                    MessageContainer.style.width = "70%";
                                }, 1000);
                            }, 3000);
                        }
                    },
                    error: function (result) {
                        var messageBox = document.getElementById("addPhotoMessageBox");
                        var message = document.getElementById("addPhotoMessage");
                        messageBox.style.display = "flex";
                        messageBox.classList.add("error");
                        message.innerText = result.message;
                        setTimeout(() => {
                            messageBox.style.display = "none";
                            messageBox.classList.remove("error");
                            message.innerText = "";
                        }, 3000);
                    }
                });
            } else {
                var messageBox = document.getElementById("addPhotoMessageBox");
                var message = document.getElementById("addPhotoMessage");
                messageBox.style.display = "flex";
                messageBox.classList.add("error");
                message.innerText = "Lütfen bir dosya seçin.";
                setTimeout(() => {
                    messageBox.style.display = "none";
                    messageBox.classList.remove("error");
                    message.innerText = "";
                }, 3000);
            }
        }
    });
});

var nickname = document.getElementById("nickname");
var addFriendButtonDisabled = document.getElementById("addFriendButton");
nickname.addEventListener("input", () => {
    if (nickname.value != null || nickname.value != "") {
        addFriendButtonDisabled.disabled = false;
    } else {
        addFriendButtonDisabled.disabled = true;
    }
});

let findId = "";
var phone_input = document.getElementById('phone_number');
phone_input.addEventListener("input", function () {
    let value = phone_input.value.replace(/\D/g, '');

    let formattedValue = '';
    for (let i = 0; i < value.length; i++) {
        if ([3, 6, 8].includes(i)) {
            formattedValue += ' ' + value[i];
        } else {
            formattedValue += value[i];
        }
    }
    phone_input.value = formattedValue;

    if (value.length == 10) {
        $.ajax({
            url: 'Base/SearchFriend',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ phoneData: "0" + formattedValue }),
            success: function (result) {
                if (result.success) {
                    document.getElementById("searchFriendName").innerText = result.name;
                    document.getElementById("addPersonPhoto").src = result.photoUrl;
                    nickname.disabled = false;
                    findId = result.id;
                } else {
                    nickname.disabled = true;
                    var messageBox = document.getElementById("addFriendMessageBox");
                    var message = document.getElementById("addFriendMessage");
                    messageBox.style.display = "flex";
                    messageBox.classList.add("error");
                    message.innerText = result.message;
                    document.getElementById("searchFriendName").innerText = "";
                    document.getElementById("addPersonPhoto").src = "https://firebasestorage.googleapis.com/v0/b/messagingapp-863cc.appspot.com/o/defaultProfilePhoto.png?alt=media&token=9479ecb3-2a48-410d-9918-685aced34f3f";
                    setTimeout(() => {
                        messageBox.style.display = "none";
                        messageBox.classList.remove("error");
                        message.innerText = "";
                    }, 3000);
                }
            },
            error: function (xhr, status, error) {
                nickname.disabled = true;
                var messageBox = document.getElementById("addFriendMessageBox");
                var message = document.getElementById("addFriendMessage");
                messageBox.style.display = "flex";
                messageBox.classList.add("error");
                message.innerText = "BEKLENMEDİK BİR HATA!";
                setTimeout(() => {
                    messageBox.style.display = "none";
                    messageBox.classList.remove("error");
                    message.innerText = "";
                }, 3000);
            }
        });
    }
});

document.getElementById("logout").addEventListener("click", function () {
    window.location.href = "/Login/Logout";
});

document.getElementById('uploadPhotoButton').addEventListener('click', function () {
    document.getElementById('file').click();
});

document.getElementById('file').addEventListener('change', function (event) {
    const file = event.target.files[0];
    if (file) {
        const reader = new FileReader();

        reader.onload = function (e) {
            const img = document.getElementById('userPhoto');
            img.src = e.target.result;
            photo_file = e.target.result;
        }

        reader.readAsDataURL(file);
    }
});

document.getElementById('removePhotoButton').addEventListener('click', function () {
    document.getElementById('userPhoto').src = "https://firebasestorage.googleapis.com/v0/b/messagingapp-863cc.appspot.com/o/defaultProfilePhoto.png?alt=media&token=9479ecb3-2a48-410d-9918-685aced34f3f";
    $.ajax({
        url: 'Base/DeletePhoto',
        type: 'GET',
        contentType: 'application/json',
        success: function (result) {
            if (result.success) {
                var messageBox = document.getElementById("addPhotoMessageBox");
                var message = document.getElementById("addPhotoMessage");
                messageBox.style.display = "flex";
                messageBox.classList.add("success");
                message.innerText = result.message;
                setTimeout(() => {
                    messageBox.style.display = "none";
                    messageBox.classList.remove("success");
                    message.innerText = "";
                }, 3000);
            } else {
                var messageBox = document.getElementById("addPhotoMessageBox");
                var message = document.getElementById("addPhotoMessage");
                messageBox.style.display = "flex";
                messageBox.classList.add("error");
                message.innerText = result.message;
                setTimeout(() => {
                    messageBox.style.display = "none";
                    messageBox.classList.remove("error");
                    message.innerText = "";
                }, 3000);
            }
        },
        error: function (result) {
            var messageBox = document.getElementById("addPhotoMessageBox");
            var message = document.getElementById("addPhotoMessage");
            messageBox.style.display = "flex";
            messageBox.classList.add("error");
            message.innerText = result.message;
            setTimeout(() => {
                messageBox.style.display = "none";
                messageBox.classList.remove("error");
                message.innerText = "";
            }, 3000);
        }
    });
});

document.getElementById("nickname").addEventListener('keydown', function (event) {
    if (event.key === "Enter") {
        document.getElementById("addFriendButton").click();
    }
});