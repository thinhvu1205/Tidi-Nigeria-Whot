using UnityEngine;

namespace Globals
{
    public class Config
    {
        public static bool isOpenSound = true;
        public static bool isOpenMusic = true;

        private const string SOUND_KEY = "sound";
        private const string MUSIC_KEY = "music";

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