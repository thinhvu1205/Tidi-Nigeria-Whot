namespace Globals
{
    public class CurrentView
    {
        public const string
            LOGIN_VIEW = "LOGIN_VIEW",
            LOBBY_VIEW = "LOBBY_VIEW",
            PAYMENT_VIEW = "PAYMENT_VIEW",
            MAIL_VIEW = "MAIL_VIEW",
            LEADER_BOARD_VIEW = "LEADER_BOARD_VIEW",
            PERSONAL = "PERSONAL",
            CHAT_FRIEND = "CHAT_FRIEND",
            RULE_VIEW = "RULE_VIEW",
            GAMELIST_VIEW = "GAMELIST_VIEW",
            FEEDBACK_VIEW = "FEEDBACK_VIEW",
            NEWS_VIEW = "NEWS_VIEW",
            SETTING_VIEW = "SETTING_VIEW",
            JACKPOT_VIEW = "JACKPOT_VIEW",
            GUIDE_INGAME = "GUIDE_INGAME",
            COUNTDOWN = "COUNTDOWN",
            REGISTER_VIEW = "REGISTER_VIEW",
            RANK_VIEW = "RANK_VIEW",
            INVITE_FRIEND_VIEW = "INVITE_FRIEND_VIEW",
            INVITE_PLAYERVIEW = "INVITE_PLAYERVIEW",
            DT_VIEW = "DT_VIEW",
            KET_VIEW = "KET_VIEW",
            CHATWORLD = "CHATWORLD",
            TOP_VIEW = "TOP_VIEW",
            FRIEND_VIEW = "FRIEND_VIEW",
            INFO_FRIEND_VIEW = "INFO_FRIEND_VIEW",
            CREATE_TABLE_GAME = "CREATE_TABLE_GAME",
            GIFT_CODE_VIEW = "GIFT_CODE_VIEW",
            MISSION_VIEW = "MISSION_VIEW",
            GAME_VIEW = "GAME_VIEW",
            SPECIAL_OFFER = "SPECIAL_OFFER",
            SEND_GIFT_VIEW = "SEND_GIFT_VIEW",
            MAIL_CHIP_VIEW = "MAIL_CHIP_VIEW",
            FREECHIP_VIEW = "FREECHIP_VIEW",
            PROFILE_VIEW = "PROFILE_VIEW",
            TOPRICH_VIEW = "TOPRICH_VIEW",
            CHECKPASS_VIEW = "CHECKPASS_VIEW",
            INFO_PLAYER_VIEW = "INFO_PLAYER_VIEW",
            GROUP_OPTION_INGAME = "GROUP_OPTION_INGAME",
            LIST_PLAYER_VIEW = "LIST_PLAYER_VIEW",
            LOTO = "LOTO";
        
        public static string currentView = "";

        public static void SetCurrentView(string view)
        {
            currentView = view;
            // if (Config.isLoginSuccess)
            // {
            //     SocketIOManager.getInstance().emitUpdateInfo();
            // }
        }

        public static string GetCurrentSceneName()
        {
            string sceneName;
            if (currentView == Config.currentGameId.ToString())
            {
                sceneName = "GAMEVIEW_" + Config.currentGameId;
            }
            else
            {
                sceneName = currentView switch
                {
                    LOGIN_VIEW => "LOGINVIEW",
                    LOBBY_VIEW => "LOBBYVIEW",
                    PAYMENT_VIEW => "PAYMENTVIEW",
                    MAIL_VIEW => "MAILVIEW",
                    PERSONAL => "PERSONALVIEW",
                    CHAT_FRIEND => "CHATFRIENDVIEW",
                    RULE_VIEW => "RULEVIEW",
                    GAMELIST_VIEW => "GAMELISTVIEW",
                    FEEDBACK_VIEW => "FEEDBACKVIEW",
                    NEWS_VIEW => "NEWSVIEW",
                    SETTING_VIEW => "SETTINGVIEW",
                    JACKPOT_VIEW => "JACKPOTVIEW",
                    GUIDE_INGAME => "GUIDEINGAMEVIEW",
                    COUNTDOWN => "COUNTDOWNVIEW",
                    REGISTER_VIEW => "REGISTERVIEW",
                    RANK_VIEW => "RANKVIEW",
                    DT_VIEW => "DTVIEW",
                    KET_VIEW => "KETVIEW",
                    CHATWORLD => "CHATWORLDVIEW",
                    TOP_VIEW => "TOPVIEW",
                    FRIEND_VIEW => "FRIENDVIEW",
                    CREATE_TABLE_GAME => "CREATETABLEVIEW",
                    GIFT_CODE_VIEW => "GIFTCODEVIEW",
                    MISSION_VIEW => "MISSIONVIEW",
                    TOPRICH_VIEW => "TOPRICHVIEW",
                    SEND_GIFT_VIEW => "SENDGIFTVIEW",
                    MAIL_CHIP_VIEW => "MAILCHIPVIEW",
                    FREECHIP_VIEW => "FREECHIPVIEW",
                    PROFILE_VIEW => "PROFILEVIEW",
                    INFO_FRIEND_VIEW => "INFOFRIENDVIEW",
                    LOTO => "LOTO",
                    _ => ""
                };
                
            }
            return sceneName;
        }
    }
}