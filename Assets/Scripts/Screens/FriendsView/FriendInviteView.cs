using Cysharp.Threading.Tasks;
using Proto;
using UnityEngine;

public class FriendInviteView : BaseView
{
    [SerializeField] private Transform content;
    [SerializeField] private FriendInviteItem friendInviteItemPrefab;
    
    public int CurrentTab { get; set; }

    public override void OnClickCloseButton()
    {
        Hide(false);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        _ = GetFriendUpgradeCandidates();
    }

    private async UniTask GetFriendUpgradeCandidates()
    {
        var data =  await DataSender.GetFriendUpgradeCandidates(CurrentTab);
        if (data != null)
        {
            foreach (Transform child in content)
            {
                Destroy(child.gameObject);
            }

            foreach (var dataItem in data.Friends)
            {
                Debug.Log("data " + dataItem.ToString());
                var friendInviteItem = Instantiate(friendInviteItemPrefab, content);
                friendInviteItem.Setup(dataItem, CurrentTab);
            }
            
        }
    }
    
}