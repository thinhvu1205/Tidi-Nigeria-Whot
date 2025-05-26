using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectTableView : BaseView
{
    [SerializeField]
    private Button selectBetButton, selectTableButton, quickStartButton, createTableButton,
    nextButton, prevButton, refreshButton, findTableButton;
    [SerializeField] private ScrollRect scrollRectTable, scrollRectBet;
    [SerializeField] private GameObject tableItemPrefab, betItemPrefab, tabItemPrefab;
    [SerializeField] private Transform tableItemParent, betItemParent, tabItemParent;
    [SerializeField] private TMP_InputField findTableInputField;
    [SerializeField] private List<Sprite> buttonSpriteList;
    private JArray mockArray = new();
    private JArray mockTableArray = new();
    private int currentTab = 0;

    protected override void Start()
    {
        base.Start();
        mockArray = CreateMockRoomList();
        mockTableArray = CreateMockTableList();
        LoadListBetItem();

        Debug.Log("Mock Table List: " + mockTableArray.ToString());
    }
    #region Data
    public JArray CreateMockRoomList()
    {
        var roomList = new JArray
        {
            new JObject
            {
                ["mark"] = 2000,
                ["ag"] = 20000,
                ["agPn"] = 0,
                ["agD"] = 0,
                ["minAgCon"] = 20000,
                ["maxAgCon"] = 0,
                ["currplay"] = 5,
                ["room"] = 0,
                ["minChipbanker"] = 0,
                ["maxBet"] = 0,
                ["agLeft"] = 10000,
                ["agRaiseFee"] = 0,
                ["fee"] = 0.0
            },
            new JObject
            {
                ["mark"] = 10000,
                ["ag"] = 150000,
                ["agPn"] = 0,
                ["agD"] = 0,
                ["minAgCon"] = 150000,
                ["maxAgCon"] = 0,
                ["currplay"] = 5,
                ["room"] = 0,
                ["minChipbanker"] = 0,
                ["maxBet"] = 0,
                ["agLeft"] = 70000,
                ["agRaiseFee"] = 100000,
                ["fee"] = 1.5
            },
            new JObject
            {
                ["mark"] = 20000,
                ["ag"] = 300000,
                ["agPn"] = 0,
                ["agD"] = 0,
                ["minAgCon"] = 300000,
                ["maxAgCon"] = 0,
                ["currplay"] = 5,
                ["room"] = 0,
                ["minChipbanker"] = 0,
                ["maxBet"] = 0,
                ["agLeft"] = 140000,
                ["agRaiseFee"] = 200000,
                ["fee"] = 1.5
            },
            new JObject
            {
                ["mark"] = 50000,
                ["ag"] = 750000,
                ["agPn"] = 0,
                ["agD"] = 0,
                ["minAgCon"] = 750000,
                ["maxAgCon"] = 0,
                ["currplay"] = 4,
                ["room"] = 0,
                ["minChipbanker"] = 0,
                ["maxBet"] = 0,
                ["agLeft"] = 350000,
                ["agRaiseFee"] = 500000,
                ["fee"] = 1.5
            },
            new JObject
            {
                ["mark"] = 100000,
                ["ag"] = 1500000,
                ["agPn"] = 0,
                ["agD"] = 0,
                ["minAgCon"] = 1500000,
                ["maxAgCon"] = 0,
                ["currplay"] = 5,
                ["room"] = 0,
                ["minChipbanker"] = 0,
                ["maxBet"] = 0,
                ["agLeft"] = 700000,
                ["agRaiseFee"] = 1000000,
                ["fee"] = 1.5
            },
            new JObject
            {
                ["mark"] = 200000,
                ["ag"] = 3000000,
                ["agPn"] = 0,
                ["agD"] = 0,
                ["minAgCon"] = 3000000,
                ["maxAgCon"] = 0,
                ["currplay"] = 5,
                ["room"] = 0,
                ["minChipbanker"] = 0,
                ["maxBet"] = 0,
                ["agLeft"] = 1400000,
                ["agRaiseFee"] = 2000000,
                ["fee"] = 1.5
            },
            new JObject
            {
                ["mark"] = 500000,
                ["ag"] = 7500000,
                ["agPn"] = 0,
                ["agD"] = 0,
                ["minAgCon"] = 7500000,
                ["maxAgCon"] = 0,
                ["currplay"] = 5,
                ["room"] = 0,
                ["minChipbanker"] = 0,
                ["maxBet"] = 0,
                ["agLeft"] = 3500000,
                ["agRaiseFee"] = 5000000,
                ["fee"] = 1.5
            },
            new JObject
            {
                ["mark"] = 1000000,
                ["ag"] = 15000000,
                ["agPn"] = 0,
                ["agD"] = 0,
                ["minAgCon"] = 15000000,
                ["maxAgCon"] = 0,
                ["currplay"] = 0,
                ["room"] = 0,
                ["minChipbanker"] = 0,
                ["maxBet"] = 0,
                ["agLeft"] = 7000000,
                ["agRaiseFee"] = 10000000,
                ["fee"] = 1.5
            }
        };

        return roomList;
    }

    public JArray CreateMockTableList()
    {
        int[] mark = { 2000, 10000, 20000, 50000, 100000, 200000, 500000, 1000000 };
        JArray roomArray = new JArray();    
        for (int i = 0; i < mark.Length; i++) 
        {
            JObject room = new JObject
            {
                ["id"] = 12148 + i,
                ["N"] = $"Let is play LUCKY9 Amateur #{i + 1}",
                ["mark"] = mark[i],
                ["isPrivate"] = false,
                ["player"] = i + 1,
                ["size"] = 8,
                ["ArrName"] = new JArray
                {
                    $"PlayerA_{i}",
                    $"PlayerB_{i}",
                    $"PlayerC_{i}",
                    $"PlayerD_{i}"
                },
                ["H"] = 999 + i * 10,
                ["minAgCon"] = 10000 + i * 2000
            };

            roomArray.Add(room);
        }

        return roomArray;
    }
    #endregion

    private void LoadListBetItem()
    {

        for (int i = 0; i < betItemParent.childCount; i++)
        {
            Destroy(betItemParent.GetChild(i).gameObject);
        }
        for (int i = 0; i < mockArray.Count; i++)
        {
            int index = i;
            var betItem = Instantiate(betItemPrefab, betItemParent);
            betItem.GetComponent<BetItem>().SetData(mockArray[index] as JObject, index);
            var tableTabItem = Instantiate(tabItemPrefab, tabItemParent);
            tableTabItem.GetComponent<TableTabItem>().SetData(mockArray[index] as JObject);
            tableTabItem.GetComponent<Button>().onClick.AddListener(() =>
            {
                OnClickTab(tableTabItem.GetComponent<TableTabItem>(), mockArray[index] as JObject, index);
            });
            if (index == 0)
            {
                OnClickTab(tableTabItem.GetComponent<TableTabItem>(), mockArray[index] as JObject, index);
            }
        }
        tabItemParent.GetChild(currentTab).GetComponent<TableTabItem>().SetSelected();
    }

    private void LoadListTableItem(int mark)
    {
        scrollRectTable.DOVerticalNormalizedPos(1.0f, 0.2f).SetEase(Ease.OutSine);

        for (int i = 0; i < tableItemParent.childCount; i++)
        {
            Destroy(tableItemParent.GetChild(i).gameObject);
        }
        for (int i = 0; i < mockTableArray.Count; i++)
        {
            if ((int)mockTableArray[i]["mark"] != mark) continue;
            int index = i;
            var tableItem = Instantiate(tableItemPrefab, tableItemParent);
            tableItem.GetComponent<TableItem>().SetData(mockTableArray[index] as JObject, index);

        }
    }

    #region Button
    public void OnClickTab(TableTabItem tableTabItem, JObject dataItem, int index)
    {
        currentTab = index;
        for (int i = 0; i < tabItemParent.childCount; i++)
        {
            var item = tabItemParent.GetChild(i).GetComponent<TableTabItem>();
            if (item != null)
            {
                if (item.gameObject.name == tableTabItem.gameObject.name)
                {
                    item.SetSelected();
                }
                else
                {
                    item.SetUnselected();
                }
            }
        }
        int mark = (int)dataItem["mark"];
        LoadListTableItem(mark);
    }

    public void OnClickSelectBet()
    {
        scrollRectTable.gameObject.SetActive(false);
        scrollRectBet.gameObject.SetActive(true);
        selectBetButton.GetComponent<Image>().sprite = buttonSpriteList[0];
        selectTableButton.GetComponent<Image>().sprite = buttonSpriteList[1];
        scrollRectBet.DOVerticalNormalizedPos(0f, 0.2f).SetEase(Ease.OutSine);

    }

    public void OnClickSelectTable()
    {
        scrollRectTable.gameObject.SetActive(true);
        scrollRectBet.gameObject.SetActive(false);
        selectBetButton.GetComponent<Image>().sprite = buttonSpriteList[1];
        selectTableButton.GetComponent<Image>().sprite = buttonSpriteList[0];
        LoadListBetItem();
    }

    public void OnClickQuickStart()
    {

    }

    public void OnClickCreateTable()
    {

    }

    public void OnClickReload()
    {

    }

    public void OnClickNext()
    {
        scrollRectBet.DOHorizontalNormalizedPos(1.0f, 0.2f).SetEase(Ease.OutSine);
        // if (isHideBtnScroll) return;
        nextButton.gameObject.SetActive(false);
        prevButton.gameObject.SetActive(true);
    }

    public void OnClickPrev()
    {
        scrollRectBet.DOHorizontalNormalizedPos(0.0f, 0.2f).SetEase(Ease.OutSine);
        // if (isHideBtnScroll) return;
        nextButton.gameObject.SetActive(true);
        prevButton.gameObject.SetActive(false);
    }

    public void OnScrollListBet()
    {
        //Globals.Logging.Log(scrBet.horizontalNormalizedPosition);
        float posX = scrollRectBet.horizontalNormalizedPosition;
        float viewportWidth = scrollRectBet.viewport.GetComponent<RectTransform>().rect.width;
        float contentWidth = scrollRectBet.content.GetComponent<RectTransform>().rect.width;
        prevButton.gameObject.SetActive(viewportWidth < contentWidth && posX > 0.25f);
        nextButton.gameObject.SetActive(viewportWidth < contentWidth && posX < 0.75f);
    }
    #endregion
}
