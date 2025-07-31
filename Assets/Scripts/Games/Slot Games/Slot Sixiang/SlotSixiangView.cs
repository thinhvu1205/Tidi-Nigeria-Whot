using System;
using System.Collections;
using System.Collections.Generic;
using Proto;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Games.Slot_Games;
using Games.Slot_Games.Slot_Sixiang;
using Globals;
using Google.Protobuf;
using Nakama;
using Newtonsoft.Json.Linq;
using Spine.Unity;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;

public class SlotSixiangView : BaseSlotXisiang
{
    public enum GAME_TYPE
    {
        NORMAL, SCATTER, DRAGON_PEARL, LUCKY_DRAW, LUCKY_GOLD, RAPID_PAY, BONUS_GAME
    }
    
    protected override Dictionary<SiXiangSymbol, int> SymbolDictionary => new()
    {
        { SiXiangSymbol._10, 0 },
        { SiXiangSymbol.J, 1 },
        { SiXiangSymbol.Q, 2 },
        { SiXiangSymbol.K, 3 },
        { SiXiangSymbol.A, 4 },
        { SiXiangSymbol.BlueDragon, 5 },
        { SiXiangSymbol.WhiteTiger, 6 },
        { SiXiangSymbol.Warrior, 7 },
        { SiXiangSymbol.VermilionBird, 8 },
        { SiXiangSymbol.Scatter, 9 },
        { SiXiangSymbol.Wild, 10 },

    };
    protected override  List<int[]> PaylineIdList => new List<int[]>
    {
        new int[] {1, 1, 1, 1, 1},
        new int[] {0, 0, 0, 0, 0},
        new int[] {2, 2, 2, 2, 2},
        new int[] {2, 1, 0, 1, 2},
        new int[] {0, 1, 2, 1, 0},
        new int[] {1, 2, 1, 0, 1},
        new int[] {1, 2, 1, 2, 1},
        new int[] {0, 1, 0, 1, 2},
        new int[] {2, 1, 2, 1, 0},
        new int[] {2, 1, 0, 1, 0},
        new int[] {0, 1, 2, 1, 2},
        new int[] {1, 0, 1, 2, 1},
        new int[] {2, 1, 2, 1, 2},
        new int[] {1, 0, 1, 0, 1},
        new int[] {0, 1, 0, 1, 0},
        new int[] {1, 1, 2, 1, 1},
        new int[] {0, 0, 1, 0, 0},
        new int[] {2, 2, 1, 2, 2},
        new int[] {1, 1, 0, 1, 1},
        new int[] {0, 0, 2, 0, 0},
        new int[] {2, 2, 0, 2, 2},
        new int[] {1, 1, 1, 2, 1},
        new int[] {0, 0, 0, 1, 2},
        new int[] {2, 2, 2, 1, 0},
        new int[] {2, 1, 1, 1, 0}
    };
    
    public static SlotSixiangView Instance;
    public List<int> bonusGame = new();
    public int bonusGameMultiplier = 1;
    [SerializeField] private List<Button> btnBuyPeals = new();
    [SerializeField] private List<Sprite> sprBgGame = new(); //0- normal ,1 inMiniGame
    [SerializeField] private List<Image> sprPeals = new();
    [SerializeField] private List<Material> materialPeals = new();
    [SerializeField] private GameObject bgQuay, bgGameGoldPick;
    [SerializeField] private SkeletonGraphic animCutScene, bgGame, animNameGame, animAnimal;
    [SerializeField] private SkeletonDataAsset animBgNormal;
    [SerializeField] private SiXiangDragonPearlView DragonPearlView;
    [SerializeField] private SixiangChooseGameBonus ChooseGameBonus;

    [HideInInspector] public bool isBonusMiniGame = false, isBonusGame = false;

    // private List<Task> taskAtion = new();
    private List<JObject> listPearls = new();
    private List<int> bonusPrices = new();
    private SlotDesk dragonPearlSpin = new(), scatterSpinGame = null;
    private SiXiangScatterView ScatterView;
    private SiXiangRapidPayView RapidPayView;
    private SiXiangLuckyGoldView LuckyGoldView;
    private SiXiangLuckyDrawView LuckyDrawView;
    private SiXiangBuyPealsPopup BuyPealsPopup;
    private int scatterSpinGameType = -1;
    private const string PATH_ANIM_THANHLONG = "SiXiang/Spine/Animal/Dragon/skeleton_SkeletonData";
    private const string PATH_ANIM_CHUTUOC = "SiXiang/Spine/Animal/Turle/skeleton_SkeletonData";
    private const string PATH_ANIM_BACHHO = "SiXiang/Spine/Animal/Tiger/skeleton_SkeletonData";
    private const string PATH_ANIM_HUYENVU = "SiXiang/Spine/Animal/Phoenix/skeleton_SkeletonData";
    private const string PATH_ANIM_WINRESULT_DP = "SiXiang/Spine/BigWinGoldPick/skeleton_SkeletonData";
    
    public override void OnDestroy() {
        base.OnDestroy();
        Instance = null;
        //SocketSend.sendExitSlotSixiang(Globals.ACTION_SLOT_SIXIANG.exitGame);
        DOTween.Kill(transform);
    }

    protected override void Update()
    {
        if (gameType == (int)SiXiangGame.Goldpick || gameType == (int)SiXiangGame.Luckdraw)
        {
            if (isHoldSpin)
            {
                isHoldSpin = false;
            }
        }
        else
        {
            base.Update();
        }
    }

    protected override void Start()
    {
        base.Start();
        //handleGetInfo(JObject.Parse("{\"minor\":10,\"major\":20,\"mega\":30,\"grand\":150,\"payTable\":[[0.0,0.0,0.5,2.5,5.0],[0.0,0.0,0.5,2.5,5.0],[0.0,0.0,0.5,2.5,5.0],[0.0,0.0,0.5,2.5,5.0],[0.0,0.0,0.5,2.5,5.0],[0.0,0.0,2.0,10.0,20.0],[0.0,0.0,1.5,7.5,15.0],[0.0,0.0,1.2,6.0,12.0],[0.0,0.0,1.0,5.0,10.0],[0.0,0.0,0.0,0.0,0.0],[0.0,0.0,0.0,0.0,0.0]],\"betLevels\":[1,5,50,500,1000,2500,5000,10000,25000,50000,100000,200000,500000,1000000,5000000,10000000],\"bet\":2500,\"userAmount\":91338,\"gameType\":0,\"isBonusGame\":false,\"bonusGameMultiplier\":4.0,\"winAmount\":0,\"pearls\":[],\"numberOflistPearlss\":0,\"numberOfPick\":0,\"cards\":[],\"rewards\":[]}"));
        PATH_ANIM_SPECICAL_WIN = "GameView/SiXiang/Spine/FourWin/skeleton_SkeletonData";
        DOTween.Sequence().AppendInterval(0.5f).AppendCallback(() =>
        {
            // SocketSend.sendGetInfoSlotSixiang(ACTION_SLOT_SIXIANG.getInfo);
        }).SetTarget(transform);
        // Config.tableId = 23521;
    }

    protected override void Awake()
    {
        base.Awake();
        Instance = this;
        bgGame.skeletonDataAsset = animBgNormal;
        bgGame.Initialize(true);
        bgGame.AnimationState.SetAnimation(0, "animation", true);
        //        {
        //            "tableid": 26423,
        //  "curGameID": 8818,
        //  "evt": "JoinPacket",
        //  "event": "packetDetail",
        //  "packetData": "JoinPacket",
        //  "isSendData": false,
        //  "timestamp": 1699945222637
        //}
        // JObject dataJson = new();
        // dataJson["tableid"] = 26423;
        // dataJson["curGameID"] = 9011;
        // SocketIOManager.getInstance().emitSIOWithValue(dataJson, "JoinPacket", false);
    }
    
    public override void HandleUpdateTable(IMatchState matchState)
        {
            base.HandleUpdateTable(matchState);
            SlotDesk deskData = SlotDesk.Parser.ParseFrom(matchState.State);
            Debug.Log("Data slot sixiang : " + deskData.ToString());
            if (deskData.CurrentSixiangGame <= SiXiangGame.Sixangbonus && deskData.NextSixiangGame > SiXiangGame.Sixangbonus) {
                // this.slotSixiangPresenter.getMatchResult()
                return;
            }
            if (deskData.CurrentSixiangGame == SiXiangGame.Sixangbonus) {
                // if (!require('SlotSixiangScatterSpin').getInstance()) {
                //     this.showScatterSpinPrefab()
                // }
                // require('SlotSixiangScatterSpin').getInstance().handleData(deskData);
                return;
            }
            _ = HandleGetInfo(deskData);
            
            // this.getInfo({
            //     bet: Number(deskData.chipsMcb),
            //     betLevels: deskData.betLevels,
            //     gameType: deskData.currentSixiangGame,
            //     numberOfDragonPearlSpins: deskData.numSpinleft,
            //     matrix: deskData.matrix,
            //     userAmount: deskData.gameReward.balanceChipsWalletAfter,
            //     winAmount: deskData.gameReward.totalChipsWinByGame,
            //     bonusGameMultiplier: deskData.currentSixiangGame > Constants.SI_XIANG_GAME.SIXANGBONUS ? 4 : 1,
            //     isBonusGame: deskData.currentSixiangGame > Constants.SI_XIANG_GAME.SIXANGBONUS,
            //     ...deskData,
            // })
            
            switch (deskData.CurrentSixiangGame) {
                case SiXiangGame.SixangbonusDragonPearl:
                    // this.setMiniGameDragonPearl(data, data.winAmount, false);
                    break; // in game dragon pearl
                case SiXiangGame.SixangbonusLuckdraw:
                    // this.setMiniGameLuckyDraw(data);
                    break; // in game lucky draw    
                case SiXiangGame.SixangbonusGoldpick:
                    // this.setMiniGameGoldPick(data);
                    break; // in game gold pick
                case SiXiangGame.SixangbonusRapidpay:
                    // this.setMiniGameRapidPay(data);
                    break; // in game rapid pay
                case SiXiangGame.Bonus:
                    // this.setPopupBonusGame();
                    break;
            }
            
            if (deskData.CurrentSixiangGame != SiXiangGame.Normal)
            {
                return;
            }
           

            // int itemIndex = 0;
            if (deskData.IsFinishGame)
            {
                HandleNormalSpin(deskData);
            }
            
        }

    public override void HandleUpdateGameState(IMatchState matchState)
    {
        base.HandleUpdateGameState(matchState);
        UpdateGameState updateGameState = UpdateGameState.Parser.ParseFrom(matchState.State);
        Debug.Log("Update Game State "+ updateGameState.ToString());
    }
    
    public override void HandleNormalSpin(SlotDesk data)
    {
        //data = JObject.Parse("{\"winAmount\":7000000,\"userAmount\":115602618,\"isGrandJackpot\":false,\"isBonusMiniGame\":false,\"payLines\":[{\"winAmount\":50000,\"line\":[0,0,0,1,2],\"lineNumber\":0,\"symbol\":0,\"numberOfSymbols\":3},{\"winAmount\":50000,\"line\":[0,0,0,1,7],\"lineNumber\":11,\"symbol\":0,\"numberOfSymbols\":3},{\"winAmount\":50000,\"line\":[9,0,0,1,4],\"lineNumber\":12,\"symbol\":0,\"numberOfSymbols\":3}],\"reels\":[[0,0,9],[3,0,3],[4,0,4],[1,1,6],[7,2,4]],\"isSelectBonusGame\":false}");
        //data = JObject.Parse(SiXiangFakeData.Instance.getNormalScatter());
        base.HandleNormalSpin(data);
        isBonusMiniGame = data.NextSixiangGame != SiXiangGame.Normal;
        isGrandJackpot = false;
        if (isGrandJackpot)
        {
            winTypeJackpot = WIN_JACKPOT_TYPE.JACKPOT_GRAND;
        }

        if (isBonusMiniGame)
        {
            // gameType = (int)GAME_TYPE.SCATTER;
        }

        winAmount = data.GameReward.TotalChipsWinByGame;
        getNormalWinAmount();
        setWinType(normalWinAmount);
    }

    private void getNormalWinAmount()
    {
        //for (int i = 0, l = spinPayLines.Count; i < l; i++)
        //{
        //    JObject dataLine = spinPayLines[i];
        //    normalWinAmount += (long)dataLine["winAmount"];
        //}
        normalWinAmount = winAmount >= validBetLevels[currentBetLevel] ? validBetLevels[currentBetLevel] + winAmount : winAmount;
        normalWinAmount = validBetLevels[currentBetLevel] + winAmount;
    }

    public void handleRapidPay(JObject data)
    {
        RapidPayView.setResult(data);
    }

    public void handleGoldPick(JObject data)
    {
        LuckyGoldView.setResult(data);
    }

    public void handleLuckyDraw(JObject data)
    {
        LuckyDrawView.setInfoItem(data);
    }

    public async UniTask handleSelectBonusGame(SlotDesk data)
    {
        //Debug.Log
        ChooseGameBonus.onClose();
        gameType = (int)data.CurrentSixiangGame;
        isBonusGame = true;
        gameState = GAME_STATE.SHOWING_RESULT;
        switch (gameType)
        {
            case (int)SiXiangGame.DragonPearl:
                await showSpineAnimalBuy(PATH_ANIM_THANHLONG);
                await showAnimCutScene();
                await showDragonPearlView(data, true);
                break;
            case (int)SiXiangGame.Luckdraw:
            {
                await showSpineAnimalBuy(PATH_ANIM_CHUTUOC);
                await showAnimCutScene();
                await showLuckyDrawView();
                break;
            }
            case (int)SiXiangGame.Rapidpay:
            {
                await showSpineAnimalBuy(PATH_ANIM_HUYENVU);
                await showAnimCutScene();
                await showRapidPayGame(initWinAmount: validBetLevels[currentBetLevel] / 2, isUltimate: true);
                break;
            }
            case (int)SiXiangGame.Goldpick:
            {
                await showSpineAnimalBuy(PATH_ANIM_BACHHO);
                await showAnimCutScene();
                await showLuckyGoldView();
                break;
            }
        }

        infoBar.setStateWin("totalWin");
        lbChipWins.SetValue(0, false);
    }

    public void handleDragonPealsSpin(JObject data)
    {
        // dragonPearlSpin = data;
        //dragonPearlSpin = JObject.Parse(SiXiangFakeData.Instance.DragonPearlSpinBug1);
        listPearls = data["pearls"].ToObject<List<JObject>>();
        // freeSpinleft = getInt(data, "totalSpins");
        // isGrandJackpot = getBool(data, "isGrandJackpot");
        spinReelView.Clear();
        for (int i = 0; i < 5; i++)
        {
            List<int> listFakeID = new() { Random.Range(0, 10), Random.Range(0, 10), Random.Range(0, 10) };
            spinReelView.Add(listFakeID);
        }

        for (int i = 0, l = listPearls.Count; i < l; i++)
        {
            // JObject dataSymbol = listPearls[i];
            // int col = getInt(dataSymbol, "col");
            // int row = getInt(dataSymbol, "row");
            //
            // if (getInt(dataSymbol, "item") == 1) //li xi
            // {
            //     spinReelView[col][row] = 13;
            // }
            // else //dong vang
            // {
            //     if (!(bool)dataSymbol["isBonusSpin"])
            //     {
            //         spinReelView[col][row] = 11;
            //     }
            // }
        }

        handleStopSpin();
    }

    public override async UniTask HandleGetInfo(SlotDesk data)
    {
        //data = JObject.Parse(SiXiangFakeData.Instance.getInfoDragonPearl);
        await base.HandleGetInfo(data);
        // SocketSend.sendPackageSlotSixiang(Globals.ACTION_SLOT_SIXIANG.getBonusGames, getInt(data, "bet"));
        gameType = (int)data.CurrentSixiangGame;
        winAmount = data.GameReward.TotalChipsWinByGame;
        bonusGameMultiplier = data.CurrentSixiangGame > SiXiangGame.Sixangbonus ? 4 : 1;
        isBonusGame = data.CurrentSixiangGame > SiXiangGame.Sixangbonus;
        if (isBonusGame)
        {
            winAmount *= bonusGameMultiplier;
        }
        
        lbChipWins.Text = Utility.FormatNumber(winAmount);
        if (gameType != (int)SiXiangGame.Normal /*&& gameType != (int)GAME_TYPE.SCATTER*/)
        {
            infoBar.setStateWin("totalWin");
        }

        switch (gameType)
        {
            case (int)SiXiangGame.Normal:
                break;
            /*case (int)GAME_TYPE.SCATTER:
                await showScatterSpin();
                break;*/
            case (int)SiXiangGame.DragonPearl:
            {
                freeSpinleft = (int) data.NumSpinLeft;
                infoBar.setDPSpinLeft(freeSpinleft);
                await showDragonPearlView(data);
                break;
            }
            case (int)SiXiangGame.Goldpick:
            {
                // await showLuckyGoldView(getInt(data, "numberOfPick"));
                setStateButtonBuyPeals(true);
                gameType = 0;
                break;
            }
            case (int)SiXiangGame.Rapidpay:
            {
                await showAnimCutScene();
                // List<JObject> rewards = data["rewards"].ToObject<List<JObject>>();
                // await showRapidPayGame((int)data.GameReward.TotalChipsWinByGame, rewards, isBonusGame);
                break;
            }
            case (int)SiXiangGame.Luckdraw:
            {
                await showAnimCutScene();
                // await showLuckyDrawView(data);
                updateWinAmount(winAmount);
                gameType = 0;
                break;
            }
            case (int)SiXiangGame.Sixangbonus:
            {
                await showAnimCutScene();
                showChooseGameBonus();
                break;
            }
        }
    }

    public async UniTask showMiniGameAfterSpinScatter(JObject data, int typeMiniGame)
    {
        infoBar.setStateWin("totalWin");
        lbChipWins.SetValue(0, false);
        if (typeMiniGame == 0) //DRAGON Pearl
        {
            await showDragonPearlView(scatterSpinGame, true, false);
            // infoBar.setDPSpinLeft(getInt(data, "numberOfDragonPearlSpins"));
            // freeSpinleft = getInt(data, "numberOfDragonPearlSpins");
            setStateBtnSpin();
        }
        else if (typeMiniGame == 3) //rapid Pay
        {
            await showRapidPayGame(validBetLevels[currentBetLevel] / 2);
        }
        else if (typeMiniGame == 2)
        {
            await showLuckyGoldView();
        }
        else if (typeMiniGame == 1)
        {
            await showLuckyDrawView();
        }

        scatterSpinGame = null;
    }

    public override void handleBonusInfo(JObject data)
    {
        // {\"bonusGames\":[2],\"prices\":[4500,5500,7500,12500],\"bet\":50}
        bonusGame = data["bonusGames"].ToObject<List<int>>();
        if (bonusGame.Count == 4)
        {
            isBonusGame = true;
        }

        bonusPrices = data["prices"].ToObject<List<int>>();

        sprPeals.ForEach(spr => spr.material = materialPeals[1]);
        btnBuyPeals.ForEach(btn =>
        {
            DOTween.Kill(btn.transform);
            btn.transform.DOLocalMoveX(82, 0.5f).SetEase(Ease.InSine)
                .OnComplete(() => { btn.gameObject.SetActive(true); });
        });
        bonusGame.ForEach((bonus) =>
        {
            int indexPeal = getIndexPeals(bonus);
            Button btn = btnBuyPeals[indexPeal];
            DOTween.Kill(btn.transform);
            sprPeals[indexPeal].material = materialPeals[0];
            btn.transform.DOLocalMoveX(-5, 0.5f).SetEase(Ease.OutSine)
                .OnComplete(() => { btn.gameObject.SetActive(false); });
        });
        if (gameType != 0) setStateButtonBuyPeals(false);
    }

    private int getIndexPeals(int bonusIndex)
    {
        int indexPeal = 0;
        switch (bonusIndex)
        {
            case 2:
            {
                indexPeal = 0;
                break;
            }
            case 3:
            {
                indexPeal = 3;
                break;
            }
            case 4:
            {
                indexPeal = 1;
                break;
            }
            case 5:
            {
                indexPeal = 2;
                break;
            }
        }

        return indexPeal;
    }

    private async UniTask checkWildSpread()
    {
        List<UniTask> tasks = new();
        bool isHasWild = false;
        listCollum.ForEach((col) =>
        {
            SiXiangCollumController collum = (SiXiangCollumController)col;
            tasks.Add(collum.checkWildSpread());
            if (collum.checkWildSymbol())
            {
                isHasWild = true;
            }
        });
        if (isHasWild)
        {
            // SoundManager.instance.playEffectFromPath(SOUND_SLOT_BASE.WILD_SYMBOL);
            DOTween.Sequence().AppendInterval(2.0f).AppendCallback(() =>
            {
                // SoundManager.instance.playEffectFromPath(SOUND_SLOT_BASE.WILD_EXPAND);
            });
        }

        await UniTask.WhenAll(tasks);
    }

    private void setStateButtonBuyPeals(bool state)
    {
        if (!state)
        {
            for (int i = 0; i < btnBuyPeals.Count; i++)
            {
                Button btn = btnBuyPeals[i];
                DOTween.Kill(btn.transform);
                btn.transform.DOLocalMoveX(-5, 0.3f).SetEase(Ease.OutSine)
                    .OnComplete(() => { btn.gameObject.SetActive(false); });
            }
        }
        else //phai check nhung thang nao dang duoc active de bam mua duoc.
        {
            List<int> listIndexPealActive = new();
            bonusGame.ForEach((bonus) => { listIndexPealActive.Add(getIndexPeals(bonus)); });
            for (int i = 0; i < btnBuyPeals.Count; i++)
            {
                Button btn = btnBuyPeals[i];
                if (listIndexPealActive.Contains(i)) //an di
                    btn.transform.DOLocalMoveX(-5, 0.3f).SetEase(Ease.OutSine).OnComplete(() =>
                    {
                        btn.gameObject.SetActive(false);
                    });
                else
                    btn.transform.DOLocalMoveX(75, 0.3f).SetEase(Ease.OutBack).SetDelay(0.05f * i)
                        .OnComplete(() => { btn.gameObject.SetActive(true); });
            }
        }
    }

    public override void onClickSpin()
    {
        base.onClickSpin();
        setStateButtonBuyPeals(false);
        if (gameType != (int)SiXiangGame.DragonPearl && isBonusMiniGame == false)
        {
            InfoBet infoBet = new()
            {
                Chips = validBetLevels[currentBetLevel],
            };
            DataSender.SendMatchState((long)OpCodeRequest.Spin, infoBet.ToByteArray());
            // SocketSend.sendPackageSlotSixiang(Globals.ACTION_SLOT_SIXIANG.normalSpin, validBetLevels[currentBetLevel]);
            if (autoSpinRemain > 0)
            {
                autoSpinRemain--;
            }
            else
            {
                spintype = SPIN_TYPE.NORMAL;
            }

            setAutoSpinRemain();
            setStateSpin(GAME_STATE.SPINNING);
            setStateBtnSpin();
        }
        else if (gameType == (int)SiXiangGame.DragonPearl && spintype == SPIN_TYPE.NORMAL)
        {
            onClickSpinDP();
        }
    }

    public void onClickSpinDP()
    {
        if (!_CanSpinDP)
            return; // code cũ đang xảy ra lỗi có lúc bấm 2 lần spin này nên kiểm soát thêm biến _CanSpinDP để tránh
        _CanSpinDP = false;
        spintype = SPIN_TYPE.AUTO;
        lbAutoRemain.gameObject.SetActive(false);
        infoBar.setStateWin("totalWin");
        setStateSpin(GAME_STATE.SPINNING);
        setStateBtnSpin();
        if (freeSpinleft > 0)
        {
            freeSpinleft--;
        }

        infoBar.setDPSpinLeft(freeSpinleft);
        hideAllSymbol();
        // SocketSend.sendGetInfoSlotSixiang(Globals.ACTION_SLOT_SIXIANG.dragonPearlSpin);
        startSpin();
    }

    public override void onSpinTriggerUp()
    {
        if (gameType == (int)SiXiangGame.Goldpick|| gameType == (int)SiXiangGame.Luckdraw)
        {
        }
        else if (gameType == (int)SiXiangGame.DragonPearl)
        {
            Debug.Log("onSpinTriggerUp DP --> Game State==" + gameState + ", " + spintype + ", " + _CanSpinDP);
            if (gameState != GAME_STATE.SPINNING && gameState != GAME_STATE.SHOWING_RESULT &&
                spintype == SPIN_TYPE.NORMAL)
            {
                onClickSpinDP();
            }

            isHoldSpin = false;
            timeHoldSpin = 0;
        }
        else
        {
            base.onSpinTriggerUp();
        }
    }

    public void onClickBuyPeals(int index)
    {
        Debug.Log("onClickBuyPeals:" + gameState);
        if (gameState == GAME_STATE.SHOWING_RESULT)
        {
            return;
        }

        if (nodeAutoSpin.gameObject.activeSelf)
        {
            onHideNodeAuto();
        }

        int price = bonusPrices[bonusGame.Count];
        showPopupBuyPeals(index, price);
    }

    private void showPopupBuyPeals(int index, int price)
    {
        if (BuyPealsPopup == null)
        {
            // BuyPealsPopup =
            //     Instantiate(UIManager.instance.loadPrefab("GameView/SiXiang/Prefab/BuyPealsPopup"), transform)
            //         .GetComponent<SiXiangBuyPealsPopup>();
        }

        BuyPealsPopup.setInfo(index, price, validBetLevels[currentBetLevel], agPlayer);
    }

    public void showChooseGameBonus()
    {
        ChooseGameBonus.gameObject.SetActive(true);
    }

    public async void handleScatterSpin(JObject data)
    {
        await ScatterView.handleScatterSpin(data);
        // scatterSpinGame = data;
        // scatterSpinGameType = getInt(data, "reward");
        // await showMiniGameAfterSpinScatter(scatterSpinGame, scatterSpinGameType);
    }

    private async void allCollumStopDP()
    {
        //Debug.Log("allCollumStopDP");
        hideAllSymbol();
        gameState = GAME_STATE.SHOWING_RESULT;
        await DragonPearlView.setInfo(dragonPearlSpin, false, true);
        resetSlotViewDP();
    }

    public override async UniTask allCollumStopCompleted()
    {
        await base.allCollumStopCompleted();
        if (gameType == (int)SiXiangGame.DragonPearl)
        {
            allCollumStopDP();
        }
        else
        {
            Debug.Log("allCollumStopCompleted:isGrandJackpot=" + isGrandJackpot);
            if (gameType == (int)SiXiangGame.Normal)
            {
                try
                {
                    await checkScatterItem();
                    await checkWildSpread();
                    if (isGrandJackpot)
                    {
                        await showSpineJackpotWin((long)(validBetLevels[currentBetLevel] * jackpotLevel[3]));
                    }
                    else
                    {
                        await showSpineSpecialWin(winType, normalWinAmount);
                    }

                    await showWinLine();
                }
                catch (OperationCanceledException e)
                {
                    Debug.Log($"{nameof(OperationCanceledException)} thrown with message: {e.Message}");
                }
            }

            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
            setStateButtonBuyPeals(true);
            bool isWaittChipEff = (winAmount > 0 || normalWinAmount > 0);
            resetSlotView();
            if (isBonusMiniGame)
            {
                setStateSpin(GAME_STATE.SHOWING_RESULT);
                if (isWaittChipEff)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(1.0));
                }

                isBonusMiniGame = false;
                setStateSpin(GAME_STATE.SHOWING_RESULT);
                await showScatterSpin();
            }
        }
    }

    public async UniTask endMinigame(JObject data)
    {
        setAnimGameName("sixiang");
        // if (data.ContainsKey("gameType"))
        // {
        //     gameType = (int)data["gameType"];
        // }

        if ((int)data["gameType"] == (int)GAME_TYPE.DRAGON_PEARL)
        {
            isGrandJackpot = (bool)data["isGrandJackpot"];
            DragonPearlView.gameObject.SetActive(false);
            activeAllSymbol();
            infoBar.prepareSpin();
        }

        if (isGrandJackpot)
        {
            winTypeJackpot = WIN_JACKPOT_TYPE.JACKPOT_GRAND;
        }

        lbChipWins.ResetValue();
        // winAmount = getLong(data, "winAmount");

        lbChipWins.SetValue(winAmount, true);
        normalWinAmount = winAmount;
        infoBar.setStateWin("totalWin");

        setWinType(winAmount);
        setStateButtonBuyPeals(true);
        if (gameType == (int)SiXiangGame.DragonPearl)
        {
            // SoundManager.instance.playMusicInGame(Globals.SOUND_SLOT_BASE.BG_GAME);
            if (isGrandJackpot)
            {
                await showSpineJackpotWin(getJackpotValue(winTypeJackpot));
                await UniTask.Yield();
            }

            await showResultMoneyAnim(PATH_ANIM_WINRESULT_DP, "eng", winAmount, new Vector2(0, -68));
            activeAllSymbol();
        }
        else
        {
            if (isGrandJackpot)
            {
                await showSpineJackpotWin(getJackpotValue(winTypeJackpot));
            }
        }

        Debug.Log("wait show specialWin");
        await showSpineSpecialWin(winType, winAmount);
        Debug.Log("endMinigame after special spine:gameState=" + gameState);
        //if (gameType != (int)GAME_TYPE.DRAGON_PEARL)
        //{
        showEffectChip();
        //}
        bgGame.skeletonDataAsset = animBgNormal;
        bgGame.Initialize(true);
        bgGame.AnimationState.SetAnimation(0, "animation", true);
        // isBonusGame = getBool(data, "isSelectBonusGame");
        if (isBonusGame)
        {
            showChooseGameBonus();
        }

        gameType = 0; //set gametype ve 0 roi
        // agPlayer = getLong(data, "userAmount");
        // SocketSend.sendPackageSlotSixiang(ACTION_SLOT_SIXIANG.getBonusGames, validBetLevels[currentBetLevel]);
        setBetLevel(validBetLevels[currentBetLevel]);
        if (autoSpinRemain > 0)
        {
            spintype = SPIN_TYPE.AUTO;
        }
        else
        {
            spintype = SPIN_TYPE.NORMAL;
        }

        setStateBtnSpin();
        await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
        isGrandJackpot = false;
        winAmount = 0;
        if (autoSpinRemain > 0 && isBonusGame == false)
        {
            lbAutoRemain.gameObject.SetActive(true);
            onClickSpin();
        }
        else
        {
            setStateSpin(GAME_STATE.PREPARE);
        }

        Debug.Log("endMinigame:gameState=" + gameState);
    }

    private void resetSlotViewDP()
    {
        infoBar.prepareSpin();
        listCollum.ForEach((col) => { col.Reset(); });
        setBetLevel(validBetLevels[currentBetLevel]);
        infoBar.setDPSpinLeft(freeSpinleft);
        if (DragonPearlView.isFinish == true)
        {
            infoBar.prepareSpin();
            DragonPearlView.isFinish = false;
            gameType = (int)SiXiangGame.Normal;
            activeAllSymbol();
            freeSpinleft = 0;
            if (autoSpinRemain == 0)
            {
                spintype = SPIN_TYPE.NORMAL;
            }
        }
        else
        {
            setStateButtonBuyPeals(false);
            hideAllSymbol();
        }

        if (freeSpinleft <= 0)
        {
            infoBar.prepareSpin();
            setStateBtnSpin();
        }

        if (spintype == SPIN_TYPE.AUTO && freeSpinleft > 0)
        {
            onClickSpinDP();
        }

        Debug.Log("resetSlotViewDP:gameState=" + gameState + "--SpintType=" + spintype);
    }

    protected override void resetSlotView()
    {
        base.resetSlotView();

        if (gameType == (int)SiXiangGame.DragonPearl /*|| gameType == (int)GAME_TYPE.SCATTER*/)
        {
            setStateButtonBuyPeals(false);
        }

        if ((winAmount > 0 || normalWinAmount > 0) &&
            (gameType == (int)GAME_TYPE.NORMAL /*|| gameType == (int)GAME_TYPE.SCATTER*/))  
        {
            lbChipWins.SetValue(normalWinAmount, true);
            infoBar.setStateWin("win");
            showEffectChip();
            normalWinAmount = 0;
        }

        if (gameType != (int)SiXiangGame.DragonPearl)
        {
            gameType = (int)SiXiangGame.Normal;
            setStateButtonBuyPeals(true);
        }

        if (autoSpinRemain > 0)
        {
            if ((gameState == GAME_STATE.PREPARE || gameState == GAME_STATE.JOIN_GAME) &&
                agPlayer < validBetLevels[currentBetLevel])
            {
                // infoBar.setInfoText(Globals.Config.getTextConfig("not_enought_gold"));
                // string textShow = Config.getTextConfig("txt_not_enough_money_gl");
                // string textBtn2 = Config.getTextConfig("shop");
                // string textBtn3 = Config.getTextConfig("label_cancel");
                // UIManager.instance.showDialog(textShow, textBtn2, () =>
                // {
                //     UIManager.instance.openShop();
                // }, textBtn3);

                // string textShow = Config.getTextConfig("txt_not_enough_money_gl");
                // string textBtn3 = Config.getTextConfig("label_cancel");
                // UIManager.instance.showDialog(textShow, textBtn3);
            }
            else
            {
                if (gameType == (int)GAME_TYPE.NORMAL && isBonusMiniGame == false)
                {
                    lbAutoRemain.gameObject.SetActive(true);
                    onClickSpin();
                }
                else
                {
                    lbAutoRemain.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            lbAutoRemain.gameObject.SetActive(false);
            spintype = SPIN_TYPE.NORMAL;
            setStateBtnSpin();
        }

        winAmount = 0;
        isGrandJackpot = false;
    }

    protected virtual async UniTask showLuckyGoldView(int remainPick = 20)
    {
        gameType = (int)SiXiangGame.Goldpick;
        setStateNodeGameForLuckyGold(false);
        if (LuckyGoldView == null)
        {
            // LuckyGoldView =
            //     Instantiate(UIManager.instance.loadPrefab("GameView/SiXiang/Prefab/LuckyGoldView"), transform)
            //         .GetComponent<SiXiangLuckyGoldView>();
            // LuckyGoldView.transform.SetSiblingIndex(animCutScene.transform.GetSiblingIndex() - 3);
        }

        LuckyGoldView.remainPick = remainPick;
        LuckyGoldView.gameObject.SetActive(true);
        infoBar.setStateWin("totalWin");
        lbChipWins.ResetValue();
        await LuckyGoldView.Show(this);
        LuckyGoldView.transform.SetSiblingIndex(LuckyGoldView.transform.GetSiblingIndex() + 2);
    }

    public void setStateNodeGameForLuckyGold(bool isShow)
    {
        //Assets/Resources/GameView/SiXiang/Spine/BgMiniGame/skeleton_SkeletonData.asset
        bgGameGoldPick.SetActive(!isShow);
        bgGame.gameObject.SetActive(isShow);
        bgQuay.SetActive(isShow);
    }

    public void setStateNodeGameForLuckyDraw(bool isShow)
    {
        if (!isShow)
        {
            // bgGame.skeletonDataAsset =
            //     UIManager.instance.loadSkeletonData("GameView/SiXiang/Spine/LuckyDraw/BgGame/skeleton_SkeletonData");
            // bgGame.Initialize(true);
            // bgGame.AnimationState.SetAnimation(0, "animation", true);
        }
        else
        {
            bgGame.skeletonDataAsset = animBgNormal;
            bgGame.Initialize(true);
            bgGame.AnimationState.SetAnimation(0, "animation", true);
        }

        collumContainer.SetActive(isShow);
    }

    protected void setAnimGameName(string gameName)
    {
        animNameGame.Initialize(true);
        animNameGame.AnimationState.SetAnimation(0, gameName, true);
    }

    protected virtual async UniTask showScatterSpin()
    {
        await showAnimCutScene();
        if (ScatterView == null)
        {
            // ScatterView = Instantiate(UIManager.instance.loadPrefab("GameView/SiXiang/Prefab/ScatterView"), transform)
                // .GetComponent<SiXiangScatterView>();
        }

        ScatterView.transform.SetSiblingIndex(animCutScene.transform.GetSiblingIndex() - 1);
        ScatterView.Show(this);
    }

    protected async UniTask showDragonPearlView(SlotDesk pearls, bool isInit6Gold = false, bool isDbSpin = false)
    {
        // SoundManager.instance.playMusicInGame(Globals.SOUND_SLOT_BASE.PEARL_BG);
        setAnimGameName("dragonpearl");
        hideAllSymbol();
        setStateButtonBuyPeals(false);
        gameType = (int)SiXiangGame.DragonPearl;
        // bgGame.skeletonDataAsset =
        //     UIManager.instance.loadSkeletonData("GameView/SiXiang/Spine/DragonPearl/BgGame/skeleton_SkeletonData");
        bgGame.Initialize(true);
        bgGame.AnimationState.SetAnimation(0, "animation", true);
        DragonPearlView.gameObject.SetActive(true);
        if (isInit6Gold)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1));
        }

        infoBar.setStateWin("totalWin");
        lbChipWins.SetValue(0, false);
        // freeSpinleft = getInt(pearls, "numberOfDragonPearlSpins");
        setStateBtnSpin();

        await DragonPearlView.setInfo(pearls, isInit6Gold, isDbSpin);
    }

    protected virtual async UniTask showRapidPayGame(int initWinAmount, List<JObject> data = null,
        bool isUltimate = false)
    {
        gameType = (int)SiXiangGame.Rapidpay;
        if (RapidPayView == null)
        {
            // RapidPayView = Instantiate(UIManager.instance.loadPrefab("GameView/SiXiang/Prefab/RapidPayView"), transform)
                // .GetComponent<SiXiangRapidPayView>();
            RapidPayView.transform.SetSiblingIndex(animCutScene.transform.GetSiblingIndex() - 2);
        }

        RapidPayView.winAmount = initWinAmount;
        await RapidPayView.Show(this, isUltimate, data);
    }

    protected virtual async UniTask showLuckyDrawView(JObject initView = null)
    {
        setAnimGameName("luckydraw");
        gameType = (int)SiXiangGame.Luckdraw;
        setStateNodeGameForLuckyDraw(false);
        setStateButtonBuyPeals(false);
        if (LuckyDrawView == null)
        {
            // LuckyDrawView =
                // Instantiate(UIManager.instance.loadPrefab("GameView/SiXiang/Prefab/LuckyDrawView"), transform)
                    // .GetComponent<SiXiangLuckyDrawView>();
            LuckyDrawView.transform.SetSiblingIndex(animCutScene.transform.GetSiblingIndex() - 1);
        }

        if (initView != null)
        {
            LuckyDrawView.setInitView(initView, this);
        }

        await LuckyDrawView.Show(this);
    }

    public async UniTask showAnimCutScene()
    {
        // SoundManager.instance.playEffectFromPath(SOUND_SLOT_BASE.CUT_SCENE);
        // animCutScene.skeletonDataAsset =
            // UIManager.instance.loadSkeletonData("GameView/SiXiang/Spine/CutSceneBonus/skeleton_SkeletonData");
        animCutScene.Initialize(true);
        animCutScene.AnimationState.SetAnimation(0, "animation", false);
        animCutScene.transform.gameObject.SetActive(true);
        DOTween.Sequence()
            .AppendInterval(animCutScene.Skeleton.Data.FindAnimation("animation").Duration)
            .AppendCallback(() => { animCutScene.gameObject.SetActive(false); }).SetTarget(transform);
        await UniTask.Delay(1500);
    }


    public async void handleBuyBonusGame(JObject data)
    {
        //{\"userAmount\":411187,\"multiplier\":90,\"price\":90,\"miniGameType\":2,\"bet\":1,\"numberOflistPearlss\":3,\"dragonPearls\":[{\"item\":6,\"luckyMoney\":0,\"multiplier\":2.695,\"winAmount\":2,\"jackpot\":0,\"row\":1,\"col\":0,\"isBonusSpin\":false,\"isJackpot\":false,\"isDoubled\":false},{\"item\":2,\"luckyMoney\":0,\"multiplier\":0.044,\"winAmount\":0,\"jackpot\":0,\"row\":0,\"col\":2,\"isBonusSpin\":false,\"isJackpot\":false,\"isDoubled\":false},{\"item\":2,\"luckyMoney\":0,\"multiplier\":0.046,\"winAmount\":0,\"jackpot\":0,\"row\":1,\"col\":2,\"isBonusSpin\":false,\"isJackpot\":false,\"isDoubled\":false},{\"item\":5,\"luckyMoney\":0,\"multiplier\":1.5580001,\"winAmount\":1,\"jackpot\":0,\"row\":1,\"col\":4,\"isBonusSpin\":false,\"isJackpot\":false,\"isDoubled\":false},{\"item\":5,\"luckyMoney\":0,\"multiplier\":1.5580001,\"winAmount\":1,\"jackpot\":0,\"row\":2,\"col\":1,\"isBonusSpin\":false,\"isJackpot\":false,\"isDoubled\":false},{\"item\":4,\"luckyMoney\":0,\"multiplier\":0.30400002,\"winAmount\":0,\"jackpot\":0,\"row\":0,\"col\":0,\"isBonusSpin\":false,\"isJackpot\":false,\"isDoubled\":false}],\"dragonPearlWinPot\":6,\"numberOfGoldPicks\":0}
        // gameType = getInt(data, "miniGameType");
        string pathAnimGameMini = "";

        switch (gameType)
        {
            case (int)SiXiangGame.SixangbonusDragonPearl:
                pathAnimGameMini = PATH_ANIM_THANHLONG;
                gameState = GAME_STATE.SHOWING_RESULT;
                await showSpineAnimalBuy(pathAnimGameMini);
                await showAnimCutScene();
                handleBuyDragonPearl(data);
                break;
            case (int)SiXiangGame.SixangbonusLuckdraw:
                //{\"userAmount\":28925472,\"multiplier\":110.0,\"price\":110,\"miniGameType\":3,\"bet\":1,\"numberOfDragonPearlSpins\":0,\"dragonPearlWinPot\":0,\"numberOfGoldPicks\":0}
                pathAnimGameMini = PATH_ANIM_CHUTUOC;
                await showSpineAnimalBuy(pathAnimGameMini);
                await showAnimCutScene();
                await showLuckyDrawView();
                break;
            case (int)SiXiangGame.SixangbonusGoldpick:
                pathAnimGameMini = PATH_ANIM_BACHHO;

                await showSpineAnimalBuy(pathAnimGameMini, "3");
                await showAnimCutScene();
                await showLuckyGoldView();
                break;
            case (int)SiXiangGame.SixangbonusRapidpay:
                pathAnimGameMini = PATH_ANIM_HUYENVU;
                // "{\"userAmount\":28925139,\"multiplier\":250.0,\"price\":250,\"miniGameType\":5,\"bet\":1,\"numberOfDragonPearlSpins\":0,\"dragonPearlWinPot\":0,\"numberOfGoldPicks\":0}"
                await showSpineAnimalBuy(pathAnimGameMini);
                await showAnimCutScene();
                await showRapidPayGame(validBetLevels[currentBetLevel] / 2);
                break;
        }

        infoBar.setStateWin("totalWin");
        lbChipWins.SetValue(0, false);
    }

    private void handleBuyDragonPearl(JObject data)
    {
        freeSpinleft = (int)data["numberOfDragonPearlSpins"];
        infoBar.setDPSpinLeft(freeSpinleft);
        // showDragonPearlView(data, true);
    }

    protected async UniTask showSpineAnimalBuy(string pathAnim, string animName = "animation")
    {
        var spineSpecialWinTask = new UniTaskCompletionSource();
        Action<SkeletonDataAsset> cb = async (skeData) =>
        {
            // SoundManager.instance.playEffectFromPath(SOUND_SLOT_BASE.SHOW_ANIMAL);
            if (pathAnim == PATH_ANIM_BACHHO)
            {
                animName = "3";
            }

            Debug.Log("showSpineAnimalBuy:" + animName);
            animAnimal.skeletonDataAsset = skeData;
            await UniTask.Delay(TimeSpan.FromSeconds(0.1f));
            effectContainer.SetActive(true);
            spineBgMoney.gameObject.SetActive(false);
            animAnimal.gameObject.SetActive(true);
            animAnimal.Initialize(true);
            animAnimal.AnimationState.SetAnimation(0, animName, false);
            //animAnimal.transform.Find("btnConfirm").gameObject.SetActive(false);
            animAnimal.transform.parent.gameObject.SetActive(true);
            //lbSpecicalWin.gameObject.SetActive(false);
            await UniTask.Delay((int)animAnimal.Skeleton.Data.FindAnimation(animName).Duration * 1000);
            animAnimal.AnimationState.Complete += delegate
            {
                spineSpecialWinTask.TrySetResult();
                animAnimal.transform.parent.gameObject.SetActive(false);
                animAnimal.gameObject.SetActive(false);
                effectContainer.SetActive(false);
            };
        };
        // UnityMainThread.instance.AddJob(() =>
        // {
        //     StartCoroutine(UIManager.instance.loadSkeletonDataAsync(pathAnim, cb));
        // });
        await spineSpecialWinTask.Task;
    }

    public void updateFreeSpinLeft()
    {
        infoBar.setDPSpinLeft(freeSpinleft);
        infoBar.effectUpdateDBFSL();
        setStateBtnSpin();
    }

    public void onClickTestData()
    {
        //handleBuyBonusGame(JObject.Parse(SiXiangFakeData.Instance.getBuyBonusDragonPearl));
        //handleDragonPealsSpin(JObject.Parse(SiXiangFakeData.Instance.getBachHoDragonPearl));
        //handleDragonPealsSpin(JObject.Parse(SiXiangFakeData.Instance.getChuTuocDP));
        //handleDragonPealsSpin(JObject.Parse(SiXiangFakeData.Instance.getHuyenVuDragonPearl()));
        // handleDragonPealsSpin(JObject.Parse(SiXiangFakeData.Instance.getDragonPearlFinishedSpin()));
    }

    public long getJackpotValue(WIN_JACKPOT_TYPE type, int typeNumber = -1)
    {
        if (typeNumber != -1)
        {
            return validBetLevels[currentBetLevel] * jackpotLevel[typeNumber - 1];
        }
        else
        {
            return validBetLevels[currentBetLevel] * jackpotLevel[(int)type - 1];
        }
    }

    public override void setStateBtnSpin()
    {
        base.setStateBtnSpin();
        animBtnSpin.Initialize(true);
        if (spintype == SPIN_TYPE.NORMAL)
        {
            if (freeSpinleft > 0)
            {
                animBtnSpin.AnimationState.SetAnimation(0, "freespin", true);
                animBtnSpin.color = Color.white;
            }
            else
            {
                animBtnSpin.AnimationState.SetAnimation(0, "spin", true);
            }
        }
        else if (spintype == SPIN_TYPE.AUTO)
        {
            animBtnSpin.AnimationState.SetAnimation(0, "stop", true);
            if (freeSpinleft > 0)
            {
                animBtnSpin.AnimationState.SetAnimation(0, "freespin", true);
                animBtnSpin.color = Color.white;
            }
        }
    }
    
    protected  void SetSpinAnimation(SpinType type)
    {
        // buttonSpinAnimation.startingAnimation = type switch
        // {
        //     SpinType.NORMAL => "spin",
        //     SpinType.FREE_NORMAL or SpinType.FREE_AUTO => "freespin",
        //     SpinType.AUTO => "stop",
        //     _ => "autospin"
        // };
    }
}
