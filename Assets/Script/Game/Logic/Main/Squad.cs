using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using ALF;
using ALF.STATE;
using ALF.CONDITION;
using ALF.LAYOUT;
using ALF.MACHINE;
using ALF.SOUND;
using DATA;
using USERDATA;
using MATCHTEAMDATA;
using STATEDATA;
using ALF.NETWORK;
using Newtonsoft.Json.Linq;

public class Squad : IBaseUI,IBaseNetwork
{
    const string SCROLL_ITEM_NAME = "SquadItem";
    enum E_TAB : byte { form = 0,name,roles,age,quality,cnd,MAX }

    class SortPlayer : IBase
    {
        public ulong Id {get; private set;}
        public string SName {get; private set;}
        public string FName {get; private set;}
        public byte Age {get; private set;}
        public int IPlay {get; private set;}
        public byte Position {get; private set;}
        public byte Formation {get; private set;}
        public uint Quality {get; private set;}
        public byte Cnd {get; private set;}

        public SortPlayer( ulong id, string sname,string fname,byte age,int iPlay, byte position, byte formation, uint quality, byte cnd)
        {
            Id = id;
            SName = sname;
            FName = fname;
            Age = age;
            IPlay = iPlay;
            Position = position;
            Formation = formation;
            Quality = quality;
            Cnd = cnd;
        }

        public void UpdateQuality(uint quality)
        {
            Quality = quality;
        }

        public void SetIPlay(int iPlay)
        {
            IPlay = iPlay;
        }

        public void SetFormation(byte formation)
        {
            Formation = formation;
        }

        public void SetupPlayerData( JObject data)
        {
            SName = data["surname"].ToString();
            FName = data["forename"].ToString();
        }

        public void Dispose()
        {
        }
    }

    class PlayerItem : IBase
    {
        public ulong Id {get; private set;}
        public string SName {get; private set;}
        public string FName {get; private set;}
        public byte Age {get; private set;}
        public int IPlay {get; set;}
        public byte Position {get; private set;}
        public byte Formation {get; private set;}
        public uint Quality {get; private set;}
        public byte Cnd {get; private set;}

        public RawImage Icon {get; private set;}
        public TMPro.TMP_Text PlayerName {get; private set;}
        public TMPro.TMP_Text FormationText {get; private set;}
        public Image HP {get; private set;}
        public TMPro.TMP_Text AgeText {get; private set;}
        public TMPro.TMP_Text TireText {get; private set;}

        public GameObject Lock {get; private set;}

        public RectTransform Target  {get; private set;}
        public Button Info  {get; private set;}

        Transform m_pRoles = null;
        Graphic m_pBG = null;

        public PlayerItem(RectTransform target)
        {
            Target = target;
            Info = target.GetComponent<Button>();
            m_pBG = target.Find("bg").GetComponent<Graphic>();
            PlayerName = target.Find("name").GetComponent<TMPro.TMP_Text>();
            Icon = target.Find("form/icon").GetComponent<RawImage>();
            FormationText = target.Find("form/text").GetComponent<TMPro.TMP_Text>();
            TireText = target.Find("tire/text").GetComponent<TMPro.TMP_Text>();
            HP = target.Find("hp/fill").GetComponent<Image>();
            
            AgeText = target.Find("age").GetComponent<TMPro.TMP_Text>();
            Lock = target.Find("lock").gameObject;

            m_pRoles = target.Find("roles");
        }

        public void Dispose()
        {
            Info.onClick.RemoveAllListeners();
            Info = null;
            TireText = null;
            if(m_pRoles != null)
            {
                RawImage[] list = m_pRoles.GetComponentsInChildren<RawImage>(true);
                for(int n = 0; n < list.Length; ++n)
                {
                    list[n].texture = null;
                }            
            }
            
            Icon.texture = null;
            Icon = null;
            FormationText = null;
            HP = null;
            AgeText = null;
            Lock = null;
            m_pBG = null;

            LayoutManager.Instance.AddItem(SCROLL_ITEM_NAME,Target);
            Target = null;
        }

        public void UpdateHP(PlayerT pPlayer)
        {
            float hp = (float)pPlayer.Hp / 100f;
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
        public void UpdateFormation(SortPlayer pSortPlayer)
        {
            if(pSortPlayer == null) return;
            GameContext pGameContext = GameContext.getCtx();
            Sprite pSprite = null;
            if(pSortPlayer.IPlay == GameContext.CONST_NUMPLAYER)
            {
                pSprite = AFPool.GetItem<Sprite>("Texture","S_S");
                FormationText.SetText("");
            }
            else if(pSortPlayer.IPlay < GameContext.CONST_NUMSTARTING)
            {
                FormationText.SetText(pGameContext.GetDisplayLocationName(pGameContext.ConvertPositionByTag((E_LOCATION)pSortPlayer.Formation)));
                pSprite = AFPool.GetItem<Sprite>("Texture","S_"+pGameContext.GetDisplayCardFormationByLocationName(pGameContext.GetDisplayLocationName(pGameContext.ConvertPositionByTag((E_LOCATION)pSortPlayer.Formation))));
            }
            else
            {
                FormationText.SetText($"S{pSortPlayer.IPlay - GameContext.CONST_NUMSTARTING +1}");
                pSprite = AFPool.GetItem<Sprite>("Texture","S_S");
            }
            
            Icon.texture = pSprite.texture;
        }

        public void UpdatePlayerData(SortPlayer pSortPlayer)
        {
            if(pSortPlayer == null) return;

            Id = pSortPlayer.Id;
            SName = pSortPlayer.SName;
            FName = pSortPlayer.FName;
            Age = pSortPlayer.Age;
            IPlay = pSortPlayer.IPlay;
            Position = pSortPlayer.Position;
            Formation = pSortPlayer.Formation;
            Quality = pSortPlayer.Quality;
            Cnd = pSortPlayer.Cnd;
            PlayerName.SetText($"{FName[0]}. {SName}");
            AgeText.SetText(Age > 40 ? "40+":$"{Age}");
            UpdateFormation(pSortPlayer);
            UpdatePlayerInfo(GameContext.getCtx().GetPlayerByID(Id));
        }
        public void UpdatePlayerName(SortPlayer pSortPlayer)
        {
            if(pSortPlayer == null) return;

            SName = pSortPlayer.SName;
            FName = pSortPlayer.FName;
            
            PlayerName.SetText($"{FName[0]}. {SName}");
        }

        public void UpdatePlayerTire(PlayerT pPlayer)
        {
            if(pPlayer == null) return;

            TireText.SetText($"{pPlayer.AbilityTier}/{pPlayer.PotentialTier}");
        }

        public void UpdatePlayerInfo(PlayerT pPlayer)
        {
            if(pPlayer == null) return;
            
            SingleFunc.SetupPlayerCard(pPlayer,Target,E_ALIGN.Left);
            
            UpdateHP(pPlayer);
            UpdatePlayerTire(pPlayer);
            
            if(pPlayer.Status == 10)
            {
                Lock.SetActive(true);
                m_pBG.color = GameContext.GRAY_W;
            }
            else
            {
                Lock.gameObject.SetActive(false);
                m_pBG.color = Color.white;
            }
        }
    }

    MainScene m_pMainScene = null;
    Image m_pHPGauge = null;
    ScrollRect m_pScrollRect = null;
    Transform m_pRecovery = null;
    TMPro.TMP_Text m_pSquadCount = null;
    Transform[] m_pTabButtonList = new Transform[(int)E_TAB.MAX];
    float m_fRecoveryValue = -1;
    List<PlayerItem> m_pPlayers = new List<PlayerItem>();
    List<SortPlayer> m_sortPlayers = new List<SortPlayer>();
    public bool Enable { set{ if (m_pScrollRect != null) m_pScrollRect.enabled = value;}}
    public RectTransform MainUI { get; private set;}
    Vector2 m_pPrevDir = Vector2.zero;
    int m_iTotalScrollItems = 0;
    int m_iStartIndex = 0;

    E_TAB m_eSelectTab = E_TAB.form;
    public Squad(){}
    
    public void Dispose()
    {
        m_pMainScene = null;
        MainUI = null;
        m_pHPGauge = null;
        m_pRecovery = null;
        
        ClearScroll();
        m_pPlayers = null;
        m_pScrollRect.onValueChanged.RemoveAllListeners();
        
        m_pScrollRect = null;
        m_pSquadCount = null;
        int i =0;
        for(i =0; i < m_pTabButtonList.Length; ++i)
        {
            m_pTabButtonList[i] = null;
        }
        m_pTabButtonList = null;
        
        for(i =0; i < m_sortPlayers.Count; ++i)
        {
            m_sortPlayers[i].Dispose();
            m_sortPlayers[i] = null; 
        }

        m_sortPlayers.Clear();
        m_sortPlayers = null;   
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "Squad : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "Squad : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;

        RectTransform ui = MainUI.Find("root/tabs").GetComponent<RectTransform>();
        int iTabIndex = -1;
        Transform item = null;

        for(int n =0; n < ui.childCount; ++n)
        {
            item = ui.GetChild(n).GetComponent<RectTransform>();
            iTabIndex = (int)((E_TAB)Enum.Parse(typeof(E_TAB), item.gameObject.name));
            m_pTabButtonList[iTabIndex] = item;
        }

        m_pSquadCount = MainUI.Find("root/add_squad/text").GetComponent<TMPro.TMP_Text>();
        m_pRecovery = MainUI.Find("root/recovery");
        m_pHPGauge = MainUI.Find("root/hp/fill").GetComponent<Image>();
        MainUI.gameObject.SetActive(false);
        m_pScrollRect = MainUI.GetComponentInChildren<ScrollRect>(true);
        m_pScrollRect.onValueChanged.AddListener( ScrollViewChangeValueEventCall);
        LayoutManager.SetReciveUIButtonEvent(MainUI,ButtonEventCall);
        SetupScroll();
    }

    
    public void UpdateSquadCount(int count)
    {
        m_pSquadCount?.SetText($"{count}/{GameContext.getCtx().GetCurrentSquadMax()}");
    }

    public void SetupPlayerData()
    {
        ClearScroll();
        SetupScroll();
    }

    void SetupScroll()
    {
        GameContext pGameContext = GameContext.getCtx();
        List<PlayerT> list = pGameContext.GetTotalPlayerList();
        m_iTotalScrollItems = 0;

        if(list == null) return;
        
        UpdateSquadCount(list.Count);
        
        m_iStartIndex = 0;
        float viewSize = m_pScrollRect.viewport.rect.height;
        float itemSize = 0;

        RectTransform pItem = null;
        Vector2 size;
        float h = 0;
        byte age = 0;
        byte formation = 0;
        int playerIndex = -1;

        float hp = 0;
        int index = pGameContext.GetActiveLineUpType();
        List<byte> pFormation = pGameContext.GetFormationByIndex(index);

        SortPlayer pSortPlayer = null;
        PlayerItem pPlayerItem = null;
        
        for(int i =0; i < list.Count; ++i)
        {
            playerIndex = pGameContext.GetLineupPlayerIndex(index,list[i].Id);
            formation = 0;
            age = pGameContext.GetPlayerAge(list[i]);
            pSortPlayer = new SortPlayer( list[i].Id, list[i].Surname,list[i].Forename,age,playerIndex,list[i].Position,formation,list[i].AbilityWeightSum,list[i].Hp);
            m_sortPlayers.Add(pSortPlayer);
            if(playerIndex != GameContext.CONST_NUMPLAYER)
            {
                if(playerIndex < GameContext.CONST_NUMSTARTING)
                {
                    formation = pFormation[playerIndex];
                    hp += list[i].Hp;
                }
                else
                {
                    hp += list[i].Hp;
                }    
            }
            pSortPlayer.SetFormation(formation);
            if(viewSize > 0)
            {
                ++m_iTotalScrollItems;
            }
            if( viewSize > -itemSize)
            {
                pItem = LayoutManager.Instance.GetItem<RectTransform>(SCROLL_ITEM_NAME);
            
                if(pItem)
                {
                    pPlayerItem = new PlayerItem(pItem);
                    m_pPlayers.Add(pPlayerItem);

                    pItem.SetParent(m_pScrollRect.content,false);

                    pItem.anchoredPosition = new Vector2(0,-h);
                    size = pItem.sizeDelta;
                    size.x = 0;
                    pItem.sizeDelta = size;
                    itemSize = pItem.rect.height;
                    h += itemSize;
                    viewSize -= itemSize;
                    pItem.gameObject.SetActive(viewSize > -itemSize);
                }
            }
            else
            {
                h += itemSize;
            }
        }
        
        UpdateHPGauge(hp);
      
        m_pScrollRect.verticalNormalizedPosition = 1;
        m_pPrevDir.y = 1;
        m_pScrollRect.content.anchoredPosition = Vector2.zero; 
        LayoutManager.SetReciveUIScollViewEvent(m_pScrollRect,ScrollViewItemButtonEventCall);
    }

    public void UpdatePlayerHP(bool bAni)
    {
        if(m_fRecoveryValue > 0) return;
        
        GameContext pGameContext = GameContext.getCtx();
        
        if(bAni)
        {
            if( m_fRecoveryValue == -1)
            {
                m_fRecoveryValue = (1 - m_pHPGauge.fillAmount) / 1f;
                BaseState pBaseState = BaseState.GetInstance(new BaseStateTarget(m_pHPGauge),1f, (uint)E_STATE_TYPE.Timer, null, executeUpdateHP,exitUpdateHP);
                StateMachine.GetStateMachine().AddState(pBaseState);
            }
        }
        else
        {
            int index = 0;
            for(int i =0; i < m_pPlayers.Count; ++i)
            { 
                index = m_iStartIndex +i;
                if( index > 0 && index < m_sortPlayers.Count )
                {
                    m_pPlayers[i].UpdateHP(pGameContext.GetPlayerByID(m_sortPlayers[index].Id));
                }
            }

            UpdateHPGauge(pGameContext.GetLineupTotalHP());
        }
    }

    bool executeUpdateHP(IState state,float dt,bool bEnd)
    {
        TimeOutCondition condition = state.GetCondition<TimeOutCondition>();
        m_pHPGauge.fillAmount += m_fRecoveryValue * dt;
        return bEnd;
    }

    IState exitUpdateHP(IState state)
    {
        m_fRecoveryValue = -1;
        UpdatePlayerHP(false);
        return null;
    }

    void UpdateHPGauge(float hp)
    {
        m_pHPGauge.fillAmount = hp / 1800.0f;
        m_pRecovery.gameObject.SetActive(m_pHPGauge.fillAmount < 1f);
        if(m_pHPGauge.fillAmount < 0.5f)
        {
            m_pHPGauge.color = GameContext.HP_L;
        }
        else if(m_pHPGauge.fillAmount < 0.7f)
        {
            m_pHPGauge.color = GameContext.HP_LH;
        }
        else if(m_pHPGauge.fillAmount < 0.9f)
        {
            m_pHPGauge.color = GameContext.HP_H;
        }
        else
        {
            m_pHPGauge.color = GameContext.HP_F;
        }
    }

    public void UpdateScrollLineUpPlayer()
    {
        GameContext pGameContext = GameContext.getCtx();
        int index = pGameContext.GetActiveLineUpType();
        List<byte> pFormation = pGameContext.GetFormationByIndex(index);
        int i =0;
        for(i =0; i < m_sortPlayers.Count; ++i)
        {
            m_sortPlayers[i].SetIPlay(pGameContext.GetLineupPlayerIndex(index,m_sortPlayers[i].Id));
            if(m_sortPlayers[i].IPlay < GameContext.CONST_NUMSTARTING)
            {
                m_sortPlayers[i].SetFormation(pFormation[m_sortPlayers[i].IPlay]);
            }
            else
            {
                m_sortPlayers[i].SetFormation(0);
            }
        }
        
        for(i =0; i < m_pPlayers.Count; ++i)
        {
            index = i + m_iStartIndex;
            if(index > -1 && index < m_sortPlayers.Count)
            {
                m_pPlayers[i].UpdateFormation(m_sortPlayers[index]);
            }
        }
    }

    public void ShowTabUI(byte eTab)
    {
        ShowSubTabUI((E_TAB)eTab);
        Sort((E_TAB)eTab,true);
        UpdatePlayerHP(false);
    }

    void ShowSubTabUI(E_TAB eTab)
    {
        m_eSelectTab = eTab;
        Transform item = null;
        bool bActive = false;
        
        for(int i = 0; i < m_pTabButtonList.Length; ++i)
        {
            bActive = (int)eTab == i;
            m_pTabButtonList[i].Find("on").gameObject.SetActive(bActive);
            m_pTabButtonList[i].Find("title").GetComponent<Graphic>().color = bActive ? Color.white : GameContext.GRAY;
            item = m_pTabButtonList[i].Find("mark");
            item.gameObject.SetActive(bActive);
            item.localScale = Vector3.one;
        }
    }

    void Sort(E_TAB tab, bool bASC)
    {
        ResetScroll();

        switch(tab)
        {
            case E_TAB.form:
            {
                //m_sortPlayers = m_sortPlayers.OrderBy(x => x.IPlay).ToList();
                List<SortPlayer> sList = new List<SortPlayer>();
                List<SortPlayer> sList1 = new List<SortPlayer>();
                List<SortPlayer> sList2 = new List<SortPlayer>();
                int n =0;
                for(n =0; n < m_sortPlayers.Count; ++n )
                {
                    if(m_sortPlayers[n].IPlay == GameContext.CONST_NUMPLAYER)
                    {
                        sList2.Add(m_sortPlayers[n]);
                    }
                    else if(m_sortPlayers[n].IPlay < GameContext.CONST_NUMSTARTING)
                    {
                        sList.Add(m_sortPlayers[n]);
                    }
                    else
                    {
                        sList1.Add(m_sortPlayers[n]);
                    }
                }

                if(bASC)
                {   
                    if(sList.Count > 1)
                    {
                        sList = sList.OrderBy(x => x.Formation).ToList();
                    }
                    if(sList1.Count > 1)
                    {
                        sList1 = sList1.OrderBy(x => x.IPlay).ToList();
                    }

                    if(sList2.Count > 1)
                    {
                        sList2 = sList2.OrderBy(x => x.Position).ToList();
                    }
                }
                else
                {
                    if(sList.Count > 1)
                    {
                        sList = sList.OrderByDescending(x => x.Formation).ToList();
                    }
                    if(sList1.Count > 1)
                    {
                        sList1 = sList1.OrderByDescending(x => x.IPlay).ToList();
                    }

                    if(sList2.Count > 1)
                    {
                        sList2 = sList2.OrderByDescending(x => x.Position).ToList();
                    }
                }

                m_sortPlayers.Clear();
                for(n =0; n < sList.Count; ++n )
                {
                    m_sortPlayers.Add(sList[n]);
                }
                for(n =0; n < sList1.Count; ++n )
                {
                    m_sortPlayers.Add(sList1[n]);
                }
                for(n =0; n < sList2.Count; ++n )
                {
                    m_sortPlayers.Add(sList2[n]);
                }
            }
            break;
            case E_TAB.name:
            {
                if(bASC)
                {
                    m_sortPlayers = m_sortPlayers.OrderByDescending(x => x.SName).ThenByDescending(x => x.FName).ToList();
                }
                else
                {
                    m_sortPlayers = m_sortPlayers.OrderBy(x => x.SName).ThenBy(x => x.FName).ToList();
                }
            }
            break;
            case E_TAB.roles:
            {
                if(bASC)
                {
                    m_sortPlayers = m_sortPlayers.OrderByDescending(x => x.Position).ToList();
                }
                else
                {
                    m_sortPlayers = m_sortPlayers.OrderBy(x => x.Position).ToList();
                }
            }
            break;
            case E_TAB.age:
            {
                if(bASC)
                {
                    m_sortPlayers = m_sortPlayers.OrderByDescending(x => x.Age).ToList();
                }
                else
                {
                    m_sortPlayers = m_sortPlayers.OrderBy(x => x.Age).ToList();
                }                    
            }
            break;
            case E_TAB.quality:
            {
                if(bASC)
                {
                    m_sortPlayers = m_sortPlayers.OrderByDescending(x => x.Quality).ToList();
                }
                else
                {
                    m_sortPlayers = m_sortPlayers.OrderBy(x => x.Quality).ToList();
                }
            }
            break;
            case E_TAB.cnd:
            {
                if(bASC)
                {
                    m_sortPlayers = m_sortPlayers.OrderByDescending(x => x.Cnd).ToList();
                }
                else
                {
                    m_sortPlayers = m_sortPlayers.OrderBy(x => x.Cnd).ToList();
                }
            }
            break;
        }
        
        for(int i =0; i < m_pPlayers.Count; ++i)
        {
            m_pPlayers[i].UpdatePlayerData(m_sortPlayers[i]);
        }
    }
    void ClearScroll()
    {
        int i = m_pPlayers.Count;
        while(i > 0)
        {
            --i;
            m_pPlayers[i].Dispose(); 
        }
        
        m_pPlayers.Clear();
        LayoutManager.SetReciveUIScollViewEvent(m_pScrollRect,null);
        
        m_pScrollRect.content.anchoredPosition = Vector2.zero;
        Vector2 size = m_pScrollRect.content.sizeDelta;
        size.y =0;
        m_pScrollRect.content.sizeDelta = size;
    }

    void ResetScroll()
    {
        Vector2 pos;
        float viewSize = m_pScrollRect.viewport.rect.height;
        float itemSize = 0;
        PlayerItem pPlayerItem = null;
        for(int i = 0; i < m_pPlayers.Count; ++i)
        {
            pPlayerItem = m_pPlayers[i];
            itemSize = pPlayerItem.Target.rect.height;
            viewSize -= itemSize;
            pPlayerItem.Target.gameObject.SetActive(viewSize > -itemSize);

            pos = pPlayerItem.Target.anchoredPosition;            
            pos.y = -i * itemSize;
            pPlayerItem.Target.anchoredPosition = pos;
        }
        
        Vector2 size = m_pScrollRect.content.sizeDelta;
        size.y = itemSize * m_sortPlayers.Count;
        m_pScrollRect.content.sizeDelta = size;
        m_pScrollRect.verticalNormalizedPosition = 1;
        m_pScrollRect.content.anchoredPosition = Vector2.zero;
        m_iStartIndex = 0;
        m_pPrevDir.y = 1;
    }

    void ScrollViewChangeData(int index,int iTarget,int iCount)
    {
        if(index == iTarget) return;
        int i = 0;
        PlayerItem pPlayerItem = null;
        if(index > iTarget)
        {
            pPlayerItem = m_pPlayers[iTarget];
            m_pPlayers[iTarget] = m_pPlayers[index];
            i = iTarget +1;
            PlayerItem pTemp = null;
            while(i < index)
            {
                pTemp = m_pPlayers[i];
                m_pPlayers[i] = pPlayerItem;
                pPlayerItem = pTemp;
                ++i;
            }
            m_pPlayers[index] = pPlayerItem;
            pPlayerItem = m_pPlayers[iTarget];
        }
        else
        {
            pPlayerItem = m_pPlayers[index];
            i = index +1;
            while(i <= iTarget)
            {
                m_pPlayers[i -1] = m_pPlayers[i];
                ++i;
            }

            m_pPlayers[iTarget] = pPlayerItem;
        }
        
        i = m_iStartIndex + iTarget + iCount;
        
        if(i < 0 || m_sortPlayers.Count <= i) return;
        
        pPlayerItem.UpdatePlayerData(m_sortPlayers[i]);
    }

    void ScrollViewChangeValueEventCall( Vector2 value)
    {
        if(m_iTotalScrollItems < m_sortPlayers.Count && value.y != m_pPrevDir.y)
        {
            m_pScrollRect.ScrollViewChangeValue(value - m_pPrevDir,ref m_iStartIndex,ScrollViewChangeData);
            m_pPrevDir = value;
        }
    }
    void ScrollViewItemButtonEventCall(ScrollRect root, Transform tm,GameObject sender)
    {
        for(int i =0; i < m_pPlayers.Count; ++i)
        {
            if(m_pPlayers[i].Target == tm)
            {
                GameContext pGameContext = GameContext.getCtx();
                if(sender.name == "form")
                {
                    PlayerT pPlayer = pGameContext.GetPlayerByID(m_pPlayers[i].Id);
                    if(pPlayer.Status == 0)
                    {
                        QuickLineUp pQuickLineUp = m_pMainScene.ShowQuickLineUpPopup();
                        pQuickLineUp.SetupPlayerInfoData(pPlayer);
                    }
                }
                else
                {
                    PlayerInfo pPlayerInfo = m_pMainScene.ShowPlayerInfoPopup();
                    pPlayerInfo.SetupPlayerInfoData(E_PLAYER_INFO_TYPE.my,pGameContext.GetPlayerByID(m_pPlayers[i].Id));
                    pPlayerInfo.SetupQuickPlayerInfoData(pGameContext.GetTotalPlayerList());
                    pPlayerInfo.ShowTabUI(E_PLAYER_INFO_TAB.information);
                }
                return;    
            }
        }
    }

    public void Close()
    {
        m_pMainScene.HideMoveDilog(MainUI,Vector3.right);
    }

    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        switch(sender.name)
        {
            case "tip":
            {
                m_pMainScene.ShowGameTip("game_tip_squad_title");
            }
            break;
            case "back":
            {
                Close();
                m_pMainScene.ResetMainButton();
            }
            break;
            case "add_squad":
            {
                m_pMainScene.ShowSquadExpansionPopup();
            }
            break;
            case "recovery":
            {
                m_pMainScene.ShowHPRecoveryPopup();
            }
            break;
            case "form":
            case "name":
            case "roles":
            case "age":
            case "quality":
            case "cnd":
            {
                E_TAB eSelectTab = (E_TAB)Enum.Parse(typeof(E_TAB), sender.name);
                Transform tm = sender.transform.Find("mark");
                if(tm.gameObject.activeSelf)
                {
                    Vector3 scale = tm.localScale;
                    scale.y *= -1;
                    tm.localScale = scale;
                    Sort(eSelectTab, tm.localScale.y > 0);
                }
                else
                {
                    ShowSubTabUI(eSelectTab);
                    Sort(eSelectTab, true);
                }
            }
            break;
        }
    }

    void CurrentSort()
    {
        Transform item = m_pTabButtonList[(int)m_eSelectTab].Find("mark");
        bool bDir = true;
        if(item.gameObject.activeSelf)
        {
            bDir = item.localScale.y > 0;
        }
        
        Sort(m_eSelectTab, bDir);
    }

    public void UpdateAllPlayerData()
    {
        GameContext pGameContext = GameContext.getCtx();
        
        for(int i = 0; i < m_pPlayers.Count; ++i)
        {
            m_pPlayers[i].UpdatePlayerInfo(pGameContext.GetPlayerByID(m_pPlayers[i].Id));
            
        }
    }

    public void NetworkProcessor(ALF.NETWORK.NetworkData data,bool bSuccess)
    {
       if( data == null) return;
        
        E_REQUEST_ID eType = (E_REQUEST_ID)data.Id;
                
        switch(eType)
        {
            case E_REQUEST_ID.player_changeProfile:
            {
                JArray pArray = (JArray)data.Json["players"];
                ulong id = 0;
                int i = 0;
                JObject pItem = null;
                GameContext pGameContext = GameContext.getCtx();
                int index = m_sortPlayers.Count;
                for(int n =0; n < pArray.Count; ++n)
                {
                    pItem = (JObject)pArray[n];
                    id = (ulong)pItem["id"];
                    for(i = 0; i < m_sortPlayers.Count; ++i)
                    {
                        if(m_sortPlayers[i].Id == id)
                        {
                            m_sortPlayers[i].SetupPlayerData(pItem);
                            if(index > i)
                            {
                                index = i;
                            }
                        }
                    }
                }

                if( !MainUI.gameObject.activeInHierarchy) return;

                if(m_iStartIndex <= index && index < m_iStartIndex + m_pPlayers.Count)
                {
                    index -= m_iStartIndex;
                    if(index > -1)
                    {
                        while(index < m_pPlayers.Count)
                        {
                            if(index + m_iStartIndex < m_sortPlayers.Count)
                            {
                                m_pPlayers[index].UpdatePlayerName(m_sortPlayers[index + m_iStartIndex]);
                            }
                            else
                            {
                                break;
                            }
                            
                            ++index;
                        }
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
                    int index = m_sortPlayers.Count;
                    for(int n =0; n < pArray.Count; ++n)
                    {
                        id = (ulong)(((JObject)(pArray[n]))["id"]);
                        
                        for(i = 0; i < m_sortPlayers.Count; ++i)
                        {
                            if(m_sortPlayers[i].Id == id)
                            {
                                m_sortPlayers[i].Dispose();
                                m_sortPlayers.RemoveAt(i);
                                if(index > i)
                                {
                                    index = i;
                                }

                                break;
                            }
                        }
                    }

                    UpdateSquadCount(GameContext.getCtx().GetTotalPlayerCount());

                    if( !MainUI.gameObject.activeInHierarchy) return;

                    if(m_iStartIndex <= index && index < m_iStartIndex + m_pPlayers.Count)
                    {
                        index -= m_iStartIndex;
                        while(index < m_pPlayers.Count)
                        {
                            if(index > -1)
                            {
                                i = index + m_iStartIndex; 
                                if(i < m_sortPlayers.Count)
                                {
                                    m_pPlayers[index].UpdatePlayerData(m_sortPlayers[i]);
                                }
                                else
                                {
                                    m_pPlayers[index].Target.gameObject.SetActive(false);
                                }   
                            }
                            ++index;
                        }
                    }

                    Vector2 size = m_pScrollRect.content.sizeDelta;
                    size.y -= m_pPlayers[0].Target.rect.height * pArray.Count;
                    m_pScrollRect.content.sizeDelta = size;
                }
            }
            break;
            case E_REQUEST_ID.lineup_put:
            case E_REQUEST_ID.tactics_put:
            {
                GameContext pGameContext = GameContext.getCtx();
                int index = pGameContext.GetActiveLineUpType();
                List<byte> pFormation = pGameContext.GetFormationByIndex(index);

                for(int i =0; i < m_sortPlayers.Count; ++i)
                {
                    m_sortPlayers[i].SetIPlay(pGameContext.GetLineupPlayerIndex(index,m_sortPlayers[i].Id));
                    if(m_sortPlayers[i].IPlay < GameContext.CONST_NUMSTARTING)
                    {
                        m_sortPlayers[i].SetFormation(pFormation[m_sortPlayers[i].IPlay]);
                    }
                    else
                    {
                        m_sortPlayers[i].SetFormation(0);
                    }
                }
                if( !MainUI.gameObject.activeInHierarchy) return;

                CurrentSort();
            }
            break;
            case E_REQUEST_ID.auction_trade:
            case E_REQUEST_ID.scout_reward:
            case E_REQUEST_ID.recruit_offer:
            case E_REQUEST_ID.youth_offer:
            case E_REQUEST_ID.tutorial_recruit:
            case E_REQUEST_ID.adReward_reward:
            {
                if(eType == E_REQUEST_ID.adReward_reward && !data.Json.ContainsKey("players")) return;
            
                JArray pArray = (JArray)data.Json["players"];
                
                ulong id = 0;
                byte age = 0;
                byte formation = 0;
                int playerIndex = -1;
                GameContext pGameContext = GameContext.getCtx();
                int index = pGameContext.GetActiveLineUpType();
                List<byte> pFormation = pGameContext.GetFormationByIndex(index);
                SortPlayer pSortPlayer = null;
                PlayerT pPlayer = null;

                for(int n =0; n < pArray.Count; ++n)
                {
                    id = (ulong)(((JObject)(pArray[n]))["id"]);
                    playerIndex = pGameContext.GetLineupPlayerIndex(index,id);
                    pPlayer = pGameContext.GetPlayerByID(id);
                    age = pGameContext.GetPlayerAge(pPlayer);
                    
                    formation = 0;
                    if(playerIndex < GameContext.CONST_NUMSTARTING)
                    {
                        formation = pFormation[playerIndex];
                    }
                    
                    pSortPlayer = new SortPlayer( id, pPlayer.Surname,pPlayer.Forename,age,playerIndex,pPlayer.Position,formation,pPlayer.AbilityWeightSum,pPlayer.Hp);
                    m_sortPlayers.Add(pSortPlayer);
                }
                UpdateSquadCount(pGameContext.GetTotalPlayerCount());
                if( !MainUI.gameObject.activeInHierarchy) return;

                CurrentSort();
            }
            break;
            case E_REQUEST_ID.player_recoverHP:
            {
                if( !MainUI.gameObject.activeInHierarchy) return;
                
                UpdatePlayerHP(true);
                if(m_eSelectTab == E_TAB.cnd)
                {
                    CurrentSort();
                }
            }
            break;
            case E_REQUEST_ID.player_positionFamiliarUp:
            case E_REQUEST_ID.player_abilityUp:
            case E_REQUEST_ID.auction_register:
            case E_REQUEST_ID.auction_cancel:
            case E_REQUEST_ID.auction_withdraw:
            {
                if( !MainUI.gameObject.activeInHierarchy) return;

                if(data.Json.ContainsKey("players"))
                {
                    JArray pArray = (JArray)data.Json["players"];
                    ulong id = 0;
                    JObject item = null;
                    GameContext pGameContext = GameContext.getCtx();
                    int index = 0;
                    for(int n =0; n < pArray.Count; ++n)
                    {
                        item = (JObject)pArray[n];
                        id = (ulong)item["id"];
                        
                        if(eType == E_REQUEST_ID.player_abilityUp)
                        {
                            PlayerT pPlayer = null;
                            for(int i = 0; i < m_sortPlayers.Count; ++i)
                            {
                                if(m_sortPlayers[i].Id == id)
                                {
                                    pPlayer = pGameContext.GetPlayerByID(id);
                                    m_sortPlayers[i].UpdateQuality(pPlayer.AbilityWeightSum);
                                    break;
                                }
                            }
                        }

                        for(int i = 0; i < m_pPlayers.Count; ++i)
                        {
                            if(m_pPlayers[i].Id == id)
                            {
                                index = m_iStartIndex + i;
                                if(index > -1 && m_sortPlayers.Count > index)
                                {
                                    m_pPlayers[i].UpdatePlayerData(m_sortPlayers[index]);
                                }
                                
                                break;
                            }
                        }
                    }

                    if(eType == E_REQUEST_ID.player_abilityUp && m_eSelectTab == E_TAB.quality)
                    {
                        CurrentSort();
                    }
                }
            }
            break;
        }
    }
}
