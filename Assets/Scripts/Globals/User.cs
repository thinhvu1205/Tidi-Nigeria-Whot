using System;
using System.Collections.Generic;
using Newtonsoft.Json;
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

        public static void UpdateConfig()
        {
            string appConfig = userProfile.AppConfig;
            string fixedJson = "{" + appConfig + "}";

            // Parse thành dictionary
            Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(fixedJson);

            // Lấy giá trị
            bool isOpenSound = bool.Parse(dict["sound"]);
            bool isOpenMusic = bool.Parse(dict["music"]);
            bool isVibration = bool.Parse(dict["vibration"]);
            Config.isOpenMusic = isOpenMusic;
            Config.isOpenSound = isOpenSound;
            Config.isVibration = isVibration;
            Config.SaveConfigSettings();
        }
    }
}