using System.Collections.Generic;
using System;
using UnityEngine;

public class ITimeFormat
{
    public const string s = @"%s"; // 1 
    public const string ss = @"ss"; // 01 
    public const string m = @"%m"; // 1
    public const string mm = @"mm"; // 01
    public const string h = @"%h"; // 1
    public const string hh = @"hh"; // 01
    public const string mm_ss = @"mm\:ss"; // 01:09
    public const string hh_mm = @"hh\:mm"; // 23:59
    public const string hh_mm_ss = @"hh\:mm\:ss"; // 23:59:59
    public const string HH_mm_dd_MM_yyyy = "HH:mm dd/MM/yyyy"; // 23:59 31/12/2000
    public const string hh_mm_ampm = "hh:mm tt"; // 11:59 AM
    public const string hh_mm_ampm_dd_MM_yy = "hh:mm tt dd/MM/yy"; // 11:59 PM 31/12/23
    public const string hh_mm_ampm_br_dd_MM_yy = "hh:mm tt\ndd/MM/yy"; // 11:59 AM<br>31/12/23</br>
    public const string hh_mm_ampm_dd_MM_yyyy = "hh:mm tt dd/MM/yyyy"; // 11:59 PM 31/12/2023
    public const string dd_MM = "dd/MM"; // 31/12
    public const string dd_MM_yy = "dd/MM/yy"; // 31/12/23
    public const string dd_MM_yyyy = "dd/MM/yyyy"; // 31/12/2023
    public const string dd_MM_HH_mm = "dd/MM HH:mm"; // 31/12 23:59
    public const string dd_MM_yy_HH_mm = "dd/MM/yy HH:mm"; // 31/12/23 23:59
    public const string dd_MM_yyyy_HH_mm = "dd/MM/yyyy HH:mm"; // 31/12/2023 23:59
    public const string dd_MM_hh_mm_ampm = "dd/MM hh:mm tt"; // 31/12 11:59 AM
    public const string dd_MM_yy_hh_mm_ampm = "dd/MM/yy hh:mm tt"; // 31/12/23 11:59 PM
    public const string dd_MM_yyyy_hh_mm_ampm = "dd/MM/yyyy hh:mm tt"; // 31/12/2023 11:59 AM
}
public class ITimer
{
    public static long UtcNowTicks { get { return DateTime.UtcNow.Ticks; } }
    private static DateTime _Jan1st1970Utc { get { return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc); } }

    public static long GetDateTimeInMilliseconds(DateTime? utc = null)
    {
        if (utc == null) utc = DateTime.UtcNow;
        return (long)(utc.Value - _Jan1st1970Utc).TotalMilliseconds;
    }
    public static long GetDateTimeInSeconds(DateTime? utc = null)
    {
        if (utc == null) utc = DateTime.UtcNow;
        return (long)(utc.Value - _Jan1st1970Utc).TotalSeconds;
    }
    public static long GetTimeFromNowInSeconds(long nextSeconds)
    {
        DateTime dateStart = DateTime.UtcNow, dateEnd = GetUtcTimeInNextSeconds(nextSeconds);
        return Math.Abs((long)(dateEnd - dateStart).TotalSeconds);
    }
    public static long GetTimeFromNowInMilliseconds(long endMilliseconds)
    {
        DateTime dateStart = DateTime.UtcNow;
        DateTime dateEnd = GetUtcTimeFromTimestamp2(endMilliseconds);
        long delta = (long)(dateEnd - dateStart).TotalMilliseconds;
        return Math.Abs(delta);
    }
    public static long GetTimePassInSeconds(long lastTicks)
    {
        long delta = UtcNowTicks - lastTicks;
        long seconds = (long)TimeSpan.FromTicks(delta).TotalSeconds;
        return seconds;
    }
    public static long GetTimePassInSeconds(DateTime utc)
    {
        return GetTimePassInSeconds(utc.Ticks);
    }
    public static long GetTimePassInMilliseconds(long lastTicks)
    {
        long delta = UtcNowTicks - lastTicks;
        long milliseconds = (long)TimeSpan.FromTicks(delta).TotalMilliseconds;
        return milliseconds;
    }
    public static long GetTimePassInMilliseconds(DateTime utc)
    {
        return GetTimePassInMilliseconds(utc.Ticks);
    }

    /// <summary>
    /// https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-timespan-format-strings?redirectedfrom=MSDN
    /// </summary>
    /// <param name="remainSeconds"></param>
    /// <param name="timeSpanFormat">Should begin with @; example: @"hh\:mm"</param>
    /// <returns></returns>
    public static string FormatTimeSpan(long remainSeconds, string timeSpanFormat = ITimeFormat.s)
    {
        TimeSpan ts = TimeSpan.FromSeconds(remainSeconds);
        return ts.ToString(timeSpanFormat);
    }
    public static string FormatTimeSpanMilliseconds(long remainMilliseconds, string timeSpanFormat = ITimeFormat.s)
    {
        TimeSpan ts = TimeSpan.FromMilliseconds(remainMilliseconds);
        return ts.ToString(timeSpanFormat);
    }

    /// <summary>
    /// https://www.c-sharpcorner.com/blogs/date-and-time-format-in-c-sharp-programming1
    /// </summary>
    /// <param name="seconds"></param>
    /// <param name="dateTimeFormat"></param>
    /// <returns></returns>
    public static string FormatTimestampToLocalTime(long seconds, string dateTimeFormat = ITimeFormat.HH_mm_dd_MM_yyyy)
    {
        DateTime local = GetLocalTimeFromTimestamp(seconds);
        return local.ToString(dateTimeFormat);
    }

    public static DateTime GetLocalTimeFromTimestamp(long seconds)
    {
        DateTime utc = _Jan1st1970Utc.AddSeconds(seconds), local = utc.ToLocalTime();
        return local;
    }
    public static DateTime GetUtcTimeInNextSeconds(long addedSeconds) { return _Jan1st1970Utc.AddSeconds(addedSeconds); }
    public static DateTime GetUtcTimeFromTimestamp2(long addedMilliseconds) { return _Jan1st1970Utc.AddMilliseconds(addedMilliseconds); }

    public static bool CheckSameDate(DateTime date1, DateTime date2)
    {
        return (date1 != null && date2 != null) ? date1.Date == date2.Date : false;
    }
    public static bool CheckSameYear(DateTime date1, DateTime date2)
    {
        return (date1 != null && date2 != null) ? date1.Year == date2.Year : false;
    }
    public static bool CheckLocalThisYear(DateTime local)
    {
        return CheckSameYear(local, DateTime.Today);
    }
    public static bool CheckLocalToday(DateTime local)
    {
        return CheckSameDate(local, DateTime.Today);
    }
    public static List<long> GetTimestampRangeInSeconds(long secondsStart, long secondsEnd, long secondsOffset = 24 * 60 * 60, bool stopInUtcNow = true)
    {
        if (stopInUtcNow)
        {
            long secondsNow = GetDateTimeInSeconds();
            if (secondsEnd > secondsNow) secondsEnd = secondsNow;
        }

        List<long> lstTimestamp = new List<long>();
        for (long seconds = secondsStart; seconds <= secondsEnd; seconds += secondsOffset)
        {
            lstTimestamp.Add(seconds);
        }
        return lstTimestamp;
    }

    public static void UpdateTimeDeltaSeconds(ref ITimeDelta timeDelta, ICallFunc.Func2<long> onValueChanged, ICallFunc.Func1 onFailed = null)
    {
        if (timeDelta > 0)
        {
            timeDelta.MakeTimePassInSeconds();
            onValueChanged?.Invoke(timeDelta.time);
        }
        else
        {
            onFailed?.Invoke();
        }
    }
    public static void UpdateTimeDeltaMilliseconds(ref ITimeDelta timeDelta, ICallFunc.Func2<long> onValueChanged, ICallFunc.Func1 onFailed = null)
    {
        if (timeDelta > 0)
        {
            timeDelta.MakeTimePassInMilliseconds();
            onValueChanged?.Invoke(timeDelta.time);
        }
        else
        {
            onFailed?.Invoke();
        }
    }
}


public class ITimeCache
{
    private long time = 60;//in seconds
    private long lastTicks = 0;//in milliseconds

    public ITimeCache(long seconds = 60, bool renew = false) { SetCache(seconds, renew); }

    public void SetCache(long seconds, bool renew = false)
    {
        time = seconds;
        if (renew) Renew();
    }
    public long GetCache() { return time; }
    public long GetCacheRemainInSeconds()
    {
        long remain = time;
        long timePass = ITimer.GetTimePassInSeconds(lastTicks);
        if (timePass > 0) remain -= timePass;
        if (remain <= 0) remain = 0;

        return remain;
    }
    public long GetCacheRemainInMilliseconds()
    {
        long remain = time * 1000;
        long timePass = ITimer.GetTimePassInMilliseconds(lastTicks);
        if (timePass > 0) remain -= timePass;
        if (remain <= 0) remain = 0;

        return remain;
    }
    public void Renew() { lastTicks = DateTime.UtcNow.Ticks; }
    public void Stop() { lastTicks = -1; }

    public bool CheckExpired(bool autoRenew = true)
    {
        bool expired = false;
        if (lastTicks <= 0) expired = true;
        else
        {
            long timePass = ITimer.GetTimePassInSeconds(lastTicks);
            if ((time - timePass) <= 0) expired = true;
        }

        if (expired && autoRenew) Renew();
        return expired;
    }

    public void ForceExpired() { lastTicks = 0; }
}


public struct ITimeDelta
{
    public long time { get; private set; }//in seconds or milliseconds
    private long lastTicks;//in milliseconds

    public static implicit operator ITimeDelta(long value)
    {
        ITimeDelta td = new ITimeDelta();
        td.Set(value);
        return td;
    }

    public static ITimeDelta operator -(ITimeDelta source, long timePass)
    {
        source.Minus(timePass);
        return source;
    }

    public static ITimeDelta operator +(ITimeDelta source, long timePass)
    {
        source.Add(timePass);
        return source;
    }

    public static bool operator >(ITimeDelta source, long value)
    {
        return source.time > value;
    }

    public static bool operator <(ITimeDelta source, long value)
    {
        return source.time < value;
    }

    public static bool operator ==(ITimeDelta source, long value)
    {
        return source.time == value;
    }

    public static bool operator !=(ITimeDelta source, long value)
    {
        return source.time != value;
    }

    public static bool operator >=(ITimeDelta source, long value)
    {
        return source.time >= value;
    }

    public static bool operator <=(ITimeDelta source, long value)
    {
        return source.time <= value;
    }

    public override bool Equals(object obj)
    {
        if (obj is ITimeDelta)
            return this == ((ITimeDelta)obj).time;
        else if (obj is long)
            return this == (long)obj;
        else if (obj is int)
            return this == (int)obj;
        return false;
    }

    public override int GetHashCode()
    {
        return time.GetHashCode() ^ lastTicks.GetHashCode();
    }

    public long timePassInSeconds
    {
        get
        {
            long delta = DateTime.UtcNow.Ticks - lastTicks;
            long seconds = (long)TimeSpan.FromTicks(delta).TotalSeconds;
            return seconds;
        }
    }

    public long timePassInMilliseconds
    {
        get
        {
            long delta = DateTime.UtcNow.Ticks - lastTicks;
            long milliseconds = (long)TimeSpan.FromTicks(delta).TotalMilliseconds;
            return milliseconds;
        }
    }

    public void MakeTimePassInSeconds()
    {
        long timePass = timePassInSeconds;
        if (timePass > 0) this -= timePass;
    }
    public void MakeTimePassInMilliseconds()
    {
        long timePass = timePassInMilliseconds;
        if (timePass > 0) this -= timePass;
    }

    public void MakeTimeNextInSeconds()
    {
        long timePass = timePassInSeconds;
        if (timePass > 0) this += timePass;
    }

    public void Set(long t)
    {
        time = t;
        lastTicks = DateTime.UtcNow.Ticks;
    }
    public void Minus(long timePass)
    {
        time -= timePass;
        if (time <= 0) time = 0;
        lastTicks = DateTime.UtcNow.Ticks;
    }
    public void Add(long timePass)
    {
        time += timePass;
        lastTicks = DateTime.UtcNow.Ticks;
    }
}