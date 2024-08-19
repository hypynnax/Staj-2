using Firebase.Database;
using Firebase.Storage;

namespace MessagingApp.Data
{
    public class Database
    {
        public FirebaseClient client;
        public FirebaseStorage storage;

        public Database()
        {
            client = new FirebaseClient("https://messagingapp-863cc-default-rtdb.europe-west1.firebasedatabase.app/");
            storage = new FirebaseStorage("messagingapp-863cc.appspot.com", new FirebaseStorageOptions
            {
                AuthTokenAsyncFactory = () => Task.FromResult("AIzaSyAoXXdcGJxOI08V1QP3Whie_Wj0RStgv9M"),
            });
        }
    }
}
