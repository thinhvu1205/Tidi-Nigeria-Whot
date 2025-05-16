using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Globals
{
    public class Utility
    {
        #region Format Money
        public static string FormatMoney(int money, bool isK = false)
        {
            double absoluteValue = Mathf.Abs(money);
            string input = absoluteValue.ToString(), floatPart = "", format = "";
            int idNumberNextToDotFromTail = 0, integerValue = 0;
            int aBillion = 1000000000, aMillion = 1000000, aThousand = 1000;
            if (absoluteValue >= aBillion)
            {
                format = "B";
                integerValue = (int)(absoluteValue / aBillion);
                idNumberNextToDotFromTail = absoluteValue.ToString().Length - 9;
            }
            else if (absoluteValue >= aMillion)
            {
                format = "M";
                integerValue = (int)(absoluteValue / aMillion);
                idNumberNextToDotFromTail = absoluteValue.ToString().Length - 6;
            }
            else
            {
                if (isK)
                {
                    if (absoluteValue >= aThousand)
                    {
                        format = "K";
                        integerValue = (int)(absoluteValue / aThousand);
                        idNumberNextToDotFromTail = absoluteValue.ToString().Length - 3;
                    }
                    else return FormatNumber(money);
                }
                else return FormatNumber(money);
            }
            bool foundNotZero = false;
            for (int i = idNumberNextToDotFromTail + 2; i >= idNumberNextToDotFromTail; i--)
            {
                if (input[i] == '0' && !foundNotZero) continue;
                floatPart = input[i] + floatPart;
                foundNotZero = true;
            }
            if (floatPart.Length > 0) floatPart = "." + floatPart;
            return (money < 0 ? "-" : "") + FormatNumber(integerValue) + floatPart + format;
        }

        public static string FormatMoney(long money, bool isK = false)
        {
            double absoluteValue = Mathf.Abs(money);
            string input = absoluteValue.ToString(), floatPart = "", format = "";
            int idNumberNextToDotFromTail = 0, integerValue = 0;
            int aBillion = 1000000000, aMillion = 1000000, aThousand = 1000;
            if (absoluteValue >= aBillion)
            {
                format = "B";
                integerValue = (int)(absoluteValue / aBillion);
                idNumberNextToDotFromTail = absoluteValue.ToString().Length - 9;
            }
            else if (absoluteValue >= aMillion)
            {
                format = "M";
                integerValue = (int)(absoluteValue / aMillion);
                idNumberNextToDotFromTail = absoluteValue.ToString().Length - 6;
            }
            else
            {
                if (isK)
                {
                    if (absoluteValue >= aThousand)
                    {
                        format = "K";
                        integerValue = (int)(absoluteValue / aThousand);
                        idNumberNextToDotFromTail = absoluteValue.ToString().Length - 3;
                    }
                    else return FormatNumber(money);
                }
                else return FormatNumber(money);
            }
            bool foundNotZero = false;
            for (int i = idNumberNextToDotFromTail + 2; i >= idNumberNextToDotFromTail; i--)
            {
                if (input[i] == '0' && !foundNotZero) continue;
                floatPart = input[i] + floatPart;
                foundNotZero = true;
            }
            if (floatPart.Length > 0) floatPart = "." + floatPart;
            return (money < 0 ? "-" : "") + FormatNumber(integerValue) + floatPart + format;
        }

        public static string FormatMoney2(int mo, bool isK = false)
        {
            if (mo > 999999999 || mo < -999999999)
            {
                return mo.ToString("0,,,.###B", CultureInfo.InvariantCulture).Replace(".", ",");
            }
            else if (mo > 999999 || mo < -999999)
            {
                return mo.ToString("0,,.##M", CultureInfo.InvariantCulture).Replace(".", ",");
            }
            else if (mo > 999 || mo < -999)
            {
                if (isK)
                {
                    //return mo.ToString("#,##0,K", CultureInfo.InvariantCulture);
                    return mo.ToString("0,.##K", CultureInfo.InvariantCulture).Replace(".", ",");
                }
                return FormatNumber(mo);
            }
            else
            {
                return mo.ToString(CultureInfo.InvariantCulture).Replace(".", ",");
            }
        }
        public static string FormatMoney2(long mo, bool isK = false, bool isBiggerThan100K = false)
        {
            if (mo > 999999999 || mo < -999999999)
            {
                return mo.ToString("#,,,.###B", CultureInfo.InvariantCulture).Replace(".", ",");
            }
            else if (mo > 999999 || mo < -999999)
            {
                return mo.ToString("#,,.##M", CultureInfo.InvariantCulture).Replace(".", ",");
            }

            else if (mo > 999 || mo < -999)
            {
                if (isK)
                {
                    if (isBiggerThan100K && (mo >= 100000 || mo <= -100000))
                    {
                        return mo.ToString("0,.##K", CultureInfo.InvariantCulture).Replace(".", ",");

                    }
                    else
                    {
                        return FormatNumber(mo);
                    }
                }
                return FormatNumber(mo);
            }
            else
            {
                return mo.ToString(CultureInfo.InvariantCulture).Replace(".", ",");
            }
        }
        public static string FormatMoney3(long mo, long valueMinFormatK = 1000)
        {
            if (mo > 999999999 || mo < -999999999)
            {
                return mo.ToString("0,,,.###B", CultureInfo.InvariantCulture).Replace(".", ",");
            }
            else if (mo > 999999 || mo < -999999)
            {
                return mo.ToString("0,,.##M", CultureInfo.InvariantCulture).Replace(".", ",");
            }

            else if (mo >= valueMinFormatK || mo <= -valueMinFormatK)
            {
                return mo.ToString("0,.##K", CultureInfo.InvariantCulture).Replace(".", ",");
            }
            else
            {
                return FormatNumber(mo);
            }
        }
        #endregion

        #region Tween Number
        public static void TweenNumberFromK(TMPro.TextMeshProUGUI lbText, int toNumber, int startNumber = 0, float timeRun = 0.3f, bool isFormatK = false)
        {
            DOTween.To(() => startNumber, x => startNumber = x, toNumber, timeRun).OnUpdate(() => lbText.text = FormatMoney2(startNumber, isFormatK));
        }

        public static void TweenNumberTo(TMPro.TextMeshProUGUI lbText, int toNumber, int startNumber = 0, float timeRun = 0.3f, bool isFormatK = false)
        {
            DOTween.To(() => startNumber, x => startNumber = x, toNumber, timeRun).OnUpdate(() => lbText.text = FormatMoney2(startNumber, isFormatK, true));
        }
        public static void TweenNumberTo(TMPro.TextMeshProUGUI lbText, long toNumber, long startNumber = 0, float timeRun = 0.3f, bool isFormatK = false, bool is2Digit = true)
        {
            if (!is2Digit)
                DOTween.To(() => startNumber, x => startNumber = x, toNumber, timeRun).OnUpdate(() => lbText.text = FormatMoney(startNumber, isFormatK));
            else
                DOTween.To(() => startNumber, x => startNumber = x, toNumber, timeRun).OnUpdate(() => lbText.text = FormatMoney2(startNumber, isFormatK, true));
        }
        public static void TweenNumberToMoney(TMPro.TextMeshProUGUI lbText, int toNumber, int startNumber = 0, float timeRun = 0.3f, long valueMinFormatK = 10000)
        {
            DOTween.To(() => startNumber, x => startNumber = x, toNumber, timeRun).OnUpdate(() => lbText.text = FormatMoney3(startNumber, valueMinFormatK));
        }
        public static void TweenNumberToMoney(TMPro.TextMeshProUGUI lbText, long toNumber, long startNumber = 0, float timeRun = 0.3f, long valueMinFormatK = 10000, bool isLowerCase = false)
        {
            DOTween.To(() => startNumber, x => startNumber = x, toNumber, timeRun).OnUpdate(()
                =>
            {
                if (isLowerCase)
                {
                    lbText.text = FormatMoney3(startNumber, valueMinFormatK).ToLower();
                }
                else
                {
                    lbText.text = FormatMoney3(startNumber, valueMinFormatK);
                }
            }
                );
        }
        public static void TweenNumberToNumber(TMPro.TextMeshProUGUI lbText, int toNumber, int startNumber = 0, float timeRun = 0.3f, bool isLowerCase = false)
        {

            DOTween.To(() => startNumber, x => startNumber = x, toNumber, timeRun).SetEase(Ease.InSine).OnUpdate(() => { if (isLowerCase) lbText.text = FormatNumber(startNumber).ToLower(); else lbText.text = FormatNumber(startNumber); });
            Vector2 normalScale = lbText.transform.localScale;
            Vector2 biggerScale = new Vector2(normalScale.x + 0.2f, normalScale.y + 0.2f);
            DOTween.Kill(lbText.transform);
            DOTween.Sequence()
            .Append(lbText.transform.DOScale(biggerScale, timeRun * 0.45f))
            .AppendInterval(timeRun * 0.45f)
            .Append(lbText.transform.DOScale(normalScale, timeRun * 0.1f));
        }
        public static void TweenNumberToNumber(TMPro.TextMeshProUGUI lbText, long toNumber, long startNumber = 0, float timeRun = 0.5f, bool isLowerCase = false)
        {
            DOTween.To(() => startNumber, x => startNumber = x, toNumber, timeRun).OnUpdate(() => { if (isLowerCase) lbText.text = FormatNumber(startNumber).ToLower(); else lbText.text = FormatNumber(startNumber); }).OnComplete(() =>
            {
            });
            Vector2 normalScale = lbText.transform.localScale;
            Vector2 biggerScale = new Vector2(normalScale.x + 0.2f, normalScale.y + 0.2f);
            DOTween.Kill(lbText.transform);
            DOTween.Sequence()
            .Append(lbText.transform.DOScale(biggerScale, timeRun * 0.45f))
            .AppendInterval(timeRun * 0.45f)
            .Append(lbText.transform.DOScale(normalScale, timeRun * 0.1f));
        }
        public static void TweenNumberTo(TMPro.TextMeshProUGUI lbText, long toNumber, long startNumber = 0, float timeRun = 0.3f)
        {
            if (toNumber < 10000)
            {
                DOTween.To(() => startNumber, x => startNumber = x, toNumber, timeRun).OnUpdate(() => lbText.text = FormatNumber(startNumber));
            }
            else
            {
                DOTween.To(() => startNumber, x => startNumber = x, toNumber, timeRun).OnUpdate(() => lbText.text = FormatMoney2(startNumber));
            }

        }
        public static void TweenNumberTo(Text lbText, int toNumber, int startNumber = 0, float timeRun = 0.3f)
        {

            if (toNumber < 999999)
            {
                DOTween.To(() => startNumber, x => startNumber = x, toNumber, timeRun).OnUpdate(() => lbText.text = FormatNumber(startNumber));
            }
            else
            {
                DOTween.To(() => startNumber, x => startNumber = x, toNumber, timeRun).OnUpdate(() => lbText.text = FormatMoney2(startNumber));
            }
        }
        #endregion

        #region Format Number
        public static string FormatNumber(int number)
        {
            return string.Format("{0:n0}", number);
        }
        public static string FormatNumber(long number)
        {
            return string.Format("{0:n0}", number);
        }
        public static string FormatNumber(float number)
        {
            return string.Format("{0:n0}", number);
        }

        public static int SplitToInt(string number)
        {
            while (number.Contains("."))
            {
                number = number.Replace(".", "");
            }
            while (number.Contains(","))
            {
                number = number.Replace(",", "");
            }
            return int.Parse(number);
        }

        public static long SplitToLong(string number)
        {
            while (number.Contains("."))
            {
                number = number.Replace(".", "");
            }
            while (number.Contains(","))
            {
                number = number.Replace(",", "");
            }
            return long.Parse(number);
        }

        #endregion

        #region Convert Time
        public static string ConvertTimeToString(int seconds)
        {
            int h = seconds / 3600 % 24;
            int p = seconds / 60 % 60;
            int s = seconds % 60;
            return (h < 10 ? "0" : "") + h + ":" + (p < 10 ? "0" : "") + p + ":" + (s < 10 ? "0" : "") + s;
        }
        // public static string ConvertSeccondToDDHHMMSS(int timeRemain)
        // {
        //     long deltaTime = timeRemain * 1000;
        //     string seconds = Math.Floor((double)(deltaTime / 1000) % 60) + "";
        //     string minutes = Math.Floor((double)(deltaTime / 1000 / 60) % 60) + "";
        //     string hours = Math.Floor((double)(deltaTime / (1000 * 60 * 60)) % 24) + "";
        //     string days = Math.Floor((double)deltaTime / (1000 * 60 * 60 * 24)) + "";
        //     double dayNum = Math.Floor((double)deltaTime / (1000 * 60 * 60 * 24));
        //     if (hours.Length < 2) hours = "0" + hours;
        //     if (minutes.Length < 2) minutes = "0" + minutes;
        //     if (seconds.Length < 2) seconds = "0" + seconds;

        //     return days + (dayNum < 2 ? " " + Config.getTextConfig("txt_day") : " " + Config.getTextConfig("txt_day")) + ", " + hours + ":" + minutes + ":" + seconds;
        // }
        #endregion
    }
}
