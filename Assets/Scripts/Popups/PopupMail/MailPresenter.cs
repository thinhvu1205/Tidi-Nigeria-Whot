using System;
using Cysharp.Threading.Tasks;
using Globals;
using Proto;
using UnityEngine;

public class MailPresenter
{
    public static MailPresenter Instance { get; private set; }
    private MailView mailView;

    public void Init(MailView view)
    {
        Instance = this;
        mailView = view;
    }

    public async UniTask<ListNotification> GetListNotification()
    {
        try
        {
            ListNotification response = await DataSender.GetListNotification();
            await mailView.OnSuccess();
            return response;
        }
        catch (Exception)
        {
            mailView.OnError("Error when update avatar!");
            throw;
        }
    }

    public async UniTask DeleteAllNotification()
    {
        try
        {
            await DataSender.DeleteAllNotifications();
        }
        catch (Exception)
        {
            mailView.OnError("Error when update avatar!");
        }
    }

    public async UniTask DeleteNotification(long notiId)
    {
        try
        {
            await DataSender.DeleteNotification(notiId);
        }
        catch (Exception)
        {
            mailView.OnError("Error when update avatar!");
        }
    }
}

