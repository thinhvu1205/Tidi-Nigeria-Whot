using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Globals;
using Proto;
using UnityEngine;
using UnityEngine.UI;

public class ListBannerView : BaseView
{
    private BannerPresenter bannerPresenter;
    [SerializeField] private Transform transformPagination;
    [SerializeField] private GameObject dotPrefab;
    [SerializeField] private RectTransform transformBanner;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Button buttonPrev, buttonNext;
    private const float SWIPE_TIME = .2f;
    private List<BannerView> listBannerView = new();
    private BannerView currentBanner;
    private bool isScrolling, isClicking, hasFetchData = false;

    private void LateUpdate()
    {
        if (isClicking) return;
        if (!Input.GetMouseButton(0))
        {
            if (!isScrolling || !hasFetchData) return;
            isScrolling = false;
            scrollRect.enabled = false; 
            scrollRect.content.DOLocalMoveX(scrollRect.content.localPosition.x - FindNearestBannerLocalPosition().x, SWIPE_TIME)
                .OnComplete(() =>
                {
                    CheckOnEdge();
                    scrollRect.enabled = true;
                    UpdatePaginationDots();
                });
        }
        else isScrolling = true;
    }
    protected override void Start()
    {
        transformBanner.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, scrollRect.viewport.rect.width);
        transformBanner.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, scrollRect.viewport.rect.height);
        // _LoadListBanner();
    }

    protected override void Awake()
    {
        base.Awake();
        bannerPresenter = new BannerPresenter();
        bannerPresenter.Init(this);
    }

    public override void OnClickCloseButton()
    {
        Hide(false);
    }

    public void SetBannerType(TypeInAppMessage type)
    {
        _ = GetBannerData(type);
    }

    public async UniTask GetBannerData(TypeInAppMessage type)
    {
        ListInAppMessage response = await bannerPresenter.GetBanner(typeInAppMessage: type);
        List<InAppMessage> listData = response.InAppMessages.ToList();
        Sprite firstSprite = null, lastSprite = null;
        InAppMessage firstBannerData = null, lastBannerData = null;
                hasFetchData = true;

        if (listData.Count == 0)
        {
            Hide();
            return;
        }
        else
        {
            transform.localScale = Vector3.one;
        }
        if (listData.Count == 1)
        {
            buttonNext.gameObject.SetActive(false);
            buttonPrev.gameObject.SetActive(false);
        }
        foreach (InAppMessage data in listData)
        {
            if (true)
            
            // if (CheckAppCondition(data.Data.Params["app"]) &&
            //     CheckOSCondition(data.Data.Params["os"]) &&
            //     CheckVersionCondition(data.Data.Params["version"])
            // )
            {
                string urlImg = data.Data.Params["images"];
                Sprite sprite = await Config.GetRemoteSprite(urlImg);
                if (sprite == null) return;
                RectTransform gameObject = Instantiate(transformBanner, scrollRect.content);
                // gameObject.name = i.ToString();
                gameObject.gameObject.SetActive(true);
                BannerView nodeBanner = gameObject.transform.GetChild(0).GetComponent<BannerView>();
                nodeBanner.transform.localScale = Vector3.one;
                await nodeBanner.SetInfo(data, sprite);
                listBannerView.Add(nodeBanner);
                if (firstSprite == null && firstBannerData == null)
                {
                    firstSprite = sprite;
                    firstBannerData = data;
                }
                lastSprite = sprite;
                lastBannerData = data;
                GameObject dot = Instantiate(dotPrefab, transformPagination);
                dot.SetActive(true);
            }
        }

        if (listBannerView.Count <= 0) return;
        if (listBannerView.Count > 1)
        {
            Transform cloneFirstTf = Instantiate(transformBanner, scrollRect.content);
            Transform cloneLastTf = Instantiate(transformBanner, scrollRect.content);
            cloneFirstTf.gameObject.SetActive(false);
            cloneLastTf.gameObject.SetActive(false);
            cloneFirstTf.localScale = Vector3.one;
            cloneLastTf.localScale = Vector3.one;
            BannerView cloneFirstBV = cloneFirstTf.GetChild(0).GetComponent<BannerView>();
            cloneFirstBV.transform.localScale = Vector3.one;
            await cloneFirstBV.SetInfo(firstBannerData, firstSprite);
            cloneFirstTf.SetAsLastSibling();
            BannerView cloneLastBV = cloneLastTf.GetChild(0).GetComponent<BannerView>();
            cloneLastBV.transform.localScale = Vector3.one;
            await cloneLastBV.SetInfo(lastBannerData, lastSprite);
            cloneLastTf.SetAsFirstSibling();
            // await Task.Yield();
            // await Task.Yield();
            // await Task.Yield();
            // await Task.Yield();
            // await Task.Yield();
            // await Task.Yield();
            // scrollRect.content.anchoredPosition -= new Vector2(transformBanner.rect.width, 0);
            scrollRect.StopMovement();
            scrollRect.velocity = Vector2.zero;
            scrollRect.content.anchoredPosition = new Vector2(-transformBanner.rect.width, 0);                    // Dừng mọi chuyển động
            scrollRect.inertia = false;                   // Tắt quán tính tạm thời

            // Bật lại inertia sau 1 frame
            await UniTask.Yield();
            scrollRect.inertia = true;
            listBannerView.Insert(0, cloneLastBV);
            listBannerView.Add(cloneFirstBV);
            cloneFirstTf.gameObject.SetActive(true);
            cloneLastTf.gameObject.SetActive(true);
            currentBanner = listBannerView[1];
        }
        else currentBanner = listBannerView[0];
        foreach (BannerView bv in listBannerView) bv.gameObject.SetActive(true);
        UpdatePaginationDots();
    }
    

    #region Button

    public void OnClickPrevious()
    {
        if (scrollRect.content.childCount <= 1) return;
        if (isClicking) return;
        scrollRect.content.DOComplete();
        isClicking = true;
        if (listBannerView.IndexOf(currentBanner) == 1) currentBanner = listBannerView[listBannerView.Count - 2];
        scrollRect.content.DOLocalMoveX(scrollRect.content.localPosition.x + transformBanner.rect.width, SWIPE_TIME)
            .OnComplete(() =>
            {
                // _CheckOnEdge();
                isClicking = false;
            });
        UpdatePaginationDots();
    }

    public void OnClickNext()
    {
        if (scrollRect.content.childCount <= 1) return;
        if (isClicking) return;
        scrollRect.content.DOComplete();
        isClicking = true;
        // if (_BannerBVs.IndexOf(_BannerNowBV) == _BannerBVs.Count - 1) _BannerNowBV = _BannerBVs[1];
        scrollRect.content.DOLocalMoveX(scrollRect.content.localPosition.x - transformBanner.rect.width, SWIPE_TIME)
            .OnComplete(() =>
            {
                CheckOnEdge();
                isClicking = false;
            });
        UpdatePaginationDots();
    }
    #endregion

    private async void _LoadListBanner()
    {
        // JObject dataBannerFirst = null, databannerLast = null;
        // Sprite firstS = null, lastS = null;
        // for (int i = 0; i < Config.arrOnlistTrue.Count; i++)
        // {
        //     JObject dataBanner = (JObject)Config.arrOnlistTrue[i];
        //     dataBanner["isClose"] = false;
        //     string urlImg = (string)dataBanner["urlImg"];
        //     Sprite spriteS = await Config.GetRemoteSprite(urlImg, true);
        //     if (spriteS == null) return;
        //     RectTransform go = Instantiate(transformBanner, scrollRect.content);
        //     go.name = i.ToString();
        //     go.gameObject.SetActive(true);
        //     BannerView nodeBanner = go.transform.GetChild(0).GetComponent<BannerView>();
        //     nodeBanner.transform.localScale = Vector3.one;
        //     nodeBanner.setInfo(dataBanner, false, () => { hide(); }, spriteS);
        //     listBannerView.Add(nodeBanner);
        //     if (listBannerView.Count == 1)
        //     {
        //         firstS = spriteS;
        //         dataBannerFirst = dataBanner;
        //     }
        //     databannerLast = dataBanner;
        //     lastS = spriteS;
        //     GameObject dot = Instantiate(transformDot, transformPagination).gameObject;
        //     dot.SetActive(true);
        // }
        // if (listBannerView.Count <= 0) return;
        // if (listBannerView.Count > 1)
        // {
        //     Transform cloneFirstTf = Instantiate(transformBanner, scrollRect.content);
        //     Transform cloneLastTf = Instantiate(transformBanner, scrollRect.content);
        //     cloneFirstTf.gameObject.SetActive(true);
        //     cloneLastTf.gameObject.SetActive(true);
        //     cloneFirstTf.localScale = Vector3.one;
        //     cloneLastTf.localScale = Vector3.one;
        //     BannerView cloneFirstBV = cloneFirstTf.GetChild(0).GetComponent<BannerView>();
        //     cloneFirstBV.transform.localScale = Vector3.one;
        //     // cloneFirstBV.setInfo(dataBannerFirst, false, () => { hide(); }, firstS);
        //     cloneFirstTf.SetAsLastSibling();
        //     BannerView cloneLastBV = cloneLastTf.GetChild(0).GetComponent<BannerView>();
        //     cloneLastBV.transform.localScale = Vector3.one;
        //     // cloneLastBV.setInfo(databannerLast, false, () => { hide(); }, lastS);
        //     cloneLastTf.SetAsFirstSibling();
        //     await Task.Yield();
        //     await Task.Yield();
        //     await Task.Yield();
        //     scrollRect.content.anchoredPosition -= new Vector2(transformBanner.rect.width, 0);
        //     listBannerView.Insert(0, cloneLastBV);
        //     listBannerView.Add(cloneFirstBV);
        //     currentBanner = listBannerView[1];
        // }
        // else currentBanner = listBannerView[0];
        // foreach (BannerView bv in listBannerView) bv.gameObject.SetActive(true);
        UpdatePaginationDots();
    }

    private void UpdatePaginationDots()
    {
        if (listBannerView.Count > 1)
        {
            for (int i = 0; i < transformPagination.childCount; i++)
                transformPagination.GetChild(i).GetChild(0).gameObject.SetActive(listBannerView.IndexOf(currentBanner) == i + 1);
        }
        else transformPagination.GetChild(0).GetChild(0).gameObject.SetActive(true);
    }

    private Vector2 FindNearestBannerLocalPosition()
    {
        RectTransform contentRT = scrollRect.content, viewportRT = scrollRect.viewport;
        Vector2 returnedV2 = new();
        float minDistance = float.MaxValue;
        for (int i = 0; i < contentRT.childCount; i++)
        {
            RectTransform childRT = contentRT.GetChild(i).GetComponent<RectTransform>();
            Vector2 childWorldV2 = childRT.position, childLocalV2 = viewportRT.InverseTransformPoint(childWorldV2);
            float distance = childLocalV2.magnitude;
            if (distance < minDistance)
            {
                minDistance = distance;
                returnedV2 = childLocalV2;
                currentBanner = childRT.GetComponentInChildren<BannerView>();
            }
        }
        return returnedV2;
    }

    private void CheckOnEdge()
    {
        int lastId = listBannerView.Count - 1, countBanners = listBannerView.Count - 2, id = listBannerView.IndexOf(currentBanner);
        if (id == 0)
        {
            currentBanner = listBannerView[lastId - 1];
            scrollRect.content.anchoredPosition -= new Vector2(countBanners * transformBanner.rect.width, 0);
        }
        else if (id == lastId)
        {
            currentBanner = listBannerView[1];
            scrollRect.content.anchoredPosition += new Vector2(countBanners * transformBanner.rect.width, 0);
        }
    }

    private bool CheckOSCondition(string os)
    {
        return true;
        if (string.IsNullOrEmpty(os))
            return true;

        var osValue = int.Parse(os);

        switch (osValue)
        {
            case 0: return true; // Cả iOS và Android
            case 1: return Config.os == RuntimePlatform.IPhonePlayer; // Chỉ iOS
            case 2: return Config.os == RuntimePlatform.Android; // Chỉ Android
            default: return true;
        }
    }

    private bool CheckAppCondition(string app)
    {
        return true;
    }

    private bool CheckVersionCondition(string version)
    {
        return true;
    }
    
}
