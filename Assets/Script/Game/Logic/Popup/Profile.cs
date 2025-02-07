using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ALF;

using ALF.STATE;
using ALF.LAYOUT;
using ALF.MACHINE;
using ALF.SOUND;
using USERDATA;
using DATA;
using USERRANK;
using UnityEngine.EventSystems;
using Newtonsoft.Json.Linq;
using STATEDATA;
using MATCHTEAMDATA;
using PLAYERNATIONALITY;
using CLUBNATIONALITY;

public class Profile : IBaseUI
{
    const string SCROLL_ITEM_NAME = "LeagueSeasonItem";
    enum E_TAB : byte { overview = 0,squad,history,league,MAX}
    enum E_TEXT_NAME : byte { USERPROFILE_HISTORY_BTN_TOTAL,USERPROFILE_HISTORY_BTN_SEASON}

    private class LeagueSeasonData : IBase
    {
        public uint SeasonNo {get; private set;}
        public int Standing {get; private set;} 
        
        public int MatchType {get; private set;} 
        
        public int Game {get; private set;} 
        public int Win {get; private set;} 
        public int Lose {get; private set;} 

        public LeagueSeasonData(JObject item)
        {
            if(item == null) return;

            Standing = (int)item["standing"];
            SeasonNo = (uint)item["seasonNo"];

            Game = (int)item["game"];
            Win = (int)item["win"];
            Lose =(int)item["lose"];
            MatchType = (int)item["matchType"];
        }

        public void Dispose()
        {
        }
    }

    private class LeagueSeasonItem : IBase
    {
        public RectTransform Target  {get; private set;}
        
        public TMPro.TMP_Text SeasonNoText {get; private set;}
        public TMPro.TMP_Text LeagueText {get; private set;}
        public TMPro.TMP_Text PText {get; private set;}
        public TMPro.TMP_Text WinText {get; private set;}
        public TMPro.TMP_Text DrawText {get; private set;}
        public TMPro.TMP_Text LoseText {get; private set;}
        public TMPro.TMP_Text RankingText {get; private set;}
        
        public LeagueSeasonItem(RectTransform taget)
        {
            Target = taget;
            SeasonNoText = Target.Find("season").GetComponent<TMPro.TMP_Text>();
            LeagueText = Target.Find("league").GetComponent<TMPro.TMP_Text>();
            PText = Target.Find("P").GetComponent<TMPro.TMP_Text>();
            WinText = Target.Find("W").GetComponent<TMPro.TMP_Text>();
            DrawText = Target.Find("D").GetComponent<TMPro.TMP_Text>();
            LoseText = Target.Find("L").GetComponent<TMPro.TMP_Text>();
            RankingText = Target.Find("ranking").GetComponent<TMPro.TMP_Text>();   
        }

        public void Dispose()
        {
            SeasonNoText = null;
            LeagueText = null;
            PText = null;
            WinText = null;
            DrawText = null;
            LoseText = null;
            RankingText = null;
            
            LayoutManager.Instance.AddItem(SCROLL_ITEM_NAME,Target);
            Target = null;
        }

        public void UpdateInfo(LeagueSeasonData pLeagueSeasonData)
        {
            if(pLeagueSeasonData == null) return;

            GameContext pGameContext = GameContext.getCtx();            
            
            int temp = (int)pLeagueSeasonData.SeasonNo / 100;
            SeasonNoText.SetText(($"{temp}_{pLeagueSeasonData.SeasonNo - (temp*100)}"));
            
            DATA.E_TEXT_NAME eTxt = (DATA.E_TEXT_NAME)(pLeagueSeasonData.MatchType - GameContext.CHALLENGE_ID);
            LeagueText.SetText(pGameContext.GetLocalizingText(eTxt.ToString()));
            LeagueText.color = pLeagueSeasonData.MatchType > GameContext.CHALLENGE_ID ? GameContext.BULE : GameContext.YELLOW;

            PText.SetText(pLeagueSeasonData.Game.ToString());
            WinText.SetText(pLeagueSeasonData.Win.ToString());
            DrawText.SetText((pLeagueSeasonData.Game - pLeagueSeasonData.Win - pLeagueSeasonData.Lose).ToString());
            LoseText.SetText(pLeagueSeasonData.Lose.ToString());
            RankingText.SetText(pLeagueSeasonData.Standing > 0 ? string.Format("#{0}",pLeagueSeasonData.Standing ) : "-");
        }
    }
    
    
    // const float LongTouchTime = 2f;
    MainScene m_pMainScene = null;
    EmblemBake m_pEmblem = null;
    float[] m_fFeildSlotXList = new float[2];
    ScrollRect m_pScrollRect = null;
    RectTransform[] m_pTabUIList = new RectTransform[(int)E_TAB.MAX];
    Transform[] m_pTabButtonList = new Transform[(int)E_TAB.MAX];
    RectTransform[] m_pFormationList = new RectTransform[11];
    E_PLAYER_INFO_TYPE m_ePlayerInfoType = E_PLAYER_INFO_TYPE.my;
    E_TEXT_NAME m_eCurrentTextName = E_TEXT_NAME.USERPROFILE_HISTORY_BTN_SEASON;
    RectTransform m_pTabUI = null;

    public bool Enable { set{ if (m_pScrollRect != null) m_pScrollRect.enabled = value;}}
    public RectTransform MainUI { get; private set; }

    List<LeagueSeasonItem> m_pLeagueSeasonItems = new List<LeagueSeasonItem>();
    List<LeagueSeasonData> m_pLeagueSeasonDataList = new List<LeagueSeasonData>();
    
    Vector2 m_pPrevDir = Vector2.zero;
    int m_iTotalScrollItems = 0;
    int m_iStartIndex = 0;

    public Profile(){}
    
    public void Dispose()
    {
        m_pTabUI = null;
        ClearScroll();
        m_pScrollRect.onValueChanged.RemoveAllListeners();
        m_pScrollRect = null;
        m_pMainScene = null;
        MainUI = null;
        int i =0;
        for( i =0; i < m_pTabButtonList.Length; ++i)
        {
            m_pTabButtonList[i] = null;
            m_pTabUIList[i] = null;
        }
        
        for( i =0; i < m_pFormationList.Length; ++i)
        {
            m_pFormationList[i] = null;
        }
        m_pFormationList = null;
        m_pTabButtonList = null;
        m_pTabUIList = null;
        
        if(m_pEmblem != null)
        {
            m_pEmblem.material = null;
        }
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "Profile : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "Profile : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;

        RectTransform item = null;
        
        RectTransform ui = MainUI.Find("back").GetComponent<RectTransform>();
        LayoutManager.SetReciveUIButtonEvent(ui,this.ButtonEventCall);
        m_pTabUI = MainUI.Find("root/tabs").GetComponent<RectTransform>();
        LayoutManager.SetReciveUIButtonEvent(m_pTabUI,this.ButtonEventCall);

        int iTabIndex = -1;
        int n =0;
        for(n =0; n < m_pTabUI.childCount; ++n)
        {
            item = m_pTabUI.GetChild(n).GetComponent<RectTransform>();
            iTabIndex = (int)((E_TAB)Enum.Parse(typeof(E_TAB), item.gameObject.name));
            m_pTabButtonList[iTabIndex] = item;
            m_pTabUIList[iTabIndex] = MainUI.Find($"root/{item.gameObject.name}").GetComponent<RectTransform>();
            LayoutManager.SetReciveUIButtonEvent(m_pTabUIList[iTabIndex],this.ButtonEventCall);
            m_pTabUIList[iTabIndex].gameObject.SetActive(false);
        }
        SetupTapUI(true);
        Transform tm = m_pTabUIList[(int)E_TAB.squad].Find("feild");
        
        m_fFeildSlotXList[0] = tm.Find("LOC_DCL").GetComponent<RectTransform>().anchoredPosition.x;
        m_fFeildSlotXList[1] = tm.Find("LOC_DCR").GetComponent<RectTransform>().anchoredPosition.x;

        m_pEmblem = m_pTabUIList[(int)E_TAB.overview].Find("info/emblem").GetComponent<EmblemBake>();

        item = m_pTabUIList[(int)E_TAB.squad];
        
        for(n =0; n < m_pFormationList.Length; ++n)
        {
            m_pFormationList[n] = item.Find(n.ToString()).GetComponent<RectTransform>();
        }
        
        m_pScrollRect = MainUI.Find("root/league/record/scroll").GetComponent<ScrollRect>();
        LayoutManager.SetReciveUIButtonEvent(MainUI.Find("root/title").GetComponent<RectTransform>(),this.ButtonEventCall);
        
        SetupScroll();
        m_pScrollRect.onValueChanged.AddListener( ScrollViewChangeValueEventCall);
        MainUI.gameObject.SetActive(false);
    }

    void ScrollViewChangeData(int index,int iTarget,int iCount)
    {
        if(index == iTarget) return;
        
        int i = 0;
        LeagueSeasonItem pItem = null;
        
        if(index > iTarget)
        {
            pItem = m_pLeagueSeasonItems[iTarget];
            m_pLeagueSeasonItems[iTarget] = m_pLeagueSeasonItems[index];
            i = iTarget +1;
            LeagueSeasonItem pTemp = null;
            while(i < index)
            {
                pTemp = m_pLeagueSeasonItems[i];
                m_pLeagueSeasonItems[i] = pItem;
                pItem = pTemp;
                ++i;
            }
            m_pLeagueSeasonItems[index] = pItem;
            pItem = m_pLeagueSeasonItems[iTarget];
        }
        else
        {
            pItem = m_pLeagueSeasonItems[index];
            i = index +1;
            while(i <= iTarget)
            {
                m_pLeagueSeasonItems[i -1] = m_pLeagueSeasonItems[i];
                ++i;
            }

            m_pLeagueSeasonItems[iTarget] = pItem;
        }
        
        i = m_iStartIndex + iTarget + iCount;
        
        if(i < 0 || m_pLeagueSeasonDataList.Count <= i) return;

        pItem.UpdateInfo(m_pLeagueSeasonDataList[i]);
    }

    void ScrollViewChangeValueEventCall( Vector2 value)
    {
        if(m_iTotalScrollItems < m_pLeagueSeasonDataList.Count && value.y != m_pPrevDir.y)
        {
            m_pScrollRect.ScrollViewChangeValue(value - m_pPrevDir,ref m_iStartIndex,ScrollViewChangeData);
            m_pPrevDir = value;
        }
    }

    public void SetupTapUI(bool bInit)
    {
        bool isLeague = GameContext.getCtx().IsLeagueOpen();
        if(!bInit)
        {
            if(m_pTabButtonList[(int)E_TAB.league].gameObject.activeSelf == isLeague)
            {
                return;
            }
        }
        
        int count = m_pTabButtonList.Length;
        if(isLeague)
        {
            m_pTabButtonList[(int)E_TAB.league].gameObject.SetActive(true);
        }
        else
        {
            count -= 1;
            m_pTabButtonList[(int)E_TAB.league].gameObject.SetActive(false);
        }
        

        float w = (m_pTabUI.rect.width / count);
        float wh = w * 0.5f;
        float ax = m_pTabUI.pivot.x * m_pTabUI.rect.width;
        int iTabIndex = -1;
        int n =0;

        RectTransform item = null;
        Vector2 pos;
        Vector2 size;
        for(n =0; n < m_pTabButtonList.Length; ++n)
        {
            if(m_pTabButtonList[n].gameObject.activeSelf)
            {
                item = m_pTabButtonList[n].GetComponent<RectTransform>();
                iTabIndex = (int)((E_TAB)Enum.Parse(typeof(E_TAB), item.gameObject.name));
                m_pTabButtonList[iTabIndex] = item;
                
                pos = item.localPosition;
                size = item.sizeDelta;
                size.x = w;
                pos.x = wh + (n * w) - ax;
                item.localPosition = pos;
                item.sizeDelta = size;
            }
        }
    }

    void ChangeSlotsPosition(List<byte> pFormation)
    {
        Transform tm = m_pTabUIList[(int)E_TAB.squad].Find("feild");

        int i = (int)E_LOCATION.LOC_DCL;
        int e = (int)E_LOCATION.LOC_DR;
        int n = i;
        bool bChange = false;
        Vector2 pos;

        RectTransform item = null;

        while(e < (int)E_LOCATION.LOC_END)
        {
            bChange = false;
            
            if(pFormation.Contains((byte)(i+1)))
            {
                bChange = true;
            }
            else
            {
                if(!pFormation.Contains((byte)i) && !pFormation.Contains((byte)(i+2)))
                {
                    bChange = true;
                }   
            }
            
            if(bChange)
            {
                item = tm.Find(((E_LOCATION)n).ToString()).GetComponent<RectTransform>();
                pos = item.anchoredPosition;
                pos.x = m_fFeildSlotXList[0];
                item.anchoredPosition = pos;
                n+=2;
                item = tm.Find(((E_LOCATION)n).ToString()).GetComponent<RectTransform>();
                pos = item.anchoredPosition;
                pos.x = m_fFeildSlotXList[1];
                item.anchoredPosition = pos;
            }
            else
            {
                item = tm.Find(((E_LOCATION)n).ToString()).GetComponent<RectTransform>();
                pos = item.anchoredPosition;
                pos.x = m_fFeildSlotXList[0] + 45;
                item.anchoredPosition = pos;
                
                n+=2;
                item = tm.Find(((E_LOCATION)n).ToString()).GetComponent<RectTransform>();
                pos = item.anchoredPosition;
                pos.x = m_fFeildSlotXList[1] - 45;
                item.anchoredPosition = pos;
            }

            i += 5;
            e += 5;
            n = i;
        }
    }

    public void UpdateClubNameAnNation(uint totalAbilityWeightSum)
    {
        TMPro.TMP_Text pText = MainUI.Find("root/title/title").GetComponent<TMPro.TMP_Text>();
        pText.SetText(GameContext.getCtx().GetClubNameInClubProfile(m_ePlayerInfoType));
        SetupClubInfo(totalAbilityWeightSum);            
    }

    public void SetupLeagueHistoryData(JObject data)
    {
        if(MainUI != null && data != null && MainUI.gameObject.activeSelf)
        {
            ResetScroll();

            int i = m_pLeagueSeasonDataList.Count;
            while(i > 0)
            {
                --i;
                m_pLeagueSeasonDataList[i].Dispose();
            }
            m_pLeagueSeasonDataList.Clear();

            Transform info = m_pTabUIList[(int)E_TAB.league].Find("info");
            int value = 0;
            Transform node = null;
            TMPro.TMP_Text pText = null;
            JArray pEntered = null;
            JArray pWin = null;
            JObject item = null;

            if(data.ContainsKey("entered") && data["entered"].Type != JTokenType.Null)
            {
                pEntered = (JArray)data["entered"];
            }

            if(data.ContainsKey("win") && data["win"].Type != JTokenType.Null)
            {
                pWin = (JArray)data["win"];
            }

            for(int n = 0; n < info.childCount; ++n)
            {
                value = 0;
                node =info.GetChild(n);
                pText = node.Find("line/entered").GetComponent<TMPro.TMP_Text>();
                if(pEntered != null)
                {
                    for(i =0; i < pEntered.Count; ++i)
                    {
                        item = (JObject)pEntered[i];
                        if(node.gameObject.name == (string)item["matchType"])
                        {
                            value = (int)item["entered"];
                            break;
                        }
                    }
                }

                pText.SetText(value.ToString());
                value = 0;
                pText = node.Find("line/win").GetComponent<TMPro.TMP_Text>();
                if(pWin != null)
                {
                    for( i =0; i < pWin.Count; ++i)
                    {
                        item = (JObject)pWin[i];
                        if(node.gameObject.name == (string)item["matchType"])
                        {
                            value = (int)item["win"];
                            break;
                        }
                    }
                }

                pText.SetText(value.ToString());
            }

            float itemSize = 0;
            
            if(data.ContainsKey("list") && data["list"].Type != JTokenType.Null)
            {
                JArray pList = new JArray(((JArray)data["list"]).OrderByDescending(obj => (int)obj["seasonNo"]));
                
                for(i =0; i < pList.Count; ++i)
                {
                    m_pLeagueSeasonDataList.Add(new LeagueSeasonData((JObject)pList[i]));
                }

                m_iStartIndex = 0;
                float viewSize = m_pScrollRect.viewport.rect.height;

                LeagueSeasonItem pLeagueSeasonItem = null;
                for(i =0; i < m_pLeagueSeasonItems.Count; ++i)
                {
                    pLeagueSeasonItem = m_pLeagueSeasonItems[i];
                    itemSize = pLeagueSeasonItem.Target.rect.height;

                    if(m_pLeagueSeasonDataList.Count <= i)
                    {
                        pLeagueSeasonItem.Target.gameObject.SetActive(false);
                    }
                    else
                    {
                        if(viewSize > -itemSize)
                        {    
                            viewSize -= itemSize;
                            pLeagueSeasonItem.Target.gameObject.SetActive(viewSize > -itemSize);
                        }
                    }

                    if(m_pLeagueSeasonDataList.Count > i)
                    {
                        pLeagueSeasonItem.UpdateInfo(m_pLeagueSeasonDataList[i]);
                    }
                }
            }
            else
            {
                for(i =0; i < m_pLeagueSeasonItems.Count; ++i)
                {
                    m_pLeagueSeasonItems[i].Target.gameObject.SetActive(false);
                }
            }
            
            Vector2 size = m_pScrollRect.content.sizeDelta;
            size.y = m_pLeagueSeasonDataList.Count * itemSize;
            m_pScrollRect.content.sizeDelta = size;
            m_pScrollRect.verticalNormalizedPosition = 1;
            m_pPrevDir.y = 1;
            m_pScrollRect.content.anchoredPosition = Vector2.zero;
        }
    }

    void ClearScroll()
    {
        int i =0;

        for(i =0; i < m_pLeagueSeasonItems.Count; ++i)
        {
            m_pLeagueSeasonItems[i].Dispose();
        }

        for(i =0; i < m_pLeagueSeasonDataList.Count; ++i)
        {
            m_pLeagueSeasonDataList[i].Dispose();
        }
        m_pLeagueSeasonDataList.Clear();
        m_pLeagueSeasonItems.Clear();

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

        LeagueSeasonItem pItem = null;
        
        for(int i = 0; i < m_pLeagueSeasonItems.Count; ++i)
        {
            pItem = m_pLeagueSeasonItems[i];
            itemSize = pItem.Target.rect.height;
            viewSize -= itemSize;
            pItem.Target.gameObject.SetActive(viewSize > -itemSize);

            pos = pItem.Target.anchoredPosition;            
            pos.y = -i * itemSize;
            pItem.Target.anchoredPosition = pos;
        }

        m_pScrollRect.verticalNormalizedPosition = 1;
        m_pPrevDir.y = 1;
        
        m_pScrollRect.content.anchoredPosition = Vector2.zero;
        m_iStartIndex = 0;
    }

    void SetupScroll()
    {
        RectTransform pItem = null;
        Vector2 size;
        
        m_iTotalScrollItems = 0;
        m_iStartIndex = 0;

        float viewSize = m_pScrollRect.viewport.rect.height;
        float itemSize = 0;
        float h = 0;
        
        while(viewSize > -itemSize)
        {
            if(viewSize > 0)
            {
                ++m_iTotalScrollItems;
            }
            
            pItem = LayoutManager.Instance.GetItem<RectTransform>(SCROLL_ITEM_NAME);
                
            if(pItem)
            {
                pItem.SetParent(m_pScrollRect.content,false);
                pItem.anchoredPosition = new Vector2(0,-h);
                pItem.localScale = Vector3.one;
                size = pItem.sizeDelta;
                size.x = 0;
                pItem.sizeDelta = size;
                itemSize = pItem.rect.height;
                h += itemSize;
                viewSize -= itemSize;

                m_pLeagueSeasonItems.Add(new LeagueSeasonItem(pItem));
            }
        }
      
        
        size = m_pScrollRect.content.sizeDelta;
        size.y = h;
        m_pPrevDir.x = 0;
        
        m_pScrollRect.content.sizeDelta = size;
        m_pScrollRect.content.anchoredPosition = Vector2.zero;
    }

    public void SetupData(E_PLAYER_INFO_TYPE eTeamType, EmblemBake pEmblemBake = null)
    {
        List<ulong> list = null;
        byte[] pFormation = null;
        int i = 0;
        m_eCurrentTextName = E_TEXT_NAME.USERPROFILE_HISTORY_BTN_SEASON;
        m_ePlayerInfoType = eTeamType;
        GameContext pGameContext = GameContext.getCtx();
        
        if(m_ePlayerInfoType == E_PLAYER_INFO_TYPE.my)
        {
            m_pEmblem.CopyPoint(pEmblemBake);
            MainUI.Find("root/title/edit").gameObject.SetActive(true);
        }
        else
        {
            MainUI.Find("root/title/edit").gameObject.SetActive(false);
            m_pEmblem.SetupEmblemData(pGameContext.GetClubEmblemDataInClubProfile(m_ePlayerInfoType));
        }
        
        list = pGameContext.GetLineupPlayerIdListInClubProfile(m_ePlayerInfoType);
        pFormation = pGameContext.GetFormationArrayInClubProfile(m_ePlayerInfoType);
        ChangeSlotsPosition(pFormation.ToList());
        Transform tm = m_pTabUIList[(int)E_TAB.squad].Find("feild");
        PlayerT pPlayer = null;
        E_LOCATION eLoc = E_LOCATION.LOC_END;
        uint total = 0;
        for(i =0; i < pFormation.Length; ++i)
        {
            eLoc = (E_LOCATION)pFormation[i];
            m_pFormationList[i].position = tm.Find(eLoc.ToString()).position;
            pPlayer = GetPlayerByID(m_ePlayerInfoType,list[i]);
            SetupFormationPlayerData(pPlayer,m_pFormationList[i],(E_LOCATION)pFormation[i]);
            total += pPlayer.AbilityWeightSum;
        }

        TMPro.TMP_Text pText = MainUI.Find("root/squad/overall/text").GetComponent<TMPro.TMP_Text>();
        pText.SetText(string.Format(pGameContext.GetLocalizingText("MAIN_TXT_OVERALL"), string.Format(" <color=#FF9F00> {0}</color>",total)));

        UpdateClubNameAnNation(total);
        UpdateClubHistory(E_TEXT_NAME.USERPROFILE_HISTORY_BTN_SEASON);
    }

    PlayerT GetPlayerByID(E_PLAYER_INFO_TYPE eTeamType, ulong id)
    {
        if(eTeamType == E_PLAYER_INFO_TYPE.away)
        {
            return GameContext.getCtx().GetAwayClubPlayerByID(id);
        }
        else if(eTeamType == E_PLAYER_INFO_TYPE.match)
        {
            return GameContext.getCtx().GetMatchAwayPlayerByID(id);
        }
        else
        {
            return GameContext.getCtx().GetPlayerByID(id);
        }
    }

    void SetupClubInfo(uint totalAbilityWeightSum)
    {
        RectTransform ui = m_pTabUIList[(int)E_TAB.overview];
        TMPro.TMP_Text text = ui.Find("info/clubName/text").GetComponent<TMPro.TMP_Text>();
        GameContext pGameContext = GameContext.getCtx();
        text.SetText(pGameContext.GetClubNameInClubProfile(m_ePlayerInfoType));
        
        text = ui.Find("info/clubName/ranking/text").GetComponent<TMPro.TMP_Text>();
        uint value = pGameContext.GetCurrentSeasonStandingInClubProfile(m_ePlayerInfoType);
        text.SetText((m_ePlayerInfoType == E_PLAYER_INFO_TYPE.away || value < 1) ? "-" : value.ToString());
        
        byte rank = pGameContext.GetClubRankInClubProfile(m_ePlayerInfoType);
        text = ui.Find("info/rank/rank/text").GetComponent<TMPro.TMP_Text>();

        UserRankList pUserRankList = pGameContext.GetFlatBufferData<UserRankList>(E_DATA_TYPE.UserRank);
        UserRankItem? pUserRankItem = pUserRankList.UserRankByKey(rank);

        text.SetText(pGameContext.GetLocalizingText(pUserRankItem.Value.Name));

        RawImage icon = text.transform.parent.GetComponent<RawImage>();
        SingleFunc.SetupRankIcon(icon,rank);
        
        text = ui.Find("info/rank/trophy/text").GetComponent<TMPro.TMP_Text>();
        value = pGameContext.GetTrophyInClubProfile(m_ePlayerInfoType);
        text.SetText(value < 1 ? "-" : value.ToString());

        CLUB_NATION_CODE eCode = pGameContext.GetClubNationCodeInClubProfile(m_ePlayerInfoType);
                
        ClubNationalityItem? pClubNationalityItem = pGameContext.GetClubNationalityDataByCode(eCode);
        text = ui.Find("info/nationality/text").GetComponent<TMPro.TMP_Text>();
        text.SetText(pGameContext.GetLocalizingText(pClubNationalityItem.Value.NationName));

        RawImage nation = text.transform.parent.Find("nation").GetComponent<RawImage>();
        Sprite pSprite = AFPool.GetItem<Sprite>("Texture",eCode.ToString());
        nation.texture = pSprite.texture;

        text = ui.Find("info/founded/text").GetComponent<TMPro.TMP_Text>();
        text.SetText(pGameContext.GetClubFoundedTimeInClubProfile(m_ePlayerInfoType));
        
        Button[] pButtonList = ui.Find("topPlayer").GetComponentsInChildren<Button>(true);
        PlayerT pPlayer = null;
        List<ulong> list = pGameContext.GetCorePlayersInClubProfile(m_ePlayerInfoType);
        for(int i =0; i < pButtonList.Length; ++i)
        {
            if(i < list.Count)
            {
                pButtonList[i].gameObject.SetActive(true);
                pPlayer = GetPlayerByID(m_ePlayerInfoType,list[i]);
                pButtonList[i].gameObject.name = list[i].ToString();
                text = pButtonList[i].transform.Find("name/text").GetComponent<TMPro.TMP_Text>();
                text.SetText($"{pPlayer.Forename[0]}.{pPlayer.Surname}");
                SingleFunc.SetupPlayerCard(pPlayer,pButtonList[i].transform.Find("player"),E_ALIGN.Left,E_ALIGN.Left);
            }
            else
            {
                pButtonList[i].gameObject.SetActive(false);
            }
        }
        
        text = ui.Find("statistics/teamMarketValue/icon/text").GetComponent<TMPro.TMP_Text>();
        ulong ulValue = pGameContext.GetPlayerValueSumInClubProfile(m_ePlayerInfoType);

        text.SetText(ulValue < 1 ? "-" : ALFUtils.NumberToString(ulValue));
        
        text = ui.Find("statistics/acerageAge/text").GetComponent<TMPro.TMP_Text>();
        text.SetText(string.Format("{0}",pGameContext.GetPlayerAgeSumInClubProfile(m_ePlayerInfoType)));
        
        text = ui.Find("statistics/enteredSeason/text").GetComponent<TMPro.TMP_Text>();
        value = pGameContext.GetSeasonEnteredInClubProfile(m_ePlayerInfoType);
        
        text.SetText(value < 1 ? "-" : value.ToString());

        text = ui.Find("statistics/totalWins/text").GetComponent<TMPro.TMP_Text>();
        value = pGameContext.GetTotalWinInClubProfile(m_ePlayerInfoType);
        text.SetText(value < 1 ? "-" : value.ToString());

        text = ui.Find("statistics/teamOverall/text").GetComponent<TMPro.TMP_Text>();
        text.SetText(totalAbilityWeightSum < 1 ? "-" : totalAbilityWeightSum.ToString());

        text = ui.Find("statistics/thisSeason/win/text").GetComponent<TMPro.TMP_Text>();
        value = pGameContext.GetThisSeasonWinInClubProfile(m_ePlayerInfoType);
        text.SetText(value < 1 ? "-" : value.ToString());
        text = ui.Find("statistics/thisSeason/draw/text").GetComponent<TMPro.TMP_Text>();
        value = pGameContext.GetThisSeasonDrawInClubProfile(m_ePlayerInfoType);
        text.SetText(value < 1 ? "-" : value.ToString());
        text = ui.Find("statistics/thisSeason/lose/text").GetComponent<TMPro.TMP_Text>();
        value = pGameContext.GetThisSeasonLoseInClubProfile(m_ePlayerInfoType);
        text.SetText(value < 1 ? "-" : value.ToString());
    }

    void ClearRankIcon()
    {
        Transform ui = m_pTabUIList[(int)E_TAB.history].Find("season");
        Transform tm = null;
   
        for(int i = 0; i < 5; ++i)
        {
            tm = ui.Find($"season{i}");
            SingleFunc.ClearRankIcon(tm.Find("rank").GetComponent<RawImage>());
        }

        ui = m_pTabUIList[(int)E_TAB.overview];
        SingleFunc.ClearRankIcon(ui.Find("info/rank/rank").GetComponent<RawImage>());        
    }
    void ClearTopPlayerFace()
    {
        RectTransform ui = m_pTabUIList[(int)E_TAB.overview];
        Button[] pButtonList = ui.Find("topPlayer").GetComponentsInChildren<Button>(true);
        for(int i =0; i < pButtonList.Length; ++i)
        {
            SingleFunc.ClearPlayerFace(pButtonList[i].transform.Find("player/icon"));
        }
    }

    void UpdateClubHistory(E_TEXT_NAME type)
    {
        m_eCurrentTextName = type;

        GameContext pGameContext = GameContext.getCtx();
        Transform ui = m_pTabUIList[(int)E_TAB.history].Find("competitions");
        TMPro.TMP_Text text = ui.Find("currentTrophy/trophy/text").GetComponent<TMPro.TMP_Text>(); 
        uint value = pGameContext.GetTrophyInClubProfile(m_ePlayerInfoType);
        text.SetText(value < 1 ? "-" : value.ToString());
        text = ui.Find("currentTrophy/ranking/text").GetComponent<TMPro.TMP_Text>();
        value = pGameContext.GetCurrentSeasonStandingInClubProfile(m_ePlayerInfoType);
        text.SetText( (m_ePlayerInfoType == E_PLAYER_INFO_TYPE.my || value > 0) ? value.ToString() : "-");
        text = ui.Find("seasonHightest/trophy/text").GetComponent<TMPro.TMP_Text>();
        value = pGameContext.GetSeasonTrophyHighInClubProfile(m_ePlayerInfoType);
        text.SetText(value < 1 ? "-" : value.ToString());
        text = ui.Find("seasonHightest/ranking/text").GetComponent<TMPro.TMP_Text>();
        value = pGameContext.GetSeasonStandingHighInClubProfile(m_ePlayerInfoType);
        text.SetText( (m_ePlayerInfoType == E_PLAYER_INFO_TYPE.my || value > 0) ? value.ToString() : "-");
        text = ui.Find("allTimeHigh/trophy/text").GetComponent<TMPro.TMP_Text>();
        value = pGameContext.GetTotalTrophyHighInClubProfile(m_ePlayerInfoType);
        text.SetText(value < 1 ? "-" : value.ToString());
        text = ui.Find("allTimeHigh/ranking/text").GetComponent<TMPro.TMP_Text>();
        value = pGameContext.GetTotalStandingHighInClubProfile(m_ePlayerInfoType);
        text.SetText( (m_ePlayerInfoType == E_PLAYER_INFO_TYPE.my || value > 0) ? value.ToString() : "-");
        
        ui = m_pTabUIList[(int)E_TAB.history].Find("season");
        Transform tm = null;
        ClubSeasonStats pClubSeasonStats = null;
        RawImage icon = null;
        
        UserRankList pUserRankList = pGameContext.GetFlatBufferData<UserRankList>(E_DATA_TYPE.UserRank);
        UserRankItem? pUserRankItem = null;
        
        for(int i = 0; i < 5; ++i)
        {
            tm = ui.Find($"season{i}");
            pClubSeasonStats = pGameContext.GetLatestSeasonStatsInClubProfile(m_ePlayerInfoType,i);
            if(pClubSeasonStats != null)
            {
                tm.gameObject.SetActive(true);

                text = tm.Find("season").GetComponent<TMPro.TMP_Text>();
                text.SetText(pClubSeasonStats.SeasonNo.ToString());
                icon = tm.Find("rank").GetComponent<RawImage>();
                byte rank = (byte)pClubSeasonStats.Rank;
                pUserRankItem = pUserRankList.UserRankByKey(pClubSeasonStats.Rank);

                SingleFunc.SetupRankIcon(icon,rank);

                text = tm.Find("trophy/text").GetComponent<TMPro.TMP_Text>();
                text.SetText(pClubSeasonStats.Trophy > 0 ? pClubSeasonStats.Trophy.ToString():"-");
                text = tm.Find("W").GetComponent<TMPro.TMP_Text>();
                text.SetText(pClubSeasonStats.Win > 0 ? pClubSeasonStats.Win.ToString():"-");
                text = tm.Find("D").GetComponent<TMPro.TMP_Text>();
                value = pClubSeasonStats.Game - ( pClubSeasonStats.Win + pClubSeasonStats.Lose);
                text.SetText(value > 0 ? value.ToString():"-");
                text = tm.Find("L").GetComponent<TMPro.TMP_Text>();
                text.SetText(pClubSeasonStats.Lose > 0 ? pClubSeasonStats.Lose.ToString():"-");
                text = tm.Find("ranking/text").GetComponent<TMPro.TMP_Text>();
                text.SetText((m_ePlayerInfoType == E_PLAYER_INFO_TYPE.away || pClubSeasonStats.Standing < 1) ? "-":pClubSeasonStats.Standing.ToString());
            }
            else
            {
                icon = tm.Find("rank").GetComponent<RawImage>();
                icon.texture = null;
                tm.gameObject.SetActive(false);
            }
        }
        
        ui = m_pTabUIList[(int)E_TAB.history].Find("history");

        LocalizingText pLocalizingText = ui.Find("toggle/text").GetComponent<LocalizingText>();
        pLocalizingText.Key = m_eCurrentTextName.ToString();
        pLocalizingText.IsLocalizing = true;
        pLocalizingText.UpdateLocalizing();
        
        for(E_HISTORY eType = E_HISTORY.matchPlayed; eType < E_HISTORY.MAX; ++eType)
        {
            text = ui.Find(eType.ToString()).Find("text").GetComponent<TMPro.TMP_Text>();
            text.SetText(pGameContext.GetHistotyInfoInClubProfile(m_ePlayerInfoType,m_eCurrentTextName == E_TEXT_NAME.USERPROFILE_HISTORY_BTN_SEASON, eType));
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
    }

    void SetupFormationPlayerData(PlayerT pPlayer,Transform group,E_LOCATION eLoc)
    {
        if(pPlayer == null || group == null) 
            return;

        group.gameObject.name = pPlayer.Id.ToString();
        
        SingleFunc.SetupQuality(pPlayer,group.Find("quality"));
        
        TMPro.TMP_Text text = group.Find("name/text").GetComponent<TMPro.TMP_Text>();
        if(text != null)
        {
            text.SetText(pPlayer.Surname);
        }

        int index = GameContext.getCtx().ConvertPositionByTag(eLoc);
        
        text = group.Find("text").GetComponent<TMPro.TMP_Text>();
        text.SetText(GameContext.getCtx().GetDisplayLocationName(index));
    }

    void ShowPlayerProfile(ulong id)
    {
        PlayerT pPlayer = null;
        GameContext pGameContext = GameContext.getCtx();
        if(m_ePlayerInfoType == E_PLAYER_INFO_TYPE.my)
        {
            pPlayer = pGameContext.GetPlayerByID(id);
        }
        else if(m_ePlayerInfoType == E_PLAYER_INFO_TYPE.away)
        {
            pPlayer = pGameContext.GetAwayClubPlayerByID(id);
        }
        
        if(pPlayer != null)
        {
            PlayerInfo pPlayerInfo = m_pMainScene.ShowPlayerInfoPopup();
            pPlayerInfo.SetupPlayerInfoData(m_ePlayerInfoType,pPlayer);
            pPlayerInfo.SetupQuickPlayerInfoData(m_ePlayerInfoType == E_PLAYER_INFO_TYPE.my ? pGameContext.GetTotalPlayerList() : pGameContext.GetTotalPlayersInClubProfile(m_ePlayerInfoType));
            pPlayerInfo.ShowTabUI(E_PLAYER_INFO_TAB.information);
        }
        else
        {
            m_pMainScene.RequestPlayerProfile(id,CallbackShowPlayerProfile);
        }
    }

    void CallbackShowPlayerProfile()
    {
        PlayerInfo pPlayerInfo = m_pMainScene.ShowPlayerInfoPopup();
        pPlayerInfo.ShowTabUI(E_PLAYER_INFO_TAB.information);
    }

    public void Close()
    {
        SingleFunc.HideAnimationDailog(MainUI, () => {
            ClearTopPlayerFace();
            ClearRankIcon();
            LayoutManager.Instance.InteractableEnabledAll();
            m_pEmblem.material = null;
            MainUI.gameObject.SetActive(false);
            MainUI.Find("root").localScale = Vector3.one;
        });
    }

    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        switch(root.gameObject.name)
        {
            case "tabs":
            {
                E_TAB eType = (E_TAB)Enum.Parse(typeof(E_TAB), sender.name);
                if(eType == E_TAB.league)
                {
                    JObject pJObject = new JObject();
                    pJObject["no"] = GameContext.getCtx().GetProfileClubID(m_ePlayerInfoType);
                    m_pMainScene.RequestAfterCall(E_REQUEST_ID.league_getHistory,pJObject,()=>{ShowTabUI((byte)eType);});
                }
                else
                {
                    ShowTabUI((byte)eType);
                }
            }
            break;
            case "overview":
            {
                ulong id = 0;
                if(ulong.TryParse(sender.name,out id))
                {
                    PlayerInfo pPlayerInfo = m_pMainScene.ShowPlayerInfoPopup();
                    pPlayerInfo.SetupPlayerInfoData(m_ePlayerInfoType,GetPlayerByID(m_ePlayerInfoType,id));
                    pPlayerInfo.SetupQuickPlayerInfoData(GameContext.getCtx().GetTotalPlayersInClubProfile(m_ePlayerInfoType));
                    pPlayerInfo.ShowTabUI(E_PLAYER_INFO_TAB.information);
                }
            }
            break;
            case "history":
            {
                if(sender.name == "toggle")
                {
                    UpdateClubHistory(m_eCurrentTextName == E_TEXT_NAME.USERPROFILE_HISTORY_BTN_TOTAL ? E_TEXT_NAME.USERPROFILE_HISTORY_BTN_SEASON : E_TEXT_NAME.USERPROFILE_HISTORY_BTN_TOTAL);
                }
            }
            break;
            case "squad":
            {
                ulong id = 0;
                if(ulong.TryParse(sender.name, out id))
                {
                    ShowPlayerProfile(id);
                }
            }
            break;
            
            default:
            {
                if(sender.name == "close" || sender.name == "back")
                {
                    Close();
                }
                else if(sender.name == "edit")
                {
                    m_pMainScene.ShowEditProfilePopup();
                }
            }
            break;
        }
    }
    
}
