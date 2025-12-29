using Common.Objects;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace Globals
{
    public class Config
    {
        public const int CODE_JOKER_BLACK = 60;
        public const int CODE_JOKER_RED = 61;
        public static string userName = "";
        public static string userPass = "";
        public static string avatar_fb = "";
        public static string userNameTemp = "";
        public static string userPassTemp = "";
        public static string usernameNormal = "";
        public static string passwordNormal = "";
        public static LoginType loginType = LoginType.NORMAL;
        public static bool isLoginSuccessful = false;

        public static string currentServerIp = "";
        public static string currentGameId = "";
        public static string currentMatchId = "";
        public static string deviceId = SystemInfo.deviceUniqueIdentifier;
        public static string versionGame = Application.version;
        public static RuntimePlatform os = Application.platform;
        public static string publisher = "diamond_domino_slots_" + versionGame.Replace('.', '_');
        public static string package_name = Application.identifier;
        public static string versionDevice = GetVersionDevice();
        public static string versionNameOS = SystemInfo.operatingSystem;
        public static string model = SystemInfo.deviceName;
        public static string brand = SystemInfo.deviceModel;
        public static bool isVibration = false;
        public static string currentUrlRule = "";
        public static string privacyPolicyLink = "";
        public static string chatTeleSupportLink = "";
        public static string chatMessSupportLink = "";
        public static string ruleLink = "";
        public static string groupLink = "";
        public static string facebookLink = "";
        public static string feedBackLink = "";

        public static bool isOpenSound = true;
        public static bool isOpenMusic = true;

        private const string USER_NAME_KEY = "user_name";
        private const string USER_PASS_KEY = "user_pass";
        private const string USER_NAME_NORMAL_KEY = "username_normal";
        private const string USER_PASS_NORMAL_KEY = "userpass_normal";
        public const string AUTO_LOGIN = "isAutoLogin";
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
        
        public static void Vibration()
        {
            if (isVibration)
                Handheld.Vibrate();
        }
        
        public static async UniTask<Sprite> GetRemoteSprite(string url, bool isLoadBanner = false)
        {
            if (isLoadBanner)
            {
                string nameImg = System.IO.Path.GetFileNameWithoutExtension(url);
                if (ImageManager.Instance.ImageExists(nameImg))
                    return ImageManager.Instance.LoadSprite(nameImg);
                else
                    return await GetRemoteSprite(url, false);
            }
            else
            {
                using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
                {
                    var op = await www.SendWebRequest().ToUniTask(); // UniTask awaitable

                    if (www.result != UnityWebRequest.Result.Success) // Unity >= 2020.1
                    {
                        Debug.Log($"Error load Image: {www.error}, URL: {www.url}");
                        return null;
                    }

                    Texture2D texture = DownloadHandlerTexture.GetContent(www);
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(texture.width / 2, texture.height / 2));
                    return sprite;
                }
            }
        }
        
        [Tooltip("Cho text vào 1 thằng cha có RectMask")]
        public static void EffectTextRunInMask(TextMeshProUGUI txtName, bool isFixLeft = false) //dieu kien la thang text phai co cha la RectMask
        {
            RectTransform txtRt = txtName.transform.GetComponent<RectTransform>();
            float textSize = txtName.preferredWidth;
            Transform maskText = txtName.transform.parent;
            RectTransform maskRt = maskText.GetComponent<RectTransform>();
            DOTween.Kill(txtName);
            if (textSize > maskRt.sizeDelta.x)
            {
                float deltaX = textSize - maskRt.sizeDelta.x;
                float minPos = -deltaX / 2;
                float maxPos = deltaX / 2;
                if (txtName.alignment == TextAlignmentOptions.MidlineLeft)
                {
                    minPos = -textSize / 2;
                    maxPos = 0;
                }
                Sequence seqMaskName = DOTween.Sequence();
                seqMaskName.Append(txtRt.DOLocalMoveX(maxPos, 0f))
                    .AppendInterval(1.0f)
                    .Append(txtRt.DOLocalMoveX(minPos, 1.0f))
                    .AppendInterval(1.0f)
                    .Append(txtRt.DOLocalMoveX(maxPos, 1.0f))
                    .AppendInterval(1.0f).SetLoops(-1);
                seqMaskName.SetTarget(txtName);

            }
            else
            {

                DOTween.Kill(txtName);
                txtRt.localPosition = Vector3.zero;
                if (isFixLeft)
                {
                    float deltaX = textSize - maskRt.sizeDelta.x;
                    txtRt.localPosition = new Vector2(deltaX / 2, 0);
                }
            }
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

public class LinkGlobalValue
{
    public string rule_link;
    public long update_at;
    public string update_by;
    public string group_link;
    public string facebook_link;
    public string feedback_link;
}