using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using ALF;

using ALF.NETWORK;
using ALF.LAYOUT;
using ALF.SOUND;
using USERDATA;
using DATA;
using Newtonsoft.Json.Linq;

public class MailBox : IBaseUI
{
    enum E_MAIL_TYPE : byte { Notice=1,Normal=10}
    enum E_MAIL_STATUS : byte { Unread=0,Read_Reward=4,Read_NoReward=5,Delete=9}

    const string SCROLL_ITEM_NAME = "MailItem";
    const string MAIL_REWARD_ITEM_NAME = "MailRewardItem";

    class MailItem : IBase
    {
        public ulong Id {get; private set;}
        public byte Type {get; private set;}
        public TMPro.TMP_Text Name {get; private set;}
        public TMPro.TMP_Text ReceivedText {get; private set;}
        public TMPro.TMP_Text ExpiresText {get; private set;}
        
        public RectTransform Rewards {get; private set;}
        public GameObject On {get; private set;}
        public GameObject Off {get; private set;}

        public GameObject ReceiveOn {get; private set;}
        public GameObject ReceiveOff {get; private set;}

        public Button Receive {get; private set;}

        public RectTransform Target  {get; private set;}
        
        Button m_pButton = null;

        public MailItem(RectTransform target)
        {
            Target = target;
            m_pButton = target.GetComponent<Button>();
            
            Name = target.Find("name").GetComponent<TMPro.TMP_Text>();
            ReceivedText = target.Find("received").GetComponent<TMPro.TMP_Text>();
            ExpiresText = target.Find("expires").GetComponent<TMPro.TMP_Text>();
            Rewards = target.Find("rewards").GetComponent<RectTransform>();
            On = target.Find("mail/on").gameObject;
            Off = target.Find("mail/off").gameObject;
            ReceiveOn = target.Find("receive/on").gameObject;
            ReceiveOff = target.Find("receive/off").gameObject;
            Receive = target.Find("receive").GetComponent<Button>();
        }

        public void Dispose()
        {
            Name = null;
            ReceivedText = null;
            ExpiresText = null;
            On = null;
            Off = null;
            ReceiveOn = null;
            ReceiveOff = null;
        
            m_pButton.onClick.RemoveAllListeners();
            m_pButton = null;

            Receive.onClick.RemoveAllListeners();
            Receive = null;
            ClearReward();

            Rewards = null;
            LayoutManager.Instance.AddItem(SCROLL_ITEM_NAME,Target);
            Target = null;
        }

        void ClearReward()
        {
            RectTransform item = null;
            Vector2 anchor = new Vector2(0.5f,0.5f);
            RawImage icon = null;
            int i = Rewards.childCount;
            while(i > 0)
            {
                --i;
                item = Rewards.GetChild(i).GetComponent<RectTransform>();
                item.anchorMax = anchor;
                item.anchorMin = anchor;
                item.pivot = anchor;
                icon = item.Find("icon").GetComponent<RawImage>();
                icon.color = Color.white;
                icon.texture = null;

                LayoutManager.Instance.AddItem(MAIL_REWARD_ITEM_NAME,item);
            }
        }

        public void UpdateData(MailInfoT pObject)
        {
            if(pObject == null) return;
            
            ClearReward();
            GameContext pGameContext = GameContext.getCtx();

            Id = pObject.Id;
            Type = pObject.Type;
            long severTime = NetworkManager.GetGameServerTime().Ticks;
            E_MAIL_STATUS eMailStatus = (E_MAIL_STATUS)pObject.Status;
            if(eMailStatus < E_MAIL_STATUS.Delete)
            {
                Name.SetText(pGameContext.GetLocalizingText(pObject.Title));
                RectTransform pReward = null;
                float w = 0;
                Vector2 anchor = new Vector2(0, 0.5f);
                Vector2 size;
                RawImage icon = null;
                Sprite pSprite = null;
                TMPro.TMP_Text text = null;

                int totalTime = (int)((pObject.TExpire - severTime) / TimeSpan.TicksPerSecond);
                Receive.enabled = (eMailStatus < E_MAIL_STATUS.Read_NoReward && totalTime > 0);

                for(int n = 0; n < pObject.Rewards.Count; ++n)
                {
                    pReward = LayoutManager.Instance.GetItem<RectTransform>(MAIL_REWARD_ITEM_NAME);
                    pReward.anchorMax = anchor;
                    pReward.anchorMin = anchor;
                    pReward.pivot = anchor;
                    pReward.SetParent(Rewards,false);

                    pReward.localScale = Vector3.one;
                    pReward.anchoredPosition = new Vector2(w,0);
                    size = pReward.sizeDelta;
                    icon = pReward.Find("icon").GetComponent<RawImage>();
                    
                    text = pReward.Find("text").GetComponent<TMPro.TMP_Text>();
                    uint itemId = pObject.Rewards[n] == GameContext.FREE_CASH_ID ? GameContext.CASH_ID : pObject.Rewards[n];
                    if(itemId == GameContext.CASH_ID)
                    {
                        text.SetText(string.Format("{0:#,0}", pObject.Amounts[n]));
                    }
                    else
                    {
                        text.SetText(ALFUtils.NumberToString(pObject.Amounts[n]));
                    }

                    pSprite = AFPool.GetItem<Sprite>("Texture",itemId.ToString());
                    icon.texture = pSprite.texture;
                    icon.color = Receive.enabled ? Color.white : Color.gray;

                    w += size.x + 20;
                }
                
                ReceiveOff.SetActive(!Receive.enabled);
                ReceiveOn.SetActive(Receive.enabled);
                
                Off.SetActive(eMailStatus == E_MAIL_STATUS.Unread);
                On.SetActive(eMailStatus != E_MAIL_STATUS.Unread);

                if(totalTime > 0)
                {
                    SingleFunc.UpdateTimeText(totalTime,ExpiresText,1);
                }
                else
                {
                    ExpiresText.SetText(GameContext.getCtx().GetLocalizingText("TIMECOUNT_TXT_EXPIRED"));
                }

                totalTime = (int)((pObject.TSend - severTime) / TimeSpan.TicksPerSecond);

                SingleFunc.UpdateTimeText(totalTime,ReceivedText,2);            
                Receive.gameObject.SetActive(pObject.Rewards.Count > 0);
            }
        }
    }


    MainScene m_pMainScene = null;
    ScrollRect m_pScrollRect = null;
    
    public RectTransform MainUI { get; private set;}
    public bool Enable {set{if (m_pScrollRect != null) m_pScrollRect.enabled = value;}}

    List<MailItem> m_pMailItems = new List<MailItem>();

    List<MailInfoT> m_pMailList = new List<MailInfoT>();

    Vector2 m_pPrevDir = Vector2.zero;
    int m_iTotalScrollItems = 0;
    int m_iStartIndex = 0;


    public MailBox(){}
    
    public void Dispose()
    {
        ClearScroll();
        m_pMailItems = null;
        m_pMailList = null;

        if(m_pScrollRect != null)
        {
            m_pScrollRect.onValueChanged.RemoveAllListeners();
        }
        m_pScrollRect = null;
        m_pMainScene = null;
        MainUI = null;   
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {        
        ALFUtils.Assert(pBaseScene != null, "MailBox : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "MailBox : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;

        LayoutManager.SetReciveUIButtonEvent(MainUI,ButtonEventCall);
        m_pScrollRect = MainUI.GetComponentInChildren<ScrollRect>(true);
        m_pScrollRect.onValueChanged.AddListener( ScrollViewChangeValueEventCall);
        MainUI.gameObject.SetActive(false);
        SetupScroll();
    }

    public void SetupData()
    {
        m_pMailList.Clear();
        ResetScroll();
        GameContext pGameContext = GameContext.getCtx();
        List<MailInfoT> mailList = pGameContext.GetMails();
        
        long severTime = NetworkManager.GetGameServerTime().Ticks;

        for(int i =0; i < mailList.Count; ++i)
        {
            if((E_MAIL_STATUS)mailList[i].Status < E_MAIL_STATUS.Delete)
            {
                m_pMailList.Add(mailList[i]);
            }
        }

        if(m_pMailList.Count > 1)
        {
            List<MailInfoT> read = new List<MailInfoT>();
            List<MailInfoT> unread = new List<MailInfoT>();

            for(int i =0; i < m_pMailList.Count; ++i)
            {
                if((E_MAIL_STATUS)m_pMailList[i].Status == E_MAIL_STATUS.Unread)
                {
                    unread.Add(m_pMailList[i]);
                }
                else
                {
                    read.Add(m_pMailList[i]);
                }
            }
            if(unread.Count > 1)
            {
                unread = unread.OrderByDescending(x => x.TSend).ToList();
            }
            if(read.Count > 1)
            {
                read = read.OrderByDescending(x => x.TSend).ToList();
            }
            
            m_pMailList.Clear();
            
            for(int i =0; i < unread.Count; ++i)
            {
                m_pMailList.Add(unread[i]);
            }
            for(int i =0; i < read.Count; ++i)
            {
                m_pMailList.Add(read[i]);
            }
        }

        float viewSize = m_pScrollRect.viewport.rect.height;
        float itemSize = 0;

        MailItem pMailItem = null;
        
        for(int i =0; i < m_pMailItems.Count; ++i)
        {
            pMailItem = m_pMailItems[i];
            itemSize = pMailItem.Target.rect.height;

            if(m_pMailList.Count <= i)
            {
                pMailItem.Target.gameObject.SetActive(false);
            }
            else
            {
                pMailItem.UpdateData(m_pMailList[i]);
            
                if(viewSize > -itemSize)
                {    
                    viewSize -= itemSize;
                    pMailItem.Target.gameObject.SetActive(viewSize > -itemSize);
                }
            }
        }

        Vector2 size = m_pScrollRect.content.sizeDelta;
        size.y = mailList.Count * itemSize;
        m_pScrollRect.content.sizeDelta = size;
        m_pScrollRect.verticalNormalizedPosition = 1;
        m_pPrevDir.y = 1;
        m_pScrollRect.content.anchoredPosition = Vector2.zero;

        SetupDeleteAll(false);
        SetupCollectAll(false);
    }

    JObject SetupDeleteAll(bool bReturn)
    {
        E_MAIL_STATUS eStatus = E_MAIL_STATUS.Unread;
        Button pButton = MainUI.Find("root/deleteAll").GetComponent<Button>();
        pButton.enabled = false;
        
        JArray pArray = null;
        if(bReturn)
        {
            pArray = new JArray();
        }
        JObject pJObject = null;
        for(int i =0; i < m_pMailList.Count; ++i)
        {
            eStatus = (E_MAIL_STATUS)m_pMailList[i].Status;
            if(eStatus < E_MAIL_STATUS.Read_NoReward ) continue;

            pButton.enabled = true;
            if(!bReturn)
            {
                break;
            }
            pJObject = new JObject();
            pJObject["id"] = m_pMailList[i].Id;
            pJObject["type"] = m_pMailList[i].Type;

            pArray.Add(pJObject);
        }

        pButton.transform.Find("on").gameObject.SetActive(pButton.enabled);
        pButton.transform.Find("off").gameObject.SetActive(!pButton.enabled);
        if(bReturn && pButton.enabled)
        {
            pJObject = new JObject();
            pJObject["ids"] = pArray;
            return pJObject;
        }
        
        return null;
    }
    JObject SetupCollectAll(bool bReturn)
    {
        long servetTime = NetworkManager.GetGameServerTime().Ticks;
        E_MAIL_STATUS eStatus = E_MAIL_STATUS.Unread;
        Button pButton = MainUI.Find("root/collectAll").GetComponent<Button>();
        pButton.enabled = false;
        
        JArray pArray = null;
        JObject pJObject = null;
        if(bReturn)
        {
            pArray = new JArray();
        }
        for(int i =0; i < m_pMailList.Count; ++i)
        {
            eStatus = (E_MAIL_STATUS)m_pMailList[i].Status;
            if(m_pMailList[i].Rewards.Count == 0 || servetTime >= m_pMailList[i].TExpire || eStatus > E_MAIL_STATUS.Read_Reward ) continue;

            pButton.enabled = true;
            if(!bReturn)
            {
                break;
            }

            pJObject = new JObject();
            pJObject["id"] = m_pMailList[i].Id;
            pJObject["type"] = m_pMailList[i].Type;

            pArray.Add(pJObject);
        }
        
        pButton.transform.Find("on").gameObject.SetActive(pButton.enabled);
        pButton.transform.Find("off").gameObject.SetActive(!pButton.enabled);
        if(bReturn && pButton.enabled)
        {
            pJObject = new JObject();
            pJObject["ids"] = pArray;

            return pJObject;
        }
        
        return  null;
    }
    void SetupScroll()
    {
        ClearScroll();
        
        m_iTotalScrollItems = 0;

        float viewSize = m_pScrollRect.viewport.rect.height;
        float itemSize = 0;

        MailItem pMailItem = null;
        RectTransform pItem = null;
        Vector2 size;
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
                pMailItem = new MailItem(pItem);
                m_pMailItems.Add(pMailItem);
                
                pItem.SetParent(m_pScrollRect.content,false);
                pItem.localScale = Vector3.one;       
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

        size = m_pScrollRect.content.sizeDelta;
        size.y = h;
        m_pScrollRect.content.sizeDelta = size;
        m_pScrollRect.verticalNormalizedPosition = 1;
        m_pPrevDir.y = 1;
        m_pScrollRect.content.anchoredPosition = Vector2.zero;
        LayoutManager.SetReciveUIScollViewEvent(m_pScrollRect,ScrollViewItemButtonEventCall);
    }

    void ClearScroll()
    {
        int i = m_pMailItems.Count;

        while(i > 0)
        {
            --i;
            m_pMailItems[i].Dispose();
        }
        m_pMailList.Clear();
        m_pMailItems.Clear();
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
        
        MailItem pItem = null;
        for(int i = 0; i < m_pMailItems.Count; ++i)
        {
            pItem = m_pMailItems[i];
            itemSize = pItem.Target.rect.height;
            viewSize -= itemSize;
            pItem.Target.gameObject.SetActive(viewSize > -itemSize);
            pos = pItem.Target.anchoredPosition;            
            pos.y = -i * itemSize;
            pItem.Target.anchoredPosition = pos;
        }
        
        m_pScrollRect.verticalNormalizedPosition = 1;
        m_pScrollRect.content.anchoredPosition = Vector2.zero;
        m_iStartIndex = 0;
        m_pPrevDir.y = 1;
    }

    void ShowMailMessagePopup(ulong id,byte type)
    {
        m_pMainScene.UpdateUnreadMailCount();
        m_pMainScene.ShowMailMessagePopup(id,type);
    }

    void ScrollViewChangeData(int index,int iTarget,int iCount)
    {
        if(index == iTarget) return;
        int i = 0;
        MailItem pItem = null;
        if(index > iTarget)
        {
            pItem = m_pMailItems[iTarget];
            m_pMailItems[iTarget] = m_pMailItems[index];
            i = iTarget +1;
            MailItem pTemp = null;
            while(i < index)
            {
                pTemp = m_pMailItems[i];
                m_pMailItems[i] = pItem;
                pItem = pTemp;
                ++i;
            }
            m_pMailItems[index] = pItem;
            pItem = m_pMailItems[iTarget];
        }
        else
        {
            pItem = m_pMailItems[index];
            i = index +1;
            while(i <= iTarget)
            {
                m_pMailItems[i -1] = m_pMailItems[i];
                ++i;
            }

            m_pMailItems[iTarget] = pItem;
        }
        
        i = m_iStartIndex + iTarget + iCount;

        if(i < 0 || m_pMailList.Count <= i) return;
        pItem.UpdateData(m_pMailList[i]);
    }

    void ScrollViewChangeValueEventCall( Vector2 value)
    {
        if(m_iTotalScrollItems < m_pMailList.Count && value.y != m_pPrevDir.y)
        {
            m_pScrollRect.ScrollViewChangeValue(value - m_pPrevDir,ref m_iStartIndex,ScrollViewChangeData);
            m_pPrevDir = value;
        }
    }

    void ScrollViewItemButtonEventCall(ScrollRect root, Transform tm,GameObject sender)
    {
        for(int i =0; i < m_pMailItems.Count; ++i)
        {
            if(m_pMailItems[i].Target == tm)
            {
                ulong receiveID = m_pMailItems[i].Id;
                byte receiveType = m_pMailItems[i].Type;

                MailInfoT pMailInfo = GameContext.getCtx().GetMailInfoById(receiveID,receiveType);
                if(sender.name == "receive")
                {
                    JObject pJObject = new JObject();
                    JArray pJArray = new JArray();
                    pJObject["id"] = receiveID;
                    pJObject["type"] = receiveType;
                    pJArray.Add(pJObject);
                    pJObject = new JObject();
                    pJObject["ids"] = pJArray;
                    m_pMainScene.RequestAfterCall(E_REQUEST_ID.mail_reward,pJObject,Close);
                }
                else
                {
                    E_MAIL_STATUS eStatus = (E_MAIL_STATUS)pMailInfo.Status;
                    if(E_MAIL_STATUS.Read_NoReward != eStatus && E_MAIL_STATUS.Read_Reward != eStatus)
                    {
                        JObject pJObject = new JObject();
                        JArray pJArray = new JArray();
                        pJObject["id"] = receiveID;
                        pJObject["type"] = receiveType;
                        pJArray.Add(pJObject);
                        pJObject = new JObject();
                        pJObject["ids"] = pJArray;
                        m_pMainScene.RequestAfterCall(E_REQUEST_ID.mail_read,pJObject,()=>{ShowMailMessagePopup(receiveID,receiveType);});
                    }
                    else
                    {
                        ShowMailMessagePopup(receiveID,receiveType);
                    }
                }
                return;
            }
        }
    }

    public void Close()
    {
        SingleFunc.HideAnimationDailog(MainUI);
    }
    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        if(sender.name == "collectAll")
        {
            JObject pData = SetupCollectAll(true);
            if(pData != null)
            {
                m_pMainScene.RequestAfterCall(E_REQUEST_ID.mail_reward,pData,Close);
            }
            else
            {
                m_pMainScene.ShowToastMessage(GameContext.getCtx().GetLocalizingText("ALERT_TXT_MAIL_EXPIRED"),null);
            }
            return;
        }
        else if(sender.name == "deleteAll")
        {
            JObject pData = SetupDeleteAll(true);
            if(pData != null)
            {
                m_pMainScene.RequestAfterCall(E_REQUEST_ID.mail_delete,pData,SetupData);
            }
            else
            {
                m_pMainScene.ShowToastMessage(GameContext.getCtx().GetLocalizingText("ALERT_TXT_MAIL_DELETED"),null);
            }
            return;
        }
        
        Close();
    }   
}
