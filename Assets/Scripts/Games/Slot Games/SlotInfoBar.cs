using System.Collections.Generic;
using DG.Tweening;
using Globals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InfoBarController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI lbInfo;

    [SerializeField] List<Image> listImgIcon;

    [SerializeField] List<Sprite> listSprIcon;
    [SerializeField] GameObject sprContainer;
    [SerializeField] Image sprStateWin;
    [SerializeField] List<Sprite> listSprStateWin;
    
    void Start()
    {
        sprContainer.SetActive(false);
    }

    public void setInfoWinLine(int id, int numberOfItem, int chipWin)
    {
        sprContainer.SetActive(true);
        for (int i = 0; i < listImgIcon.Count; i++)
        {
            Image img = listImgIcon[i];
            img.sprite = listSprIcon[id];
            img.gameObject.SetActive((i < numberOfItem));
        }
        lbInfo.text = Utility.FormatString("Pay Chip", Utility.FormatNumber(chipWin));
    }
    public void setInfoText(string msg)
    {
        lbInfo.text = msg;
    }
    public void setSpining()
    {
        lbInfo.text = Utility.FormatString("Playing line", 25);
    }
    public void prepareSpin()
    {
        sprContainer.SetActive(false);
        lbInfo.text = "Press Play To Spin";
    }
    public void setNotEnoughChip()
    {
        sprContainer.SetActive(false);
        lbInfo.text = "Not enough chip";
    }
    public void setStateWin(string type)
    {
        if (type == "totalWin")
        {
            sprStateWin.sprite = listSprStateWin[0];
        }
        else if (type == "lastWin")
        {
            sprStateWin.sprite = listSprStateWin[1];
        }
        else if (type == "win")
        {
            sprStateWin.sprite = listSprStateWin[2];
        }
        sprStateWin.SetNativeSize();
    }
    public void setDPSpinLeft(int number)
    {
        sprContainer.SetActive(false);
        lbInfo.text = number + " " + "free spin";
    }
    public void effectUpdateDBFSL()
    {
        TextMeshProUGUI lbInfoShadow = Instantiate(lbInfo.gameObject, transform).GetComponent<TextMeshProUGUI>();

        lbInfoShadow.transform.localPosition = transform.InverseTransformPoint(lbInfo.transform.position);
        lbInfoShadow.transform.DOScale(new Vector2(1.3f, 1.3f), 1.0f).SetEase(Ease.OutSine);
        lbInfoShadow.DOFade(0, 1.0f).SetEase(Ease.OutSine);
        lbInfoShadow.transform.DOLocalMoveY(1.0f, lbInfoShadow.transform.localPosition.y + 50).SetEase(Ease.OutSine).OnComplete(() =>
        {
            //Destroy(lbInfoShadow.gameObject);
        });
    }
}
