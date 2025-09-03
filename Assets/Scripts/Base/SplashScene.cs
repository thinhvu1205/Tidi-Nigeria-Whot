using System.Collections;
using Globals;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashScene : MonoBehaviour
{
    //https://console.cloud.google.com/storage/browser/whot
    [SerializeField] private BundleDownloader m_BundleBD;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        // https://storage.googleapis.com/whot/AssetBundles
        string storedUrl = PlayerPrefs.GetString(BundleDownloader.STORED_BUNDLE_URL, "");
        // storedUrl = "D:/Unity projects/Tidi-Phil-Win777/Assets/AssetBundles";
        storedUrl = "https://storage.googleapis.com/whot/AssetBundles";
        m_BundleBD.CheckAndDownloadAssets(storedUrl, 1f,
            () =>
            {
                m_BundleBD.SetProgressText("Retrying ...");
                StartCoroutine(retry());
            },
            () =>
            {
                SceneManager.LoadScene("Login");
            });

        IEnumerator retry()
        {
            while (BundleHandler.MAIN.BundleUrl == null || BundleHandler.MAIN.BundleUrl.Equals(""))
                yield return new WaitForSeconds(1f);
            m_BundleBD.CheckAndDownloadAssets(BundleHandler.MAIN.BundleUrl, 0,
                () =>
                {
                    m_BundleBD.SetProgressText("Fail to get assets!");
                },
                () =>
                {
                    SceneManager.LoadScene("Login");
                });
        }

    }
}
