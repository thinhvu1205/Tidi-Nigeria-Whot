using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Yuujins.User.V1;

namespace Globals
{
    public class User
    {
        public static Action OnProfileUpdated;
        public User() { }
        public static UserAccount UserAccount;
        public static string AccessToken = "";
        public static string FacebookID;

        public static void UpdateProfile()
        {
            OnProfileUpdated?.Invoke();
        }

        public static void UpdateConfig()
        {
            string appConfig = UserAccount.Profile.AppConfig;
            if (string.IsNullOrEmpty(appConfig)) return;
            string fixedJson = "{" + appConfig + "}";

            // Parse thành dictionary
            Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(fixedJson);

            // Lấy giá trị
            if (dict.TryGetValue("sound", out string sound))
            {
                Config.isOpenSound = bool.Parse(sound);
            }
            if (dict.TryGetValue("music", out string music))
            {
                Config.isOpenMusic = bool.Parse(music);
            }
            if (dict.TryGetValue("vibration", out string vibration))
            {
                Config.isVibration = bool.Parse(vibration);
            }
            Config.SaveConfigSettings();
        }
    }
}