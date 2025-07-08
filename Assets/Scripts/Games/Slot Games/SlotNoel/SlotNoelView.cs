using System.Collections;
using System.Collections.Generic;
using Globals;
using UnityEngine;

public class SlotNoelView : BaseSlotView
{
    protected override void UpdateSpinButtonUI()
    {
        base.UpdateSpinButtonUI();

        // Mặc định màu trắng
        buttonSpinAnimation.color = Color.white;

        // Hàm set animation theo loại spin


        // Xử lý theo state
        switch (gameState)
        {
            case SlotGameState.SPINNING:
                SetSpinAnimation(spinType);

                // Nếu đang spin mà không phải auto, set màu xám
                if (spinType == SpinType.NORMAL || spinType == SpinType.FREE_NORMAL)
                {
                    buttonSpinAnimation.color = Color.gray;
                }
                break;

            case SlotGameState.SHOWING_RESULT:
                SetSpinAnimation(spinType);
                break;

            case SlotGameState.PREPARE:
            case SlotGameState.JOIN_GAME:
                SetSpinAnimation(spinType);

                // Nếu hết tiền và không phải free spin => disable
                // if (listBetRoom.Count > 0 && agPlayer < totalListBetRoom[currentMarkBet] && !isFreeSpin)
                // {
                //     buttonSpinAnimation.color = Color.gray;
                // }
             
                break;
        }

        buttonSpinAnimation.Initialize(true);
    }
    
    private void SetSpinAnimation(SpinType type)
    {
        buttonSpinAnimation.startingAnimation = type switch
        {
            SpinType.NORMAL => "autospin",
            SpinType.FREE_NORMAL => "freespin",
            SpinType.AUTO or SpinType.FREE_AUTO => "stop",
            _ => "autospin"
        };
    }
}
