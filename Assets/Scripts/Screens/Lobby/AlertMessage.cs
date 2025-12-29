using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Newtonsoft.Json.Linq;
using System;

public class AlertMessage : Singleton<AlertMessage>
{
    [SerializeField] TextMeshProUGUI textAlert;

    private RectTransform rectTransform;
    private RectTransform rectTransformParent;

    private bool isRunning = false;
    private Vector2 posInView;
    private Vector2 sizeBg;
    private Rect parentRect;

    void Update()
    {
        //checkPosition();
    }
    protected override void Awake()
    {
        base.Awake();
        // rectTfParent = transform.parent.GetComponent<RectTransform>();
        // parentRect = rectTfParent.rect;
        // lbAlert.transform.localPosition = new Vector2(parentRect.width / 2, 17);
    }

    // Update is called once per frame
    public void addAlertMessage(JObject data)
    {

        // listData.Add(data);
        if (!isRunning)
        {
            showAlertMessage();
        }
    }
    //Guid uid_action;
    public void showAlertMessage()
    {

        // if (listData.Count > 0 && !UIManager.instance.isLoginShow())
        // {
        //     if (UIManager.instance.gameView == null)
        //     {
        //         if (!gameObject.activeSelf)
        //         {
        //             UIManager.instance.showAlert(true);
        //         }
        //     }
        //     if (!gameObject.activeSelf)
        //         gameObject.SetActive(true);
        //     isRunning = true;
        //     JObject data = listData[0];
        //     listData.RemoveAt(0);
        //     Globals.Config.list_Alert.Remove(data);
        //     lbAlert.text = (string)data["data"];
        //     Vector2 posEnd = Vector2.zero;
        //     if (transform.localEulerAngles.z == 0)
        //     {
        //         lbAlert.transform.localPosition = new Vector2(parentRect.width / 2, 17);
        //         posEnd = new Vector2(-parentRect.width / 2 - lbAlert.preferredWidth, 17);
        //     }
        //     else
        //     {
        //         lbAlert.transform.localPosition = new Vector2(parentRect.height / 2 + 17, 17);
        //         posEnd = new Vector2(-parentRect.height / 2 - lbAlert.preferredWidth, 17);
        //     }
        //     lbAlert.transform.DOLocalMoveX(posEnd.x, 12.5f).OnComplete(() =>
        //     {
        //         isRunning = false;
        //         DOTween.Sequence().AppendInterval(0.5f).AppendCallback(() =>
        //         {
        //             showAlertMessage();
        //         });

        //     });
        // }
        // else
        // {
        //     DOTween.Kill(lbAlert.transform);
        //     isRunning = false;
        //     UIManager.instance.showAlert(false);
        //     gameObject.SetActive(false);
        // }
    }
}
