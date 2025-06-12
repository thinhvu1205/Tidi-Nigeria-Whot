using UnityEngine;

namespace Globals
{
    public class Config
    {
        public static string userName = "";
        public static string userPass = "";
        public static string userNameTemp = "";
        public static string userPassTemp = "";
        public static string usernameNormal = "";
        public static string passwordNormal = "";
        public static LoginType loginType = LoginType.NORMAL;
        public static bool isLoginSuccessful = false;

        public static string currentServerIp = "";
        public static string currentGameId = "";
        public static string deviceId = SystemInfo.deviceUniqueIdentifier;
        public static string versionGame = Application.version;
        public static string publisher = "diamond_domino_slots_" + versionGame.Replace('.', '_');
        public static string package_name = Application.identifier;
        public static string versionDevice = GetVersionDevice();
        public static string versionNameOS = SystemInfo.operatingSystem;
        public static string model = SystemInfo.deviceName;
        public static string brand = SystemInfo.deviceModel;

        public static bool isOpenSound = true;
        public static bool isOpenMusic = true;

        private const string USER_NAME_KEY = "user_name";
        private const string USER_PASS_KEY = "user_pass";
        private const string USER_NAME_NORMAL_KEY = "username_normal";
        private const string USER_PASS_NORMAL_KEY = "userpass_normal";
        public const string TYPE_LOGIN_KEY = "type_login";
        private const string SOUND_KEY = "sound";
        private const string MUSIC_KEY = "music";

        public const string LOGIN_SCENE = "Login";
        public const string MAIN_SCENE = "MainScene";


        public static string GetVersionDevice()
        {
#if UNITY_ANDROID
            var clazz = AndroidJNI.FindClass("android/os/Build$VERSION");
            var fieldID = AndroidJNI.GetStaticFieldID(clazz, "SDK_INT", "I");
            var sdkLevel = AndroidJNI.GetStaticIntField(clazz, fieldID);
            return sdkLevel.ToString();
#elif UNITY_IOS
                return UnityEngine.iOS.Device.systemVersion;
#else

            return SystemInfo.operatingSystem;
#endif
        }

        public static void SaveLoginAccount()
        {
            PlayerPrefs.SetString(USER_NAME_KEY, userName);
            PlayerPrefs.SetString(USER_PASS_KEY, userPass);
            usernameNormal = userName;
            passwordNormal = userPass;
        }

        public static void GetUserData()
        {
            userName = PlayerPrefs.GetString(USER_NAME_KEY, "");
            userPass = PlayerPrefs.GetString(USER_PASS_KEY, "");
            loginType = (LoginType)PlayerPrefs.GetInt(TYPE_LOGIN_KEY, (int)LoginType.NONE);

            usernameNormal = PlayerPrefs.GetString(USER_NAME_NORMAL_KEY, "");
            passwordNormal = PlayerPrefs.GetString(USER_PASS_NORMAL_KEY, "");
        }

        public static void SaveUserData()
        {
            PlayerPrefs.SetString(USER_NAME_KEY, userName);
            PlayerPrefs.SetString(USER_PASS_KEY, userPass);
            // if (loginType == LoginType.NORMAL)
            // {
            //     PlayerPrefs.SetString(USER_NAME_NORMAL_KEY, usernameNormal);
            //     PlayerPrefs.SetString(USER_PASS_NORMAL_KEY, passwordNormal);
            // }
            PlayerPrefs.Save();
        }
        public static void UpdateConfigSettings()
        {
            isOpenSound = PlayerPrefs.GetInt(SOUND_KEY, 1) == 1;
            isOpenMusic = PlayerPrefs.GetInt(MUSIC_KEY, 1) == 1;
        }
        public static void SaveConfigSettings()
        {
            PlayerPrefs.SetInt(SOUND_KEY, isOpenSound ? 1 : 0);
            PlayerPrefs.SetInt(MUSIC_KEY, isOpenMusic ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}