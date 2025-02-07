using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ALF;
using ALF.LAYOUT;
using ALF.SOUND;
using DATA;
using USERDATA;
using ALF.STATE;
using ALF.CONDITION;
using ALF.MACHINE;


public class PlayerComparison : IBaseUI
{
    enum E_TAB{ information,playerList }
    readonly string[] SCROLL_ITEM_NAME = new string[]{"PlayerComparisonIteam","PlayerNameItem"};
    readonly string[] AbilityNodeNameList = new string[]{"ABILITY_TXT_ATT_SHOOT","ABILITY_TXT_ATT_PASS","ABILITY_TXT_ATT_DRIBBLE","ABILITY_TXT_ATT_CROSS","ABILITY_TXT_ATT_TOUCH","ABILITY_TXT_ATT_OFFTHEBALL","ABILITY_TXT_DEF_TACKLE","ABILITY_TXT_DEF_MARKING","ABILITY_TXT_DEF_POSITIONING","ABILITY_TXT_DEF_PRESSURE","ABILITY_TXT_DEF_INTERCEPT","ABILITY_TXT_DEF_CONCENTRATION","ABILITY_TXT_PHY_SPEED","ABILITY_TXT_PHY_ACCEL","ABILITY_TXT_PHY_AGILITY","ABILITY_TXT_PHY_BALANCE","ABILITY_TXT_PHY_STAMINA","ABILITY_TXT_PHY_JUMP","ABILITY_TXT_MEN_CREATIVITY","ABILITY_TXT_MEN_VISION","ABILITY_TXT_MEN_DETERMINATION","ABILITY_TXT_MEN_DICISION","ABILITY_TXT_MEN_AGGRESSIVE","ABILITY_TXT_MEN_INFLUENCE","ABILITY_TXT_GK_REFLEX","ABILITY_TXT_GK_COMMAND","ABILITY_TXT_GK_GOALKICK","ABILITY_TXT_GK_ONEONONE","ABILITY_TXT_GK_HANDLING","ABILITY_TXT_GK_AERIALABILITY"};

    class PlayerNameItem : IBase
    {
        public ulong Id {get; private set;}
        
        public TMPro.TMP_Text PlayerName {get; private set;}
        public TMPro.TMP_Text FormationText {get; private set;}
        public RawImage Formation {get; private set;}
        
        string m_pItemName = null;

        public RectTransform Target  {get; private set;}
        
        Button m_pButton = null;

        public PlayerNameItem(RectTransform target,string pItemName)
        {
            m_pItemName = pItemName;
            Target = target;
            m_pButton = target.GetComponent<Button>();
            PlayerName = target.Find("name").GetComponent<TMPro.TMP_Text>();
            FormationText = target.Find("form/text").GetComponent<TMPro.TMP_Text>();
            Formation = target.Find("form").GetComponent<RawImage>();
        }

        public void Dispose()
        {
            FormationText = null;
            Formation.texture = null;
            Formation = null;

            m_pButton.onClick.RemoveAllListeners();
            m_pButton = null;
            PlayerName = null;
            LayoutManager.Instance.AddItem(m_pItemName,Target);
            Target = null;
        }

        public void UpdatePlayerInfo(PlayerT pPlayData)
        {
            if(pPlayData == null) return;
            Id = pPlayData.Id;
            PlayerName.SetText($"{pPlayData.Forename} {pPlayData.Surname}");

            byte locValue = 0;
            string loc = null;
            GameContext pGameContext = GameContext.getCtx();
            for(int n =0; n < pPlayData.PositionFamiliars.Count; ++n)
            {
                if(pPlayData.PositionFamiliars[n] >= 80)
                {
                    if(locValue < pPlayData.PositionFamiliars[n])
                    {
                        locValue = pPlayData.PositionFamiliars[n];
                        loc = pGameContext.GetDisplayLocationName(n);
                    }
                }
            }

            if(string.IsNullOrEmpty(loc))
            {
                Formation.gameObject.SetActive(false);
            }
            else
            {
                Formation.gameObject.SetActive(true);
                FormationText.SetText(loc);

                Sprite pSprite = AFPool.GetItem<Sprite>("Texture",pGameContext.GetDisplayCardFormationByLocationName(loc));
                Formation.texture = pSprite.texture;
            }
        }
    }

    MainScene m_pMainScene = null;
    ScrollRect[] m_pScrollRect = new ScrollRect[2];
    TMPro.TMP_Text[] m_PlayerNames = new TMPro.TMP_Text[2];
    Transform[] m_QualityGroups = new Transform[2];
    TMPro.TMP_Text[] m_QualityTexts = new TMPro.TMP_Text[2];
    Transform[] m_pPlayerCards = new Transform[2];
    TMPro.TMP_Text[] m_Ages = new TMPro.TMP_Text[2];
    TMPro.TMP_Text[] m_Values = new TMPro.TMP_Text[2];
    Transform[] m_MainRoles = new Transform[2];
    PlayerT[] m_pPlayerDatas = new PlayerT[2];
    List<PlayerT> m_pPlayerDataList = new List<PlayerT>();

    EmblemBake[] m_pEmblemList = new EmblemBake[2];

    float m_fInfoHeight = 0;

    public RectTransform MainUI { get; private set;}

    public bool Enable 
    {
        set{        
            if (m_pScrollRect != null) 
            {
                for(int i =0; i < m_pScrollRect.Length; ++i)
                {
                    m_pScrollRect[i].enabled = value;
                }
            }
        }
    }

    List<PlayerNameItem> m_pPlayers = new List<PlayerNameItem>();
    Vector2 m_pPrevDir = Vector2.zero;
    int m_iTotalScrollItems = 0;
    int m_iStartIndex = 0;
    
    public PlayerComparison(){}
    
    public void Dispose()
    {
        m_pMainScene = null;
        MainUI = null;
    
        int i =0;
        for(i =0; i < m_pScrollRect.Length; ++i)
        {
            ClearScroll((E_TAB)i);
            m_pScrollRect[i].onValueChanged.RemoveAllListeners();
            m_pScrollRect[i] = null;
        }

        m_pScrollRect = null;
        m_pPlayerDataList.Clear();
        m_pPlayerDataList = null;
        ClearPlayerFace();
        
        for(i =0; i < m_pPlayerCards.Length; ++i)
        {
            m_pPlayerCards[i] = null;
            m_QualityGroups[i] = null;
            m_PlayerNames[i] = null;
            m_Ages[i] = null;
            m_Values[i] = null;
            m_MainRoles[i] = null;
            m_pPlayerDatas[i] = null;
            m_QualityTexts[i]= null;
        }

        m_pEmblemList[0].Dispose();
        m_pEmblemList[0] = null;

        m_pEmblemList[1].material = null;
        m_pEmblemList[1] = null;
        m_pEmblemList = null;

        m_pPlayerCards = null;
        m_QualityTexts = null;
        m_QualityGroups = null;
        m_PlayerNames = null;
        m_Ages = null;
        m_Values = null;
        m_MainRoles = null;
        m_pPlayerDatas = null;
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "PlayerComparison : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "PlayerComparison : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;

        Transform tm = MainUI.Find("root/playerInfo");
        m_fInfoHeight = tm.GetComponent<RectTransform>().rect.height;
        m_pPlayerCards[0] = tm.Find("player");
        m_pPlayerCards[1] = tm.Find("my");

        m_pEmblemList[0] = m_pPlayerCards[0].Find("emblem").GetComponent<EmblemBake>();
        m_pEmblemList[1] = m_pPlayerCards[1].Find("emblem").GetComponent<EmblemBake>();

        m_PlayerNames[0] = m_pPlayerCards[0].Find("name").GetComponent<TMPro.TMP_Text>();
        m_PlayerNames[1] = m_pPlayerCards[1].Find("name").GetComponent<TMPro.TMP_Text>();
        
        
        m_QualityGroups[0] = m_pPlayerCards[0].Find("quality");
        m_QualityTexts[0] = m_pPlayerCards[0].Find("info/quality/point").GetComponent<TMPro.TMP_Text>();
        m_QualityGroups[1] = m_pPlayerCards[1].Find("quality");
        m_QualityTexts[1] = m_pPlayerCards[1].Find("info/quality/point").GetComponent<TMPro.TMP_Text>();

        m_MainRoles[0] = m_pPlayerCards[0].Find("info/roles/roles");
        m_MainRoles[1] = m_pPlayerCards[1].Find("info/roles/roles");

        m_Ages[0] = m_pPlayerCards[0].Find("info/age/text").GetComponent<TMPro.TMP_Text>();
        m_Ages[1] = m_pPlayerCards[1].Find("info/age/text").GetComponent<TMPro.TMP_Text>();
        
        m_Values[0] = m_pPlayerCards[0].Find("info/value/text").GetComponent<TMPro.TMP_Text>();
        m_Values[1] = m_pPlayerCards[1].Find("info/value/text").GetComponent<TMPro.TMP_Text>();

        MainUI.gameObject.SetActive(false);
        
        m_pScrollRect[0] = MainUI.Find("root/information").GetComponent<ScrollRect>();
        m_pScrollRect[1] = MainUI.Find("playerList/scroll").GetComponent<ScrollRect>();

        m_pScrollRect[1].transform.position = m_PlayerNames[1].transform.parent.position;
        m_pScrollRect[1].onValueChanged.AddListener( ScrollViewChangeValueEventCall);
        RectTransform rt = m_pScrollRect[1].GetComponent<RectTransform>();
        Vector2 pos = rt.anchoredPosition;
        pos.y -= 10;
        rt.anchoredPosition = pos;
        
        LayoutManager.SetReciveUIButtonEvent(MainUI,this.ButtonEventCall);
        SetupScroll(E_TAB.information);
        SetupScroll(E_TAB.playerList);
    }

    void UpdatePlayerList()
    {
        ResetScroll(E_TAB.playerList);
        
        ScrollRect pScrollRect = m_pScrollRect[(int)E_TAB.playerList];

        m_iStartIndex = 0;

        float viewSize = pScrollRect.viewport.rect.height;
        float itemSize = 0;
        PlayerNameItem pPlayerNameItem = null;
        
        for(int i =0; i < m_pPlayers.Count; ++i)
        {
            pPlayerNameItem = m_pPlayers[i];
            itemSize = pPlayerNameItem.Target.rect.height;

            if(m_pPlayerDataList.Count <= i)
            {
                pPlayerNameItem.Target.gameObject.SetActive(false);
            }
            else
            {
                pPlayerNameItem.UpdatePlayerInfo(m_pPlayerDataList[i]);
            
                if(viewSize > -itemSize)
                {    
                    viewSize -= itemSize;
                    pPlayerNameItem.Target.gameObject.SetActive(viewSize > -itemSize);
                }
            }
        }

        Vector2 size = pScrollRect.content.sizeDelta;
        size.y = m_pPlayerDataList.Count * itemSize;
        pScrollRect.content.sizeDelta = size;
        pScrollRect.verticalNormalizedPosition = 1;
        m_pPrevDir.y = 1;
        pScrollRect.content.anchoredPosition = Vector2.zero;
        float h = m_fInfoHeight;
        if(m_pPlayerDataList.Count <= 0)
        {
            h = 40;
        }
        RectTransform pItem = pScrollRect.GetComponent<RectTransform>(); 
        size = pItem.sizeDelta;
        size.y = h;
        pItem.sizeDelta = size;
    }

    void SetupScroll(E_TAB eType)
    {
        RectTransform pItem = null;
        Vector2 size;
        float h = 0;
        ScrollRect pScrollRect = m_pScrollRect[(int)eType];
        string itemName = SCROLL_ITEM_NAME[(int)eType];
        
        if(eType == E_TAB.information)
        {
            LocalizingText text = null;
            E_ABILINDEX eStart = E_ABILINDEX.AB_ATT_PASS;
            E_TRAINING_TYPE eAttType = E_TRAINING_TYPE.attacking;
            Transform tm = null;
            int n = 0;
            for(E_TRAINING_TYPE i =0; i < E_TRAINING_TYPE.MAX; ++i)
            {
                pItem = LayoutManager.Instance.GetItem<RectTransform>(itemName);
                    
                if(pItem)
                {
                    for(E_TRAINING_TYPE t =0; t < E_TRAINING_TYPE.MAX; ++t)
                    {
                        pItem.Find($"top/{t.ToString()}").gameObject.SetActive(false);
                    }
                    
                    pItem.Find($"top/{i.ToString()}").gameObject.SetActive(true);
                    text = pItem.Find("top/title").GetComponent<LocalizingText>();
                    text.Key = TacticsFormation.GetAbilityTitleNameByIndex((int)i);
                    text.UpdateLocalizing();
                    
                    eStart = (E_ABILINDEX)((int)E_ABILINDEX.AB_ATT_PASS + ((int)eAttType * 6));
                    n = 0;
                    
                    while(n < 6)
                    {
                        tm = pItem.Find($"item{n}");
                        text = tm.Find("title").GetComponent<LocalizingText>();
                        text.Key = AbilityNodeNameList[(int)eStart];
                        text.UpdateLocalizing();
                        ++eStart;
                        ++n;
                    }
                    
                    ++eAttType;

                    pItem.SetParent(pScrollRect.content,false);
                    
                    pItem.localScale = Vector3.one;       
                    pItem.anchoredPosition = new Vector2(0,-h);
                    size = pItem.sizeDelta;
                    h += size.y;
                    size.x = 0;
                    pItem.sizeDelta = size;
                }
            }

            tm = pScrollRect.content.GetChild(0);
            pItem.position = tm.position;
            size = pItem.sizeDelta;
            h -= size.y;
        }
        else
        {
            m_iTotalScrollItems = 0;
            m_iStartIndex = 0;

            float viewSize = pScrollRect.viewport.rect.height;
            float itemSize = 0;
            PlayerNameItem pPlayerItem = null;

            while(viewSize > -itemSize)
            {
                if(viewSize > 0)
                {
                    ++m_iTotalScrollItems;
                }
                
                pItem = LayoutManager.Instance.GetItem<RectTransform>(itemName);
                    
                if(pItem)
                {
                    pPlayerItem = new PlayerNameItem(pItem,itemName);
                    m_pPlayers.Add(pPlayerItem);
                    
                    pItem.SetParent(pScrollRect.content,false);
                    pItem.anchoredPosition = new Vector2(0,-h);
                    size = pItem.sizeDelta;
                    size.x = 0;
                    pItem.sizeDelta = size;
                    itemSize = pItem.rect.height;
                    h += itemSize;
                    viewSize -= itemSize;
                }
            }

            LayoutManager.SetReciveUIScollViewEvent(pScrollRect,ScrollViewItemButtonEventCall);
        }
        
        size = pScrollRect.content.sizeDelta;
        size.y = h;
        pScrollRect.content.sizeDelta = size;
        pScrollRect.content.anchoredPosition = Vector2.zero; 
    }

    
    void ClearScroll(E_TAB eType)
    {
        ScrollRect pScrollRect = m_pScrollRect[(int)eType];

        if(eType == E_TAB.playerList)
        {
            int i = m_pPlayers.Count;
            while(i > 0)
            {
                --i;
                m_pPlayers[i].Dispose(); 
            }
            
            m_pPlayers.Clear();
        }
        else
        {
            int i = pScrollRect.content.childCount;
            string itemName = SCROLL_ITEM_NAME[(int)eType];
            
            while(i > 0)
            {
                --i;
                LayoutManager.Instance.AddItem(itemName,pScrollRect.content.GetChild(i).GetComponent<RectTransform>());
            }

        }

        LayoutManager.SetReciveUIScollViewEvent(pScrollRect,null);
        
        pScrollRect.content.anchoredPosition = Vector2.zero;
        
        Vector2 size = pScrollRect.content.sizeDelta;
        size.y =0;
        pScrollRect.content.sizeDelta = size;
    }

    void ResetScroll(E_TAB eType)
    {
        if(eType == E_TAB.playerList)
        {
            ScrollRect pScrollRect = m_pScrollRect[(int)eType];
            Vector2 pos;
            float viewSize = pScrollRect.viewport.rect.height;
            float itemSize = 0;
            PlayerNameItem pPlayerItem = null;
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

            pScrollRect.verticalNormalizedPosition = 1;
            pScrollRect.content.anchoredPosition = Vector2.zero;
            m_iStartIndex = 0;
            m_pPrevDir.y = 1;
        }
    }

    void UpdatePlayerInfoData()
    {
        TMPro.TMP_Text text = null;
        Transform pItem = null;
        ScrollRect pScrollRect = m_pScrollRect[(int)E_TAB.information];
        
        E_ABILINDEX eStart = E_ABILINDEX.AB_ATT_PASS;
        E_TRAINING_TYPE eAttType = E_TRAINING_TYPE.attacking;
        Transform tm = null;
        int n = 0;
        int total =0;
        int total1 =0;
        for(E_TRAINING_TYPE i =0; i < E_TRAINING_TYPE.MAX; ++i)
        {
            pItem = pScrollRect.content.GetChild((int)i);
                
            eStart = (E_ABILINDEX)((int)E_ABILINDEX.AB_ATT_PASS + ((int)eAttType * 6));
            n = 0;
            total = 0;
            total1 = 0;
            while(n < 6)
            {
                total += m_pPlayerDatas[0].Ability[(int)eStart].Current;
                total1 += m_pPlayerDatas[1].Ability[(int)eStart].Current;

                tm = pItem.Find($"item{n}");
                UpdatePlayerAbility(eStart,tm);
                ++eStart;
                ++n;
            }
            
            text = pItem.Find("top/player").GetComponent<TMPro.TMP_Text>();
            text.SetText(((int)(total / 6)).ToString());
            text = pItem.Find("top/player1").GetComponent<TMPro.TMP_Text>();
            text.SetText(((int)(total1 / 6)).ToString());

            ++eAttType;
        }

        tm = pScrollRect.content.GetChild(0);

        if( (E_LOCATION)m_pPlayerDatas[0].Position == E_LOCATION.LOC_GK)
        {    
            tm.gameObject.SetActive(false);
            pItem.gameObject.SetActive(true);
        }        
        else
        {
            tm.gameObject.SetActive(true);
            pItem.gameObject.SetActive(false);
        }
    }

    public void SetupPlayerInfoData(PlayerT pPlayer,byte[] pEmblemData)
    {
        if(pEmblemData != null)
        {
            m_pEmblemList[0].SetupEmblemData(pEmblemData);
            m_pEmblemList[0].gameObject.SetActive(true);
        }
        else
        {
            m_pEmblemList[0].gameObject.SetActive(false);
        }
        
        m_pPlayerDatas[0] = pPlayer;
        m_pEmblemList[1].CopyPoint(m_pMainScene.GetMyEmblem());
        
        m_pPlayerDataList.Clear();
        List<PlayerT> pPlayerList = GameContext.getCtx().GetTotalPlayerList();
        for(int i =0; i < pPlayerList.Count; ++i)
        {
            if(pPlayer.Id != pPlayerList[i].Id)
            {
                if(pPlayer.Position == 0 && 0 == pPlayerList[i].Position)
                {
                    m_pPlayerDataList.Add(pPlayerList[i]);
                }
                else if(pPlayer.Position != 0 && 0 != pPlayerList[i].Position)
                {
                    m_pPlayerDataList.Add(pPlayerList[i]);
                }
            }
        }

        m_pPlayerDatas[1] = m_pPlayerDataList[0];
        m_pPlayerDataList.RemoveAt(0);

        SetupPlayerInformation(0);
        SetupPlayerInformation(1);
        UpdatePlayerInfoData();
        UpdatePlayerList();
        
        m_pScrollRect[(int)E_TAB.playerList].transform.parent.gameObject.SetActive(false);
    }
    void SetupPlayerInformation(int i)
    {
        m_PlayerNames[i].SetText($"{m_pPlayerDatas[i].Forename} {m_pPlayerDatas[i].Surname}");
        SingleFunc.SetupQuality(m_pPlayerDatas[i],m_QualityGroups[i]);
        SingleFunc.SetupPlayerFace(m_pPlayerDatas[i],m_pPlayerCards[i].Find("icon"));
        
        byte age = GameContext.getCtx().GetPlayerAge(m_pPlayerDatas[i]);
        m_Ages[i].SetText(age > 40 ? "40+":age.ToString());
        m_Values[i].SetText(ALFUtils.NumberToString(m_pPlayerDatas[i].Price));

        int n = 0;
        for(n = 0; n < m_MainRoles[i].childCount; ++n)
        {
            m_MainRoles[i].GetChild(n).gameObject.SetActive(false);                
        }
        
        List<string> locList= new List<string>();
        
        RawImage icon = null;
        Sprite pSprite = null;

        for(n = 0; n < m_pPlayerDatas[i].PositionFamiliars.Count; ++n)
        {
            if(m_pPlayerDatas[i].PositionFamiliars[n] >= 80)
            {
                locList.Add(GameContext.getCtx().GetDisplayLocationName(n));
            }
        }
        
        for(n = 0; n < locList.Count; ++n)
        {
            if(m_MainRoles[n].childCount > n)
            {
                icon = m_MainRoles[i].GetChild(m_MainRoles[i].childCount -1 - n).GetComponent<RawImage>();
                icon.gameObject.SetActive(true);

                pSprite = AFPool.GetItem<Sprite>("Texture",GameContext.getCtx().GetDisplayCardFormationByLocationName(locList[n]));
                icon.texture = pSprite.texture;
                icon.transform.Find("text").GetComponent<TMPro.TMP_Text>().SetText(locList[n]);
            }
        }

        m_QualityTexts[i].gameObject.SetActive(true);
        m_QualityTexts[i].SetText($"{m_pPlayerDatas[i].AbilityTier}/{m_pPlayerDatas[i].PotentialTier}");
    }
    
    void UpdatePlayerAbility(E_ABILINDEX index, Transform tm)
    {
        RectTransform item = tm.Find("gauge").GetComponent<RectTransform>();
        float w = item.rect.width;

        TMPro.TMP_Text text = tm.Find("player").GetComponent<TMPro.TMP_Text>();
        int ability0 = m_pPlayerDatas[0].Ability[(int)index].Current;
        int ability1 = m_pPlayerDatas[1].Ability[(int)index].Current;
        text.SetText(ability0.ToString());
        text = tm.Find("player1").GetComponent<TMPro.TMP_Text>();
        text.SetText(ability1.ToString());
        
        Image gauge = tm.Find("gauge/player").GetComponent<Image>();
        if(ability0 == ability1)
        {
            gauge.fillAmount = 0.5f;
        }
        else
        {
            gauge.fillAmount = (float)ability0 / (float)(ability0 + ability1);
        }
    }

    void ClearPlayerFace()
    {
        SingleFunc.ClearPlayerFace(m_pPlayerCards[0].Find("icon"));
        SingleFunc.ClearPlayerFace(m_pPlayerCards[1].Find("icon"));
    }

    void ScrollViewChangeData(int index,int iTarget,int iCount)
    {
        if(index == iTarget) return;
        int i = 0;
        PlayerNameItem pPlayerItem = null;
        if(index > iTarget)
        {
            pPlayerItem = m_pPlayers[iTarget];
            m_pPlayers[iTarget] = m_pPlayers[index];
            i = iTarget +1;
            PlayerNameItem pTemp = null;
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
        
        if(i < 0 || m_pPlayerDataList.Count <= i) return;
        
        pPlayerItem.UpdatePlayerInfo(m_pPlayerDataList[i]);
    }

    void ScrollViewChangeValueEventCall( Vector2 value)
    {
        if(m_iTotalScrollItems < m_pPlayerDataList.Count && value.y != m_pPrevDir.y)
        {
            m_pScrollRect[(int)E_TAB.playerList].ScrollViewChangeValue(value - m_pPrevDir,ref m_iStartIndex,ScrollViewChangeData);
            m_pPrevDir = value;
        }
    }

    IState exitCallback(IState state)
    {
        LayoutManager.Instance.InteractableEnabledAll();
        ALF.NETWORK.NetworkManager.ShowWaitMark(false);

        SetupPlayerInformation(1);
        UpdatePlayerInfoData();

        return null;
    }
    
    void ScrollViewItemButtonEventCall(ScrollRect root, Transform tm,GameObject sender)
    {
        for(int i =0; i < m_pPlayers.Count; ++i)
        {
            if(m_pPlayers[i].Target == tm)
            {
                PlayerT pPlayerData = m_pPlayerDataList[m_iStartIndex + i];
                
                m_pPlayers[i].UpdatePlayerInfo(m_pPlayerDatas[1]);
                m_pPlayerDataList[m_iStartIndex + i] = m_pPlayerDatas[1];
                m_pPlayerDatas[1] = pPlayerData; 
                root.transform.parent.gameObject.SetActive(false);

                ALF.NETWORK.NetworkManager.ShowWaitMark(true);
                LayoutManager.Instance.InteractableDisableAll();

                BaseState pBaseState = BaseState.GetInstance(new BaseStateTarget(MainUI),0.5f, (uint)STATEDATA.E_STATE_TYPE.Timer, null,null,exitCallback);
                StateMachine.GetStateMachine().AddState(pBaseState);
                
                return;
            }
        }
    }

    public void Close()
    {
        Enable = false;
        List<BaseState> list = StateMachine.GetStateMachine().GetCurrentTargetStates<BaseState>(MainUI,(uint)STATEDATA.E_STATE_TYPE.Timer);
        for(int i =0; i < list.Count; ++i)
        {
            list[i].SetExitCallback(null);
            ALF.NETWORK.NetworkManager.ShowWaitMark(false);
            list[i].Exit(true);
        }

        SingleFunc.HideAnimationDailog(MainUI,()=>{
            m_pEmblemList[0].Dispose();
            m_pEmblemList[1].material = null;
            ClearPlayerFace();
            MainUI.gameObject.SetActive(false);
            MainUI.Find("root").localScale = Vector3.one;
            LayoutManager.Instance.InteractableEnabledAll();
        });
    }

    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        if(sender.name == "list")
        {
            bool bActive = m_pScrollRect[(int)E_TAB.playerList].transform.parent.gameObject.activeSelf;
            m_pScrollRect[(int)E_TAB.playerList].transform.parent.gameObject.SetActive(!bActive);
            return;
        }
        else if(sender.name == "playerList")
        {
            sender.SetActive(false);
            return;
        }
        
        Close();
    }
}
