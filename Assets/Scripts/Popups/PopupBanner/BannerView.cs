using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Globals;
using Newtonsoft.Json;
using Proto;
using UnityEngine;
using UnityEngine.UI;

public class BannerView : BaseView
{
    [SerializeField] private Image imageBanner;
    [SerializeField] private GameObject buttonClose;

    private Action callback = null;

    public async void SetInfo(InAppMessage data, Sprite sprite)
    {
        bool isClose = data.Data.Params["isClose"] == "true" ? true : false;
        List<ButtonData> listButton = JsonConvert.DeserializeObject<List<ButtonData>>(data.Data.Params["listButton"]);
        if (listButton.Count > 0)
        {
            Debug.Log("Count: " + listButton.Count); // 1
            Debug.Log("urlButton: " + listButton[0].urlButton);
            Debug.Log("x: " + listButton[0].pos.x);
            Debug.Log("y: " + listButton[0].pos.y);

        }
        // Image
        imageBanner.sprite = sprite;
        imageBanner.SetNativeSize();
        var scale = 1.0f;
        if (imageBanner.rectTransform.rect.width >= 1280)
        {
            scale = 1280f / imageBanner.rectTransform.rect.width - 0.1f;
        }
        imageBanner.transform.localScale = new Vector3(scale, scale, scale);

        // Close Button
        buttonClose.SetActive(isClose);
        if (buttonClose != null)
        {
            RectTransform closeButtonRT = buttonClose.GetComponent<RectTransform>();
            closeButtonRT.anchorMax = new Vector2(1, 1);
            closeButtonRT.anchorMin = new Vector2(1, 1);
        }

        // Button
        for (var i = 0; i < listButton.Count; i++)
        {
            ButtonData buttonData = listButton[i];
            Pos position = buttonData.pos;

            //Texture2D tex = await Globals.Config.GetRemoteTexture((string)dtBtn["urlBtn"]);
            Sprite buttonSprite = null;
            if (!string.IsNullOrEmpty(buttonData.urlButton))
            {
                buttonSprite = await Config.GetRemoteSprite(buttonData.urlButton);
            }
            if (buttonSprite != null)
            {
                //var sp = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);

                var btnView = Utility.CreateButton(buttonSprite);
                btnView.GetComponent<Image>().SetNativeSize();
                btnView.transform.SetParent(imageBanner.transform, false);
                btnView.transform.localScale = Vector3.one;
                if (imageBanner.rectTransform.rect.width == float.NaN)
                {
                    imageBanner.rectTransform.sizeDelta = new Vector2(270, 479);
                }
                Vector2 posBtn = new Vector3(imageBanner.rectTransform.rect.width * (position.x - 0.5f), imageBanner.rectTransform.rect.height * (position.y - 0.5f));
                btnView.transform.localPosition = posBtn;
                btnView.transform.localEulerAngles = new Vector3(0, 0, 0);
                btnView.onClick.RemoveAllListeners();
                btnView.onClick.AddListener(() =>
                {
                    callback?.Invoke();
                });
                
            }
        }
    }

}

[Serializable]
public struct Pos
{
    public int x;
    public int y;
}

[Serializable]
public struct ButtonData
{
    public string urlButton;
    public string urlLink;
    public Pos pos;
}

