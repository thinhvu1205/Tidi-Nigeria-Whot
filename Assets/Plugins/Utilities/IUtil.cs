using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEditor;
using UnityEngine.Events;
using System.Globalization;
using System.IO;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
// using TMPro.EditorUtilities;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using System;

public class DelayChainProps
{
    public float DelayTime;
    public ICallFunc.Func1 OnBeforeCb, OnAfterCb;
}
public class FormatNumberProps
{
    public string space = "";
    public bool use1000Separator = true, isFormatK = true;
    public int countDecimals = 1;
}
public class IUtil
{
    public const long TRILLION = 1000000000000;
    public const int BILLION = 1000000000, MILLION = 1000000, THOUSAND = 1000;

#if UNITY_EDITOR
    [MenuItem("Tools/Capture Screenshot")]
    public static void CaptureScreenshot()
    {   // inside Assets folder
        ScreenCapture.CaptureScreenshot(Application.dataPath + "/screenshot" + System.DateTime.Now.Ticks + ".png");
        Debug.Log("|   ) )=3 Capture screenshot successful!");
    }
    public static void SaveBytesToFile(string fileName, byte[] data)
    {
        string path = Path.Combine(Application.streamingAssetsPath, fileName);
        if (File.Exists(path)) File.Delete(path);
        File.WriteAllBytes(path, data);
        Debug.Log("|   ) )=3 Bytes saved to file: " + path);
    }
#endif
    #region Texture and Sprite
    public static Sprite CreateSpriteFromTexture2D(Texture2D dataT2D, float pivotX = 0.5f, float pivotY = 0.5f)
    {
        Rect rect = new(0, 0, dataT2D.width, dataT2D.height);
        Vector2 pivot = new(pivotX, pivotY);
        return Sprite.Create(dataT2D, rect, pivot);
    }
    #endregion
    #region String
    public static string StringSplit(string content, int maxLength, string more = "...")
    {   // hello world -> (maxLength = 5, more = "...vv) -> hello...vv
        string result = "";
        char[] arrText = content.Trim().ToArray();
        int maxIndex = Mathf.Min(arrText.Length, maxLength) - 1;
        for (int i = 0; i <= maxIndex; i++) result += arrText[i];
        if (arrText.Length > maxLength) result += more;
        return result;
    }
    public static string StringBig1stChar(string input)
    {   // hello -> Hello
        if (string.IsNullOrEmpty(input)) return input;
        return input.First().ToString().ToUpper() + input.Substring(1);
    }
    public static string StringBigEveryWord1stChar(string input)
    {   // hello buddy ->Hello Buddy
        if (string.IsNullOrEmpty(input)) return input;
        if (input.EndsWith(' ')) input = input.Trim(input[input.Length - 1]);
        string[] split = input.Split(' ');
        for (int i = 0; i < split.Length; i++) split[i] = split[i].First().ToString().ToUpper() + split[i].Substring(1);
        return string.Join(" ", split);
    }
    public static string StringColor(string source, string colorHTMLformat)
    { return string.IsNullOrEmpty(colorHTMLformat) ? source : string.Format("<color={0}>{1}</color>", colorHTMLformat, source); }
    public static string StringSize(string source, int size)
    { return size <= 0 ? source : string.Format("<size={0}>{1}</size>", size, source); }
    public static string StringBold(string source) { return string.Format("<b>{0}</b>", source); }
    public static string StringItalics(string source) { return string.Format("<i>{0}</i>", source); }
    public static string StringStrikethrough(string source) { return string.Format("<s>{0}</s>", source); }
    public static string StringUnderline(string source) { return string.Format("<u>{0}</u>", source); }
    // the sprite asset file must be in Assets/TextMesh Pro/Resources/Sprite Assets
    public static string StringSprite(string spriteAsset = "", int index = 0, string attribute = "")
    {   // example: <sprite="EmojiOne" index=0 color=#0EFF00>
        if (!attribute.StartsWith(" ")) attribute = " " + attribute;
        return "<sprite" + (spriteAsset.Equals("") ? "" : "=\"" + spriteAsset + "\"") + " index=" + index + attribute + ">";
    }
    public static string StringSprite(string spriteAsset = "", string name = "", string attribute = "")
    {   // example: <sprite="EmojiOne" name="test_0" color=#0EFF00>
        if (!attribute.StartsWith(" ")) attribute = " " + attribute;
        return "<sprite" + (spriteAsset.Equals("") ? "" : "=\"" + spriteAsset + "\"") + " name=\"" + name + "\"" + attribute + ">";
    }
    public static string StringVerticalOffset(string source, float offset, string unit = "em")
    { return string.Format("<voffset={0}{1}>{2}</voffset>", offset, unit, source); }
    #endregion
    #region Currency
    public static string GetCurrencySymbol(string currencyCode)
    {   // currencyCode examples: USD
        return CultureInfo.GetCultures(CultureTypes.AllCultures)
                          .Where(c => !c.IsNeutralCulture)
                          .Select(culture =>
                          {
                              try { return new RegionInfo(culture.Name); }
                              catch { return null; }
                          })
                          .Where(ri => ri != null && ri.ISOCurrencySymbol.ToUpper().Equals(currencyCode.ToUpper()))
                          .Select(ri => ri.CurrencySymbol)
                          .FirstOrDefault();
    }
    public static CultureInfo GetCultureInfo(string currencyCode)
    {   // currencyCode examples: USD
        CultureInfo[] dataCIs = CultureInfo.GetCultures(CultureTypes.AllCultures).Where(aCI => !aCI.IsNeutralCulture).ToArray();
        if (dataCIs != null)
        {
            for (int i = 0; i < dataCIs.Length; i++)
            {
                try
                {
                    RegionInfo aRI = new(dataCIs[i].Name);
                    if (aRI != null && aRI.ISOCurrencySymbol.ToUpper().Equals(currencyCode.ToUpper())) return dataCIs[i];
                }
                catch { }
            }
        }
        return null;
    }
    public static string FormatCurrencyWithSymbol(double amount, string currencyCode)
    {   // currencyCode examples: USD. Ex: amount = 12345, currencyCode = "usd" -> $123,45
        string symbol = GetCurrencySymbol(currencyCode);
        if (!string.IsNullOrEmpty(symbol)) return symbol + FormatNumberUse1000Separator(amount);
        return FormatNumberUse1000Separator(amount);
    }
    public static string FormatCurrencyWithCode(double amount, string currencyCode)
    {   // currencyCode examples: USD. Ex: amount = 12345, currencyCode = "usd" -> 123,45 USD
        return FormatNumberUse1000Separator(amount) + currencyCode.ToUpper();
    }
    public static void SplitNumberDecimal(double number, out double firstPart, out string secondPart, int countDecimals = 1)
    {   // ex: number = 123.456 -> firstPart = 123 -> secondPart = .45 (countDecimals = 2) | secondPart = .456 (countDecimals = -1)
        firstPart = Math.Floor(number);
        secondPart = "";
        if (countDecimals == 0) return;
        if (number - firstPart > 0)
        {   // Some countries use the ',' instead of '.' 
            string[] parts = number.ToString().Replace(',', '.').Split('.');
            if (parts.Length > 1)
            {
                if (countDecimals > 0)
                {
                    string sub = parts[1].Substring(0, countDecimals);
                    if (double.TryParse(sub, out double x)) if (x > 0) secondPart = "." + sub;
                }
                else if (countDecimals < 0) secondPart = "." + parts[1];
            }
        }
    }

    public static string FormatNumberUse1000Separator(double number, int countDecimals = -1)
    {   // ex: 123456789 -> 123,456,789
        SplitNumberDecimal(number, out double firstPart, out string secondPart, countDecimals);
        return firstPart.ToString("N0") + secondPart;
    }
    public static string FormatNumberAndRoundUp(double number, double minRoundUp = BILLION, int countDecimals = -1, bool isK = true, bool is1000Separator = true)
    {   // ex: 1234567899 -> minRoundUp = BILLION -> 1.234567899 B | minRoundUp = TRILLION -> 1,234,567,899 
        string suffix = "";
        double absNumber = Math.Abs(number), factor = 1;
        if (absNumber >= minRoundUp)
        {
            if (absNumber >= TRILLION)
            {
                factor = TRILLION;
                suffix = "T";
            }
            else if (absNumber >= BILLION)
            {
                factor = BILLION;
                suffix = "B";
            }
            else if (absNumber >= MILLION)
            {
                factor = MILLION;
                suffix = "M";
            }
            else if (absNumber >= THOUSAND && isK)
            {
                factor = THOUSAND;
                suffix = "K";
            }
        }
        SplitNumberDecimal(number / factor, out double firstPart, out string secondPart, countDecimals);
        return (is1000Separator ? firstPart.ToString("N0") : "" + firstPart) + secondPart + " " + suffix;
    }
    #endregion  
    // ======= Other =======
    public static Color ParseHEXColor(string colorHexCode)
    {
        ColorUtility.TryParseHtmlString(!colorHexCode.StartsWith("#") ? "#" + colorHexCode : colorHexCode, out Color c);
        return c;
    }
    public static void SetTMPVertexGradient(TextMeshProUGUI aTMP, ColorMode modeCM, string hex1, string hex2, string hex3, string hex4)
    {
        aTMP.color = Color.white;
        aTMP.enableVertexGradient = true;
        aTMP.colorGradientPreset = new() { colorMode = modeCM };
        aTMP.colorGradientPreset = new(ParseHEXColor(hex1), ParseHEXColor(hex2), ParseHEXColor(hex3), ParseHEXColor(hex4));
    }
    public static void CopyToClipBoard(string value)
    {
#if UNITY_WEBGL
        WebGLNative.CopyToClipboardJS(value);
#else
        TextEditor te = new() { text = value };
        te.SelectAll();
        te.Copy();
#endif
    }

    // ----------------------------------------------------------------------
    // ----------------------------------------------------------------------
    // --------------------- DEFINE OWN FUNCTION BELOW ---------------------
    // ----------------------------------------------------------------------
    // ----------------------------------------------------------------------

    /// <summary>
    /// Just split a List with sample: [0,1,2,...splitBy,...splitBy,...]
    /// </summary>
    /// <param name="listIn"></param>
    /// <param name="listOut"></param>
    /// <param name="splitBy"></param>
    public static List<List<long>> SplitListLong(List<long> listIn, long splitBy)
    {   // ex: listIn: [1,2,3,-1,2,3,4,-1,-4-5-6], splitBy -1 -> listOut:[[1,2,3],[2,3,4],[-4,-5,-6]]
        List<List<long>> listOut = new();
        listOut.Add(new List<long>());
        int iRun = 0;
        int count = listIn.Count;
        for (int i = 0; i < count; i++)
        {
            if (listIn[i] == splitBy)
            {
                if (i != count - 1)
                {
                    listOut.Add(new List<long>());
                    iRun++;
                }
            }
            else listOut[iRun].Add(listIn[i]);
        }
        return listOut;
    }
    public static Tweener PlayTween(float duration, ICallFunc.Func2<float> onUpdate = null, ICallFunc.Func2<float> onComplete = null, Tweener tweenExist = null)
    {
        if (tweenExist != null)
            tweenExist.Kill();

        float amount = 0;
        Tweener t = DOTween.To(() => amount, x => amount = x, 1, duration)
               .OnUpdate(() => { onUpdate?.Invoke(amount); })
               .OnComplete(() => { onComplete?.Invoke(amount); });
        return t;
    }
}