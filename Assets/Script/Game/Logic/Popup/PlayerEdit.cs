using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ALF;
using ALF.LAYOUT;
using DATA;
using USERDATA;
using Newtonsoft.Json.Linq;
using PLAYERNATIONALITY;
using CONSTVALUE;
public class PlayerEdit : IBaseUI
{
    const string SCROLL_ITEM_NAME = "NationItem";
    MainScene m_pMainScene = null;
    ScrollRect m_pScrollRect = null;
    Button m_pNationalityButton = null;
    Button m_pOKButton = null;
    TMPro.TMP_InputField m_pFirstName = null;
    TMPro.TMP_InputField m_pLastName = null;
    string m_strFristName = null;
    string m_strLastName = null;
    string m_strNationCode = null;
    
    PlayerT m_pPlayerData = null;
    List<NationItem> m_pNationItemList = new List<NationItem>();
    Vector2 m_pPrevDir = Vector2.zero;
    int m_iTotalScrollItems = 0;
    int m_iTotalItems = 0;
    int m_iStartIndex = 0;
    public RectTransform MainUI { get; private set;}
    public bool Enable { set{ if (m_pScrollRect != null) m_pScrollRect.enabled = value; }}

    public PlayerEdit(){}
    
    public void Dispose()
    {
        RawImage icon = m_pNationalityButton.transform.Find("nation").GetComponent<RawImage>();
        icon.texture = null;

        m_pMainScene = null;
        MainUI = null;

        m_pFirstName.onSubmit.RemoveAllListeners();
        m_pFirstName.onSelect.RemoveAllListeners();
        m_pFirstName.onDeselect.RemoveAllListeners();
        m_pLastName.onDeselect.RemoveAllListeners();

        m_pLastName.onSubmit.RemoveAllListeners();
        m_pLastName.onSelect.RemoveAllListeners();

        ClearScroll();

        m_pScrollRect.onValueChanged.RemoveAllListeners();
        m_pNationItemList = null;
        m_pFirstName = null;
        m_pLastName = null;
        m_pScrollRect = null;
        m_pNationalityButton = null;
        m_pPlayerData = null;
        m_strFristName = null;
        m_strLastName = null;
        m_pOKButton = null;
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "PlayerEdit : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "PlayerEdit : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;

        LayoutManager.SetReciveUIButtonEvent(MainUI,this.ButtonEventCall);
        
        m_pFirstName = MainUI.Find("root/fName").GetComponent<TMPro.TMP_InputField>();
        m_pFirstName.characterLimit = GameContext.getCtx().GetConstValue(E_CONST_TYPE.playerforenameLengthLimit);
        m_pLastName = MainUI.Find("root/lName").GetComponent<TMPro.TMP_InputField>();
        m_pLastName.characterLimit = GameContext.getCtx().GetConstValue(E_CONST_TYPE.playerSurnameLengthLimit);
        
        // m_PlayerName = MainUI.Find("root/title/text").GetComponent<TMPro.TMP_Text>();
        m_pNationalityButton = MainUI.Find("root/nationality").GetComponent<Button>();
        m_pOKButton = MainUI.Find("root/confirm").GetComponent<Button>();
        m_pScrollRect = MainUI.Find("root/nationList").GetComponent<ScrollRect>();
        m_pScrollRect.onValueChanged.AddListener( ScrollViewChangeValueEventCall);
        m_pFirstName.onSubmit.AddListener(delegate {  InputSumit(m_pFirstName); });
        m_pLastName.onSubmit.AddListener(delegate {  InputSumit(m_pLastName); });
        m_pFirstName.onDeselect.AddListener(delegate {  InputSumit(m_pFirstName); });
        m_pLastName.onDeselect.AddListener(delegate {  InputSumit(m_pLastName); });
        
        m_pFirstName.onValueChanged.AddListener(delegate {  InputValue(m_pFirstName); });
        m_pLastName.onValueChanged.AddListener(delegate {  InputValue(m_pLastName); });

        m_pFirstName.onSelect.AddListener(delegate { m_pScrollRect.transform.gameObject.SetActive(false);});
        m_pLastName.onSelect.AddListener(delegate {m_pScrollRect.transform.gameObject.SetActive(false); });

        m_pScrollRect.transform.gameObject.SetActive(false);
        MainUI.gameObject.SetActive(false);
        SetupScroll();
    }

    void InputValue(TMPro.TMP_InputField input )
    {
        if(input == null) return;
        
        if(SingleFunc.IsMatchString(input.text,@"[\;\'\""\/\`\\\r\n]"))
        {
            m_pMainScene.ShowToastMessage(GameContext.getCtx().GetLocalizingText("MSG_TXT_STRING_ERROR"),null);
        }
        
        if(input == m_pLastName)
        {
            m_strLastName = m_pLastName.text;
        }
        else if(input == m_pFirstName)
        {
            m_strFristName = m_pFirstName.text;
        }
    }

    void InputSumit(TMPro.TMP_InputField input )
    {
        if(input == null) return;
        string msg = null;
        if ( string.IsNullOrEmpty(input.text))
        {
            msg = "MSG_TXT_TRY_AGAIN";

            if(input == m_pLastName)
            {
                m_pLastName.text = m_strLastName;
            }
            else if(input == m_pFirstName)
            {
                m_pFirstName.text = m_strFristName;
            }
        }

        if(SingleFunc.IsMatchString(input.text,@"[\;\'\""\/\`\\\r\n]"))
        {
            msg = "MSG_TXT_STRING_ERROR";
        }
        
        if(input == m_pLastName)
        {
            m_strLastName = m_pLastName.text;
            if(string.IsNullOrEmpty(m_strLastName))
            {
                m_strLastName = m_pPlayerData.Surname;
                input.text = m_strLastName;
            }
        }
        else if(input == m_pFirstName)
        {
            m_strFristName = m_pFirstName.text;
            if(string.IsNullOrEmpty(m_strFristName))
            {
                m_strFristName = m_pPlayerData.Forename;
                input.text = m_strFristName;
            }
        }
        
        if(string.IsNullOrEmpty(msg))
        {
            EnableConfirm(CheckConfirm());   
        }
        else
        {
            EnableConfirm(false);
            Director.StartCoroutine(ShowNextFrameToastMessage(msg));
        }
    }

    IEnumerator ShowNextFrameToastMessage(string msg)
    {
        yield return null;

        m_pMainScene.ShowToastMessage(GameContext.getCtx().GetLocalizingText(msg),null);
    }


    void SetupScroll()
    {
        GameContext pGameContext = GameContext.getCtx();
        
        RectTransform pItem = null;
        Vector2 size;
        float h = 0;

        List<string> list = pGameContext.GetPlayerNationCodeList();
        m_iTotalScrollItems = 0;
        m_iTotalItems = list.Count;
        m_iStartIndex = 0;
        float viewSize = m_pScrollRect.viewport.rect.height;
        float itemSize = 0;
        NationItem pNationItem = null;

        for(int i = 0; i < list.Count; ++i)
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
                    pNationItem = new NationItem(pItem,SCROLL_ITEM_NAME);
                    m_pNationItemList.Add(pNationItem);

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
        for(int i = 0; i < m_pNationItemList.Count; ++i)
        {
            m_pNationItemList[i].Dispose();
        }
        m_pNationItemList.Clear();
        
        LayoutManager.SetReciveUIScollViewEvent(m_pScrollRect,null);

        m_pScrollRect.content.anchoredPosition = Vector2.zero;        
        Vector2 size = m_pScrollRect.content.sizeDelta;
        size.y =0;
        m_pScrollRect.content.sizeDelta = size;
    }

    bool CheckConfirm()
    {
        return (m_strFristName != m_pPlayerData.Forename || m_strLastName != m_pPlayerData.Surname || m_strNationCode != m_pPlayerData.Nation);
    }

    void EnableConfirm(bool bEnable)
    {
        if(m_pOKButton.enabled == bEnable) return;

        m_pOKButton.transform.Find("on").gameObject.SetActive(bEnable);
        m_pOKButton.transform.Find("off").gameObject.SetActive(!bEnable);
        m_pOKButton.enabled = bEnable;
    }
    public void SetupPlayerInfoData(PlayerT pPlayer)
    {
        m_pPlayerData = pPlayer;
        
        ResetScroll();
        GameContext pGameContext = GameContext.getCtx();
        RawImage icon = m_pNationalityButton.transform.Find("nation").GetComponent<RawImage>();
        Sprite pSprite = AFPool.GetItem<Sprite>("Texture",m_pPlayerData.Nation);
        icon.texture = pSprite.texture;

        TMPro.TMP_Text text = m_pNationalityButton.transform.Find("text").GetComponent<TMPro.TMP_Text>();
        
        NATION_CODE code = pGameContext.ConvertPlayerNationCodeByString(m_pPlayerData.Nation,m_pPlayerData.Id);
        PlayerNationalityItem? pPlayerNationalityItem = pGameContext.GetPlayerNationDataByCode(code);
        text.SetText(pGameContext.GetLocalizingText(pPlayerNationalityItem.Value.List(0).Value.NationName));
        m_strNationCode = m_pPlayerData.Nation;
        m_strFristName = m_pPlayerData.Forename;
        m_strLastName = m_pPlayerData.Surname;
        m_pFirstName.text = m_strFristName;
        m_pLastName.text = m_strLastName;

        List<string> list = pGameContext.GetPlayerNationCodeList();
        int i = 0;
        Vector2 pos = Vector2.zero;
        for(i = 0; i < list.Count; ++i)
        {
            if(list[i].Contains(m_strNationCode))
            {
                m_iStartIndex = i;
                pos = m_pScrollRect.content.anchoredPosition;
                pos.y = m_pNationItemList[0].Target.rect.height * i;
                m_pScrollRect.content.anchoredPosition = pos;
                break;
            }
        }
        pos.x =0;
        int index = 0;
        for(i = 0; i < m_pNationItemList.Count; ++i)
        {
            m_pNationItemList[i].Target.anchoredPosition = m_pNationItemList[i].Target.anchoredPosition - pos;
            index = m_iStartIndex +i;
            if( index > -1 && index < list.Count)
            {
                m_pNationItemList[i].UpdateNationItem(list[index],m_strNationCode);
            }
        }
    }

	void ResetScroll()
    {
        Vector2 pos;
        float viewSize = m_pScrollRect.viewport.rect.height;
        float itemSize = 0;
        NationItem pNationItem = null;
        for(int i = 0; i < m_pNationItemList.Count; ++i)
        {
            pNationItem = m_pNationItemList[i];
            itemSize = pNationItem.Target.rect.height;
            viewSize -= itemSize;
            pNationItem.Target.gameObject.SetActive(viewSize > -itemSize);
            pNationItem.On.SetActive(false);
            pos = pNationItem.Target.anchoredPosition;            
            pos.y = -i * itemSize;
            pNationItem.Target.anchoredPosition = pos;
        }

        m_pNationalityButton.enabled = true;
        m_pScrollRect.verticalNormalizedPosition = 1;
        m_pScrollRect.content.anchoredPosition = Vector2.zero;
        m_iStartIndex = 0;
        m_pPrevDir.y = 1;
        m_strFristName = null;
        m_strLastName = null;
        m_strNationCode = null;
        m_pScrollRect.transform.gameObject.SetActive(false);
        EnableConfirm(false);
    }

    public void Close()
    {
        m_pScrollRect.transform.gameObject.SetActive(false);
        SingleFunc.HideAnimationDailog(MainUI);
    }
    void ScrollViewChangeData(int index,int iTarget,int iCount)
    {
        if(index == iTarget) return;
        int i = 0;
        NationItem pNationItem = null;
        if(index > iTarget)
        {
            pNationItem = m_pNationItemList[iTarget];
            m_pNationItemList[iTarget] = m_pNationItemList[index];
            i = iTarget +1;
            NationItem pTemp = null;
            while(i < index)
            {
                pTemp = m_pNationItemList[i];
                m_pNationItemList[i] = pNationItem;
                pNationItem = pTemp;
                ++i;
            }
            m_pNationItemList[index] = pNationItem;
            pNationItem = m_pNationItemList[iTarget];
        }
        else
        {
            pNationItem = m_pNationItemList[index];
            i = index +1;
            while(i <= iTarget)
            {
                m_pNationItemList[i -1] = m_pNationItemList[i];
                ++i;
            }

            m_pNationItemList[iTarget] = pNationItem;
        }
        GameContext pGameContext = GameContext.getCtx();
        List<string> list = pGameContext.GetPlayerNationCodeList();
        i = m_iStartIndex + iTarget + iCount;
        
        if(i < 0 || list.Count <= i) return;
        
        pNationItem.UpdateNationItem(list[i],m_strNationCode);
    }

    void ScrollViewChangeValueEventCall( Vector2 value)
    {
        if(m_iTotalScrollItems < m_iTotalItems && value.y != m_pPrevDir.y)
        {
            m_pScrollRect.ScrollViewChangeValue(value - m_pPrevDir,ref m_iStartIndex,ScrollViewChangeData);
            m_pPrevDir = value;
        }
    }

    void ScrollViewItemButtonEventCall(ScrollRect root, Transform tm,GameObject sender)
    {
        for(int i =0; i < m_pNationItemList.Count; ++i)
        {
            if(m_pNationItemList[i].Target == tm)
            {
                if( m_strNationCode != m_pNationItemList[i].ID)
                {
                    m_pNationItemList[i].On.SetActive(true);
                    m_strNationCode = m_pNationItemList[i].ID;
                    RawImage icon = m_pNationalityButton.transform.Find("nation").GetComponent<RawImage>();
                    icon.texture = m_pNationItemList[i].Icon.texture;
                    TMPro.TMP_Text text = m_pNationalityButton.transform.Find("text").GetComponent<TMPro.TMP_Text>();
                    text.SetText(m_pNationItemList[i].NameText.text);
                    EnableConfirm(CheckConfirm());
                }
            }
            else
            {
                m_pNationItemList[i].On.SetActive(false);
            }
        }

        root.gameObject.SetActive(false);
    }


    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        if(sender.name == "confirm" )
        {
            GameContext pGameContext = GameContext.getCtx();

            if((SingleFunc.IsMatchString(m_strLastName,@"[\;\'\""\/\`\\\r\n]") && SingleFunc.IsMatchString(m_strFristName,@"[\;\'\""\/\`\\\r\n]")) || ALFUtils.IsFwordFilter(m_strLastName) || ALFUtils.IsFwordFilter(m_strFristName))
            {
                m_pMainScene.ShowToastMessage(pGameContext.GetLocalizingText("MSG_TXT_STRING_ERROR"),null);
                
                m_strFristName = m_pPlayerData.Forename;
                m_strLastName = m_pPlayerData.Surname;
                m_pFirstName.text = m_strFristName;
                m_pLastName.text = m_strLastName;
                EnableConfirm(false);
                return;
            }

            m_pMainScene.ShowConfirmPopup(pGameContext.GetLocalizingText("DIALOG_CHANGEPLAYERNAME_TITLE"), string.Format(pGameContext.GetLocalizingText("DIALOG_CHANGEPLAYERNAME_TXT"),1),pGameContext.GetLocalizingText("DIALOG_BTN_CONFIRM"),pGameContext.GetLocalizingText("DIALOG_BTN_CALCEL"),false,()=>{
                JObject pJObject = new JObject();
                pJObject["player"] = m_pPlayerData.Id;
                pJObject["forename"] = m_strFristName;
                pJObject["surname"] = m_strLastName;
                pJObject["nation"] = m_strNationCode;

                m_pMainScene.RequestAfterCall(E_REQUEST_ID.player_changeProfile,pJObject,Close);
            });

            return;
        }
        else if(sender.name == "nationality" )
        {
            bool bActive = m_pScrollRect.transform.gameObject.activeSelf;
            m_pScrollRect.transform.gameObject.SetActive(!bActive);
            return;
        }
        else if(sender.name == "back")
        {
            if(m_pScrollRect.transform.gameObject.activeSelf)
            {
                m_pScrollRect.transform.gameObject.SetActive(false);
            }

            return;
        }
        
        Close();
    }
}
