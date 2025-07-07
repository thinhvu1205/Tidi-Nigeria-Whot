using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BasePopupGuide : BaseView
{
    [SerializeField] ScrollRect scrView;

    int currentPage = 1;

    public void OnClickPrevious()
    {
        RectTransform rectContent = scrView.content.GetComponent<RectTransform>();
        if (currentPage == 1)
        {
            scrView.normalizedPosition = new Vector2(1, 0);
            currentPage = scrView.content.childCount;
        }
        else
        {
            currentPage--;
            float previosPos = scrView.content.localPosition.x + scrView.content.GetChild(0).GetComponent<RectTransform>().sizeDelta.x;
            rectContent.DOLocalMoveX(previosPos, 0.3f);

        }
    }
    public void OnClickNext()
    {
        RectTransform rectContent = scrView.content.GetComponent<RectTransform>();
        if (currentPage == scrView.content.childCount)
        {
            scrView.normalizedPosition = new Vector2(0.0f, 0.0f);
            currentPage = 1;
        }
        else
        {
            currentPage++;
            float nextPos = scrView.content.localPosition.x - scrView.content.GetChild(0).GetComponent<RectTransform>().sizeDelta.x;
            rectContent.DOLocalMoveX(nextPos, 0.3f);
        }
    }
}
