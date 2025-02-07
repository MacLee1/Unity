using System;
using System.Linq;
// using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ALF;
using ALF.STATE;
using ALF.LAYOUT;
using ALF.MACHINE;
// using ALF.SOUND;
using ALF.CONDITION;
using DATA;
using USERDATA;
// using UnityEngine.EventSystems;
using STATEDATA;
using MATCHTEAMDATA;
using Newtonsoft.Json.Linq;
using CONSTVALUE;
using ALF.NETWORK;

public class TacticsFormation : IBaseUI,IBaseNetwork
{
    internal class MoveStateData : IStateData
    {
        public float Distance {get; set;}
        public Vector3 Original {get; set;}
        public Vector3 Direction {get; set;}
        public Transform TargetNode {get; set;}

        public MoveStateData(Transform m1, Vector3 endSpot,Transform target)
        {
            Original = m1.position;
            Direction = Vector3.Normalize(endSpot - m1.position);
            Distance = Vector3.Distance(endSpot,m1.position);
            TargetNode = target;
        }
        public void Dispose(){TargetNode = null;}
    }

    public static readonly List<byte[]> formation_preset = new List<byte[]>()
    {
        new byte[]{(byte)E_LOCATION.LOC_GK,(byte)E_LOCATION.LOC_DL,(byte)E_LOCATION.LOC_DCL,(byte)E_LOCATION.LOC_DCR,(byte)E_LOCATION.LOC_DR,(byte)E_LOCATION.LOC_ML,(byte)E_LOCATION.LOC_MCL,(byte)E_LOCATION.LOC_MCR,(byte)E_LOCATION.LOC_MR,(byte)E_LOCATION.LOC_FCL,(byte)E_LOCATION.LOC_FCR},
        new byte[]{(byte)E_LOCATION.LOC_GK,(byte)E_LOCATION.LOC_DL,(byte)E_LOCATION.LOC_DCL,(byte)E_LOCATION.LOC_DCR,(byte)E_LOCATION.LOC_DR,(byte)E_LOCATION.LOC_DMCC,(byte)E_LOCATION.LOC_MCL,(byte)E_LOCATION.LOC_MCR,(byte)E_LOCATION.LOC_AMCC,(byte)E_LOCATION.LOC_FCL,(byte)E_LOCATION.LOC_FCR},
        new byte[]{(byte)E_LOCATION.LOC_GK,(byte)E_LOCATION.LOC_DL,(byte)E_LOCATION.LOC_DCL,(byte)E_LOCATION.LOC_DCR,(byte)E_LOCATION.LOC_DR,(byte)E_LOCATION.LOC_MCL,(byte)E_LOCATION.LOC_MCC,(byte)E_LOCATION.LOC_MCR,(byte)E_LOCATION.LOC_FCL,(byte)E_LOCATION.LOC_FCC,(byte)E_LOCATION.LOC_FCR},
        
        new byte[]{(byte)E_LOCATION.LOC_GK,(byte)E_LOCATION.LOC_DL,(byte)E_LOCATION.LOC_DCL,(byte)E_LOCATION.LOC_DCR,(byte)E_LOCATION.LOC_DR,(byte)E_LOCATION.LOC_DMCC,(byte)E_LOCATION.LOC_MCL,(byte)E_LOCATION.LOC_MCR,(byte)E_LOCATION.LOC_AML,(byte)E_LOCATION.LOC_AMR,(byte)E_LOCATION.LOC_FCC},
        new byte[]{(byte)E_LOCATION.LOC_GK,(byte)E_LOCATION.LOC_DL,(byte)E_LOCATION.LOC_DCL,(byte)E_LOCATION.LOC_DCR,(byte)E_LOCATION.LOC_DR,(byte)E_LOCATION.LOC_MCL,(byte)E_LOCATION.LOC_MCR,(byte)E_LOCATION.LOC_AML,(byte)E_LOCATION.LOC_AMCC,(byte)E_LOCATION.LOC_AMR,(byte)E_LOCATION.LOC_FCC},
        new byte[]{(byte)E_LOCATION.LOC_GK,(byte)E_LOCATION.LOC_DL,(byte)E_LOCATION.LOC_DCL,(byte)E_LOCATION.LOC_DCR,(byte)E_LOCATION.LOC_DR,(byte)E_LOCATION.LOC_DMCL,(byte)E_LOCATION.LOC_DMCR,(byte)E_LOCATION.LOC_AML,(byte)E_LOCATION.LOC_AMCC,(byte)E_LOCATION.LOC_AMR,(byte)E_LOCATION.LOC_FCC},
        
        new byte[]{(byte)E_LOCATION.LOC_GK,(byte)E_LOCATION.LOC_DL,(byte)E_LOCATION.LOC_DCL,(byte)E_LOCATION.LOC_DCR,(byte)E_LOCATION.LOC_DR,(byte)E_LOCATION.LOC_MCL,(byte)E_LOCATION.LOC_MCR,(byte)E_LOCATION.LOC_AML,(byte)E_LOCATION.LOC_AMR,(byte)E_LOCATION.LOC_FCL,(byte)E_LOCATION.LOC_FCR},
        new byte[]{(byte)E_LOCATION.LOC_GK,(byte)E_LOCATION.LOC_DL,(byte)E_LOCATION.LOC_DCL,(byte)E_LOCATION.LOC_DCR,(byte)E_LOCATION.LOC_DR,(byte)E_LOCATION.LOC_DMCL,(byte)E_LOCATION.LOC_DMCR,(byte)E_LOCATION.LOC_AML,(byte)E_LOCATION.LOC_AMR,(byte)E_LOCATION.LOC_FCL,(byte)E_LOCATION.LOC_FCR},
        new byte[]{(byte)E_LOCATION.LOC_GK,(byte)E_LOCATION.LOC_DL,(byte)E_LOCATION.LOC_DCL,(byte)E_LOCATION.LOC_DCR,(byte)E_LOCATION.LOC_DR,(byte)E_LOCATION.LOC_MCL,(byte)E_LOCATION.LOC_MCC,(byte)E_LOCATION.LOC_MCR,(byte)E_LOCATION.LOC_AMCL,(byte)E_LOCATION.LOC_AMCR,(byte)E_LOCATION.LOC_FCC},
        
        new byte[]{(byte)E_LOCATION.LOC_GK,(byte)E_LOCATION.LOC_DL,(byte)E_LOCATION.LOC_DCL,(byte)E_LOCATION.LOC_DCR,(byte)E_LOCATION.LOC_DR,(byte)E_LOCATION.LOC_MCL,(byte)E_LOCATION.LOC_MCC,(byte)E_LOCATION.LOC_MCR,(byte)E_LOCATION.LOC_AMCC,(byte)E_LOCATION.LOC_FCL,(byte)E_LOCATION.LOC_FCR},
        new byte[]{(byte)E_LOCATION.LOC_GK,(byte)E_LOCATION.LOC_DL,(byte)E_LOCATION.LOC_DCL,(byte)E_LOCATION.LOC_DCR,(byte)E_LOCATION.LOC_DR,(byte)E_LOCATION.LOC_DMCC,(byte)E_LOCATION.LOC_ML,(byte)E_LOCATION.LOC_MCL,(byte)E_LOCATION.LOC_MCR,(byte)E_LOCATION.LOC_MR,(byte)E_LOCATION.LOC_FCC},
        new byte[]{(byte)E_LOCATION.LOC_GK,(byte)E_LOCATION.LOC_DL,(byte)E_LOCATION.LOC_DCL,(byte)E_LOCATION.LOC_DCR,(byte)E_LOCATION.LOC_DR,(byte)E_LOCATION.LOC_ML,(byte)E_LOCATION.LOC_MCL,(byte)E_LOCATION.LOC_MCR,(byte)E_LOCATION.LOC_MR,(byte)E_LOCATION.LOC_AMCC,(byte)E_LOCATION.LOC_FCC},
        
        new byte[]{(byte)E_LOCATION.LOC_GK,(byte)E_LOCATION.LOC_DL,(byte)E_LOCATION.LOC_DCL,(byte)E_LOCATION.LOC_DCR,(byte)E_LOCATION.LOC_DR,(byte)E_LOCATION.LOC_ML,(byte)E_LOCATION.LOC_MCL,(byte)E_LOCATION.LOC_MCC,(byte)E_LOCATION.LOC_MCR,(byte)E_LOCATION.LOC_MR,(byte)E_LOCATION.LOC_FCC},
        new byte[]{(byte)E_LOCATION.LOC_GK,(byte)E_LOCATION.LOC_DCL,(byte)E_LOCATION.LOC_DCC,(byte)E_LOCATION.LOC_DCR,(byte)E_LOCATION.LOC_ML,(byte)E_LOCATION.LOC_MCL,(byte)E_LOCATION.LOC_MCR,(byte)E_LOCATION.LOC_MR,(byte)E_LOCATION.LOC_FCL,(byte)E_LOCATION.LOC_FCC,(byte)E_LOCATION.LOC_FCR},
        new byte[]{(byte)E_LOCATION.LOC_GK,(byte)E_LOCATION.LOC_DCL,(byte)E_LOCATION.LOC_DCC,(byte)E_LOCATION.LOC_DCR,(byte)E_LOCATION.LOC_ML,(byte)E_LOCATION.LOC_MCL,(byte)E_LOCATION.LOC_MCR,(byte)E_LOCATION.LOC_MR,(byte)E_LOCATION.LOC_AMCC,(byte)E_LOCATION.LOC_FCL,(byte)E_LOCATION.LOC_FCR},

        new byte[]{(byte)E_LOCATION.LOC_GK,(byte)E_LOCATION.LOC_DCL,(byte)E_LOCATION.LOC_DCC,(byte)E_LOCATION.LOC_DCR,(byte)E_LOCATION.LOC_DMCC,(byte)E_LOCATION.LOC_ML,(byte)E_LOCATION.LOC_MCL,(byte)E_LOCATION.LOC_MCR,(byte)E_LOCATION.LOC_MR,(byte)E_LOCATION.LOC_FCL,(byte)E_LOCATION.LOC_FCR},
        new byte[]{(byte)E_LOCATION.LOC_GK,(byte)E_LOCATION.LOC_DCL,(byte)E_LOCATION.LOC_DCC,(byte)E_LOCATION.LOC_DCR,(byte)E_LOCATION.LOC_ML,(byte)E_LOCATION.LOC_MCL,(byte)E_LOCATION.LOC_MCC,(byte)E_LOCATION.LOC_MCR,(byte)E_LOCATION.LOC_MR,(byte)E_LOCATION.LOC_FCL,(byte)E_LOCATION.LOC_FCR},
        new byte[]{(byte)E_LOCATION.LOC_GK,(byte)E_LOCATION.LOC_DCL,(byte)E_LOCATION.LOC_DCC,(byte)E_LOCATION.LOC_DCR,(byte)E_LOCATION.LOC_DML,(byte)E_LOCATION.LOC_DMR,(byte)E_LOCATION.LOC_MCL,(byte)E_LOCATION.LOC_MCR,(byte)E_LOCATION.LOC_AMCC,(byte)E_LOCATION.LOC_FCL,(byte)E_LOCATION.LOC_FCR},

        new byte[]{(byte)E_LOCATION.LOC_GK,(byte)E_LOCATION.LOC_DCL,(byte)E_LOCATION.LOC_DCC,(byte)E_LOCATION.LOC_DCR,(byte)E_LOCATION.LOC_DML,(byte)E_LOCATION.LOC_DMR,(byte)E_LOCATION.LOC_MCL,(byte)E_LOCATION.LOC_MCC,(byte)E_LOCATION.LOC_MCR,(byte)E_LOCATION.LOC_FCL,(byte)E_LOCATION.LOC_FCR},
        new byte[]{(byte)E_LOCATION.LOC_GK,(byte)E_LOCATION.LOC_DL,(byte)E_LOCATION.LOC_DCL,(byte)E_LOCATION.LOC_DCC,(byte)E_LOCATION.LOC_DCR,(byte)E_LOCATION.LOC_DR,(byte)E_LOCATION.LOC_ML,(byte)E_LOCATION.LOC_MCL,(byte)E_LOCATION.LOC_MCR,(byte)E_LOCATION.LOC_MR,(byte)E_LOCATION.LOC_FCC}
    };

    public static readonly string[] formation_preset_names = new string[] { "SELECTFORMATION_BTN_442","SELECTFORMATION_BTN_442DIAMOND","SELECTFORMATION_BTN_433","SELECTFORMATION_BTN_41221","SELECTFORMATION_BTN_4231","SELECTFORMATION_BTN_4231DM","SELECTFORMATION_BTN_424","SELECTFORMATION_BTN_4222","SELECTFORMATION_BTN_4321","SELECTFORMATION_BTN_4312","SELECTFORMATION_BTN_4141","SELECTFORMATION_BTN_4411","SELECTFORMATION_BTN_451","SELECTFORMATION_BTN_343","SELECTFORMATION_BTN_3412","SELECTFORMATION_BTN_3142","SELECTFORMATION_BTN_352","SELECTFORMATION_BTN_32212","SELECTFORMATION_BTN_3232","SELECTFORMATION_BTN_541"};
    const string SCROLL_ITEM_NAME = "BenchPlayerItem";
    const string FORMATION_ITEM_NAME = "FormationPlayerItem";
    
    readonly string[] AbilityNodeNameList = new string[]{"passing","crossing","shooting","offtheball","dribbling","touch","pressing","marking","positioning","tackle","intercept","concentate","speed","accelate","agility","balance","stamina","jump","creativity","vision","determination","decisions","aggression","influence","reflex","command","goalkick","oneonone","handling","aerialability"};
    static readonly string[] AbilityTitleNameList = new string[]{"ABILITY_TXT_CATEGORY_ATTACKINNG","ABILITY_TXT_CATEGORY_DEFENDING","ABILITY_TXT_CATEGORY_PHYSICALITY","ABILITY_TXT_CATEGORY_MENTALITY","ABILITY_TXT_CATEGORY_GOALKEEPING"};
    const string MAIN_TXT_OVERALL = "MAIN_TXT_OVERALL";
    const string MAIN_TXT_OVERALL_TOKEN = " <color=#FF9F00> {0}</color>"; 
    readonly string[] PLAYER_TACTICS_NAME = new string[]{ "TACTICS_PLAYER_TXT_PASS_LENGTH","TACTICS_PLAYER_TXT_DRIBBLE","TACTICS_PLAYER_TXT_LONG_SHOT","TACTICS_PLAYER_TXT_CROSS","TACTICS_PLAYER_TXT_THROUGH_PASS","TACTICS_PLAYER_TXT_DEFENCE_TYPE_MARKING","TACTICS_PLAYER_TXT_PRESSURE_RANGE","TACTICS_PLAYER_TXT_MARKING_DISTANCE","TACTICS_PLAYER_TXT_OVERLAPPING"};
    enum E_TAB : byte { formation = 0,teamTactics,playerTactics,MAX}
    enum E_CONTROL : byte { defenseLine = 0,passDirectionWidth,tempo,width,wasteTime,offsideTrap,MAX}

    class BenchPlayer : IBase
    {
        public ulong Id {get; private set;}
        public string Name {get; private set;}
        public int IPlay {get; private set;}
        public byte Position {get; private set;}
        public byte Cnd {get; private set;}

        public BenchPlayer( ulong id, string name,int iPlay, byte position, byte cnd)
        {
            Id = id;
            Name = name;
            IPlay = iPlay;
            Position = position;
            Cnd = cnd;
        }

        public void SetIPlay(int iPlay)
        {
            IPlay = iPlay;
        }

        public void SetupPlayerData( JObject data)
        {
            Name = data["surname"].ToString();
        }

        public void UpdatePlayerData(PlayerT pPlayer)
        {
            if(pPlayer != null)
            {
                Name = pPlayer.Surname;
                Id = pPlayer.Id;

                Position = pPlayer.Position;
                Cnd = pPlayer.Hp;
            }
        }

        public void Dispose()
        {
        }
    }
    class PlayerItem : IBase
    {
        public ulong Id {get; private set;}
        public int Index {get; private set;}
        
        public TMPro.TMP_Text PlayerName {get; private set;}
        public TMPro.TMP_Text FormationText {get; private set;}
        public TMPro.TMP_Text NoText {get; private set;}
        public Image HP {get; private set;}

        public GameObject Add {get; private set;}
        public GameObject In {get; private set;}
        public GameObject Out {get; private set;}
        public GameObject Card {get; private set;}
        public GameObject Red {get; private set;}
        public GameObject Yellow {get; private set;}
        public GameObject Injury {get; private set;}
        
        public GameObject Select {get; private set;}
        public GameObject Info  {get; private set;}
        public Graphic Icon  {get; private set;}
        public Transform Quality  {get; private set;}
        
        GameObject[] Icons = null;

        string m_pItemName = null;

        public RectTransform Target  {get; private set;}
        
        Button m_pButton = null;

        public PlayerItem(RectTransform target,string pItemName,int index = -1)
        {
            Index = index;
            m_pItemName = pItemName;
            Target = target;
            Quality = target.Find("quality");
            m_pButton = target.GetComponent<Button>();
            Icon = target.Find("icons").GetComponent<Graphic>();
            PlayerName = target.Find("name/text").GetComponent<TMPro.TMP_Text>();
            FormationText = target.Find("text").GetComponent<TMPro.TMP_Text>();
            Transform tm = target.Find("no");
            if(tm != null)
            {
                NoText = tm.GetComponent<TMPro.TMP_Text>();
            }
            else
            {
                NoText = null;
            }

            HP = target.Find("hp/fill").GetComponent<Image>();
            
            Add = target.Find("add").gameObject;
            In = target.Find("add/in").gameObject;
            Out = target.Find("add/out").gameObject;
            Card = target.Find("card").gameObject;
            if(Card.transform.childCount == 0)
            {
                Yellow = Card;
            }
            else
            {
                Red = Card.transform.Find("r").gameObject;
                Yellow = Card.transform.Find("y").gameObject;
            }
            tm = PlayerName.transform.parent.Find("injury");
            if(tm != null)
            {
                Injury = tm.gameObject;
            }
            else
            {
                Injury = null;
            }
            
            Info = target.Find("info").gameObject;
            Select = target.Find("select").gameObject;
            
            if(Icon.transform.childCount > 0)
            {
                Icons = new GameObject[4];
                Icons[0] = target.Find("icons/red").gameObject;
                Icons[1] = target.Find("icons/orange").gameObject;
                Icons[2] = target.Find("icons/yellow").gameObject;
                Icons[3] = target.Find("icons/green").gameObject;
            }
            else
            {
                Icons = new GameObject[0];
            }
        }

        public void Dispose()
        {
            for(int i =0; i < Icons.Length; ++i)
            {
                Icons[i] = null;
            }

            m_pButton.onClick.RemoveAllListeners();
            m_pButton = null;
            if(Icon != null)
            {
                Icon.color = Color.white;
            }

            PlayerName = null;
            FormationText = null;
            NoText = null;
            HP = null;

            Select = null;
            Info = null;
            Quality = null;
        
            Icon = null;
            Icons = null;

            Add = null;
            In = null;
            Out = null;
            Card = null;
            Red = null;
            Yellow = null;
            Injury = null;

            LayoutManager.Instance.AddItem(m_pItemName,Target);
            Target = null;
        }

        public void CloneInfoUI(PlayerItem src)
        {
            FormationText.SetText(src.FormationText.text);

            Transform item = src.Quality;
            Transform item1 = Quality;
            int i =0;
            Transform temp = null;
            Transform temp1 = null;
            
            for(i =0; i < item.childCount; ++i)
            {
                temp = item.GetChild(i);
                temp1 = item1.GetChild(i);
                temp1.localPosition = temp.localPosition;
                temp1.gameObject.SetActive(temp.gameObject.activeSelf);
                if(temp.Find("on") != null)
                {
                    temp1.Find("on").gameObject.SetActive(temp.Find("on").gameObject.activeSelf);
                }
                if(temp.Find("h") != null)
                {
                    temp1.Find("h").gameObject.SetActive(temp.Find("h").gameObject.activeSelf);
                }
            }
            
            HP.fillAmount = src.HP.fillAmount;
            PlayerName.SetText(src.PlayerName.text);

            for(i =0; i < Icons.Length; ++i)
            {
                Icons[i].SetActive(false);
            }

            if(src.Icons.Length == Icons.Length)
            {
                for(i =0; i < Icons.Length; ++i)
                {
                    Icons[i].SetActive(src.Icons[i].activeSelf);
                }
            }

            Add.SetActive(false);
            Card.SetActive(false);
            Injury.SetActive(false);
            Select.SetActive(false);
            Info.SetActive(false);
        }

        public void UpdateHP(float hp)
        {
            HP.fillAmount = hp;
            if(hp < 0.5f)
            {
                HP.color = GameContext.HP_L;
            }
            else if(hp < 0.7f)
            {
                HP.color = GameContext.HP_LH;
            }
            else if(hp < 0.9f)
            {
                HP.color = GameContext.HP_H;
            }
            else
            {
                HP.color = GameContext.HP_F;
            }
        }

        public void UpdateFormation(BenchPlayer pBenchPlayer)
        {
            if(pBenchPlayer == null) return;
            GameContext pGameContext = GameContext.getCtx();
            
            if(NoText != null)
            {
                NoText.gameObject.SetActive(pBenchPlayer.IPlay != GameContext.CONST_NUMPLAYER);

                if(NoText.gameObject.activeSelf)
                {
                    NoText.SetText($"S{pBenchPlayer.IPlay - GameContext.CONST_NUMSTARTING +1}");
                }
            }

            FormationText.SetText(pGameContext.GetDisplayLocationName(pBenchPlayer.Position));
        }

        public void ResetTag(bool bSubstitution)
        {
            Card.SetActive(false);
            Info.SetActive(false);
            Select.SetActive(false);
            Injury.SetActive(false);
            if(bSubstitution)
            {
                m_pButton.enabled = false;
                Add.SetActive(true);
                Out.SetActive(true);
                In.SetActive(false);
            }
            else
            {
                Add.SetActive(false);
                m_pButton.enabled = true;
            }
            if(Icon != null)
            {
                Icon.color = m_pButton.enabled ? Color.white : GameContext.GRAY;
            }

            for(int i =0; i < Icons.Length; ++i)
            {
                Icons[i].SetActive(false);
            }
        }

        public void UpdatePlayerData(BenchPlayer pBenchPlayer)
        {
            if(pBenchPlayer == null) return;

            UpdatePlayerInfo(GameContext.getCtx().GetPlayerByID(pBenchPlayer.Id));
            UpdateFormation(pBenchPlayer);
        }
        public void UpdatePlayerName(BenchPlayer pBenchPlayer)
        {
            if(pBenchPlayer == null) return;

            PlayerName.SetText(pBenchPlayer.Name);
        }

        public void UpdatePlayerInfo(PlayerT pPlayer)
        {
            if(pPlayer == null) return;
            Id = pPlayer.Id;
            PlayerName.SetText(pPlayer.Surname);
            SingleFunc.SetupQuality(pPlayer,Quality);
            UpdateHP((float)pPlayer.Hp / 100f);
        }

        public void UpdateFormationPlayerData(PlayerT pPlayer,E_LOCATION eLoc)
        {
            if(pPlayer == null ) return;

            GameContext pGameContext = GameContext.getCtx();
            int index = pGameContext.ConvertPositionByTag(eLoc);
            if(Index > -1)
            {
                if(eLoc != E_LOCATION.LOC_NONE)
                {
                    byte value = pPlayer.PositionFamiliars[index];

                    if(value >= 90)
                    {
                        value = 3;
                    }
                    else if(value >= 60)
                    {
                        value = 2;
                    }
                    else if(value >= 30)
                    {
                        value = 1;
                    }
                    else
                    {
                        value = 0;
                    }
                    
                    for(int i =0; i < Icons.Length; ++i)
                    {
                        Icons[i].SetActive(i == value);
                    }
                }
            }
            
            FormationText.SetText(pGameContext.GetDisplayLocationName(index));
        }
    }

    MainScene m_pMainScene = null;
    ScrollRect m_pScrollRect = null;
    BaseState m_pRollingNumberState = null;
    E_TAB m_eCurrentTab = E_TAB.MAX;
    int m_iCurrentSelectPlayerIndex = -1;
    E_TRAINING_TYPE eCurrentSelectPlayerType = E_TRAINING_TYPE.attacking;
    RectTransform[] m_pTabUIList = new RectTransform[(int)E_TAB.MAX];
    Transform[] m_pTabButtonList = new Transform[(int)E_TAB.MAX];
    
    uint m_uiCurrentOverall = 0;

    float[] m_fFeildSlotXList = new float[]{0,0};
    TacticsT m_pTempTactics = new TacticsT();
    LineupPlayerT m_TempLineupPlayer = new LineupPlayerT();
    Transform m_pBench = null;
    RectTransform[] m_pFeildSlots = new RectTransform[(int)E_LOCATION.LOC_END];
    RectTransform[] m_pFamiliarsList = new RectTransform[(int)E_LOCATION.LOC_END];
    PlayerItem[] m_pFormationPlayers = new PlayerItem[GameContext.CONST_NUMSTARTING];

    PlayerItem[] m_pMovePlayers = new PlayerItem[2];

    GameObject m_pRefresh = null;
    GameObject m_pSlots = null;

    GameObject m_pRecommend = null;
    GameObject m_pReset = null;
    
    PlayerItem m_pSelectFormation = null;
    TMPro.TMP_Text m_pFormationText = null;
    TMPro.TMP_Text m_pOverallText = null;
    TMPro.TMP_Text m_pDiffText = null;

    Vector2 m_OffsetMax = Vector2.zero;
    Vector2 m_OffsetMin = Vector2.zero;

    Button m_pChangeButton = null;
    Button m_pCancelButton = null;
    
    ulong[] m_SubstitutionInList = new ulong[GameContext.CONST_NUMSTARTING];
    ulong[] m_SubstitutionOutList = new ulong[GameContext.CONST_NUMSTARTING];
    List<ulong> m_SubstitutionList = new List<ulong>();

    bool m_bChangeSubstitution = false;
    bool m_bChange = false;
    int m_iActiveTacticsType = 0;
    int m_iActiveLineUpType =0;
    
    List<PlayerItem> m_pBenchPlayerItems = new List<PlayerItem>();
    List<BenchPlayer> m_pBenchPlayers = new List<BenchPlayer>();
    
    
    public bool Enable { set{ if (m_pScrollRect != null) m_pScrollRect.enabled = value;}}
    public RectTransform MainUI { get; private set;}
    Vector2 m_pPrevDir = Vector2.zero;
    int m_iTotalScrollItems = 0;
    int m_iStartIndex = 0;

    public TacticsFormation(){}
    
    public void Dispose()
    {
        ClearScroll();
        m_pScrollRect.onValueChanged.RemoveAllListeners();
        m_pBenchPlayers = null;
        m_pBenchPlayerItems = null;

        Transform tm = m_pTabUIList[(int)E_TAB.playerTactics].Find("playerInfo");
        RawImage icon = tm.Find("formation/icon").GetComponent<RawImage>();
        icon.texture = null;
        int i =0;
        tm = tm.Find("player/roles");
        if(tm != null)
        {
            for(i = 0; i < tm.childCount; ++i)
            {
                icon = tm.GetChild(i).GetComponent<RawImage>();
                icon.texture = null;
            }  
        }

        tm = m_pTabUIList[(int)E_TAB.playerTactics].Find("control");
        for( i = 0; i < tm.childCount; ++i)
        {
            tm.GetChild(i).Find("slider").GetComponent<Slider>().onValueChanged.RemoveAllListeners();
        }        

        for( i =0; i < m_pTabButtonList.Length; ++i)
        {
            m_pTabButtonList[i] = null;
            m_pTabUIList[i] = null;
        }
        for(i =0; i < m_pFeildSlots.Length; ++i)
        {    
            m_pFeildSlots[i] = null;
            m_pFamiliarsList[i] = null;
        }
        
        for(i =0; i < m_pFormationPlayers.Length; ++i)
        {   
            m_pFormationPlayers[i].Dispose();
            m_pFormationPlayers[i] = null;
        }
        m_pFormationPlayers = null;
        
        if(m_pRollingNumberState != null)
        {
            m_pRollingNumberState.Exit(true);
        }

        for(i =0; i < m_pMovePlayers.Length; ++i)
        {
            m_pMovePlayers[i].Dispose();
            m_pMovePlayers[i] = null;
        }

        m_pChangeButton = null;
        m_pCancelButton = null;

        m_pRefresh = null;
        m_pSlots = null;

        m_pRecommend = null;
        m_pReset = null;
        m_pScrollRect = null;
        m_pTabButtonList = null;
        m_pTabUIList = null;
        m_pMovePlayers = null;
        m_pFamiliarsList = null;
        m_pBench = null;
        m_pFeildSlots = null;
        m_pSelectFormation = null;
        m_pFormationText = null;
        m_pOverallText = null;
        m_pDiffText = null;
        GameContext.getCtx().ClearCacheLineupPlayerData();
        m_TempLineupPlayer = null;
        m_pMainScene = null;
        MainUI = null;
        m_pTempTactics = null;
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "TacticsFormation : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "TacticsFormation : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;

        m_OffsetMax = MainUI.offsetMax;
        m_OffsetMin = MainUI.offsetMin;

        m_pTempTactics.Formation = new List<byte>();
        m_pTempTactics.TeamTactics = new List<byte>();
        m_pTempTactics.PlayerTactics = new List<PlayerTacticsT>();

        m_TempLineupPlayer.Type = 1;
        m_TempLineupPlayer.Data = new List<ulong>();

        RectTransform item = null;
        Vector3 pos;
        Vector2 size;
        GameContext pGameContext = GameContext.getCtx();
        RectTransform ui = MainUI.Find("root/bg/tabs").GetComponent<RectTransform>();
        LayoutManager.SetReciveUIButtonEvent(ui,ButtonEventCall);
        float w = (ui.rect.width / ui.childCount);
        float wh = w * 0.5f;
        float ax = ui.pivot.x * ui.rect.width;
        int iTabIndex = -1;
        int n =0;
        for(n =0; n < ui.childCount; ++n)
        {
            item = ui.GetChild(n).GetComponent<RectTransform>();
            iTabIndex = (int)((E_TAB)Enum.Parse(typeof(E_TAB), item.gameObject.name));
            m_pTabButtonList[iTabIndex] = item;
            m_pTabUIList[iTabIndex] = MainUI.Find($"root/bg/{item.gameObject.name}").GetComponent<RectTransform>();
            LayoutManager.SetReciveUIButtonEvent(m_pTabUIList[iTabIndex],ButtonEventCall);
            m_pTabUIList[iTabIndex].gameObject.SetActive(false);

            pos = item.localPosition;
            size = item.sizeDelta;
            size.x = w;
            pos.x = wh + (n * w) - ax;
            item.localPosition = pos;
            item.sizeDelta = size;
        }

        m_pRefresh = m_pTabUIList[(int)E_TAB.formation].Find("board/feild/refresh").gameObject;
        m_pSlots = MainUI.Find("root/bg/slots").gameObject;

        m_pRecommend = m_pTabUIList[(int)E_TAB.formation].Find("recommend").gameObject;
        m_pReset = m_pTabUIList[(int)E_TAB.formation].Find("reset").gameObject;

        m_pChangeButton = MainUI.Find("root/bg/change").GetComponent<Button>();
        m_pCancelButton = MainUI.Find("root/bg/cancel").GetComponent<Button>();
        LayoutManager.SetReciveUIButtonEvent(m_pCancelButton.GetComponent<RectTransform>(),ButtonEventCall);
        LayoutManager.SetReciveUIButtonEvent(m_pChangeButton.GetComponent<RectTransform>(),ButtonEventCall);
        LayoutManager.SetReciveUIButtonEvent(MainUI.Find("root/bg/tip").GetComponent<RectTransform>(),ButtonEventCall);

        m_pScrollRect = m_pTabUIList[(int)E_TAB.formation].GetComponentInChildren<ScrollRect>(true);
        m_pScrollRect.onValueChanged.AddListener( ScrollViewChangeValueEventCall);
            
        m_pOverallText = m_pTabUIList[(int)E_TAB.formation].Find("overall/text").GetComponent<TMPro.TMP_Text>();
        m_pDiffText = m_pOverallText.transform.parent.Find("diff/text").GetComponent<TMPro.TMP_Text>();
        
        ui = m_pTabUIList[(int)E_TAB.teamTactics].Find("control").GetComponent<RectTransform>();

        w = (ui.rect.width - 90) * 0.5f;
        int i =0;
        float w1 = 0;
        RectTransform tm = null;
        for(n =0; n < (int)E_CONTROL.MAX; ++n)
        {
            item = ui.Find(((E_CONTROL)n).ToString()).GetComponent<RectTransform>();
            size = item.sizeDelta;
            size.x = w;
            item.sizeDelta = size;
            
            w1 = (w - (20 * item.childCount)) / (item.childCount -1);
            wh = w1 * 0.5f;
            ax = item.pivot.x * item.rect.width;
            for(i =0; i < item.childCount; ++i)
            {
                tm = item.GetChild(i).GetComponent<RectTransform>();
                if(tm.gameObject.name != "title")
                {
                    size = tm.sizeDelta;
                    size.x = w1;
                    tm.sizeDelta = size;

                    pos = tm.localPosition;
                    pos.x = wh + ((i -1) * w1) - ax +((i -1) * 20 + 20);
                    tm.localPosition = pos;
                }
            }
        }

        MainUI.gameObject.SetActive(false);

        for(n =0; n < m_pMovePlayers.Length; ++n)
        {
            tm = LayoutManager.Instance.GetItem<RectTransform>(FORMATION_ITEM_NAME);
            tm.localScale = Vector3.one;
            tm.anchoredPosition = Vector2.zero;
            tm.SetParent(m_pTabUIList[(int)E_TAB.formation],false);

            m_pMovePlayers[n] = new PlayerItem(tm,FORMATION_ITEM_NAME);
            m_pMovePlayers[n].ResetTag(false);
        }

        ui = m_pTabUIList[(int)E_TAB.formation].Find("board").GetComponent<RectTransform>();
        for(n =0; n < m_pFormationPlayers.Length; ++n)
        {
            tm = LayoutManager.Instance.GetItem<RectTransform>(FORMATION_ITEM_NAME);
            tm.localScale = Vector3.one;
            tm.anchoredPosition = Vector2.zero;
            tm.SetParent(ui,false);
            m_pFormationPlayers[n] = new PlayerItem(tm,FORMATION_ITEM_NAME,n);
            m_pFormationPlayers[n].ResetTag(false);
        }
        ui.Find("familiars").SetAsLastSibling();
        
        LayoutManager.SetReciveUIButtonEvent(ui,null);
        LayoutManager.SetReciveUIButtonEvent(ui,ButtonEventCall);

        ui = ui.Find("feildSlots").GetComponent<RectTransform>();
        for(n =0; n < ui.childCount; ++n)
        {
            tm = ui.GetChild(n).GetComponent<RectTransform>();
            iTabIndex = (int)((E_LOCATION)Enum.Parse(typeof(E_LOCATION), tm.gameObject.name));
            m_pFeildSlots[iTabIndex] = tm;
            m_pFamiliarsList[iTabIndex] = ui.parent.Find($"familiars/{tm.gameObject.name}").GetComponent<RectTransform>();
            m_pFamiliarsList[iTabIndex].position = tm.position;
        }
        m_fFeildSlotXList[0] = m_pFeildSlots[2].anchoredPosition.x;
        m_fFeildSlotXList[1] = m_pFeildSlots[4].anchoredPosition.x;
        LayoutManager.SetReciveUIButtonEvent(ui,null);
        LayoutManager.SetReciveUIButtonEvent(ui,ButtonEventCall);

        Slider pSlider = null;
        
        tm = m_pTabUIList[(int)E_TAB.playerTactics].Find("control").GetComponent<RectTransform>();
        Transform slot = null;
        for( n = 0; n < tm.childCount; ++n)
        {
            slot = tm.GetChild(n);
            int index = int.Parse(slot.gameObject.name);
            slot.Find("title").GetComponent<TMPro.TMP_Text>().SetText(pGameContext.GetLocalizingText(PLAYER_TACTICS_NAME[index]));
            pSlider = slot.Find("slider").GetComponent<Slider>();
            
            pSlider.onValueChanged.AddListener(delegate(float value) { 
                OnSlideValueChanged(index,(byte)value);
            });           
        }

        m_pFormationText = m_pTabUIList[(int)E_TAB.formation].Find("selectFormation/text").GetComponent<TMPro.TMP_Text>();
        LayoutManager.SetReciveUIButtonEvent(MainUI.Find("back").GetComponent<RectTransform>(),ButtonEventCall);
        LayoutManager.SetReciveUIButtonEvent(MainUI.Find("root/bg/slots").GetComponent<RectTransform>(),ButtonEventCall);

        SetupScroll();
    }

    public ScrollRect GetBenchScroll()
    {
        if(MainUI == null) return null;
        return m_pScrollRect;
    }

    public static string GetAbilityTitleNameByIndex(int index)
    {
        return AbilityTitleNameList[index];
    }

    public static string UpdateFormationText(List<byte> list)
    {
        if(list != null)
        {
            int i =0;
            
            byte[] checkList = new byte[GameContext.CONST_NUMSTARTING];
            List<byte> tempList = new List<byte>();
            for(i =1; i < list.Count; ++i)
            {
                tempList.Add(list[i]);
            }

            byte temp = (byte)E_LOCATION.LOC_DML;
            int count =0;
            string text ="";
            int n = 0;
            while(tempList.Count > 0)
            {
                count =0;
                for(i = tempList.Count -1; i >= 0; --i)
                {
                    if(count == 5)
                    {
                        temp += 5;
                        break;
                    }

                    if(temp > tempList[i])
                    {
                        checkList[n] = tempList[i];
                        ++count;
                        ++n;
                        tempList.RemoveAt(i);
                    }
                }

                if( count > 0)
                {
                    text += $"-{count}";
                }
                if(temp >= (byte)E_LOCATION.LOC_END)
                {
                    break;
                }
                temp += 5;
            }

            bool bCheck = false;

            System.Array.Sort(checkList);
            
            for(i =0; i < formation_preset.Count; ++i)
            {
                bCheck = true;
                for(n = 0; n < formation_preset[i].Length; ++n)
                {
                    if(formation_preset[i][n] != checkList[n])
                    {
                        bCheck = false;
                        break;
                    }
                }

                if(bCheck)
                {
                    return GameContext.getCtx().GetLocalizingText(formation_preset_names[i]);
                }
            }

            text = text.Substring(1) + "*";
            return text;
        }

        return "";
    }

    public void UpdateFormationFromPresetData(int iActiveTacticsType, int index)
    {
        m_iActiveTacticsType = iActiveTacticsType;
        for(int i =0; i < formation_preset[index].Length; ++i)
        {
            m_pTempTactics.Formation[i] = formation_preset[index][i];
        }
        
        if(MainUI.gameObject.activeInHierarchy)
        {
            SetupFormationPlayer(true);
            if(m_pMainScene.IsMatch())
            {
                UpdateChangeButton(IsMatchView());
            }
        }
    }

    void SetupPlayerTactics()
    {
        Transform tm = m_pTabUIList[(int)E_TAB.playerTactics].Find("feild");
        
        Transform item = null;
        E_LOCATION eLoc = E_LOCATION.LOC_END;
        TMPro.TMP_Text text = null;
        int index = 0;
        GameContext pGameContext = GameContext.getCtx();
        for(int i =0; i < m_pFormationPlayers.Length; ++i)
        {
            item = tm.Find(i.ToString());
            item.gameObject.SetActive(true);
            eLoc = (E_LOCATION)m_pTempTactics.Formation[i];
            item.position = tm.Find(eLoc.ToString()).position;
            
            index = pGameContext.ConvertPositionByTag(eLoc);

            text = item.Find("text").GetComponent<TMPro.TMP_Text>();
            text.SetText(pGameContext.GetDisplayLocationName(index));
            item = item.Find("select");
            item.gameObject.SetActive(m_iCurrentSelectPlayerIndex == i);
        }

        UpdatePlayerTacticsPlayerInfo(m_TempLineupPlayer.Data[m_iCurrentSelectPlayerIndex]);
    }

    void UpdatePlayerTacticsPlayerInfo(ulong playeID)
    {
        GameContext pGameContext = GameContext.getCtx();
        PlayerT pPlayer = pGameContext.GetPlayerByID(playeID);
        Transform tm = m_pTabUIList[(int)E_TAB.playerTactics].Find("playerInfo");

        SingleFunc.SetupPlayerCard(pPlayer,tm.Find("player"),E_ALIGN.Left);
        TMPro.TMP_Text text = tm.Find("name").GetComponent<TMPro.TMP_Text>();
        text.SetText($"{pPlayer.Forename} {pPlayer.Surname}");
        text = tm.Find("age/text").GetComponent<TMPro.TMP_Text>();
        byte age = pGameContext.GetPlayerAge(pPlayer);
        text.SetText(age > 40 ? "40+":$"{age}");
        
        RawImage icon = tm.Find("formation/icon").GetComponent<RawImage>();
        text = icon.transform.Find("text").GetComponent<TMPro.TMP_Text>();
        text.SetText(pGameContext.GetDisplayLocationName(pPlayer.Position));
        Sprite pSprite = AFPool.GetItem<Sprite>("Texture",pGameContext.GetDisplayCardFormationByLocationName(text.text));
        icon.texture = pSprite.texture;

        text = tm.Find("value/text").GetComponent<TMPro.TMP_Text>();
        text.SetText(ALFUtils.NumberToString(pPlayer.Price));
        SetupPlayerAbilityInformation(pPlayer);
    }

    void SetupPlayerAbilityInformation(PlayerT pPlayer)
    {
        if((E_LOCATION)pPlayer.Position == E_LOCATION.LOC_GK ) return;

        E_ABILINDEX eStart = E_ABILINDEX.AB_ATT_PASS;
        E_TRAINING_TYPE eType = E_TRAINING_TYPE.attacking;
        E_ABILINDEX eEnd = E_ABILINDEX.AB_ATT_PASS;
        Transform tm = null;
        TMPro.TMP_Text text = null;
        Transform info = m_pTabUIList[(int)E_TAB.playerTactics].Find("playerInfo/information");
        GameContext pGameContext = GameContext.getCtx();
        uint total = 0;
        int index = 0;
        for(eType = E_TRAINING_TYPE.attacking; eType < E_TRAINING_TYPE.goalkeeping; ++eType)
        {
            tm = info.Find(eType.ToString());
            
            text = tm.Find("title").GetComponent<TMPro.TMP_Text>();
            total = 0;
            
            eStart = (E_ABILINDEX)((int)E_ABILINDEX.AB_ATT_PASS + ((int)eType * 6));
            index = (int)eStart / 6;
            eEnd = eStart + 6;
            
            while(eStart < eEnd)
            {
                if(pPlayer.Ability[(int)eStart].Current > 0)
                {
                    total += (uint)(pPlayer.Ability[(int)eStart].Current);
                }
                
                UpdatePlayerAbility(pPlayer,eStart,tm.Find(AbilityNodeNameList[(int)eStart]));
                ++eStart;
            }
            
            text.SetText( string.Format(pGameContext.GetLocalizingText(AbilityTitleNameList[index]),(int)(total / 6)));
        }
    }

    void UpdatePlayerAbility(PlayerT pPlayer,E_ABILINDEX index, Transform gauge)
    {
        Image item = gauge.Find("gauge/fill_o").GetComponent<Image>();
        item.fillAmount = pPlayer.Ability[(int)index].Current * 0.01f;
        Debug.Log(pPlayer.Ability[(int)index].Current);
        Debug.Log(pPlayer.Ability[(int)index].Current * 0.01f);
        item = gauge.Find("gauge/fill").GetComponent<Image>();
        item.fillAmount = pPlayer.Ability[(int)index].Origin * 0.01f;
        
        TMPro.TMP_Text text = gauge.Find("text").GetComponent<TMPro.TMP_Text>();
        text.SetText(pPlayer.Ability[(int)index].Current.ToString());
        if(pPlayer.Ability[(int)index].Current >= 90)
        {
            text.color = Color.red;
        }
        else if(pPlayer.Ability[(int)index].Current >= 80)
        {
            text.color = Color.yellow;
        }
        else
        {
            text.color = Color.gray;
        }
        int changed = pPlayer.Ability[(int)index].Changed;
        text = gauge.Find("text_a").GetComponent<TMPro.TMP_Text>();
        text.SetText( changed > 0 ? $"(+{changed})" : $"({changed})");
        text.gameObject.SetActive(changed > 0);
    }

    void UpdatePlayerTacticsByIndex(int index, byte value)
    {
        Transform slot = m_pTabUIList[(int)E_TAB.playerTactics].Find("control").GetChild(index);
        Slider pSlider = slot.Find("slider").GetComponent<Slider>();
        pSlider.value = value;
        string token = PLAYER_TACTICS_NAME[index];
        
        if(value >= 80 )
        {
            token = token + "_5";
        }
        else if(value >= 60 )
        {
            token = token + "_4";
        }
        else if(value >= 40 )
        {
            token = token + "_3";
        }
        else if(value >= 20 )
        {
            token = token + "_2";
        }
        else
        {
            token = token + "_1";
        }

        TMPro.TMP_Text text = slot.Find("value").GetComponent<TMPro.TMP_Text>();
        text.SetText(GameContext.getCtx().GetLocalizingText(token));
    }

    void SetupTeamTactics()
    {
        int value = 0;
        E_CONTROL eControl = E_CONTROL.MAX;
        for(int n =0; n < m_pTempTactics.TeamTactics.Count; ++n)
        {
            if(m_pTempTactics.TeamTactics[n] > 74)
            {
                value = 3;
            }
            else if(m_pTempTactics.TeamTactics[n] > 49)
            {
                value = 2;
            }
            else
            {
                value = 1;
            }
            eControl = (E_CONTROL)n;
            if(eControl == E_CONTROL.offsideTrap ||eControl == E_CONTROL.wasteTime)
            {
                value = Mathf.Min(2,value);
            }
            UpdateTeamTactics(eControl,value);
        }
    }
    void UpdateTeamTactics(E_CONTROL eControl, int value)
    {
        Transform display = m_pTabUIList[(int)E_TAB.teamTactics].Find("display");
        Transform ui = m_pTabUIList[(int)E_TAB.teamTactics].Find("control");
        Transform item = null;
        Transform tm = null;
        
        int i =0;
        string token = eControl.ToString();
        item = ui.Find(token);

        for(i =0; i < item.childCount; ++i)
        {
            tm = item.GetChild(i);
            if(tm.gameObject.name != "title")
            {
                tm.Find("on").gameObject.SetActive(false);
                tm.Find("off").gameObject.SetActive(true);
            }
        }
        
        tm = item.GetChild(value);
        while(value > 0 && tm == null)
        {
            --value;
            tm = item.GetChild(value);
        }

        if(tm != null)
        {
            tm.Find("on").gameObject.SetActive(true);
            tm.Find("off").gameObject.SetActive(false);
            item = display.Find(token);
            
            switch(eControl)
            {
                case E_CONTROL.defenseLine:
                {
                    RectTransform temp = item.GetComponent<RectTransform>();
                    for(i =0; i < item.childCount; ++i)
                    {
                        tm = item.GetChild(i);
                        tm.localScale = (value == 1) ? new Vector3(1,-1,1) : Vector3.one;
                    }
                    
                    value = (value - 1) * (int)temp.rect.height;
                    Vector2 ap = temp.anchoredPosition;
                    ap.y = value;
                    temp.anchoredPosition = ap;
                    item = display.Find(E_CONTROL.offsideTrap.ToString());
                    item.position = temp.position;
                }
                break;
                case E_CONTROL.offsideTrap:
                {
                    item.gameObject.SetActive(value > 1);
                }
                break;
                case E_CONTROL.passDirectionWidth:
                {
                    for(i =0; i < item.childCount; ++i)
                    {
                        tm = item.GetChild(i);
                        tm.gameObject.SetActive(i == value - 1);
                    }
                }
                break;
                case E_CONTROL.wasteTime:
                {
                    item.gameObject.SetActive(value > 1);
                }
                break;
                case E_CONTROL.tempo:
                {
                    item = display.Find("widthtempo");
                    value -=1;
                    for(i =0; i < item.childCount; ++i)
                    {
                        tm = item.GetChild(i);
                        for(int t =0; t < tm.childCount; ++t)
                        {
                            tm.GetChild(t).gameObject.SetActive(t == value);
                        }
                    }
                }
                break;
                case E_CONTROL.width:
                {
                    item = display.Find("widthtempo");
                    RectTransform temp = item.Find("2").GetComponent<RectTransform>();
                    Vector2 ap = temp.anchoredPosition;
                    ap.x = -200 +( -100 *(value-1));
                    temp.anchoredPosition = ap;
                    temp = item.Find("3").GetComponent<RectTransform>();
                    ap.x *= -1; 
                    temp.anchoredPosition = ap;
                }
                break;
            }
        }
    }
    void SetupFormationPlayer(bool bSkip = false)
    {
        m_pFormationText.SetText(UpdateFormationText(m_pTempTactics.Formation));
        PlayerT pPlayer = null;
        GameContext pGameContext = GameContext.getCtx();
        for(int i =0; i < m_pFormationPlayers.Length; ++i)
        {
            m_pFormationPlayers[i].Target.position = m_pFeildSlots[(int)m_pTempTactics.Formation[i]].position;
            pPlayer = pGameContext.GetPlayerByID(m_TempLineupPlayer.Data[i]);
            SetupFormationPlayerData(pPlayer,m_pFormationPlayers[i],(E_LOCATION)m_pTempTactics.Formation[i]);
            
            m_pFormationPlayers[i].Target.gameObject.name = i.ToString();
            if(!bSkip)
            {   
                m_pFormationPlayers[i].Add.SetActive(false);
                m_pFormationPlayers[i].Card.SetActive(false);
            }
            
            m_pFormationPlayers[i].Info.SetActive(false);
        }
    }

    void UpdateCardInfo(ulong id,PlayerItem pPlayerItem)
    {
        if(pPlayerItem == null) return;
        if(IsMatchView())
        {
            MatchView pMatchView = m_pMainScene.GetInstance<MatchView>();
            Dictionary<int,List<ulong>> cardInfo = pMatchView.GetCurrentPlayerYRCardInfo();
            if(cardInfo[0].Contains(id))
            {
                pPlayerItem.Card.SetActive(true);
                pPlayerItem.Red.SetActive(true);
                pPlayerItem.Yellow.SetActive(false);
            }
            else if(cardInfo[1].Contains(id))
            {
                pPlayerItem.Card.SetActive(true);
                pPlayerItem.Red.SetActive(false);
                pPlayerItem.Yellow.SetActive(true);
            }
            else
            {
                pPlayerItem.Card.SetActive(false);
            }

            List<ulong> injuryInfo = pMatchView.GetCurrentPlayerInjuryInfo();
            pPlayerItem.Injury.SetActive(injuryInfo.Contains(id));

            pPlayerItem.Add.SetActive(false);

            if(m_SubstitutionInList.Contains(id))
            {
                pPlayerItem.Add.SetActive(true);
                pPlayerItem.In.SetActive(true);
                pPlayerItem.Out.SetActive(false);
            }

            if(m_SubstitutionOutList.Contains(id))
            {
                pPlayerItem.Add.SetActive(true);
                pPlayerItem.In.SetActive(false);
                pPlayerItem.Out.SetActive(true);
            }
        }
    }

    void SetupFormationPlayerData(PlayerT pPlayer,PlayerItem pPlayerItem,E_LOCATION eLoc)
    {
        if(pPlayer == null || pPlayerItem == null) 
            return;

        pPlayerItem.Select.SetActive(false);
        pPlayerItem.HP.gameObject.SetActive(true);
        pPlayerItem.UpdatePlayerInfo(pPlayer);
        pPlayerItem.UpdateFormationPlayerData(pPlayer,eLoc);
        UpdateCardInfo(pPlayer.Id,pPlayerItem);
    }

    void UpdatePlayerTacticsScroll(int index)
    {
        Transform slot = null;
        PlayerTacticsT pPlayerTactics = m_pTempTactics.PlayerTactics[index];
        Transform tm = m_pTabUIList[(int)E_TAB.playerTactics].Find("control");
        for(int i =0; i < tm.childCount; ++i)
        {
            slot = tm.GetChild(i);
                
            if(slot)
            {
                UpdatePlayerTacticsByIndex(i,pPlayerTactics.Tactics[i]);
            }
        }
    }
    void ChangePlayerInfomation(bool bRight)
    {
        Transform tm = m_pTabUIList[(int)E_TAB.playerTactics].Find("playerInfo/information");
        tm.Find(eCurrentSelectPlayerType.ToString()).gameObject.SetActive(false);
        
        if(bRight )
        {
            ++eCurrentSelectPlayerType;
            if(eCurrentSelectPlayerType == E_TRAINING_TYPE.goalkeeping)
            {
                eCurrentSelectPlayerType = E_TRAINING_TYPE.attacking;
            }
        }
        else
        {
            --eCurrentSelectPlayerType;
            if((byte)eCurrentSelectPlayerType == 255)
            {
                eCurrentSelectPlayerType = E_TRAINING_TYPE.mentality;
            }
        }
        tm.Find(eCurrentSelectPlayerType.ToString()).gameObject.SetActive(true);
    }

    void UpdateBenchPlayer()
    {
        ResetScroll();
        
        if(m_TempLineupPlayer.Data.Count == 0) return;
        
        GameContext pGameContext = GameContext.getCtx();
        int i =0;
        int count = 0;
        PlayerT pPlayer = null;        
        BenchPlayer pBenchPlayer = null;
        m_pBenchPlayers.Clear();

        for(i =GameContext.CONST_NUMSTARTING; i < m_TempLineupPlayer.Data.Count; ++i)
        {
            pPlayer = pGameContext.GetPlayerByID(m_TempLineupPlayer.Data[i]);
                
            pBenchPlayer = new BenchPlayer(pPlayer.Id,pPlayer.Surname,i,pPlayer.Position,pPlayer.Hp);
            m_pBenchPlayers.Add(pBenchPlayer);
            if(count < m_pBenchPlayerItems.Count)
            {
                m_pBenchPlayerItems[count].UpdatePlayerData(m_pBenchPlayers[count]);
                m_pBenchPlayerItems[count].ResetTag(m_SubstitutionList.Contains(m_TempLineupPlayer.Data[i]));
            }
            ++count;
        }
        
        if(!m_pMainScene.IsMatch())
        {
            List<PlayerT> pTotalPlayerList = pGameContext.GetTotalPlayerList();

            if(m_TempLineupPlayer.Data.Count < pTotalPlayerList.Count)
            {
                for(i =0; i < pTotalPlayerList.Count; ++i)
                {                
                    if(m_TempLineupPlayer.Data.Contains(pTotalPlayerList[i].Id) || pTotalPlayerList[i].Status != 0)
                    {
                        continue;
                    }

                    pBenchPlayer = new BenchPlayer(pTotalPlayerList[i].Id,pTotalPlayerList[i].Surname,GameContext.CONST_NUMPLAYER,pTotalPlayerList[i].Position,pTotalPlayerList[i].Hp);
                    m_pBenchPlayers.Add(pBenchPlayer);
            
                    if(count < m_pBenchPlayerItems.Count)
                    {
                        m_pBenchPlayerItems[count].UpdatePlayerData(m_pBenchPlayers[count]);
                        m_pBenchPlayerItems[count].ResetTag(false);
                    }
                    ++count;
                }
            }
        }

        Vector2 size = m_pScrollRect.content.sizeDelta;
        size.x = m_pBenchPlayerItems[0].Target.rect.width * count;
        m_pScrollRect.content.sizeDelta = size;
        m_pScrollRect.horizontalNormalizedPosition = 0;
        m_pPrevDir.x = 0;
        m_pScrollRect.content.anchoredPosition = Vector2.zero;
    }

    public void SetupPlayerData()
    {
        ClearScroll();
        SetupScroll();
    }

    void SetupScroll()
    {
        GameContext pGameContext = GameContext.getCtx();
        m_iTotalScrollItems = 0;

        int total = pGameContext.GetTotalPlayerCount();

        if(total <= 0) return;

        m_iStartIndex = 0;
        float viewSize = m_pScrollRect.viewport.rect.width;
        float itemSize = 0;
        PlayerItem pPlayerItem = null;
        
        int i =0;
        float w = 0;
        RectTransform pItem = null;

        for(i =0; i < total - GameContext.CONST_NUMSTARTING; ++i)
        {
            if(viewSize > 0)
            {
                ++m_iTotalScrollItems;
            }

            if(viewSize > -itemSize)
            {
                pItem = LayoutManager.Instance.GetItem<RectTransform>(SCROLL_ITEM_NAME);
            
                if(pItem)
                {
                    pPlayerItem = new PlayerItem(pItem,SCROLL_ITEM_NAME);
                    m_pBenchPlayerItems.Add(pPlayerItem);
                    pPlayerItem.ResetTag(false);
                    pItem.SetParent(m_pScrollRect.content,false);
                    
                    pItem.localScale = Vector3.one;
                    pItem.anchoredPosition = new Vector2(w,0);
                    itemSize = pItem.rect.width;
                    w += itemSize;
                    viewSize -= itemSize;
                    pItem.gameObject.SetActive(viewSize > -itemSize);
                }
            }
            else
            {
                break;
            }
        }
        
        Vector2 size = m_pScrollRect.content.sizeDelta;
        size.x = w;
        m_pScrollRect.content.sizeDelta = size;
        m_pScrollRect.horizontalNormalizedPosition = 0;
        m_pPrevDir.x = 0;
        m_pScrollRect.content.anchoredPosition = Vector2.zero;
        LayoutManager.SetReciveUIScollViewEvent(m_pScrollRect,ScrollViewItemButtonEventCall);
    }

    void OnSlideValueChanged(int index,byte value)
    {
        PlayerTacticsT pPlayerTactics = m_pTempTactics.PlayerTactics[(int)m_pTempTactics.Formation[m_iCurrentSelectPlayerIndex]];
        pPlayerTactics.Tactics[index] = value;
        UpdatePlayerTacticsByIndex(index,value);
        if(m_pMainScene.IsMatch())
        {
            SteupCancelButton(true);
        }
    }

    public void CurrentClearScroll()
    {
        m_pSelectFormation = null;
    }
    void ClearScroll()
    {
        int i = m_pBenchPlayerItems.Count;
                
        while(i > 0)
        {
            --i;
            m_pBenchPlayerItems[i].Dispose();
        }

        m_pBenchPlayerItems.Clear();
        
        i = m_pBenchPlayers.Count;
                
        while(i > 0)
        {
            --i;
            m_pBenchPlayers[i].Dispose();
        }
        m_pBenchPlayers.Clear();
        LayoutManager.SetReciveUIScollViewEvent(m_pScrollRect,null);
        m_pScrollRect.content.anchoredPosition = Vector2.zero;
        Vector2 size = m_pScrollRect.content.sizeDelta;
        size.x =0;
        m_pScrollRect.content.sizeDelta = size;
    }
    void ResetScroll()
    {
        Vector2 pos;
        float viewSize = m_pScrollRect.viewport.rect.width;
        float itemSize = 0;
        PlayerItem pPlayerItem = null;
        for(int i = 0; i < m_pBenchPlayerItems.Count; ++i)
        {
            pPlayerItem = m_pBenchPlayerItems[i];
            itemSize = pPlayerItem.Target.rect.width;
            viewSize -= itemSize;
            pPlayerItem.Target.gameObject.SetActive(viewSize > -itemSize);

            pos = pPlayerItem.Target.anchoredPosition;            
            pos.x = i * itemSize;
            pPlayerItem.Target.anchoredPosition = pos;
        }

        m_pScrollRect.horizontalNormalizedPosition = 0;
        m_pScrollRect.content.anchoredPosition = Vector2.zero;
        m_iStartIndex = 0;
        m_pPrevDir.x = 0;
    }
    public void SetupView(bool bMatch)
    {
        m_pTabButtonList[(int)E_TAB.playerTactics].Find("lock").gameObject.SetActive(!GameContext.getCtx().IsLicenseContentsUnlock(E_CONST_TYPE.licenseContentsUnlock_1));

        RectTransform back = MainUI.Find("back").GetComponent<RectTransform>();
        if(bMatch)
        {
            Vector2 offset = m_OffsetMax;
            
            offset.y = m_OffsetMin.y;
            back.offsetMax = offset;
            offset.y *= -1;
            MainUI.offsetMax = offset;
            
            offset = m_OffsetMin;
            offset.y = 0;
            MainUI.offsetMin = offset;
        }
        else
        {
            MainUI.offsetMax = m_OffsetMax;
            MainUI.offsetMin = m_OffsetMin;
            back.offsetMax = Vector2.zero;
        }
    }

    void ShowMessageMaxSubstitutionCunt()
    {
        m_pMainScene.ShowMessagePopup(GameContext.getCtx().GetLocalizingText("MSG_TXT_MATCH_SUBSTITUTION_LIMIT"),GameContext.getCtx().GetLocalizingText("MSG_BTN_OKAY"));
    }

    void ShowMessageRedCard()
    {
        m_pMainScene.ShowMessagePopup(GameContext.getCtx().GetLocalizingText("MSG_TXT_MATCH_SUBSTITUTION_RED_CARD"),GameContext.getCtx().GetLocalizingText("MSG_BTN_OKAY"));
    }

    public void SetupData(TeamDataT pTeamData)
    {
        m_bChange = false;
        m_pMovePlayers[0].Target.gameObject.SetActive(false);
        m_pMovePlayers[1].Target.gameObject.SetActive(false);
        GameContext pGameContext = GameContext.getCtx();
        m_iActiveLineUpType = pGameContext.GetActiveLineUpType();
        m_uiCurrentOverall = pGameContext.GetTotalPlayerAbility(m_iActiveLineUpType);
        m_iActiveTacticsType = pGameContext.GetActiveTacticsType();

        m_TempLineupPlayer.Data.Clear();
        m_TempLineupPlayer.Type = (byte)m_iActiveLineUpType;
        List<ulong> list = null;
        int i = 0;
        m_SubstitutionList.Clear();
        eCurrentSelectPlayerType = E_TRAINING_TYPE.attacking;
        
        m_pOverallText.SetText(string.Format(pGameContext.GetLocalizingText(MAIN_TXT_OVERALL), string.Format(MAIN_TXT_OVERALL_TOKEN,m_uiCurrentOverall)));
        m_pDiffText.transform.parent.gameObject.SetActive(false);

        List<int> changeIconList = new List<int>();
        List<int> changedIconList = new List<int>();
        if(pTeamData == null)
        {
            list = pGameContext.GetLineupPlayerIdListByIndex(m_iActiveLineUpType);
            pGameContext.CloneTacticsDataByIndex(m_iActiveTacticsType,ref m_pTempTactics);
            
        }
        else
        {
            list = new List<ulong>();
            SingleFunc.CloneTacticsDataByIndex(pTeamData.Tactics,ref m_pTempTactics);
            
            for(i = 0; i < pTeamData.LineUp.Count; ++i)
            {
                list.Add(pTeamData.PlayerData[pTeamData.LineUp[i]].PlayerId);
            }

            if(pTeamData.Subst != null )
            {
                for(i = 0; i < pTeamData.Subst.Count; ++i)
                {
                    m_SubstitutionList.Add(pTeamData.PlayerData[pTeamData.Subst[i]].PlayerId);
                }
            }
        }

        bool bFail = false;

        for(i = 0; i < list.Count; ++i)
        {
            if(m_TempLineupPlayer.Data.Contains(list[i]))
            {
                list[i]=0;
                pGameContext.DefaultLineup(i,ref list);
                bFail = true;
            }
            m_TempLineupPlayer.Data.Add(list[i]);
        }

        if(bFail)
        {
            m_pMainScene.ShowMessagePopup(pGameContext.GetLocalizingText("MSG_TXT_LINEUP_ERROR"),pGameContext.GetLocalizingText("MSG_BTN_OKAY"));
        }

        ChangeSlotsPosition();

        Transform tm = m_pTabUIList[(int)E_TAB.formation];
        m_pRefresh.SetActive(m_bChangeSubstitution);
        m_pSlots.SetActive(!m_pMainScene.IsMatch());

        for(i = 0; i < m_SubstitutionInList.Length; ++i)
        {
            m_SubstitutionInList[i] = 0;
            m_SubstitutionOutList[i] = 0;
        }
        
        m_pRecommend.SetActive(!m_pMainScene.IsMatch());
        m_pReset.SetActive(!m_pMainScene.IsMatch());
        
        m_pChangeButton.gameObject.SetActive(m_pMainScene.IsMatch());
        m_pCancelButton.gameObject.SetActive(m_pMainScene.IsMatch());

        SteupCancelButton(false);
        SetupFormationPlayer();
        SetupTeamTactics();

        Transform info = m_pTabUIList[(int)E_TAB.playerTactics].Find("playerInfo/information");
        
        for(E_TRAINING_TYPE eType = E_TRAINING_TYPE.attacking; eType < E_TRAINING_TYPE.goalkeeping; ++eType)
        {
            tm = info.Find(eType.ToString());
            tm.gameObject.SetActive(eCurrentSelectPlayerType == eType);
        }
        
        UpdateBenchPlayer();
        
        if(IsMatchView())
        {
            MatchView pMatchView = m_pMainScene.GetInstance<MatchView>();
            Dictionary<int,List<ulong>> cardInfo = pMatchView.GetCurrentPlayerYRCardInfo();

            for(i= 0; i < GameContext.CONST_NUMSTARTING; ++i)
            {
                if(cardInfo[0].Contains(list[i]))
                {
                    m_pFormationPlayers[i].Card.SetActive(true);
                    m_pFormationPlayers[i].Red.SetActive(true);
                    m_pFormationPlayers[i].Yellow.SetActive(false);
                }
                else if(cardInfo[1].Contains(list[i]))
                {
                    m_pFormationPlayers[i].Card.SetActive(true);
                    m_pFormationPlayers[i].Red.SetActive(false);
                    m_pFormationPlayers[i].Yellow.SetActive(true);
                }
                else
                {
                    m_pFormationPlayers[i].Card.SetActive(false);
                }
            }

            List<ulong> injuryInfo = pMatchView.GetCurrentPlayerInjuryInfo();

            for(i= 0; i < GameContext.CONST_NUMSTARTING; ++i)
            {
                m_pFormationPlayers[i].Injury.SetActive(injuryInfo.Contains(list[i]));
            }
        }
        else
        {
            pGameContext.SetCacheLineupPlayerData(m_TempLineupPlayer);

            for(i= 0; i < GameContext.CONST_NUMSTARTING; ++i)
            {
                m_pFormationPlayers[i].Injury.SetActive(false);
            }
        } 
    }

    public void ShowTabUI(byte eTab)
    {
        int i = 0;
        for(i = 0; i < m_pTabUIList.Length; ++i)
        {
            m_pTabUIList[i].gameObject.SetActive(eTab == i);
            m_pTabButtonList[i].Find("on").gameObject.SetActive(eTab == i);
            m_pTabButtonList[i].Find("title").GetComponent<Graphic>().color = eTab == i ? Color.white : GameContext.GRAY;
        }
        
        ShowFeildSlots(false);
        
        m_eCurrentTab = (E_TAB)eTab;

        m_iCurrentSelectPlayerIndex = -1;
        if(m_eCurrentTab == E_TAB.playerTactics)
        {
            m_iCurrentSelectPlayerIndex = GameContext.CONST_NUMSTARTING -1;
            SetupPlayerTactics();
            GameContext pGameContext = GameContext.getCtx();
            PlayerTacticsT pPlayerTactics = m_pTempTactics.PlayerTactics[(int)m_pTempTactics.Formation[m_iCurrentSelectPlayerIndex]];
            Transform tm = m_pTabUIList[(int)m_eCurrentTab].Find("control");
            for(i = 0; i < tm.childCount; ++i)
            {
                UpdatePlayerTacticsByIndex(i,pPlayerTactics.Tactics[i]);
            }

            if(m_pMainScene.IsMatch() && !m_bChange)
            {
                SteupCancelButton(false);
            }
        }
    }

    bool CheckRedCardPlayer(ulong playerId)
    {
        if(IsMatchView())
        {
            MatchView pMatchView = m_pMainScene.GetInstance<MatchView>();
            Dictionary<int,List<ulong>> cardInfo = pMatchView.GetCurrentPlayerYRCardInfo();
            return cardInfo[0].Contains(playerId);
        }
        return false;
    }
    
    bool UpdateSelectFormation(Vector3 pos,int index, bool bFeild)
    {
        GameContext pGameContext = GameContext.getCtx();
        if(bFeild)
        {
            if(m_pSelectFormation.Index ==0 || index == 0)
            {
                return false;
            }

            m_pTempTactics.Formation[m_pSelectFormation.Index] = (byte)index;
            m_pSelectFormation.UpdateFormationPlayerData(pGameContext.GetPlayerByID(m_pSelectFormation.Id),(E_LOCATION)index);
            m_pSelectFormation.Target.position = pos;
            m_pFormationText.SetText(UpdateFormationText(m_pTempTactics.Formation));
            m_iCurrentSelectPlayerIndex = -1;
        }
        else
        {
            if(m_iCurrentSelectPlayerIndex > -1)
            {
                PlayerItem pPlayerItem = null;
                ulong id = m_pSelectFormation.Id;
                if(!m_pSelectFormation.Select.activeSelf)
                {
                    id = m_pBenchPlayers[m_iCurrentSelectPlayerIndex].Id;
                }
                else
                {
                    pPlayerItem = m_pSelectFormation;
                }
                
                if(CheckRedCardPlayer(id) || CheckRedCardPlayer(m_TempLineupPlayer.Data[index]))
                {
                    ShowMessageRedCard();
                    return false;
                }

                PlayerT pPlayer = pGameContext.GetPlayerByID(id);

                if((index == 0 && pPlayer.Position == 0) || (index != 0 && pPlayer.Position != 0))
                {
                    if(IsMatchView())
                    {
                        if(m_pMainScene.GetInstance<MatchView>().GetSubstitutionCunt() < 3)
                        {
                            if(!UpdateSubstitutionList(index,m_pSelectFormation,pPlayer.Id,m_TempLineupPlayer.Data[index]))
                            {
                                return false;
                            }
                        }
                        else
                        {
                            ShowMessageMaxSubstitutionCunt();
                            return false;
                        }
                    }
                    
                    SetupFormationPlayerData(pPlayer,m_pFormationPlayers[index],(E_LOCATION)m_pTempTactics.Formation[index]);

                    pPlayer = pGameContext.GetPlayerByID(m_TempLineupPlayer.Data[index]);
                    int i =0;
                    if(IsSubPlayer(id, ref i))
                    {
                        m_TempLineupPlayer.Data[i] = pPlayer.Id;
                    }

                    m_TempLineupPlayer.Data[index] = id;
                    
                    m_pBenchPlayers[m_iCurrentSelectPlayerIndex].UpdatePlayerData(pPlayer);
                    if(pPlayerItem != null)
                    {
                        pPlayerItem.UpdatePlayerData(m_pBenchPlayers[m_iCurrentSelectPlayerIndex]);
                        UpdateCardInfo(pPlayer.Id,pPlayerItem);
                    }
                    UpdateOverall();
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if(m_pSelectFormation.Index ==0 || m_pTempTactics.Formation[index] == 0) 
                {
                    return false;
                }
                
                if(IsMatchView())
                {
                    if(m_SubstitutionInList[m_pSelectFormation.Index] == m_pSelectFormation.Id)
                    {
                        m_SubstitutionInList[m_pSelectFormation.Index] = m_SubstitutionInList[index];
                        m_SubstitutionInList[index] = m_pSelectFormation.Id;

                        ulong temp = m_SubstitutionOutList[m_pSelectFormation.Index];
                        m_SubstitutionOutList[m_pSelectFormation.Index] = m_SubstitutionOutList[index];
                        m_SubstitutionOutList[index] = temp;
                    }
                    
                    m_pFormationPlayers[index].Out.SetActive(m_pSelectFormation.Out.activeSelf);
                    m_pFormationPlayers[index].In.SetActive(m_pSelectFormation.In.activeSelf);
                    m_pFormationPlayers[index].Add.SetActive(m_pSelectFormation.Add.activeSelf);
                    m_pFormationPlayers[m_pSelectFormation.Index].Add.SetActive(false);
                }
                
                ulong id = m_pSelectFormation.Id;
                SetupFormationPlayerData(pGameContext.GetPlayerByID(m_TempLineupPlayer.Data[index]),m_pSelectFormation,(E_LOCATION)m_pTempTactics.Formation[m_pSelectFormation.Index]);
                SetupFormationPlayerData(pGameContext.GetPlayerByID(id),m_pFormationPlayers[index],(E_LOCATION)m_pTempTactics.Formation[index]);
                
                m_TempLineupPlayer.Data[m_pSelectFormation.Index] = m_TempLineupPlayer.Data[index];
                m_TempLineupPlayer.Data[index] = id;
            }
        }
        
        return true;
    }

    public void OnChangeSubstitution(bool bChange)
    {
        m_bChangeSubstitution = bChange;
    }

    public bool IsChangeSubstitution()
    {
        return m_bChangeSubstitution;
    }

    void SteupCancelButton(bool bActive)
    {
        m_pCancelButton.enabled = bActive;
        m_pCancelButton.transform.Find("on").gameObject.SetActive(m_pCancelButton.enabled);
        m_pCancelButton.transform.Find("off").gameObject.SetActive(!m_pCancelButton.enabled);
    }

    void ClearSubstitutionList()
    {
        int i =0;
        for(i =0; i < m_SubstitutionInList.Length; ++i)
        {
            m_pFormationPlayers[i].Add.SetActive(false);
            m_SubstitutionOutList[i] = 0;
            m_SubstitutionInList[i] = 0;
        }
        
        for(i =0; i < m_pFormationPlayers.Length; ++i)
        {
            m_pFormationPlayers[i].Add.SetActive(false);
        }
        m_bChange = false;
        SteupCancelButton(false);
    }
    bool UpdateSubstitutionList(int index,PlayerItem pPlayerItem, ulong _in,ulong _out)
    {
        bool bReset = false;
        MatchView pMatchView = m_pMainScene.GetInstance<MatchView>();
        if(pMatchView.IsCurrentActivePlayer(_in))
        {
            if(m_SubstitutionInList[index] != _out)
            {
                m_pMainScene.ShowMessagePopup(GameContext.getCtx().GetLocalizingText("MSG_TXT_MATCH_SUBSTITUTION_LOCKED"),GameContext.getCtx().GetLocalizingText("MSG_BTN_OKAY"));
                ClearSelectPlayer();
                return false;
            }
            else
            {
                if(m_SubstitutionOutList[index] != _in)
                {
                    m_pMainScene.ShowMessagePopup(GameContext.getCtx().GetLocalizingText("MSG_TXT_MATCH_SUBSTITUTION_LOCKED"),GameContext.getCtx().GetLocalizingText("MSG_BTN_OKAY"));
                    ClearSelectPlayer();
                    return false;
                }
                bReset = true;
                ulong temp = _out;
                _out = _in;
                _in = temp;
                pPlayerItem.Add.SetActive(false);
                pPlayerItem.In.SetActive(false);
                pPlayerItem.Out.SetActive(false);
                UpdateChangeButton(IsMatchView());
            }
        }

        int i =0;
        byte count =0;
        
        for(i =0; i < m_SubstitutionInList.Length; ++i)
        {
            if(m_SubstitutionInList[i] == _in)
            {
                m_SubstitutionInList[i] = 0;
                m_pFormationPlayers[i].Add.SetActive(false);

                for(int n =0; n < m_pBenchPlayerItems.Count; ++n)
                {
                    if(m_pBenchPlayerItems[n].Id == m_SubstitutionOutList[i])
                    {
                        m_pBenchPlayerItems[n].Add.SetActive(false);
                        m_pBenchPlayerItems[n].In.SetActive(false);
                        m_pBenchPlayerItems[n].Out.SetActive(false);
                        break;
                    }
                }
                m_SubstitutionOutList[i] = 0;
            }
            
            if(m_SubstitutionInList[i] > 0)
            {
                ++count;
            }
        }
        
        if(bReset) return true;
        
        count += pMatchView.GetSubstitutionCunt();
        
        if( count > 2)
        {
            ShowMessageMaxSubstitutionCunt();
            return false;
        }
        
        pPlayerItem.In.SetActive(false);
        if(pMatchView.IsCurrentActivePlayer(_out))
        {
            m_SubstitutionOutList[index] = _out;        
            pPlayerItem.Add.SetActive(true);
            pPlayerItem.Out.SetActive(true);
        }
        else
        {
            pPlayerItem.Add.SetActive(false);
            pPlayerItem.Out.SetActive(false);
        }

        m_SubstitutionInList[index] = _in;
        
        m_pFormationPlayers[index].Add.SetActive(true);
        m_pFormationPlayers[index].Out.SetActive(false);
        m_pFormationPlayers[index].In.SetActive(true);

        UpdateChangeButton(IsMatchView());
        return true;
    }

    void UpdateChangeButton(bool bMatchView)
    {
        bool bChange = false;
        if(bMatchView)
        {
            bChange = m_pMainScene.GetInstance<MatchView>().CheckChangeFormationData(m_pTempTactics);
        }
        else
        {
            bChange = GameContext.getCtx().CheckChangeFormationDataByIndex(m_iActiveTacticsType,m_pTempTactics);
        }
        
        if(!bChange)
        {
            bChange = CheckChangeLineupData(bMatchView);
        }
        if(bChange)
        {
            m_bChange = true;
        }

        SteupCancelButton(bChange);
    }

    bool IsMatchView()
    {
        return m_pMainScene.IsShowInstance<MatchView>();
    }

    bool CheckChangeSubstitutionData()
    {
        for(int i =0; i < m_SubstitutionInList.Length; ++i)
        {
            if(m_SubstitutionOutList[i] != 0 || m_SubstitutionInList[i] != 0)
            {
                return true;
            }
        }

        return false;
    }

    bool CheckChangeLineupData(bool isMatch)
    {
        if(isMatch && CheckChangeSubstitutionData())
        {
            return true;
        }
       
        return GameContext.getCtx().CheckChangeLineupDataByIndex(m_iActiveLineUpType,m_TempLineupPlayer.Data);
    }

    public void UpdatePlayerHP()
    {
        if(m_eCurrentTab == E_TAB.formation && !m_pMainScene.IsMatch())
        {
            if(m_pBenchPlayerItems == null) return;

            GameContext pGameContext = GameContext.getCtx();
            PlayerT pPlayer = null;
            int i =0;
            PlayerItem pPlayerItem = null;
            
            for(i =0; i < m_pBenchPlayerItems.Count; ++i)
            {
                pPlayerItem = m_pBenchPlayerItems[i];
                pPlayer = pGameContext.GetPlayerByID(pPlayerItem.Id);
                if(pPlayer == null) continue;
                pPlayerItem.UpdateHP((float)pPlayer.Hp / 100f);
            }

            for(i =0; i < m_pFormationPlayers.Length; ++i)
            {
                pPlayerItem = m_pFormationPlayers[i];
                pPlayer = pGameContext.GetPlayerByID(pPlayerItem.Id);
                if(pPlayer == null) continue;
                pPlayerItem.UpdateHP((float)pPlayer.Hp / 100f);
            }
        }
    }

    public void UpdateAllPlayerData()
    {
        GameContext pGameContext = GameContext.getCtx();
        
        PlayerItem pPlayerItem = null;
        int i = 0;
        for(i = 0; i < m_pBenchPlayerItems.Count; ++i)
        {
            pPlayerItem = m_pBenchPlayerItems[i];
            pPlayerItem.UpdatePlayerInfo(pGameContext.GetPlayerByID(pPlayerItem.Id));
        }

        for(i = 0; i < m_pFormationPlayers.Length; ++i)
        {
            pPlayerItem = m_pFormationPlayers[i];
            pPlayerItem.UpdatePlayerInfo(pGameContext.GetPlayerByID(pPlayerItem.Id));
        }
    }

    public JObject MakeLineupData()
    {
        GameContext pGameContext = GameContext.getCtx();
        ALFUtils.Assert(pGameContext.CheckSameLineupData(m_TempLineupPlayer.Data), "MakeLineupData m_TempLineupPlayer same data!!!");
        JObject json = new JObject();
        json["type"] = m_iActiveLineUpType;
        JArray jArray = new JArray();
        uint total =0;
        PlayerT pPlayer = null;

        for(int i = 0; i < m_TempLineupPlayer.Data.Count; ++i)
        {
            pPlayer = pGameContext.GetPlayerByID(m_TempLineupPlayer.Data[i]);
            if( i < GameContext.CONST_NUMSTARTING)
            {
                total += pPlayer.AbilityWeightSum;
            }
            jArray.Add(m_TempLineupPlayer.Data[i]);
        }
        
        json["data"] = jArray.ToString(Newtonsoft.Json.Formatting.None);
        json["squadPower"] = total;
        json["playerCount"] = pGameContext.GetTotalPlayerCount();
        json["totalValue"] = pGameContext.GetTotalPlayerValue(0);
        json["countQualified"] = pGameContext.GetTotalPlayerNAbilityTier(null,true);
        json["avgAge"] = pGameContext.GetPlayerAvgAge(null,true);

        return json;
    }
    public JObject MakeTacticsData(int type)
    {
        JObject json = new JObject();
        json["type"] = type;
        List<byte> temp = m_pTempTactics.Formation;
        m_pTempTactics.Formation = null;
        string data = Newtonsoft.Json.JsonConvert.SerializeObject(m_pTempTactics, Newtonsoft.Json.Formatting.None);
        data = data.Replace("\"formation\":null,","");
        json["data"] = data;
        json["formation"] = Newtonsoft.Json.JsonConvert.SerializeObject(temp, Newtonsoft.Json.Formatting.None);
        m_pTempTactics.Formation = temp;
        return json;
    }

    public bool CheckChangeTacticsData(bool isMatch)
    {
        if(isMatch)
        {
            return m_pMainScene.GetInstance<MatchView>().CheckChangeTacticsData(m_pTempTactics);
        }
        
        return GameContext.getCtx().CheckChangeTacticsDataByIndex(m_iActiveTacticsType,m_pTempTactics);
    }

    bool executeRollingNumberCallback(IState state,float dt,bool bEnd)
    {
        if( state.StateData is RollingNumberStateData data)
        {
            GameContext pGameContext = GameContext.getCtx();
            int current = (int)(data.Add * state.GetCondition<TimeOutCondition>().GetTimePercent());
            m_pOverallText.SetText(string.Format(pGameContext.GetLocalizingText(MAIN_TXT_OVERALL), string.Format(MAIN_TXT_OVERALL_TOKEN,data.Current + current)));
        }
        return bEnd;
    }
    IState exitRollingNumberCallback(IState state)
    {
        GameContext pGameContext = GameContext.getCtx();
        m_pOverallText.SetText(string.Format(pGameContext.GetLocalizingText(MAIN_TXT_OVERALL), string.Format(MAIN_TXT_OVERALL_TOKEN,m_uiCurrentOverall)));
        m_pDiffText.transform.parent.gameObject.SetActive(false);
        m_pRollingNumberState = null;
        return null;
    }

    void UpdateOverall()
    {
        if(m_pRollingNumberState != null)
        {
            m_pRollingNumberState.Exit(true);
        }

        uint total = 0;
        PlayerT pPlayer = null;
        GameContext pGameContext = GameContext.getCtx();
        for(int i =0; i < GameContext.CONST_NUMSTARTING; ++i)
        {
            pPlayer = pGameContext.GetPlayerByID(m_TempLineupPlayer.Data[i]);
            total += pPlayer.AbilityWeightSum;
        }
        
        int diff = (int)(total - m_uiCurrentOverall);
        
        m_pDiffText.transform.parent.gameObject.SetActive(true);
        m_pDiffText.SetText( diff > 0 ? $"+{diff}" : $"{diff}");

        if(diff == 0) 
        {
            m_pRollingNumberState = BaseState.GetInstance(new BaseStateTarget(m_pOverallText),0.5f, (uint)E_STATE_TYPE.Timer, null,null, this.exitRollingNumberCallback);
        }
        else
        {
            m_pRollingNumberState = BaseState.GetInstance(new BaseStateTarget(m_pOverallText),1, (uint)E_STATE_TYPE.Timer, null,this.executeRollingNumberCallback, this.exitRollingNumberCallback);
            m_pRollingNumberState.StateData = new RollingNumberStateData((double)total,(double)m_uiCurrentOverall,(double)diff);
        }

        StateMachine.GetStateMachine().AddState(m_pRollingNumberState);
        m_uiCurrentOverall = total;
    }

    bool IsSubPlayer(ulong id,ref int index)
    {
        int i = GameContext.CONST_NUMSTARTING;
        index = -1;
        for(i =GameContext.CONST_NUMSTARTING; i < m_TempLineupPlayer.Data.Count; ++i)
        {
            if(m_TempLineupPlayer.Data[i] == id)
            {
                index = i;
                return true;
            }
        }

        return false;
    }

    void SaveLineupData()
    {
        GameContext pGameContext = GameContext.getCtx();
        for(int i =0; i < m_TempLineupPlayer.Data.Count; ++i)
        {
            pGameContext.ChangeLineupPlayerIdByIndex(m_iActiveLineUpType,i,m_TempLineupPlayer.Data[i]);
        }
    }

    void SaveTacticsData()
    {
        GameContext.getCtx().ChangeTacticsDataByIndex(m_iActiveLineUpType,m_pTempTactics);
    }

    public void Close()
    {
        GameContext.getCtx().ClearCacheLineupPlayerData();
        m_pSelectFormation = null;
        
        if(m_pRollingNumberState != null)
        {
            m_pRollingNumberState.Exit(true);
        }
        if(!m_pMainScene.IsMatch())
        {
            m_pMainScene.ShowMainUI(true);

            if(CheckChangeLineupData(false))
            {
                m_pMainScene.RequestAfterCall(E_REQUEST_ID.lineup_put,MakeLineupData());
                SaveLineupData();
            }
            if(CheckChangeTacticsData(false))
            {
                m_pMainScene.RequestAfterCall(E_REQUEST_ID.tactics_put,MakeTacticsData(m_iActiveTacticsType));
                SaveTacticsData();
            }
        }
        else
        {
            bool isMatchView = IsMatchView();
            if(isMatchView && !m_pMainScene.GetInstance<MatchView>().IsPauseMatchButton())
            {
                m_pMainScene.GetInstance<MatchView>().ResumeMatch();
            }

            ChangeMatchTacticsData(isMatchView);
        }
        
        m_pMainScene.HideMoveDilog(MainUI,Vector3.right);
    }

    void ChangeMatchTacticsData(bool isMatchView)
    {
        int i = 0;
        bool bChangeSubstitution = CheckChangeSubstitutionData();
        if(bChangeSubstitution || CheckChangeLineupData(isMatchView) || CheckChangeTacticsData(isMatchView))
        {
            if(isMatchView)
            {
                m_pMainScene.UpdateMatchTacticsData(m_pTempTactics, bChangeSubstitution ? m_SubstitutionInList : null,m_TempLineupPlayer);
            }
            else
            {
                m_pMainScene.UpdateMatchData(m_pTempTactics,m_TempLineupPlayer);
            }
        }

        for(i = 0; i < m_SubstitutionInList.Length; ++i)
        {
            m_SubstitutionInList[i] = 0;
            m_SubstitutionOutList[i] = 0;
        }

        for(i = 0; i < m_pFormationPlayers.Length; ++i)
        {
            m_pFormationPlayers[i].Add.SetActive(false);
        }
    }

    void ChangeSlotsPosition()
    {
        int i = (int)E_LOCATION.LOC_DCL;
        int e = (int)E_LOCATION.LOC_DR;
        int n = i;
        bool bChange = false;
        Vector2 pos;
        while(e < (int)E_LOCATION.LOC_END)
        {
            bChange = false;
            
            if(m_pTempTactics.Formation.Contains((byte)(i+1)))
            {
                bChange = true;
            }
            else
            {
                if(!m_pTempTactics.Formation.Contains((byte)i) && !m_pTempTactics.Formation.Contains((byte)(i+2)))
                {
                    bChange = true;
                }   
            }
            
            if(bChange)
            {
                pos = m_pFeildSlots[n].anchoredPosition;
                pos.x = m_fFeildSlotXList[0];
                m_pFeildSlots[n].anchoredPosition = pos;
                m_pFamiliarsList[n].anchoredPosition = pos;
                n+=2;
                pos = m_pFeildSlots[n].anchoredPosition;
                pos.x = m_fFeildSlotXList[1];      
                m_pFeildSlots[n].anchoredPosition = pos;
                m_pFamiliarsList[n].anchoredPosition = pos;
            }
            else
            {
                pos = m_pFeildSlots[n].anchoredPosition;
                pos.x = m_fFeildSlotXList[0] + 35;
                m_pFeildSlots[n].anchoredPosition = pos;
                m_pFamiliarsList[n].anchoredPosition = pos;
                n+=2;
                pos = m_pFeildSlots[n].anchoredPosition;
                pos.x = m_fFeildSlotXList[1] - 35;
                m_pFeildSlots[n].anchoredPosition = pos;
                m_pFamiliarsList[n].anchoredPosition = pos;
            }

            i += 5;
            e += 5;
            n = i;
        }

        for(i =0; i < m_pFormationPlayers.Length; ++i)
        {
            m_pFormationPlayers[i].Target.position = m_pFeildSlots[(int)m_pTempTactics.Formation[i]].position;
        }
    }
    void ShowFeildSlots(bool bShow)
    {
        m_pFeildSlots[0].parent.gameObject.SetActive(bShow);
        m_pFamiliarsList[0].parent.gameObject.SetActive(bShow);
        Animation[] list = m_pFamiliarsList[0].parent.GetComponentsInChildren<Animation>(!bShow);
        if(bShow)
        {
            for(int i =0; i < list.Length; ++i)
            {
                list[i].Play();
            }
        }
        else
        {
            for(int i =0; i < list.Length; ++i)
            {
                list[i].Stop();
            }
        }
    }

    bool executeMoveCallback(IState state,float dt,bool bEnd)
    {
        if( state.StateData is MoveStateData data)
        {
            BaseStateTarget target = state.GetTarget<BaseStateTarget>();
            Transform tm = target.GetMainTarget<Transform>();
            TimeOutCondition condition = state.GetCondition<TimeOutCondition>();
            float p = condition.GetTimePercent();
            Vector2 pos = data.Original + ( data.Direction * (data.Distance * p));
            tm.position = pos;
        }
        return bEnd;
    }
    IState exitMoveCallback(IState state)
    {
        if( state.StateData is MoveStateData data)
        {
            BaseStateTarget target = state.GetTarget<BaseStateTarget>();
            RectTransform tm = target.GetMainTarget<RectTransform>();
            tm.gameObject.SetActive(false);
            if(data.TargetNode != null)
            {
                data.TargetNode.gameObject.SetActive(true);
            }
            Enable = true;
            LayoutManager.Instance.InteractableEnabledAll();
        }

        return null;
    }

    void UpdateFamiliarsList(ulong id)
    {
        int index =0;
        byte value = 0;
        GameContext pGameContext = GameContext.getCtx();
        PlayerT pPlayer = pGameContext.GetPlayerByID(id);
        int i =0;
        for(i =0; i < m_pFamiliarsList.Length; ++i)
        {
            m_pFamiliarsList[i].Find("c1").gameObject.SetActive(false);
            m_pFamiliarsList[i].Find("c2").gameObject.SetActive(false);
            m_pFamiliarsList[i].Find("c3").gameObject.SetActive(false);
            m_pFamiliarsList[i].Find("c4").gameObject.SetActive(false);
            m_pFamiliarsList[i].gameObject.SetActive(true);
        }

        for(i =0; i < m_pFamiliarsList.Length; ++i)
        {
            index = pGameContext.ConvertPositionByTag((E_LOCATION)i);
            value = pPlayer.PositionFamiliars[index];
            
            if(value > 0)
            {
                if(value >= 90)
                {
                    m_pFamiliarsList[i].Find("c4").gameObject.SetActive(true);
                }
                else if(value >= 60)
                {
                    m_pFamiliarsList[i].Find("c3").gameObject.SetActive(true);
                }
                else if(value >= 30)
                {
                    m_pFamiliarsList[i].Find("c2").gameObject.SetActive(true);
                }
                else
                {
                    m_pFamiliarsList[i].Find("c1").gameObject.SetActive(true);
                }
            }
        }
    }

    public RectTransform GetTutorialFormation()
    {
        if(m_pFormationPlayers != null)
        {
            return m_pFormationPlayers[m_pFormationPlayers.Length -1].Target;
        }
        return null;
    }

    public void SetTutorial()
    {
        byte eStep = GameContext.getCtx().GetTutorial();
        if(eStep == 19)
        {
            Transform tm  = m_pBenchPlayerItems[m_pBenchPlayerItems.Count - 2].Target;
            ScrollViewItemButtonEventCall(m_pScrollRect,tm,tm.gameObject);
        }
        else if(eStep == 20)
        {
            BoardClick(m_pFormationPlayers[m_pFormationPlayers.Length -1]);
        }
        else if(eStep == 21)
        {
            Close();
        }
    }
    void ScrollViewChangeData(int index,int iTarget,int iCount)
    {
        if(index == iTarget) return;
        int i = 0;
        PlayerItem pPlayerItem = null;
        if(index > iTarget)
        {
            pPlayerItem = m_pBenchPlayerItems[iTarget];
            m_pBenchPlayerItems[iTarget] = m_pBenchPlayerItems[index];
            i = iTarget +1;
            PlayerItem pTemp = null;
            while(i < index)
            {
                pTemp = m_pBenchPlayerItems[i];
                m_pBenchPlayerItems[i] = pPlayerItem;
                pPlayerItem = pTemp;
                ++i;
            }
            m_pBenchPlayerItems[index] = pPlayerItem;
            pPlayerItem = m_pBenchPlayerItems[iTarget];
        }
        else
        {
            pPlayerItem = m_pBenchPlayerItems[index];
            i = index +1;
            while(i <= iTarget)
            {
                m_pBenchPlayerItems[i -1] = m_pBenchPlayerItems[i];
                ++i;
            }

            m_pBenchPlayerItems[iTarget] = pPlayerItem;
        }
        
        i = m_iStartIndex + iTarget + iCount;

        if(i < 0 || m_pBenchPlayers.Count <= i) return;

        pPlayerItem.UpdatePlayerData(m_pBenchPlayers[i]);
        if(m_iCurrentSelectPlayerIndex > -1 )
        {
            if(m_iCurrentSelectPlayerIndex == i)
            {
                m_pSelectFormation = pPlayerItem;
                m_pSelectFormation.Select.SetActive(true);
                m_pSelectFormation.Info.SetActive(true);
            }
            else
            {
                pPlayerItem.Select.SetActive(false);
                pPlayerItem.Info.SetActive(false);
            }
        }
    }

    void ScrollViewChangeValueEventCall( Vector2 value)
    {
        if(m_iTotalScrollItems < m_pBenchPlayers.Count && value.x != m_pPrevDir.x)
        {
            m_pScrollRect.ScrollViewChangeValue(value - m_pPrevDir,ref m_iStartIndex,ScrollViewChangeData);
            m_pPrevDir = value;
        }
    }

    void ClearSelectPlayer()
    {
        if(m_pSelectFormation != null)
        {
            m_pSelectFormation.Select.SetActive(false);
            m_pSelectFormation.Info.SetActive(false);
        }
        
        m_pSelectFormation = null;
        m_iCurrentSelectPlayerIndex = -1;
    }
    void ScrollViewItemButtonEventCall(ScrollRect root, Transform tm,GameObject sender)
    {
        if(m_pSelectFormation == null)
        {
            for(int i =0; i < m_pBenchPlayerItems.Count; ++i)
            {
                if(m_pBenchPlayerItems[i].Target == tm)
                {
                    m_iCurrentSelectPlayerIndex = m_iStartIndex + i;
                    m_pSelectFormation = m_pBenchPlayerItems[i];
                    m_pSelectFormation.Select.SetActive(true);
                    m_pSelectFormation.Info.SetActive(true);

                    UpdateFamiliarsList(m_pSelectFormation.Id);
                    i =0;
                    for(i =0; i < m_pFamiliarsList.Length; ++i)
                    {
                        m_pFamiliarsList[i].gameObject.SetActive(false);
                    }

                    for(i =0; i < m_pTempTactics.Formation.Count; ++i)
                    {
                        m_pFamiliarsList[(int)m_pTempTactics.Formation[i]].gameObject.SetActive(true);
                    }

                    break;
                }
            }
            
            m_pFamiliarsList[0].parent.gameObject.SetActive(true);
        }
        else
        {
            ShowFeildSlots(false);

            if(CheckRedCardPlayer(m_pSelectFormation.Id))
            {
                ShowMessageRedCard();
                ClearSelectPlayer();
                return;
            }

            PlayerItem pPlayerItem = null;
            bool bFind = false;

            if(m_iCurrentSelectPlayerIndex > -1)
            {
                if(m_pSelectFormation.Select.activeSelf && tm == m_pSelectFormation.Target)
                {
                    ShowPlayerInfo(m_pSelectFormation.Id);
                    ClearSelectPlayer();
                    return;
                }
                else
                {
                    bFind = true;
                }
            }
            else
            {
                bFind = true;
            }
            int iSelectPlayerIndex = -1;
            if(bFind)
            {
                for(int i =0; i < m_pBenchPlayerItems.Count; ++i)
                {
                    if(m_pBenchPlayerItems[i].Target == tm)
                    {
                        pPlayerItem = m_pBenchPlayerItems[i];
                        iSelectPlayerIndex = i;
                        break;
                    }
                }
                if(pPlayerItem == null)
                {
                    ClearSelectPlayer();
                    return;
                }
            }

            if(CheckRedCardPlayer(pPlayerItem.Id))
            {
                ShowMessageRedCard();
                ClearSelectPlayer();
                return;
            }
            
            GameContext pGameContext = GameContext.getCtx();
            PlayerT pPlayer = pGameContext.GetPlayerByID(pPlayerItem.Id);
            PlayerT pOldSelectPlayer = null;
            
            bool isMatchView = IsMatchView();

            if(m_iCurrentSelectPlayerIndex > -1)
            {
                if(!isMatchView)
                {
                    Transform target = null;
                    ulong uSelectId = m_pBenchPlayers[m_iCurrentSelectPlayerIndex].Id;
                    Vector3 endSpot;
                    if(m_pSelectFormation.Select.activeSelf)
                    {
                        m_pMovePlayers[0].Target.position = m_pSelectFormation.Icon.transform.position;
                        m_pMovePlayers[0].CloneInfoUI(m_pSelectFormation);
                        target = m_pSelectFormation.Target;
                        endSpot = m_pSelectFormation.Icon.transform.position;
                        m_pSelectFormation.Target.gameObject.SetActive(false);
                    }
                    else
                    {
                        int index = m_iCurrentSelectPlayerIndex - m_iStartIndex > 0 ? m_pBenchPlayerItems.Count -1 : 0;
                        if(index == 0)
                        {
                            m_pMovePlayers[0].Target.position = m_pBenchPlayerItems[index].Target.position - new Vector3(m_pBenchPlayerItems[index].Target.rect.width,0);
                        }
                        else
                        {
                            m_pMovePlayers[0].Target.position = m_pBenchPlayerItems[index].Target.position + new Vector3(m_pBenchPlayerItems[index].Target.rect.width,0);
                        }

                        m_pMovePlayers[0].UpdatePlayerData(m_pBenchPlayers[m_iCurrentSelectPlayerIndex]);
                        endSpot = m_pMovePlayers[0].Target.position;
                    }
                    
                    pOldSelectPlayer = pGameContext.GetPlayerByID(uSelectId);

                    int i = -1;
                    IsSubPlayer(pPlayer.Id, ref i);
                    int ii = -1;
                    IsSubPlayer(pOldSelectPlayer.Id, ref ii);
                    if(i > -1)
                    {
                        m_TempLineupPlayer.Data[i] = pOldSelectPlayer.Id;
                    }

                    if(ii > -1)
                    {
                        m_TempLineupPlayer.Data[ii] = pPlayer.Id;
                    }

                    m_pMovePlayers[0].Target.gameObject.SetActive(true);
                    m_pMovePlayers[1].Target.gameObject.SetActive(true);
                    
                    m_pMovePlayers[1].CloneInfoUI(pPlayerItem);
                    m_pMovePlayers[1].Target.position = pPlayerItem.Icon.transform.position;
                    
                    MoveAnimation(m_pMovePlayers[0].Target,m_pMovePlayers[1].Target.position,target);
                    MoveAnimation(m_pMovePlayers[1].Target,endSpot,pPlayerItem.Target);
                    
                    BenchPlayer pBenchPlayer = m_pBenchPlayers[m_iCurrentSelectPlayerIndex];
                    i = pBenchPlayer.IPlay;
                    ii = m_pBenchPlayers[m_iStartIndex + iSelectPlayerIndex].IPlay;
                    pBenchPlayer.SetIPlay(ii);
                    m_pBenchPlayers[m_iStartIndex + iSelectPlayerIndex].SetIPlay(i);
                    m_pBenchPlayers[m_iCurrentSelectPlayerIndex] = m_pBenchPlayers[m_iStartIndex + iSelectPlayerIndex];
                    m_pBenchPlayers[m_iStartIndex + iSelectPlayerIndex] = pBenchPlayer;

                    if(target != null)
                    {
                        m_pSelectFormation.UpdatePlayerData(m_pBenchPlayers[m_iCurrentSelectPlayerIndex]);
                    }
                    
                    pPlayerItem.UpdatePlayerData(pBenchPlayer);
                    
                    pPlayerItem.Target.gameObject.SetActive(false);
                }
            }
            else
            {
                if((m_pSelectFormation.Index != 0 && pPlayer.Position != 0) || (m_pSelectFormation.Index == 0 && pPlayer.Position == 0)) 
                {
                    if(isMatchView)
                    {
                        if(m_pMainScene.GetInstance<MatchView>().GetSubstitutionCunt() < 3)
                        {
                            if(!UpdateSubstitutionList(m_pSelectFormation.Index,m_pSelectFormation,pPlayer.Id,m_TempLineupPlayer.Data[m_pSelectFormation.Index]))
                            {
                                ClearSelectPlayer();
                                return;
                            }
                        }
                        else
                        {
                            ShowMessageMaxSubstitutionCunt();
                        }
                    }
                    
                    pOldSelectPlayer = pGameContext.GetPlayerByID(m_pSelectFormation.Id);
                    SetupFormationPlayerData(pPlayer,m_pSelectFormation,(E_LOCATION)m_pTempTactics.Formation[m_pSelectFormation.Index]);

                    int i = -1;
                    if(IsSubPlayer(pPlayer.Id, ref i))
                    {
                        m_TempLineupPlayer.Data[i] = pOldSelectPlayer.Id;
                    }
                    
                    m_TempLineupPlayer.Data[m_pSelectFormation.Index] = pPlayer.Id;

                    m_pMovePlayers[0].CloneInfoUI(m_pSelectFormation);
                    m_pMovePlayers[1].CloneInfoUI(pPlayerItem);

                    m_pMovePlayers[0].Target.gameObject.SetActive(true);
                    m_pMovePlayers[1].Target.gameObject.SetActive(true);
                    m_pMovePlayers[0].Target.position = m_pSelectFormation.Icon.transform.position;
                    m_pMovePlayers[1].Target.position = pPlayerItem.Icon.transform.position;
                    MoveAnimation(m_pMovePlayers[0].Target,pPlayerItem.Icon.transform.position,m_pSelectFormation.Target);
                    MoveAnimation(m_pMovePlayers[1].Target,m_pSelectFormation.Icon.transform.position,pPlayerItem.Target);
                    m_pSelectFormation.Target.gameObject.SetActive(false);
                    pPlayerItem.Target.gameObject.SetActive(false);

                    UpdateOverall();

                    if(m_pMainScene.IsMatch())
                    {
                        UpdateChangeButton(isMatchView);
                    }
                    
                    m_pBenchPlayers[m_iStartIndex + iSelectPlayerIndex].UpdatePlayerData(pOldSelectPlayer);
                    UpdateCardInfo(pOldSelectPlayer.Id,pPlayerItem);
                    pPlayerItem.UpdatePlayerData(m_pBenchPlayers[m_iStartIndex + iSelectPlayerIndex]);
                }
            }

            ClearSelectPlayer();
        }
    }

    void MoveAnimation(Transform m1, Vector3 endSpot,Transform target)
    {
        if(m1 != null && endSpot != null)
        {
            Enable = false;
            LayoutManager.Instance.InteractableDisableAll();
            BaseState pBaseState = BaseState.GetInstance(new BaseStateTarget(m1),0.3f, (uint)E_STATE_TYPE.ObjectMove, null,executeMoveCallback, exitMoveCallback);
            pBaseState.StateData = new MoveStateData(m1,endSpot,target);
            StateMachine.GetStateMachine().AddState(pBaseState);
        }
    }

    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        switch(root.gameObject.name)
        {
            case "tip":
            {
                m_pMainScene.ShowGameTip("game_tip_tactics_title");
            }
            break;
            case "board":
            {
                for(int i =0; i < m_pFormationPlayers.Length; ++i)
                {
                    if(m_pFormationPlayers[i].Target == sender.transform)
                    {
                        BoardClick(m_pFormationPlayers[i]);
                        break;
                    }
                }
            }
            return;
            case "feildSlots":
            {
                FeildSlotClick(sender);
            }
            return;
            case "tabs":
            {
                E_TAB eType = (E_TAB)Enum.Parse(typeof(E_TAB), sender.name);
                if(eType == E_TAB.playerTactics)
                {
                    if(sender.transform.Find("lock").gameObject.activeSelf)
                    {
                        GameContext pGameContext = GameContext.getCtx();
                        m_pMainScene.ShowMessagePopup(pGameContext.GetLocalizingText("MSG_TXT_LOCKED_PLAYER_TACTICS"),pGameContext.GetLocalizingText("MSG_BTN_OKAY"));
                        return;
                    }
                }
                
                ShowTabUI((byte)eType);
            }
            break;
            case "cancel":
            {
                TeamDataT pTeamData = null;
                if(IsMatchView())
                {
                    pTeamData = m_pMainScene.GetInstance<MatchView>().GetMyTeamData();
                }
                
                CurrentClearScroll();
                ClearSubstitutionList();
                SetupData(pTeamData);
                UpdateOverall();
                if(m_eCurrentTab == E_TAB.playerTactics)
                {
                    PlayerTacticsT pPlayerTactics = m_pTempTactics.PlayerTactics[(int)m_pTempTactics.Formation[m_iCurrentSelectPlayerIndex]];
                    Transform tm = m_pTabUIList[(int)m_eCurrentTab].Find("control");
                    for(int i = 0; i < tm.childCount; ++i)
                    {
                        UpdatePlayerTacticsByIndex(i,pPlayerTactics.Tactics[i]);
                    }
                }
            }
            break;
            case "change":
            {
                ClearSelectPlayer();
                ShowFeildSlots(false);
                if(IsMatchView())
                {
                    if(CheckChangeLineupData(true) || CheckChangeTacticsData(true))
                    {
                        m_bChange = true;
                        GameContext pGameContext = GameContext.getCtx();
                        m_pMainScene.ShowConfirmPopup( pGameContext.GetLocalizingText("DIALOG_MATCHTACTICSCHANGE_TITLE"),pGameContext.GetLocalizingText("DIALOG_MATCHTACTICSCHANGE_TXT"),pGameContext.GetLocalizingText("DIALOG_BTN_CONFIRM"),pGameContext.GetLocalizingText("DIALOG_BTN_CALCEL"),false,Close,()=>{m_bChange =false;});
                        return;
                    }   
                }
                Close();
            }
            break;
            case "formation":
            {
                m_iCurrentSelectPlayerIndex = -1;
                ShowFeildSlots(false);
                if(m_pSelectFormation != null)
                {
                    ClearSelectPlayer();
                }

                switch(sender.name)
                {
                    case "selectFormation":
                    {
                        m_pMainScene.ShowSelectFormationPopup().SetupData();
                    }
                    break;
                    case "reset":
                    {
                        bool bReset = false;
                        if(CheckChangeLineupData(false))
                        {
                            bReset = true;
                            m_TempLineupPlayer.Data.Clear();
                            List<ulong> list = GameContext.getCtx().GetLineupPlayerIdListByIndex(m_TempLineupPlayer.Type);
                            for(int i = 0; i < list.Count; ++i)
                            {
                                m_TempLineupPlayer.Data.Add(list[i]);
                            }
                        }

                        if( CheckChangeTacticsData(false))
                        {
                            bReset = true;
                            GameContext.getCtx().CloneTacticsDataByIndex(m_iActiveTacticsType,ref m_pTempTactics);
                        }
                        if(bReset)
                        {
                            CurrentClearScroll();
                            SetupFormationPlayer(true);
                            UpdateBenchPlayer();
                            UpdateOverall();
                        }
                    }
                    break;
                    case "recommend":
                    {
                        GameContext.getCtx().SuggestionPlayer(ref m_TempLineupPlayer, m_pTempTactics.Formation.ToList());
                        SetupFormationPlayer(true);
                        CurrentClearScroll();
                        UpdateBenchPlayer();
                        UpdateOverall();
                    }
                    break;
                }
            }
            break;
            case "playerTactics":
            {
                if(sender.name == "player")
                {
                    ShowPlayerInfo(m_TempLineupPlayer.Data[m_iCurrentSelectPlayerIndex]);
                }
                else
                {
                    int index = -1;
                    if(int.TryParse(sender.name,out index))
                    {
                        Transform tm = m_pTabUIList[(int)E_TAB.playerTactics].Find($"feild/{m_iCurrentSelectPlayerIndex}");
                        tm.Find("select").gameObject.SetActive(false);
                        m_iCurrentSelectPlayerIndex = index;
                        sender.transform.Find("select").gameObject.SetActive(true);
                        UpdatePlayerTacticsScroll((int)m_pTempTactics.Formation[m_iCurrentSelectPlayerIndex]);
                        UpdatePlayerTacticsPlayerInfo(m_TempLineupPlayer.Data[m_iCurrentSelectPlayerIndex]);
                    }
                    else
                    {
                        ChangePlayerInfomation(sender.name == "right");
                    }
                }
            }
            break;
            case "teamTactics":
            {
                E_CONTROL eControl = (E_CONTROL)Enum.Parse(typeof(E_CONTROL), sender.transform.parent.gameObject.name);
                
                byte value = 0;
                int index =1;
                if(eControl == E_CONTROL.wasteTime || eControl == E_CONTROL.offsideTrap)
                {
                    if(sender.name == "on")
                    {
                        value = 100;
                        index =2;
                    }
                }
                else
                {
                    if(sender.name == "3")
                    {
                        value = 75;
                        index =3;
                    }
                    else if(sender.name == "2")
                    {
                        value = 50;
                        index =2;
                    }
                    else
                    {
                        value = 25;
                        index =1;
                    }
                }

                m_pTempTactics.TeamTactics[(int)eControl] = value;
                UpdateTeamTactics(eControl,index);
                if(m_pMainScene.IsMatch())
                {
                    m_bChange = true;
                    SteupCancelButton(true);
                }
            }
            break;
            case "slots":
            {
                if(sender.name == "save")
                {
                    m_pMainScene.ShowSaveTacticsPopup();
                }
                else if(sender.name == "load")
                {
                    m_pMainScene.ShowLoadTacticsPopup();
                }
            }
            break;
        }

        if(m_pSelectFormation != null)
        {
            ShowFeildSlots(false);
            ClearSelectPlayer();
        }
    }

    void FeildSlotClick(GameObject obj)
    {
        if(m_pSelectFormation == null ) return;
        
        if(CheckRedCardPlayer(m_pSelectFormation.Id))
        {
            ShowMessageRedCard();
            ClearSelectPlayer();
            return;
        }

        ShowFeildSlots(false);
        int index = (int)((E_LOCATION)Enum.Parse(typeof(E_LOCATION), obj.name));
        Vector3 pos = m_pSelectFormation.Target.position;
        bool bCheck = UpdateSelectFormation(obj.transform.position,index,true);
        if(bCheck)
        {
            m_pMovePlayers[0].CloneInfoUI(m_pSelectFormation);
            m_pMovePlayers[0].Target.gameObject.SetActive(true);
            m_pMovePlayers[0].Target.position = pos;
            MoveAnimation(m_pMovePlayers[0].Target,obj.transform.position,m_pSelectFormation.Target);
            m_pSelectFormation.Target.gameObject.SetActive(false);
        }
        ChangeSlotsPosition();
        if(m_pMainScene.IsMatch())
        {
            UpdateChangeButton(IsMatchView());
        }
        ClearSelectPlayer();
    }

    void ShowPlayerInfo(ulong id)
    {
        GameContext pGameContext = GameContext.getCtx();
        PlayerInfo pPlayerInfo = m_pMainScene.ShowPlayerInfoPopup();
        pPlayerInfo.SetupPlayerInfoData(E_PLAYER_INFO_TYPE.my,pGameContext.GetPlayerByID(id));
        pPlayerInfo.SetupQuickPlayerInfoData(pGameContext.GetTotalPlayerList());
        pPlayerInfo.ShowTabUI(E_PLAYER_INFO_TAB.information);
    }

    void BoardClick(PlayerItem pPlayerItem)
    {
        if(m_pSelectFormation == null )
        {
            m_iCurrentSelectPlayerIndex = -1;
            m_pSelectFormation = pPlayerItem;
            UpdateFamiliarsList(m_TempLineupPlayer.Data[m_pSelectFormation.Index]);
            
            ShowFeildSlots(true);
            m_pSelectFormation.Select.SetActive(true);
            m_pSelectFormation.Info.SetActive(true);
        }
        else
        {
            bool bCheck = false;

            if(m_pSelectFormation == pPlayerItem)
            {
                ShowPlayerInfo(m_TempLineupPlayer.Data[m_pSelectFormation.Index]);
            }
            else
            {
                bool bSelect = m_pSelectFormation.Select.activeSelf;
                bCheck = UpdateSelectFormation(pPlayerItem.Target.position,pPlayerItem.Index,false);
                if(bCheck)
                {
                    Transform target = null;
                    Transform target1 = null;
                    if(m_iCurrentSelectPlayerIndex > -1)
                    {
                        if(bSelect)
                        {
                            m_pMovePlayers[0].Target.position = m_pSelectFormation.Icon.transform.position;
                            m_pMovePlayers[0].CloneInfoUI(m_pSelectFormation);
                            m_pSelectFormation.Target.gameObject.SetActive(false);
                            target = m_pSelectFormation.Target;
                        }
                        else
                        {
                            int index = m_iCurrentSelectPlayerIndex - m_iStartIndex > 0 ? m_pBenchPlayerItems.Count -1 : 0;
                            if(index == 0)
                            {
                                m_pMovePlayers[0].Target.position = m_pBenchPlayerItems[index].Target.position - new Vector3(m_pBenchPlayerItems[index].Target.rect.width,0);
                            }
                            else
                            {
                                m_pMovePlayers[0].Target.position = m_pBenchPlayerItems[index].Target.position + new Vector3(m_pBenchPlayerItems[index].Target.rect.width,0);
                            }
                            
                            m_pMovePlayers[0].UpdatePlayerData(m_pBenchPlayers[m_iCurrentSelectPlayerIndex]);
                        }

                        m_pMovePlayers[1].Target.position = pPlayerItem.Icon.transform.position;
                        target1 = pPlayerItem.Target;
                    }
                    else
                    {
                        target = pPlayerItem.Target;
                        m_pMovePlayers[0].Target.position = pPlayerItem.Icon.transform.position;
                        m_pMovePlayers[1].Target.position = m_pSelectFormation.Icon.transform.position;

                        m_pMovePlayers[0].CloneInfoUI(m_pSelectFormation);
                        target1 = m_pSelectFormation.Target;

                        m_pSelectFormation.Target.gameObject.SetActive(false);
                    }
                    
                    m_pMovePlayers[1].CloneInfoUI(pPlayerItem);
                    m_pMovePlayers[0].Target.gameObject.SetActive(true);
                    m_pMovePlayers[1].Target.gameObject.SetActive(true);
                    
                    MoveAnimation(m_pMovePlayers[0].Target,m_pMovePlayers[1].Target.position,target);
                    MoveAnimation(m_pMovePlayers[1].Target,m_pMovePlayers[0].Target.position,target1);
                    
                    pPlayerItem.Target.gameObject.SetActive(false);
                }
            }
            
            ShowFeildSlots(false);
            ClearSelectPlayer();

            if(m_pMainScene.IsMatch() && bCheck)
            {
                UpdateChangeButton(IsMatchView());
            }
        }
    }

    public void NetworkProcessor(ALF.NETWORK.NetworkData data,bool bSuccess)
    {
        if( data == null || !MainUI.gameObject.activeInHierarchy) return;

        E_REQUEST_ID eType = (E_REQUEST_ID)data.Id;

        switch(eType)
        {
            case E_REQUEST_ID.player_changeProfile:
            {
                JArray pArray = (JArray)data.Json["players"];
                ulong id = 0;
                
                JObject pItem = null;
                GameContext pGameContext = GameContext.getCtx();
                
                PlayerT pPlayerData = null;
                int iPlay = 0;
                TMPro.TMP_Text text = null;

                for(int n =0; n < pArray.Count; ++n)
                {
                    pItem = (JObject)pArray[n];
                    id = (ulong)pItem["id"];

                    iPlay = pGameContext.GetLineupPlayerIndex(m_iActiveLineUpType,id);
                    if(iPlay < GameContext.CONST_NUMSTARTING)
                    {
                        text = m_pFormationPlayers[iPlay].PlayerName;
                        if(text != null)
                        {
                            pPlayerData = pGameContext.GetPlayerByID(id);
                            text.SetText(pPlayerData.Surname);
                        }
                    }
                    else
                    {
                        for(int i =0; i < m_pBenchPlayers.Count; ++i)
                        {
                            if(m_pBenchPlayers[i].Id == id)
                            {
                                m_pBenchPlayers[i].SetupPlayerData(pItem);

                                if(m_iStartIndex <= i && i < m_iStartIndex + m_pBenchPlayers.Count)
                                {
                                    m_pBenchPlayerItems[i - m_iStartIndex].UpdatePlayerName(m_pBenchPlayers[i]);
                                }

                                break;
                            }
                        }
                    }

                    if(m_eCurrentTab == E_TAB.playerTactics )
                    {
                        UpdatePlayerTacticsPlayerInfo(id);
                    }
                }
            }
            break;
            case E_REQUEST_ID.player_release:
            case E_REQUEST_ID.auction_reward:
            {
                JArray pArray = (JArray)data.Json["players"];
                if(pArray.Count > 0)
                {
                    ulong id = 0;
                    int i = 0;
                    int index = m_pBenchPlayers.Count;
                    for(int n =0; n < pArray.Count; ++n)
                    {
                        id = (ulong)(((JObject)(pArray[n]))["id"]);
                        
                        for(i = 0; i < m_pBenchPlayers.Count; ++i)
                        {
                            if(m_pBenchPlayers[i].Id == id)
                            {
                                m_pBenchPlayers[i].Dispose();
                                m_pBenchPlayers.RemoveAt(i);
                                if(index > i)
                                {
                                    index = i;
                                }
                                break;
                            }
                        }
                    }

                    if(m_iStartIndex <= index && index < m_iStartIndex + m_pBenchPlayerItems.Count)
                    {
                        index -= m_iStartIndex;
                        
                        while(index < m_pBenchPlayerItems.Count)
                        {
                            i = index + m_iStartIndex; 
                            if(i < m_pBenchPlayers.Count)
                            {
                                m_pBenchPlayerItems[index].UpdatePlayerData(m_pBenchPlayers[index + m_iStartIndex]);    
                            }
                            else
                            {
                                m_pBenchPlayerItems[index].Target.gameObject.SetActive(false);
                            }
                            ++index;
                        }
                    }

                    Vector2 size = m_pScrollRect.content.sizeDelta;
                    size.x -= m_pBenchPlayerItems[0].Target.rect.width * pArray.Count;
                    m_pScrollRect.content.sizeDelta = size;
                }
            }
            break;
            case E_REQUEST_ID.auction_trade:
            case E_REQUEST_ID.scout_reward:
            case E_REQUEST_ID.recruit_offer:
            case E_REQUEST_ID.adReward_reward:
            case E_REQUEST_ID.youth_offer:
            case E_REQUEST_ID.tutorial_recruit:
            case E_REQUEST_ID.auction_withdraw:
            case E_REQUEST_ID.auction_cancel:
            {
                if(eType == E_REQUEST_ID.adReward_reward && !data.Json.ContainsKey("players")) return;

                JArray pArray = (JArray)data.Json["players"];
                ulong id = 0;
                GameContext pGameContext = GameContext.getCtx();
                PlayerT pPlayer = null;
                BenchPlayer pBenchPlayer = null;
                
                for(int n =0; n < pArray.Count; ++n)
                {
                    id = (ulong)(((JObject)(pArray[n]))["id"]);
                    pPlayer = pGameContext.GetPlayerByID(id);

                    pBenchPlayer = new BenchPlayer(pPlayer.Id,pPlayer.Surname,GameContext.CONST_NUMPLAYER,pPlayer.Position,pPlayer.Hp);
                    m_pBenchPlayers.Add(pBenchPlayer);
                }

                Vector2 size = m_pScrollRect.content.sizeDelta;
                float w = pArray.Count * m_pBenchPlayerItems[0].Target.rect.width;
                size.x += w;
                m_pScrollRect.content.sizeDelta = size;
            }
            break;
            case E_REQUEST_ID.auction_register:
            {  
                if(data.Json.ContainsKey("players"))
                {
                    JArray pArray = (JArray)data.Json["players"];
                    ulong id = 0;
                    JObject item = null;
                    GameContext pGameContext = GameContext.getCtx();
                    int index = m_pBenchPlayers.Count;
                    int i = 0;
                    for(int n =0; n < pArray.Count; ++n)
                    {
                        item = (JObject)pArray[n];
                        id = (ulong)item["id"];
                        for(i = 0; i < m_pBenchPlayers.Count; ++i)
                        {
                            if(m_pBenchPlayers[i].Id == id)
                            {
                                if(index > i)
                                {
                                    index = i;
                                }

                                m_pBenchPlayers.RemoveAt(i);

                                break;
                            }
                        }
                    }

                    if(m_iStartIndex <= index && index < m_iStartIndex + m_pBenchPlayerItems.Count)
                    {
                        index -= m_iStartIndex;

                        while(index < m_pBenchPlayerItems.Count)
                        {
                            i = index + m_iStartIndex; 
                            if(i < m_pBenchPlayers.Count)
                            {
                                m_pBenchPlayerItems[index].UpdatePlayerData(m_pBenchPlayers[i]);
                            }
                            else
                            {
                                m_pBenchPlayerItems[index].Target.gameObject.SetActive(false);
                            }
                            ++index;
                        }

                        Vector2 size = m_pScrollRect.content.sizeDelta;
                        size.x -= m_pBenchPlayerItems[0].Target.rect.width * pArray.Count;
                        m_pScrollRect.content.sizeDelta = size;
                        size = m_pScrollRect.content.anchoredPosition;
                        size.x -= m_pBenchPlayerItems[0].Target.rect.width * pArray.Count;;
                        m_pScrollRect.content.anchoredPosition = size;
                    }
                }
            }
            break;
            case E_REQUEST_ID.tactics_load:
            {
                SetupData(null);
                ShowTabUI(0);
            }
            break;
            case E_REQUEST_ID.player_positionFamiliarUp:
            case E_REQUEST_ID.player_abilityUp:
            {
                GameContext pGameContext = GameContext.getCtx();
                JArray pArray = (JArray)data.Json["players"];
                JObject item = (JObject)pArray[0];
                ulong id = (ulong)item["id"];
                PlayerT pPlayerData = pGameContext.GetPlayerByID(id);
                if(m_eCurrentTab == E_TAB.playerTactics && m_iCurrentSelectPlayerIndex > -1)
                {
                    if(m_pFormationPlayers[m_iCurrentSelectPlayerIndex].Id == id)
                    {
                        Transform tm = m_pTabUIList[(int)E_TAB.playerTactics].Find("playerInfo");
                        TMPro.TMP_Text text = tm.Find("value/text").GetComponent<TMPro.TMP_Text>();                                
                        text.SetText(ALFUtils.NumberToString(pPlayerData.Price));
                        SetupPlayerAbilityInformation(pPlayerData);
                    }
                }
                else if(m_eCurrentTab == E_TAB.formation)
                {
                    E_LOCATION eLoc = E_LOCATION.LOC_NONE;
                    PlayerItem pPlayerItem = null;
                    int i =0;
                    for(i =0; i < m_pFormationPlayers.Length; ++i)
                    {
                        if(m_pFormationPlayers[i].Id == id)
                        {
                            pPlayerItem = m_pFormationPlayers[i];
                            eLoc = (E_LOCATION)m_pTempTactics.Formation[m_pFormationPlayers[i].Index];
                            break;
                        }
                    }
                    
                    if(eType == E_REQUEST_ID.player_positionFamiliarUp)
                    {
                        if(pPlayerItem == null) return;
                        pPlayerItem.UpdateFormationPlayerData(pPlayerData,eLoc);
                    }
                    else
                    {
                        if(pPlayerItem == null)
                        {
                            for(i =0; i < m_pBenchPlayerItems.Count; ++i)
                            {
                                if(m_pBenchPlayerItems[i].Id == id)
                                {
                                    pPlayerItem = m_pBenchPlayerItems[i];
                                    break;
                                }
                            }
                        }
                        
                        if(pPlayerItem == null) return;
                        pPlayerItem.UpdatePlayerInfo(pPlayerData);
                        UpdateOverall();
                    }
                }
            }
            break;
        }
    }
}
