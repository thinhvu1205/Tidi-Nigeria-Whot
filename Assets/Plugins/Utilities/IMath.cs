using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class IMath
{
    public static float DivideFloat(float a, float b, float replaceZero = 1f)
    {
        if (replaceZero == 0f) replaceZero = 1f;
        if (b == 0) b = replaceZero;
        return a / b;
    }
    public static double DivideDouble(double a, double b, double replaceZero = 1)
    {
        if (replaceZero == 0f) replaceZero = 1;
        if (b == 0) b = replaceZero;
        return a / b;
    }
    public static float MinMaxLimit(float x, float min, float max) { return Mathf.Max(Mathf.Min(x, min), max); }
    public static string GetString(string s, string defaultValue = "", bool ignoreEmpty = true)
    { return (s == null || ignoreEmpty && string.IsNullOrEmpty(s)) ? defaultValue : s; }
    public static int ParseInt(string s, int defaultValue = 0)
    {
        int r = defaultValue;
        int.TryParse(s, out r);
        return r;
    }
    public static long ParseLong(string s, long defaultValue = 0)
    {
        long r = defaultValue;
        long.TryParse(s, out r);
        return r;
    }
    public static float ParseFloat(string s, float defaultValue = 0f)
    {
        float r = defaultValue;
        float.TryParse(s, out r);
        return r;
    }
    public static int RandomInt(int fromInclusive, int toInclusive, int time = 1)
    {
        time = Mathf.Max(time, 1);
        int result = fromInclusive;
        for (int i = 0; i < time; i++)
            result = System.Convert.ToInt32(fromInclusive + (toInclusive - fromInclusive) * Random.value);
        return result;
    }
    public static long RandomLong(long fromInclusive, long toInclusive, int time = 1)
    {
        time = Mathf.Max(time, 1);
        long result = fromInclusive;
        for (int i = 0; i < time; i++)
            result = System.Convert.ToInt64(fromInclusive + (toInclusive - fromInclusive) * Random.value);
        return result;
    }
    public static float RandomFloat(float fromInclusive, float toInclusive, int time = 1)
    {
        time = Mathf.Max(time, 1);
        float result = fromInclusive;
        for (int i = 0; i < time; i++)
            result = fromInclusive + (toInclusive - fromInclusive) * Random.value;
        return result;
    }
    public static float ToDegree(float rad) { return rad * (180f / Mathf.PI); }
    public static float ToRadian(float degree) { return degree * (Mathf.PI / 180f); }
    public static List<int> Shuffle(int fromInclusive, int toExclusive, int time = 5)
    {
        List<int> numbers = new();
        for (int i = fromInclusive; i < toExclusive; i++) numbers.Add(i);
        int num = numbers.Count;
        while (num > 1)
        {
            num--;
            int k = RandomInt(0, num, time);
            (numbers[num], numbers[k]) = (numbers[k], numbers[num]);
        }
        return numbers;
    }
    public static List<T> Shuffle<T>(List<T> ret, int time = 1)
    {
        int num = ret.Count;
        while (num > 1)
        {
            num--;
            int k = RandomInt(0, num, time);
            (ret[num], ret[k]) = (ret[k], ret[num]);
        }
        return ret;
    }
    public static T[] Shuffle<T>(T[] ret, int time = 1)
    {
        int num = ret.Length;
        while (num > 1)
        {
            num--;
            int k = RandomInt(0, num, time);
            (ret[num], ret[k]) = (ret[k], ret[num]);
        }
        return ret;
    }
    public static List<int> ConvertListLongToListInt(List<long> longNumbers)
    {
        List<int> intNumbers = new();
        for (int i = 0; i < longNumbers.Count; i++)
        {
            int tmp = unchecked((int)longNumbers[i]);// It'll throw OverflowException in checked context if the value doesn't fit in an int:
            intNumbers.Add(tmp);
        }
        return intNumbers;
    }
    public static List<long> ConvertListIntToListLong(List<int> intNumbers)
    {
        List<long> longNumbers = new();
        for (int i = 0; i < intNumbers.Count; i++) longNumbers.Add(intNumbers[i]);
        return longNumbers;
    }
    public static List<int> GetIntDigits(string s)
    {
        List<int> ret = new();
        string[] digits = Regex.Split(s, @"\D+");
        foreach (string v in digits) if (int.TryParse(v, out int num)) ret.Add(num);
        return ret;
    }
    public static List<long> GetLongDigits(string s)
    {
        List<long> ret = new();
        string[] digits = Regex.Split(s, @"\D+");
        foreach (string v in digits) if (long.TryParse(v, out long num)) ret.Add(num);
        return ret;
    }
    public static List<float> GetFloatDigits(string s)
    {
        List<float> ret = new();
        string[] digits = Regex.Split(s, @"[^0-9.+-]+");
        foreach (string v in digits) if (float.TryParse(v, out float num)) ret.Add(num);
        return ret;
    }
    public static string LongToRoman(long num)
    {
        string[] romanLetters = { "M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I" };
        long[] numbers = { 1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1 };

        string romanResult = "";
        int index = 0;
        while (num != 0)
        {
            if (num >= numbers[index])
            {
                num -= numbers[index];
                romanResult += romanLetters[index];
            }
            else index++;
        }
        return romanResult;
    }
    public static List<List<T>> GetCombinationOfAList<T>(int k, List<T> list)
    {   // get all the combination of k different numbers
        int n = (list == null) ? 0 : list.Count;
        List<List<T>> result = new();
        if (n > 0 && n >= k)
        {
            int[] tmp = new int[n + 1];
            ICallFunc.Func1 print = () =>
            {
                List<T> combination = new();
                for (int i = 1; i <= k; i++)
                {
                    int index = tmp[i] - 1;
                    combination.Add(list[index]);
                }
                if (combination.Count == k) result.Add(combination);
            };
            for (int i = 1; i <= k; i++) tmp[i] = i;
            int idx = 0;
            do
            {
                print();
                idx = k;
                while (idx > 0 && tmp[idx] == (n - k + idx)) --idx;
                if (idx > 0)
                {
                    tmp[idx]++;
                    for (int i = idx + 1; i <= k; i++) tmp[i] = tmp[i - 1] + 1;
                }
            } while (idx != 0);
        }
        return result;
    }
    public static float GetMoveTime(List<Vector3> pointsV3s, float velocity, float defaultValue)
    {   // linear movement
        float s = 0;
        for (int i = 0; i < pointsV3s.Count; i++)
            if (i + 1 < pointsV3s.Count)
                s += Vector3.Distance(pointsV3s[i], pointsV3s[i + 1]);

        return (velocity > 0) ? (s / velocity) : Mathf.Max(0, defaultValue);
    }
    //https://javascript.info/bezier-curve
    public float Bezier2(float P1, float P2, float t) { return (1 - t) * P1 + t * P2; }
    public float Bezier3(float P1, float P2, float P3, float t)
    { return Mathf.Pow(1 - t, 2) * P1 + 2 * (1 - t) * t * P2 + Mathf.Pow(t, 2) * P3; }
    public float Bezier4(float P1, float P2, float P3, float P4, float t)
    { return Mathf.Pow(1 - t, 3) * P1 + 3 * Mathf.Pow(1 - t, 2) * t * P2 + 3 * (1 - t) * Mathf.Pow(t, 2) * P3 + Mathf.Pow(t, 3) * P4; }
}
