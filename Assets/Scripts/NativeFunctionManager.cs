using System;
using UnityEngine;

public static class NativeFunctionManager
{
    public static void OnLoginFb(string reLogin = "0", Action<string> callback = null)
    {
        Debug.Log("onLoginFb: Login FB native");
        // CallNativeFunction(Constants.NativeCommand.LOGIN_FACEBOOK, reLogin, callback);
    }

    public static void OnGetDeviceId(Action<string> callback = null)
    {
        Debug.Log("onGetDeviceId: get device id");
        // CallNativeFunction(Constants.NativeCommand.GET_DEVICE_ID, "", callback);
    }

    public static void OnVibrator()
    {
        Debug.Log("on vibrator");
        // CallNativeFunction(Constants.NativeCommand.ON_VIBRATOR, "");
    }

    public static void OpenFanpage()
    {
        // string pageID = Global.Profile?.LinkFanpageFb?.Replace("https://www.facebook.com/", "");
        // string pageUrl = Global.Profile?.LinkFanpageFb;
        //
        // var data = new { pageID, pageUrl };
        // CallNativeFunction(Constants.NativeCommand.OPEN_FANPAGE, JsonUtility.ToJson(data));
    }

    public static void OpenGroup()
    {
        // string groupID = Global.Profile?.LinkGroup?.Replace("https://www.facebook.com/groups/", "");
        // string groupUrl = Global.Profile?.LinkGroup;
        //
        // var data = new { groupID, groupUrl };
        // CallNativeFunction(Constants.NativeCommand.OPEN_GROUP, JsonUtility.ToJson(data));
    }

    public static void OpenFeedback()
    {
    //     string pageID = Global.Profile?.LinkFanpageFb?.Replace("https://www.facebook.com/", "");
    //     var data = new { pageID };
    //     CallNativeFunction(Constants.NativeCommand.CHAT_ADMIN, JsonUtility.ToJson(data));
    }

    public static void BuyItem(string param, Action<string> callback = null)
    {
        // CallNativeFunction(Constants.NativeCommand.BUY_IAP, param, callback);
    }

    public static void OpenUrl(string url)
    {
        // CallNativeFunction(Constants.NativeCommand.OPEN_URL, url);
    }

    // === Core Native Function Call ===
    private static void CallNativeFunction(string method, string param, Action<string> callback = null)
    {
#if UNITY_ANDROID
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
        {
            currentActivity.Call("callNativeFunction", method, param);
        }
#elif UNITY_IOS
        _CallNativeFunctionIOS(method, param);
#endif

        // If native calls back to Unity, handle via UnitySendMessage or delegate event system
        if (callback != null)
        {
            // You need to define how native returns result back to Unity (e.g., UnitySendMessage)
            // NativeCallbackHandler.OnCallbackReceived += callback;
        }
    }

#if UNITY_IOS
    [DllImport("__Internal")]
    private static extern void _CallNativeFunctionIOS(string method, string param);
#endif
}
