using System;
using Proto;

namespace Globals
{
    public class User
    {
        public static Action OnProfileUpdated;
        public User() { }
        public static Profile userProfile;
        public static string AccessToken = "";
        public static string FacebookID;

        public static void UpdateProfile()
        {
            OnProfileUpdated?.Invoke();
        }
    }
}