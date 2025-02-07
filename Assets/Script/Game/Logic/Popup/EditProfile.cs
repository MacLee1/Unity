using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ALF;
using ALF.STATE;
using ALF.CONDITION;
using ALF.LAYOUT;
using ALF.MACHINE;
using ALF.SOUND;
using DATA;
using USERDATA;
using TRAININGCOST;
// using UnityEngine.EventSystems;
using STATEDATA;
using Newtonsoft.Json.Linq;
using PLAYERNATIONALITY;
using CLUBNATIONALITY;
using CONSTVALUE;
public class EditProfile : IBaseUI
{
    const string SCROLL_ITEM_NAME = "NationItem";
    MainScene m_pMainScene = null;
    ScrollRect m_pScrollRect = null;
    Button m_pNationalityButton = null;
    Button m_pOKButton = null;
    TMPro.TMP_InputField m_pClubName = null;
    string m_strClubName = null;
    string m_strNationCode = null;

    EmblemBake m_pEmblem = null;
    Transform m_pCoin = null;
    Transform m_pButtonCoin = null;
    Transform m_pOK = null;

    public RectTransform MainUI { get; private set;}
    public bool Enable { set{ if (m_pScrollRect != null) m_pScrollRect.enabled = value; }}
    List<NationItem> m_pNationItemList = new List<NationItem>();
    Vector2 m_pPrevDir = Vector2.zero;
    int m_iTotalScrollItems = 0;
    int m_iTotalItems = 0;
    int m_iStartIndex = 0;

    public EditProfile(){}
    
    public void Dispose()
    {
        m_pMainScene = null;
        MainUI = null;

        m_pClubName.onSubmit.RemoveAllListeners();
        m_pClubName.onSelect.RemoveAllListeners();
        m_pClubName.onDeselect.RemoveAllListeners();
        
        ClearScroll();
        m_pScrollRect.onValueChanged.RemoveAllListeners();
        m_pNationItemList = null;
        m_pClubName = null;
        m_pScrollRect = null;
        m_pNationalityButton = null;
        m_strClubName = null;
        m_pOKButton = null;
        m_pCoin = null;
        m_pOK = null;
        m_pButtonCoin = null;
        m_pEmblem.material = null;
        m_pEmblem = null;
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "EditProfile : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "EditProfile : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;

        LayoutManager.SetReciveUIButtonEvent(MainUI,this.ButtonEventCall);
        
        m_pEmblem = MainUI.Find("root/emblem/emblem").GetComponent<EmblemBake>();
        m_pClubName = MainUI.Find("root/clubName").GetComponent<TMPro.TMP_InputField>();
        m_pClubName.characterLimit = GameContext.getCtx().GetConstValue(E_CONST_TYPE.clubNameLengthLimit);

        m_pCoin = m_pClubName.textViewport.transform.Find("icon");
        m_pOK = m_pClubName.textViewport.transform.Find("ok");
        
        TMPro.TMP_Text text = m_pCoin.Find("count").GetComponent<TMPro.TMP_Text>();
        int count = GameContext.getCtx().GetConstValue(E_CONST_TYPE.editClubNameTokenCost);
        text.SetText(count.ToString());

        m_pNationalityButton = MainUI.Find("root/nationality").GetComponent<Button>();
        m_pOKButton = MainUI.Find("root/confirm").GetComponent<Button>();
        m_pButtonCoin = m_pOKButton.transform.Find("icon");
        text = m_pButtonCoin.Find("count").GetComponent<TMPro.TMP_Text>();
        text.SetText(count.ToString());
        m_pOKButton.transform.Find("on").gameObject.SetActive(true);
        m_pOKButton.transform.Find("off").gameObject.SetActive(false);
        
        m_pScrollRect = MainUI.Find("root/nationList").GetComponent<ScrollRect>();
        m_pScrollRect.onValueChanged.AddListener( ScrollViewChangeValueEventCall);
        m_pClubName.onSubmit.AddListener(delegate {  InputSumit(m_pClubName); });
        m_pClubName.onDeselect.AddListener(delegate {  InputSumit(m_pClubName); });
        
        m_pClubName.onSelect.AddListener(CheckEditName);

        // m_pScrollRect.transform.gameObject.SetActive(false);
        MainUI.gameObject.SetActive(false);
        SetupScroll();
    }
    void CheckEditName(string token)
    {
        m_pScrollRect.transform.gameObject.SetActive(false);
    }

    void InputSumit(TMPro.TMP_InputField input )
    {
        if(input == null) return;
        
        string currentName = GameContext.getCtx().GetClubName();
        string msg = null;
        if ( string.IsNullOrEmpty(input.text))
        {
            msg = "MSG_TXT_TRY_AGAIN";
            m_strClubName = currentName;
            input.text = m_strClubName;
        }

        bool bOk = input.text != currentName;
        if(bOk)
        {
            if(SingleFunc.IsMatchString(input.text,@"[\;\'\""\/\`\\\r\n]"))
            {
                msg = "MSG_TXT_STRING_ERROR";
                bOk = false;
                m_strClubName = currentName;
                input.text = m_strClubName;
            }
            else
            {
                m_strClubName = input.text;
            }
        }

        m_pOK.gameObject.SetActive(bOk);
        m_pCoin.gameObject.SetActive(!bOk);
        m_pButtonCoin.gameObject.SetActive(bOk);

        EnableConfirm(CheckConfirm(true));
        if(!string.IsNullOrEmpty(msg))
        {
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
        m_strNationCode = null;
        m_pScrollRect.transform.gameObject.SetActive(false);
        m_pButtonCoin.gameObject.SetActive(false);
        m_pOK.gameObject.SetActive(false);
        m_pCoin.gameObject.SetActive(true);
        EnableConfirm(false);
        m_pEmblem.material = null;
    }

    public void CheckEnableConfirm()
    {
        GameContext pGameContext = GameContext.getCtx();
        if(m_pOKButton.enabled && m_strClubName != pGameContext.GetClubName())
        {
            EnableConfirm(pGameContext.GetTotalCash() >= pGameContext.GetConstValue(E_CONST_TYPE.editClubNameTokenCost));
        }
    }

    bool CheckConfirm(bool bName)
    {
        GameContext pGameContext = GameContext.getCtx();
        if(bName)
        {
            if(pGameContext.GetTotalCash() >= pGameContext.GetConstValue(E_CONST_TYPE.editClubNameTokenCost))
            {
                return m_strClubName != pGameContext.GetClubName();
            }
        }
        else
        {
            return m_strNationCode != pGameContext.GetClubNation();
        }

        return false;
    }

    void EnableConfirm(bool bEnable)
    {
        if(m_pOKButton.enabled == bEnable) return;

        m_pOKButton.transform.Find("on").gameObject.SetActive(bEnable);
        m_pOKButton.transform.Find("off").gameObject.SetActive(!bEnable);
        m_pOKButton.enabled = bEnable;
    }
    public void SetupProfileData(EmblemBake pEmblemBake)
    {
        ResetScroll();
        
        GameContext pGameContext = GameContext.getCtx();
        m_strNationCode = pGameContext.GetClubNation();
        
        RawImage icon = m_pNationalityButton.transform.Find("nation").GetComponent<RawImage>();
        Sprite pSprite = AFPool.GetItem<Sprite>("Texture",m_strNationCode);
        icon.texture = pSprite.texture;

        TMPro.TMP_Text text = m_pNationalityButton.transform.Find("text").GetComponent<TMPro.TMP_Text>();

        ClubNationalityItem? pClubNationalityItem = pGameContext.GetClubNationalityDataByCode(pGameContext.GetClubNationCode());
        text.SetText(pGameContext.GetLocalizingText(pClubNationalityItem.Value.NationName));

        m_strClubName = pGameContext.GetClubName();
        m_pClubName.text = m_strClubName;
        m_pEmblem.CopyPoint(pEmblemBake);
        m_pClubName.interactable = pGameContext.GetTotalCash() >= pGameContext.GetConstValue(E_CONST_TYPE.editClubNameTokenCost);

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
                    EnableConfirm(CheckConfirm(false));
                }
            }
            else
            {
                m_pNationItemList[i].On.SetActive(false);
            }
        }

        root.gameObject.SetActive(false);
    }

    public void Close()
    {
        m_pScrollRect.transform.gameObject.SetActive(false);
        SingleFunc.HideAnimationDailog(MainUI);
    }

    void ChangeProfileData()
    {
        JObject pJObject = new JObject();
        pJObject["name"] = m_strClubName;
        pJObject["nation"] = m_strNationCode;

        m_pMainScene.RequestAfterCall(E_REQUEST_ID.club_changeProfile,pJObject, ()=>{
            GameContext pGameContext = GameContext.getCtx();
            pGameContext.SetClubName(m_strClubName);
            pGameContext.SetClubNation(m_strNationCode);
            m_pMainScene.UpdateTopClubName();
            if(m_pMainScene.IsShowInstance<Profile>())
            {
                m_pMainScene.GetInstance<Profile>().UpdateClubNameAnNation(pGameContext.GetTotalPlayerAbility(pGameContext.GetActiveLineUpType()));
            }
            Close();
        });
    }

    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        if (sender.name == "confirm")
        {
            GameContext pGameContext = GameContext.getCtx();
            if(pGameContext.IsLoadGameData())
            {
                if(SingleFunc.IsMatchString(m_strClubName,@"[\;\'\""\/\`\\\r\n]") || ALFUtils.IsFwordFilter(m_strClubName))
                {
                    m_pMainScene.ShowToastMessage(pGameContext.GetLocalizingText("MSG_TXT_STRING_ERROR"),null);
                    m_strClubName = pGameContext.GetClubName();

                    m_pCoin.gameObject.SetActive(false);
                    m_pButtonCoin.gameObject.SetActive(false);
                    m_pOK.gameObject.SetActive(false);
                    EnableConfirm(false);
                    m_pClubName.text = m_strClubName;
                    return;
                }

                if(m_strClubName != pGameContext.GetClubName())
                {
                    m_pMainScene.ShowConfirmPopup(pGameContext.GetLocalizingText("DIALOG_CHANGECLUBNAME_TITLE"), string.Format(pGameContext.GetLocalizingText("DIALOG_CHANGECLUBNAME_TXT"),pGameContext.GetConstValue(E_CONST_TYPE.editClubNameTokenCost)),pGameContext.GetLocalizingText("DIALOG_BTN_CONFIRM"),pGameContext.GetLocalizingText("DIALOG_BTN_CALCEL"),false,ChangeProfileData);
                }
                else
                {
                    ChangeProfileData();
                }
            }

            return;
        }
        else if (sender.name == "emblem")
        {
            m_pMainScene.ShowEditEmblemPopup();
            return;
        }
        else if (sender.name == "nationality")
        {
            m_pScrollRect.transform.gameObject.SetActive(!m_pScrollRect.transform.gameObject.activeSelf);
            return;
        }
        else if (sender.name == "back")
        {
            if (m_pScrollRect.transform.gameObject.activeSelf)
            {
                m_pScrollRect.transform.gameObject.SetActive(false);
            }
            return;
        }

        Close();
    }
}
