using System;
// using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using ALF;
// using ALF.STATE;
using ALF.NETWORK;
using ALF.LAYOUT;
// using ALF.MACHINE;
using ALF.SOUND;
using DATA;
using USERDATA;
using UnityEngine.EventSystems;
using STATEDATA;
using Newtonsoft.Json.Linq;

public class Chat : IBaseUI,ISoketNetwork
{
    const string SCROLL_ITEM_NAME = "ChatItem";
    const int CHAT_MSG_LIMIT = 300;

    enum E_MSG_TYPE : byte { None =0, me,you,system,share,MAX }
    bool m_bJoinChat = false;
    string m_strJoinChannelId = $"Channel{UnityEngine.Random.Range(1,5)}";//;$"{Application.systemLanguage.ToString().ToLower()}1";
    ulong m_ulSendCustomerNo = 0;
    MainScene m_pMainScene = null;
    ScrollRect m_pScrollRect = null;
    Button m_pSend = null;
    Button m_pGo = null;
    TMPro.TMP_InputField m_pChatInput = null;
    string m_strMsg = null;
    GameObject m_pChannelList = null;
    GameObject m_pChannelListBack = null;
    TMPro.TMP_Text m_pChannelText = null;
    Button[] m_pChannelButtonList = null;

    Dictionary<GameObject,ulong> m_pSharePlayerIDList = new Dictionary<GameObject,ulong>();
    Dictionary<ulong,EmblemBake> m_pEmblemList = new Dictionary<ulong, EmblemBake>();
    List<ChatMsgT> m_pChatMsgList = null;
    
    public RectTransform MainUI { get; private set;}
    public bool Enable { set{ if (m_pScrollRect != null) m_pScrollRect.enabled = value;}}
    public Chat(){}
    
    public void Dispose()
    {
        m_pChannelList = null;
        m_pChannelText = null;
        m_pChannelListBack = null;
        for(int i =0; i < m_pChannelButtonList.Length; ++i)
        {
            m_pChannelButtonList[i]= null;
        }
        m_pChannelButtonList = null;

        if(m_pChatInput != null)
        {
            m_pChatInput.onSubmit.RemoveAllListeners();
            // m_pChatInput.onSelect.RemoveAllListeners();
            m_pChatInput.onDeselect.RemoveAllListeners();
            m_pChatInput.onValueChanged.RemoveAllListeners();
        }
        
        m_pChatInput = null;
        m_pMainScene = null;
        MainUI = null;
        m_pSend = null;
        m_pGo = null;
        if(m_pScrollRect != null)
        {
            m_pScrollRect.onValueChanged.RemoveAllListeners();
            ClearScroll();
        }

        // EventTrigger trigger = m_pScrollRect.GetComponent<EventTrigger>();
        
        // for(int i =0; i < trigger.triggers.Count; ++i)
        // {
        //     trigger.triggers[i].callback.RemoveAllListeners();
        // }
		// trigger.triggers.Clear();
        m_pScrollRect = null;
        m_pChatMsgList = null;   
        m_pEmblemList = null;
        // for(i =0; i < m_sortPlayers.Count; ++i)
        // {
        //     m_sortPlayers[i].Dispose();
        //     m_sortPlayers[i] = null; 
        // }
        // m_sortPlayers.Clear();
        // m_sortPlayers = null;
        
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "Chat : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "Chat : targetUI is null!!");

        m_pChatMsgList = new List<ChatMsgT>();
        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;

        GameContext pGameContext = GameContext.getCtx();
        m_pChatInput = MainUI.Find("root/input/text").GetComponent<TMPro.TMP_InputField>();

        m_pChannelList = MainUI.Find("root/box").gameObject;
        m_pChannelListBack = MainUI.Find("root/list").gameObject;
        m_pChannelButtonList = m_pChannelList.GetComponentsInChildren<Button>(true);

        for(int i =0; i < m_pChannelButtonList.Length; ++i)
        {
            m_pChannelButtonList[i].transform.Find("text").GetComponent<TMPro.TMP_Text>().SetText(string.Format(pGameContext.GetLocalizingText("CHAT_TXT_CHANNEL_NAME"),i +1));
        }    

        m_pChannelText = MainUI.Find("root/channel/text").GetComponent<TMPro.TMP_Text>();
       
        // m_pChatInput.characterLimit = GameContext.getCtx().GetConstValue(E_CONST_TYPE.clubNameLengthLimit);

        m_pChatInput.onSubmit.AddListener(delegate {  InputSumit(m_pChatInput); });
        m_pChatInput.onDeselect.AddListener(delegate {  InputDeselect(m_pChatInput); });
        m_pChatInput.onValueChanged.AddListener(delegate {  InputValue(m_pChatInput); });
        // m_pChatInput.onSelect.AddListener(CheckSelect);
        m_pSend = MainUI.Find("root/send").GetComponent<Button>();
        m_pGo = MainUI.Find("root/go").GetComponent<Button>();
        m_pGo.enabled = false;
        m_pGo.gameObject.SetActive(false);
        m_pGo.GetComponent<Graphic>().color = m_pGo.enabled ? Color.white : GameContext.GRAY_W;
        SetSendButton(false);
        m_pScrollRect = MainUI.GetComponentInChildren<ScrollRect>(true);
        // EventTrigger trigger = m_pScrollRect.gameObject.AddComponent<EventTrigger>();

		// EventTrigger.Entry entry = new EventTrigger.Entry( );
		// entry.eventID = EventTriggerType.PointerDown;
		// entry.callback.AddListener( ( data ) => {
        //      m_bMoving = true;} );
		// trigger.triggers.Add( entry );

        // entry = new EventTrigger.Entry( );
		// entry.eventID = EventTriggerType.PointerUp;
		// entry.callback.AddListener( ( data ) => { m_bMoving = false;} );
		// trigger.triggers.Add( entry );

        m_pScrollRect.onValueChanged.AddListener( this.ScrollViewChangeValueEventCall);
        LayoutManager.SetReciveUIButtonEvent(MainUI,this.ButtonEventCall);

        MainUI.gameObject.SetActive(false);
    }

    void ScrollViewChangeValueEventCall( Vector2 value)
    {
        m_pGo.enabled = false;
        if(m_pScrollRect.content.sizeDelta.y > m_pScrollRect.viewport.rect.height + 130)
        {
            Transform tm = m_pScrollRect.content.GetChild(m_pScrollRect.content.childCount -1);
            Vector2 endPos = (Vector2)m_pScrollRect.viewport.InverseTransformPoint( tm.position );
            m_pGo.enabled = endPos.y <= -m_pScrollRect.viewport.rect.height;
        }
        m_pGo.gameObject.SetActive(m_pGo.enabled);
    }

    void SetSendButton(bool bActive)
    {
        m_pSend.transform.Find("on").gameObject.SetActive(bActive);
        m_pSend.transform.Find("off").gameObject.SetActive(!bActive);
        // m_pSend.transform.Find("icon").GetComponent<Graphic>().color = bActive ? GameContext.BULE : GameContext.GRAY;
        m_pSend.enabled = bActive;
    }

    void InputValue(TMPro.TMP_InputField input )
    {
        if(input == null) return;
        
        // input.text = ALFUtils.FwordFilter(input.text);
        m_strMsg = input.text;
        SetSendButton(!string.IsNullOrEmpty(m_strMsg));
    }

    void InputDeselect(TMPro.TMP_InputField input )
    {
        if(input == null) return;
        
        m_strMsg = input.text;
    }

    void InputSumit(TMPro.TMP_InputField input )
    {
        if(input == null) return;
        
        if(SingleFunc.IsMatchString(input.text,@"[\;\'\""\/\`\\]"))
        {
            m_pMainScene.ShowToastMessage(GameContext.getCtx().GetLocalizingText("MSG_TXT_STRING_ERROR"),null);
            input.text = "";
            return;
        }
        input.text = ALFUtils.FwordFilter(input.text);
        m_strMsg = input.text;

        if(!string.IsNullOrEmpty(m_strMsg))
        {
            ButtonEventCall(MainUI,m_pSend.gameObject);
        }
    }

    void SetupChannelName()
    {
        string strJoinChannelName = "";
        bool bOn = false;
        TMPro.TMP_Text text = null;
        for(int i =0; i < m_pChannelButtonList.Length; ++i)
        {
            bOn = m_strJoinChannelId == m_pChannelButtonList[i].gameObject.name;
            text = m_pChannelButtonList[i].transform.Find("text").GetComponent<TMPro.TMP_Text>();
            if(bOn)
            {
                m_pChannelButtonList[i].transform.Find("on").gameObject.SetActive(bOn);
                strJoinChannelName = text.text;
            }
            else
            {
                m_pChannelButtonList[i].transform.Find("on").gameObject.SetActive(bOn);
            }
            
            text.color = bOn ? Color.white : GameContext.GRAY;
        }

        m_pChannelText.SetText(strJoinChannelName);
    }

    public void JoinRoom()
    {
        m_pChannelList.SetActive(false);
        m_pChannelListBack.SetActive(false);
        SetupChannelName();
        if(m_pChatMsgList.Count == 0)
        {
            ChatMsgT pChatMsg = MakeChatMsg();
            pChatMsg.Type = (byte)E_MSG_TYPE.system;
            pChatMsg.Msg = GameContext.getCtx().GetLocalizingText("CHAT_TXT_NOTICE_JOINED");
            AddChatMsg(pChatMsg);
        }
    }

    public void SetJoin(bool bJoin)
    {
        m_bJoinChat = bJoin;
    }

    public void JoinChatServer()
    {
        if(!m_bJoinChat)
        {
            JObject pJObject = new JObject();
            pJObject["type"] = E_SOKET.join.ToString();
            JArray pJArray = new JArray();
            pJArray.Add(m_strJoinChannelId);
            pJObject["channelIds"] = pJArray;
            NetworkManager.SendMessage(pJObject);
        }
    }

    public void UpdateTimer(float dt)
    {
        for(int i =0; i < m_pChatMsgList.Count; ++i)
        {
            m_pChatMsgList[i].Tick += dt;
            if(m_pChatMsgList[i].Tick >= 60)
            {
                while(m_pChatMsgList[i].Tick < 60)
                {
                    m_pChatMsgList[i].Tick -= 60;
                }
                if(MainUI.gameObject.activeSelf)
                {
                    UpdateTime(m_pChatMsgList[i]);
                }
            }
        }
    }

    void UpdateTime(ChatMsgT pChatMsg)
    {
        TMPro.TMP_Text text = null;
        DateTime serverTime = NetworkManager.GetGameServerTime(); 
        TimeSpan ts;
        Transform item = null;
        E_MSG_TYPE eMsg = E_MSG_TYPE.MAX;
        
        if(pChatMsg == null)
        {
            int i = m_pChatMsgList.Count;
            while(i > 0)
            {
                --i;
                eMsg = (E_MSG_TYPE)m_pChatMsgList[i].Type;
                item = m_pScrollRect.content.GetChild(i).Find(eMsg.ToString());
                if(eMsg == E_MSG_TYPE.share)
                {
                    if(m_pChatMsgList[i].Id == GameContext.getCtx().GetClubID())
                    {
                        text = item.Find("me/bg/time/time").GetComponent<TMPro.TMP_Text>();
                    }
                    else
                    {
                        text = item.Find("you/bg/time/time").GetComponent<TMPro.TMP_Text>();
                    }
                }
                else
                {
                    text = item.Find("bg/time/time").GetComponent<TMPro.TMP_Text>();
                }

                ts = serverTime - new DateTime(m_pChatMsgList[i].Time);
                if(ts.TotalSeconds < 60)
                {
                    text.SetText(GameContext.getCtx().GetLocalizingText("TIMECOUNT_TXT_RECEIVE_TIME_NOW"));
                }
                else
                {
                    SingleFunc.UpdateTimeText((int)ts.TotalSeconds,text,2);
                }
            }
        }
        else
        {
            eMsg = (E_MSG_TYPE)pChatMsg.Type;
            item = m_pScrollRect.content.GetChild(pChatMsg.Index).Find(eMsg.ToString());
            if(eMsg == E_MSG_TYPE.share)
            {
                if(pChatMsg.Id == GameContext.getCtx().GetClubID())
                {
                    text = item.Find("me/bg/time/time").GetComponent<TMPro.TMP_Text>();
                }
                else
                {
                    text = item.Find("you/bg/time/time").GetComponent<TMPro.TMP_Text>();
                }
            }
            else
            {
                text = item.Find("bg/time/time").GetComponent<TMPro.TMP_Text>();
            }

            ts = serverTime - new DateTime(pChatMsg.Time);
            if(ts.TotalSeconds < 60)
            {
                text.SetText(GameContext.getCtx().GetLocalizingText("TIMECOUNT_TXT_RECEIVE_TIME_NOW"));
            }
            else
            {
                SingleFunc.UpdateTimeText((int)ts.TotalSeconds,text,2);
            }
        }
    }

    void PopChatMsg()
    {
        int count = m_pChatMsgList.Count - CHAT_MSG_LIMIT;

        if(count > 0)
        {
            Transform pItem =  null;
            ChatMsgT pChatMsg = null;
            RectTransform tm = null;
            EmblemBake[] pEmblemBakes = null;
            float h = 0;
            while(m_pChatMsgList.Count >= CHAT_MSG_LIMIT)
            {
                pChatMsg = m_pChatMsgList[0];
                pItem = m_pScrollRect.content.Find(pChatMsg.Id.ToString());
                if(pItem != null)
                {
                    tm = pItem.GetComponent<RectTransform>();
                    h += tm.rect.height;

                    E_MSG_TYPE eMsg = (E_MSG_TYPE)pChatMsg.Type;
            
                    pItem = pItem.Find(eMsg.ToString());
                    GameObject obj = pItem.Find("bg/share").gameObject;
                    if(m_pSharePlayerIDList.ContainsKey(obj))
                    {
                        m_pSharePlayerIDList.Remove(obj);
                    }

                    pEmblemBakes = pItem.GetComponentsInChildren<EmblemBake>(true);
                    for(int n = 0; n < pEmblemBakes.Length; ++n)
                    {
                        pEmblemBakes[n].material = null;
                        pEmblemBakes[n].Dispose();
                    }

                    LayoutManager.SetReciveUIButtonEvent(tm,null);
                    LayoutManager.Instance.AddItem(SCROLL_ITEM_NAME,tm);
                }
                m_pChatMsgList.RemoveAt(0);
            }
            int i =0;

            for(i =0; i < m_pChatMsgList.Count; ++i)
            {
                m_pChatMsgList[i].Index = i;
            }
            
            Vector2 size = m_pScrollRect.content.sizeDelta;
            size.y -= h;
            m_pScrollRect.content.sizeDelta = size;
            h = 0;
            for(i =0; i < m_pScrollRect.content.childCount; ++i)
            {
                tm = m_pScrollRect.content.GetChild(i).GetComponent<RectTransform>();
                tm.anchoredPosition = new Vector2(0,-h);
                size = tm.sizeDelta;
                h = size.y;
            }
        }
    }

    void AddChatMsg(ChatMsgT pChatMsg)
    {
        if(pChatMsg != null)
        {
            PopChatMsg();
            pChatMsg.Index = m_pChatMsgList.Count;
            m_pChatMsgList.Add(pChatMsg);

            RectTransform pItem = LayoutManager.Instance.GetItem<RectTransform>(SCROLL_ITEM_NAME);
            pItem.gameObject.name = pChatMsg.Id.ToString();

            E_MSG_TYPE eMsg = (E_MSG_TYPE)pChatMsg.Type;
            float y = m_pScrollRect.content.sizeDelta.y;
            float fMaxH = 0;
            
            for(E_MSG_TYPE i = E_MSG_TYPE.me; i < E_MSG_TYPE.MAX; ++i )
            {
                pItem.Find(i.ToString()).gameObject.SetActive(i == eMsg);
            }

            RectTransform pBox = pItem.Find(eMsg.ToString()).GetComponent<RectTransform>();
            
            if(eMsg == E_MSG_TYPE.share)
            {
                pBox.Find("me").gameObject.SetActive(false);
                pBox.Find("you").gameObject.SetActive(false);

                if(pChatMsg.Id == GameContext.getCtx().GetClubID())
                {
                    pBox = pBox.Find("me").GetComponent<RectTransform>();
                }
                else
                {
                    pBox = pBox.Find("you").GetComponent<RectTransform>();
                }
                pBox.gameObject.SetActive(true);
            }

            TMPro.TMP_Text text = null;
            if(pBox.Find("bg/rank") != null && pChatMsg.Rank > 0)
            {
                SingleFunc.SetupRankIcon(pBox.Find("bg/rank").GetComponent<RawImage>(),pChatMsg.Rank);
            }

            if(pBox.Find("bg/trophy") != null)
            {
                text = pBox.Find("bg/trophy/text").GetComponent<TMPro.TMP_Text>();
                text.SetText(pChatMsg.Trophy.ToString());
            }

            text = pBox.Find("bg/name").GetComponent<TMPro.TMP_Text>();
            text.SetText(pChatMsg.Name);
            float h = text.GetComponent<RectTransform>().rect.height;
            string msg = null;
            if(eMsg == E_MSG_TYPE.you)
            {
                fMaxH = 230;
                msg = pChatMsg.Msg;
                EmblemBake pEmblemBake = pBox.Find("emblem").GetComponent<EmblemBake>();

                if(!m_pEmblemList.ContainsKey(pChatMsg.Id))
                {
                    pEmblemBake.SetupEmblemData(pChatMsg.Emblem.ToArray());
                    m_pEmblemList.Add(pChatMsg.Id,pEmblemBake);
                }
                else
                {
                    pEmblemBake.CopyPoint(m_pEmblemList[pChatMsg.Id]);
                }

                LayoutManager.SetReciveUIButtonEvent(pItem,ButtonEventCallClubProfile);
            }
            else if(eMsg == E_MSG_TYPE.share)
            {
                if(pBox.Find("emblem") != null)
                {
                    EmblemBake pEmblemBake = pBox.Find("emblem").GetComponent<EmblemBake>();

                    if(!m_pEmblemList.ContainsKey(pChatMsg.Id))
                    {
                        pEmblemBake.SetupEmblemData(pChatMsg.Emblem.ToArray());
                        m_pEmblemList.Add(pChatMsg.Id,pEmblemBake);
                    }
                    else
                    {
                        pEmblemBake.CopyPoint(m_pEmblemList[pChatMsg.Id]);
                    }
                }
                
                GameObject obj = pBox.Find("bg/share").gameObject;
                JObject pJObject = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(pChatMsg.Msg, new Newtonsoft.Json.JsonSerializerSettings {DateParseHandling = Newtonsoft.Json.DateParseHandling.None});
                text = obj.transform.Find("name").GetComponent<TMPro.TMP_Text>();
                text.SetText((string)pJObject["msg"]);
                msg = null;
                
                if(m_pSharePlayerIDList.ContainsKey(obj))
                {
                    m_pSharePlayerIDList[obj] = (ulong)pJObject["playerID"];
                }
                else
                {
                    m_pSharePlayerIDList.Add(obj,(ulong)pJObject["playerID"]);
                }
                LayoutManager.SetReciveUIButtonEvent(pItem,ButtonEventCallPlayerInfo);
                fMaxH = 290;
                h = fMaxH;

                text = pBox.Find("bg/text").GetComponent<TMPro.TMP_Text>();
                text.SetText(GameContext.getCtx().GetLocalizingText("CHAT_TXT_PLAYER_PROFILE_SHARED"));
            }
            else
            {
                msg = pChatMsg.Msg;
                fMaxH = 160;
            }
        
            if(!string.IsNullOrEmpty(msg))
            {
                text = pBox.Find("bg/text").GetComponent<TMPro.TMP_Text>();
                text.SetText(msg);
                h += text.preferredHeight + 40;
                h = Mathf.Max(h,fMaxH);
            }
            
            Vector2 size = pItem.sizeDelta;
            if(size.y < h)
            {
                size.y = h;
                pItem.sizeDelta = size;
            }
            else
            {
                h = size.y;
            }
            
            pItem.SetParent(m_pScrollRect.content,false);
            pItem.localScale = Vector3.one;
            pItem.anchoredPosition = new Vector2(0,-y);

            size = m_pScrollRect.content.sizeDelta;
            size.y += h + 20;
            m_pScrollRect.content.sizeDelta = size;
            
            if(m_pScrollRect.velocity.y == 0)
            {
                m_pScrollRect.verticalNormalizedPosition = 0;
            }

            UpdateTime(pChatMsg);
        }
    }

    public void AllClear()
    {
        ClearScroll();
        m_strMsg = "";
        m_pChatInput.text = m_strMsg;
    }

    public void LeaveRoom(bool bClose)
    {
        if(bClose && MainUI.gameObject.activeSelf)
        {
            Close();
        }

        AllClear();
        JObject pJObject = new JObject();
        pJObject["type"] = E_SOKET.leave.ToString();
        JArray pJArray = new JArray();
        pJArray.Add(m_strJoinChannelId);
        pJObject["channelIds"] = pJArray;
        NetworkManager.SendMessage(pJObject);
    }
    
    void ClearScroll()
    {
        m_pSharePlayerIDList.Clear();
        int i = m_pScrollRect.content.childCount;
        RectTransform pGroup = null;
        EmblemBake[] pEmblemBakes = null;
        
        while(i > 0)
        {
            --i;
            pGroup = m_pScrollRect.content.GetChild(i).GetComponent<RectTransform>();
            pEmblemBakes = pGroup.GetComponentsInChildren<EmblemBake>(true);
            for(int n = 0; n < pEmblemBakes.Length; ++n)
            {
                pEmblemBakes[n].material = null;
                pEmblemBakes[n].Dispose();
            }
            LayoutManager.SetReciveUIButtonEvent(pGroup,null);
            LayoutManager.Instance.AddItem(SCROLL_ITEM_NAME,pGroup);
        }

        Vector2 size = m_pScrollRect.content.sizeDelta;
        size.y =0;
        m_pScrollRect.content.sizeDelta = size;
        m_pScrollRect.content.anchoredPosition = Vector2.zero;

        var itr = m_pEmblemList.GetEnumerator();

        while(itr.MoveNext())
        {
            itr.Current.Value.Dispose();
        }
        m_pEmblemList.Clear();
        m_pChatMsgList.Clear();
    }
    public void Close()
    {
        m_strMsg = "";
        m_pChatInput.text = m_strMsg;
        m_pMainScene.HideMoveDilog(MainUI,Vector3.down);
    }

    void ShowClubProfileFormation()
    {
        m_pMainScene.ShowUserProfile(m_ulSendCustomerNo,0);
        m_ulSendCustomerNo = 0;
    }

    void CallbackShowPlayerProfile()
    {
        PlayerInfo pPlayerInfo = m_pMainScene.ShowPlayerInfoPopup();
        pPlayerInfo.ShowTabUI(E_PLAYER_INFO_TAB.information);
    }

    ChatMsgT MakeChatMsg()
    {
        GameContext pGameContext = GameContext.getCtx();
        ChatMsgT pChatMsg = new ChatMsgT();
        pChatMsg.Id = pGameContext.GetClubID();
        pChatMsg.Name = pGameContext.GetClubName();
        pChatMsg.Msg = m_strMsg;
        pChatMsg.Time = NetworkManager.GetGameServerTime().Ticks;
        pChatMsg.Emblem = pGameContext.GetEmblemInfo().ToList();
        pChatMsg.Trophy = pGameContext.GetCurrentUserTrophy();
        pChatMsg.Rank = pGameContext.GetCurrentUserRank();
        pChatMsg.Tick = 0;
        pChatMsg.Index = 0;

        return pChatMsg;
    }

    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        switch(sender.name)
        {
            case "Channel1":
            case "Channel2":
            case "Channel3":
            case "Channel4":
            {
                LeaveRoom(false);
                m_bJoinChat = false;
                m_strJoinChannelId = sender.name;
                JoinChatServer();
                JoinRoom();
            }
            break;
            case "list":
            {
                m_pChannelList.SetActive(false);
                m_pChannelListBack.SetActive(false);
            }
            break;
            case "channel":
            {
                m_pChannelList.SetActive(true);
                m_pChannelListBack.SetActive(true);
            }
            break;
            case "close":
            {
                Close();
            }
            break;
            case "go":
            {
                m_pScrollRect.verticalNormalizedPosition = 0;
                m_pGo.gameObject.SetActive(false);
            }
            break;
            case "send":
            {
                if(SingleFunc.IsMatchString(m_strMsg,@"[\;\'\""\/\`\\]"))
                {
                    m_pMainScene.ShowToastMessage(GameContext.getCtx().GetLocalizingText("MSG_TXT_STRING_ERROR"),null);
                    m_strMsg = "";
                    m_pChatInput.text = m_strMsg;
                    return;
                }
                m_strMsg = ALFUtils.FwordFilter(m_strMsg);

                ChatMsgT pChatMsg = MakeChatMsg();
               
                JObject pJObject = new JObject();
                pJObject["type"] = E_SOKET.chat.ToString();
                pJObject["channelId"] = m_strJoinChannelId;
                pJObject["msg"] = Newtonsoft.Json.JsonConvert.SerializeObject(pChatMsg, Newtonsoft.Json.Formatting.None,new Newtonsoft.Json.JsonSerializerSettings { NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore});
                
                NetworkManager.SendMessage(pJObject);
                m_strMsg = "";
                m_pChatInput.text = m_strMsg;
                SetSendButton(false);
            }
            break;
        }
    }

    void ButtonEventCallClubProfile(RectTransform root ,GameObject sender)
    {
        m_ulSendCustomerNo = 0;
        if(ulong.TryParse(root.gameObject.name,out m_ulSendCustomerNo))
        {
            m_pMainScene.RequestClubProfile(m_ulSendCustomerNo,ShowClubProfileFormation);
        }
    }

    void ButtonEventCallPlayerInfo(RectTransform root ,GameObject sender)
    {
        m_ulSendCustomerNo = 0;
        if(ulong.TryParse(root.gameObject.name,out m_ulSendCustomerNo))
        {
            if(m_pSharePlayerIDList.ContainsKey(sender))
            {
                GameContext pGameContext = GameContext.getCtx();
                ulong id = m_pSharePlayerIDList[sender];
                PlayerT pPlayer = null;
                if(m_ulSendCustomerNo == pGameContext.GetClubID())
                {
                    pPlayer = pGameContext.GetPlayerByID(id);    
                }

                if(pPlayer != null)
                {
                    PlayerInfo pPlayerInfo = m_pMainScene.ShowPlayerInfoPopup();
                    pPlayerInfo.SetupPlayerInfoData(E_PLAYER_INFO_TYPE.my,pPlayer);
                    pPlayerInfo.SetupQuickPlayerInfoData(null);
                    pPlayerInfo.ShowTabUI(E_PLAYER_INFO_TAB.information);
                }
                else
                {
                    m_pMainScene.RequestPlayerProfile(id,CallbackShowPlayerProfile);
                }
            }
            else
            {
                m_pMainScene.RequestClubProfile(m_ulSendCustomerNo,ShowClubProfileFormation);
            }
        }
    }

    public void SharePlayerInfo(ulong playerID)
    {
        GameContext pGameContext = GameContext.getCtx();

        if(m_bJoinChat)
        {
            PlayerT pPlayer = pGameContext.GetPlayerByID(playerID);
            if(pPlayer != null)
            {
                JoinRoom();

                ChatMsgT pChatMsg = MakeChatMsg();
                pChatMsg.Type = (byte)E_MSG_TYPE.share;
                
                JObject pMsg = new JObject();
                pMsg["playerID"] = playerID;
                pMsg["msg"] = string.Format("{0}.{1} ({2}/{3})",pPlayer.Forename[0],pPlayer.Surname,pPlayer.AbilityTier,pPlayer.PotentialTier);

                pChatMsg.Msg = pMsg.ToString();

                JObject pJObject = new JObject();
                pJObject["type"] = E_SOKET.chat.ToString();
                pJObject["channelId"] = m_strJoinChannelId;
                pJObject["msg"] = Newtonsoft.Json.JsonConvert.SerializeObject(pChatMsg, Newtonsoft.Json.Formatting.None,new Newtonsoft.Json.JsonSerializerSettings { NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore});
                
                m_pMainScene.ShowToastMessage(pGameContext.GetLocalizingText("CHAT_TXT_PLAYER_PROFILE_SHARED"),null);
                NetworkManager.SendMessage(pJObject);
            }
        }
        else
        {
            JoinChatServer();
            m_pMainScene.ShowMessagePopup(pGameContext.GetLocalizingText("MSG_TXT_TRY_AGAIN"),pGameContext.GetLocalizingText("MSG_BTN_OKAY"));
        }
    }

    public void SoketProcessor(NetworkData data)
    {
        ChatMsgT pChatMsg = Newtonsoft.Json.JsonConvert.DeserializeObject<ChatMsgT>((string)data.Json["payload"]);
        
        if(pChatMsg.Type == (byte)E_MSG_TYPE.None)
        {
            pChatMsg.Type = pChatMsg.Id == GameContext.getCtx().GetClubID() ? (byte)E_MSG_TYPE.me : (byte)E_MSG_TYPE.you;
        }
        
        string strMsg = null;
        if(pChatMsg.Type == (byte)E_MSG_TYPE.share)
        {
            strMsg = SingleFunc.CheckStringLength(GameContext.getCtx().GetLocalizingText("CHAT_TXT_PLAYER_PROFILE_SHARED"),35);
            strMsg = string.Format("<color={2}>[{0}]:{1}</color>",pChatMsg.Name,strMsg,Color.green.ToHexString());
        }
        else
        {
            strMsg = SingleFunc.CheckStringLength(pChatMsg.Msg,35);
            strMsg = string.Format("[{0}]:{1}",pChatMsg.Name,strMsg);
        }

        
        m_pMainScene.UpdateTopChatMsg(strMsg);        
        AddChatMsg(pChatMsg);
    }
}
