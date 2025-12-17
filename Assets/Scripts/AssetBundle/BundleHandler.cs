using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Spine.Unity;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Video;

public class BundleHandler
{
    public enum PLATFORM { Android, iOS }
    public static BundleHandler MAIN
    {
        get
        {
            if (_INSTANCE == null) _INSTANCE = new();
            return _INSTANCE;
        }
    }
    private static string[] _PREFAB_TAILS = { ".prefab" }, _TEXT_TAILS = { ".txt" }, _IMAGE_TAILS = { ".png", ".jpg", ".jpeg" },
        _AUDIO_TAILS = { ".mp3", ".ogg" }, _VIDEO_TAILS = { ".mp4" }, _MATERIAL_TAILS = { ".mat" }, _SKELETON_TAILS = { ".asset" };
    private static BundleHandler _INSTANCE;
    public const string BASE_PATH = "Assets/AssetBundles/", CATEGORY = "category.txt", SPLIT = "_hash_", RESOURCES = "Assets/Resources/";
    public string BundleUrl;
    private Dictionary<string, BundleVersion> _AssetsMapD = new();
    private Dictionary<Object, BundleLoader> _LoadersBLs = new();


    public void AddLoader(Object loader) => _LoadersBLs.Add(loader, loader.GetComponent<BundleLoader>());
    public void RemoveLoader(Object loader) => _LoadersBLs.Remove(loader);
    public void ClearAssetsDictionary() => _AssetsMapD.Clear();
    public void AddToLocalMap(BundleVersion aBV)
    {
        foreach (string assetName in aBV.AssetNamesHS)
        {
            if (_AssetsMapD.ContainsKey(assetName)) _AssetsMapD[assetName] = aBV;
            else _AssetsMapD.Add(assetName, aBV);
        }
    }
    //-------------------------------------------------- |   ) )=3 --------------------------------------------------
    //                                      path must starts from Assets/Resources
    private static T _GetAsset<T>(string path, string[] tails) where T : Object
    {
        if (!path.StartsWith(RESOURCES)) path = RESOURCES + path;
        foreach (string tail in tails)
        {
            if (path.EndsWith(tail)) path = path.Replace(tail, "");
            string fullPath = (path + tail).ToLower();
            if (MAIN._AssetsMapD.ContainsKey(fullPath)) return MAIN._AssetsMapD[fullPath].BundleAB.LoadAsset<T>(fullPath);
        }
        return Resources.Load<T>(path.Replace(RESOURCES, ""));
    }
    private static ResourceRequest _GetAssetAsync<T>(string path, string[] tails) where T : Object
    {
        if (!path.StartsWith(RESOURCES)) path = RESOURCES + path;
        foreach (string tail in tails)
        {
            if (path.EndsWith(tail)) path = path.Replace(tail, "");
            string fullPath = (path + tail).ToLower();
            if (MAIN._AssetsMapD.ContainsKey(fullPath))
                return MAIN._AssetsMapD[fullPath].BundleAB.LoadAssetAsync<T>(fullPath);
        }
        return Resources.LoadAsync<T>(path.Replace(RESOURCES, ""));
    }
    private static T[] _GetAssetWithSubAssets<T>(string path, string[] tails) where T : Object
    {
        if (!path.StartsWith(RESOURCES)) path = RESOURCES + path;
        foreach (string tail in tails)
        {
            if (path.EndsWith(tail)) path = path.Replace(tail, "");
            string fullPath = (path + tail).ToLower();
            if (MAIN._AssetsMapD.ContainsKey(fullPath)) return MAIN._AssetsMapD[fullPath].BundleAB.LoadAssetWithSubAssets<T>(fullPath);
        }
        return Resources.LoadAll<T>(path.Replace(RESOURCES, ""));
    }
    private static void _PrepareLoadersIfNeeded(GameObject parent)
    {
        BundleLoader[] itemBLs = parent.GetComponentsInChildren<BundleLoader>(true);
        foreach (BundleLoader itemBL in itemBLs) itemBL.PrepareData();
    }
    #region Load Assets
    public static T Instantiate<T>(T prefab, Transform parentTf = null) where T : Object
    {
        T output = GameObject.Instantiate(prefab, parentTf);
        _PrepareLoadersIfNeeded(output.GameObject());
        return output;
    }
    // GameObject
    public static GameObject LoadGameObject(string path)
    {
        GameObject output = _GetAsset<GameObject>(path, _PREFAB_TAILS);
        if (output != null) _PrepareLoadersIfNeeded(output);
        return output;
    }
    // TextAsset
    public static TextAsset LoadTextAsset(string path) { return _GetAsset<TextAsset>(path, _TEXT_TAILS); }
    // Sprite
    public static Sprite LoadSprite(string path) { return _GetAsset<Sprite>(path, _IMAGE_TAILS); }
    // Texture
    public static Texture LoadTexture(string path) { return _GetAsset<Texture>(path, _IMAGE_TAILS); }
    // Texture2D
    public static Texture2D LoadTexture2D(string path) { return _GetAsset<Texture2D>(path, _IMAGE_TAILS); }
    // AudioClip
    public static AudioClip LoadAudioClip(string path) { return _GetAsset<AudioClip>(path, _AUDIO_TAILS); }
    // VideoClip    
    public static VideoClip LoadVideoClip(string path) { return _GetAsset<VideoClip>(path, _VIDEO_TAILS); }
    public static async UniTask<VideoClip> LoadVideoClipAsync(string path)
    {
        ResourceRequest rr = _GetAssetAsync<VideoClip>(path, _VIDEO_TAILS);
        await rr;
        return rr.asset as VideoClip;
    }
    // Material
    public static Material LoadMaterial(string path) { return _GetAsset<Material>(path, _MATERIAL_TAILS); }
    // SkeletonDataAsset
    public static SkeletonDataAsset LoadSkeletonDataAsset(string path) { return _GetAsset<SkeletonDataAsset>(path, _SKELETON_TAILS); }
    public static async UniTask<SkeletonDataAsset> LoadSkeletonDataAssetAsync(string path)
    {
        ResourceRequest rr = _GetAssetAsync<SkeletonDataAsset>(path, _SKELETON_TAILS);
        await rr;
        return rr.asset as SkeletonDataAsset;
    }
    // Sprite[]
    public static Sprite[] LoadMultipleSprites(string path) { return _GetAssetWithSubAssets<Sprite>(path, _IMAGE_TAILS); }
    #endregion
    public static bool SetDataForASkeletonGraphic(SkeletonGraphic targetSG, string path, string animName = "", bool loop = true)
    {
        if (targetSG == null) return false;
        SkeletonDataAsset skeDataSDA = LoadSkeletonDataAsset(path);
        if (skeDataSDA == null) return false;
        targetSG.skeletonDataAsset = skeDataSDA;
        targetSG.Initialize(true);
        if (!animName.Equals("")) targetSG.AnimationState.SetAnimation(0, animName, loop);
        return true;
    }
}

public class BundleVersion
{
    public enum STATE { Cloud, Downloading, Downloaded }
    public STATE State = STATE.Cloud;
    public HashSet<string> AssetNamesHS = new();
    public AssetBundle BundleAB;
    public Hash128 HashH128;
    public string Name, Url;
}
