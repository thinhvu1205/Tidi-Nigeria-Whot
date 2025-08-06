using System.Collections;
using Globals;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashScene : MonoBehaviour
{
    //https://console.cloud.google.com/storage/browser/tongitswar;tab=objects?inv=1&invt=Abzosg&project=philippines-253209&prefix=&forceOnObjectsSortingFiltering=false
    [SerializeField] private BundleDownloader m_BundleBD;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        // "D:/Unity projects/Tidi-Phil-Win777/Assets/AssetBundles";
        // https://storage.googleapis.com/tongitswar/AssetBundles;
        string storedUrl = PlayerPrefs.GetString(BundleDownloader.STORED_BUNDLE_URL, "");
        // storedUrl = "D:/Unity projects/Tidi-Phil-Win777/Assets/AssetBundles";
        m_BundleBD.CheckAndDownloadAssets(storedUrl, 1f,
            () =>
            {
                m_BundleBD.SetProgressText("Retrying ...");
                StartCoroutine(retry());
            },
            () =>
            {
                SceneManager.LoadScene("MainScene");
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
                    SceneManager.LoadScene("MainScene");
                });
        }

    }
}
