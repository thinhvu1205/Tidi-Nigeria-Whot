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
            if (dict.TryGetValue("sound", out var soundStr))
                Config.isOpenSound = bool.Parse(soundStr);

            if (dict.TryGetValue("music", out var musicStr))
                Config.isOpenMusic = bool.Parse(musicStr);

            if (dict.TryGetValue("vibration", out var vibrationStr))
                Config.isVibration = bool.Parse(vibrationStr);
            
            Config.SaveConfigSettings();
        }
    }
}