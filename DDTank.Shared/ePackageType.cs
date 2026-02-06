namespace DDTank.Shared
{
    public enum ePackageType
    {
        // Handshake & System
        RSAKEY = 7,
        PING = 4,
        SYS_MESSAGE = 3,
        SYS_NOTICE = 10,
        SYS_DATE = 5,
        PING_CENTER = 12, // Center use 12

        // Login & Session
        LOGIN = 1,
        KIT_USER = 2,
        SCENE_LOGIN = 16,
        GAME_VISITOR_DATA = 224,

        // Scenes & Chat
        SCENE_CHAT = 19,
        SCENE_FACE = 20,
        SCENE_ADD_USER = 18,
        SCENE_REMOVE_USER = 21,
        SCENE_USERS_LIST = 69,
        CHAT_PERSONAL = 37,
        IM_CMD = 160,
        
        // Game & Combat
        GAME_CMD = 91,
        GAME_ROOM = 94,
        GAME_INVITE = 70,
        GAME_TAKE_OUT = 106,
        GAME_TAKE_TEMP = 108,
        GAME_CHANGE_MAP = 104,
        GAME_CAPTAIN_CHOICE = 110,
        GAME_CAPTAIN_AFFIRM = 111,
        
        // Player & Items
        UPDATE_PLAYER_INFO = 67,
        UPDATE_GOODS = 51,
        DELETE_GOODS = 42,
        BUY_GOODS = 44,
        SELL_GOODS = 48,
        ITEM_EQUIP = 74,
        ITEM_STRENGTHEN = 59,
        ITEM_COMPOSE = 58,
        ITEM_TRANSFER = 61,
        ITEM_FUSION = 78,
        ITEM_INLAY = 121,
        PROP_USE = 66,
        
        // Mail
        SEND_MAIL = 116,
        UPDATE_MAIL = 114,
        UPDATE_NEW_MAIL = 115,
        GET_MAIL_ATTACHMENT = 113,
        DELETE_MAIL = 112,
        MAIL_RESPONSE = 117,

        // Consortia
        CONSORTIA_CMD = 129,
        CONSORTIA_RESPONSE = 128,

        // Marry
        MARRY_CMD = 249,
        MARRY_ROOM_CREATE = 241,
        MARRY_ROOM_LOGIN = 242,

        // Achievement & Quests
        QUEST_ADD = 176,
        QUEST_UPDATE = 178,
        QUEST_FINISH = 179,
        ACHIEVEMENT_INIT = 228,
        ACHIEVEMENT_FINISH = 230,

        // Other
        CHECK_CODE = 200,
        LOTTERY_OPEN_BOX = 26,
        WORLDBOSS_CMD = 102,
        VIP_RENEWAL = 92
    }
}
