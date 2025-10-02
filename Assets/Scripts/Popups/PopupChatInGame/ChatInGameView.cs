using System.Collections;
using System.Collections.Generic;
using GIKCore.Pool;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatInGameView : BaseView
{
    [SerializeField] private ChatWorldItem messagePrefab;
    [SerializeField] private Transform messageContentParent;
    [SerializeField] private TMP_InputField chatInputField;
    [SerializeField] private TextMeshProUGUI textAccountChip;
    [SerializeField] private VerticalPoolGroup verticalPoolGroup;
    private List<ChatData> _PoolData = new();



    protected override void Awake()
    {
        base.Awake();
        verticalPoolGroup.SetCellDataCallback<ChatData>((go, data, index) =>
        {
            ChatInGameItem dataCIGI = go.GetComponent<ChatInGameItem>();
            dataCIGI.SetInfo(data);
        });
        ChatData a1 = new();
        ChatData a2 = new();
        ChatData a3 = new();
        ChatData a4 = new();
        _PoolData.Clear();
        _PoolData.Add(a1);
        _PoolData.Add(a2);
        _PoolData.Add(a3);
        _PoolData.Add(a4);
        verticalPoolGroup.SetAdapter(_PoolData);
        verticalPoolGroup.ReloadDataToVisibleCell();
    }
}
public class ChatData
{
    public int Id;
    public string Content;
    public bool IsUser;
    public bool IsSelect;
}