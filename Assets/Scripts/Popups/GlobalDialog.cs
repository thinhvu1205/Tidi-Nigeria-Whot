using System;
using UnityEngine;

public class GlobalDialog : Singleton<GlobalDialog>
{
    [SerializeField] private DialogView dialogView;

    public void SetInfo(string content, Action confirmCallback = null)
    {
        dialogView.SetContent(content);
        dialogView.ConfigConfirmButton(true, "OK", confirmCallback);
        dialogView.ConfigCancelButton(false, "Cancel", null);
    }
}
