using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatWorldView : BaseView
{
    [SerializeField] private ChatWorldItem messagePrefab;
    [SerializeField] private Transform messageContentParent;
    [SerializeField] private TMP_InputField chatInputField;
    [SerializeField] private TextMeshProUGUI textAccountChip;
    [SerializeField] private ScrollRect scrollRect;
    protected override void Awake()
    {
        base.Awake();
        Init();
    }
    
    private void Init()
    {
        StartCoroutine(WaitAndScrollToEnd());
        chatInputField.characterLimit = 200;
        foreach (Transform child in messageContentParent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < 10; i++)
        {
            Instantiate(messagePrefab, messageContentParent).SetInfo(i);
        }
    }

    private IEnumerator WaitAndScrollToEnd()
    {
        yield return null; // chờ 1 frame
        scrollRect.verticalNormalizedPosition = 0f; // 0 = cuối, 1 = đầu
    }

}
