using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ALF;
using ALF.LAYOUT;
using DATA;
using Newtonsoft.Json.Linq;
using PLAYERNATIONALITY;
using CLUBNATIONALITY;
using CONSTVALUE;

public class CreateClub : IBaseUI
{
    // Start is called before the first frame update
    // const uint Limit_Min_age = 15;
    // const uint Limit_Max_age = 85;
    const string SCROLL_ITEM_NAME = "NationItem";
    MainScene m_pMainScene = null;
    ScrollRect m_pScrollRect = null;
    EmblemBake m_pEmblem = null;
    Button m_pNationalityButton = null;
    Button m_pCreateButton = null;
    string m_strNationCode = null;
    TMPro.TMP_InputField m_pClubNameInput = null;
    List<NationItem> m_pNationItemList = new List<NationItem>();
    Vector2 m_pPrevDir = Vector2.zero;
    int m_iTotalScrollItems = 0;
    int m_iTotalItems = 0;
    int m_iStartIndex = 0;
    public RectTransform MainUI { get; private set;}
    public bool Enable { set{ if (m_pScrollRect != null) m_pScrollRect.enabled = value; }}

    public CreateClub(){}
    
    public void Dispose()
    {
        RawImage icon = m_pNationalityButton.transform.Find("nation").GetComponent<RawImage>();
        icon.texture = null;

        m_pMainScene = null;
        MainUI = null;
        // m_pClubNameInput.onValueChanged.RemoveAllListeners();
        m_pClubNameInput.onSubmit.RemoveAllListeners();
        m_pClubNameInput.onSelect.RemoveAllListeners();
        m_pClubNameInput.onDeselect.RemoveAllListeners();
        m_pClubNameInput = null;
        m_pEmblem.material = null;
        m_pEmblem = null;
        m_pNationalityButton = null;
        m_pCreateButton = null;
        ClearScroll();

        m_pScrollRect.onValueChanged.RemoveAllListeners();
        m_pNationItemList = null;
        m_pScrollRect = null;
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "CreateClub : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "CreateClub : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;

        m_pEmblem = MainUI.Find("root/emblem/emblem").GetComponent<EmblemBake>();
        m_pClubNameInput = MainUI.Find("root/clubName").GetComponent<TMPro.TMP_InputField>();
        m_pClubNameInput.characterLimit = GameContext.getCtx().GetConstValue(E_CONST_TYPE.clubNameLengthLimit) +1;
        m_pClubNameInput.onSubmit.AddListener(delegate {  InputSumit(m_pClubNameInput); });
        m_pClubNameInput.onDeselect.AddListener(delegate {  InputSumit(m_pClubNameInput); });
        // m_pClubNameInput.onValueChanged.AddListener(delegate {  InputSumit(m_pClubNameInput); });
        m_pClubNameInput.onSelect.AddListener(delegate { m_pScrollRect.transform.gameObject.SetActive(false);});

        m_pScrollRect = MainUI.Find("root/nationList").GetComponent<ScrollRect>();
        m_pScrollRect.onValueChanged.AddListener( ScrollViewChangeValueEventCall);
        m_pNationalityButton = MainUI.Find("root/nationality").GetComponent<Button>();
        m_pCreateButton = MainUI.Find("root/create").GetComponent<Button>();

        m_pScrollRect.transform.gameObject.SetActive(false);
        
        LayoutManager.SetReciveUIButtonEvent(MainUI,this.ButtonEventCall);
        MainUI.gameObject.SetActive(false);
        SetupScroll();
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
                   pNationItem.UpdateNationItem(list[i],m_strNationCode);
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

    public void SetupData()
    {
        ResetScroll();
        GameContext pGameContext = GameContext.getCtx();
        CLUB_NATION_CODE code = pGameContext.GetClubNationCode();
        m_strNationCode = code.ToString();
        RawImage icon = m_pNationalityButton.transform.Find("nation").GetComponent<RawImage>();
        Sprite pSprite = AFPool.GetItem<Sprite>("Texture",m_strNationCode);
        if(pSprite == null)
        {
            SingleFunc.SendLog($"CreateClub::SetupData pSprite = null !! :{m_strNationCode}");
        }
        else
        {
            icon.texture = pSprite.texture;
        }

        TMPro.TMP_Text text = m_pNationalityButton.transform.Find("text").GetComponent<TMPro.TMP_Text>();
        
        ClubNationalityItem? pClubNationalityItem = pGameContext.GetClubNationalityDataByCode(code);
        text.SetText(pGameContext.GetLocalizingText(pClubNationalityItem.Value.NationName));
        m_pEmblem.SetupEmblemData(pGameContext.GetEmblemInfo());

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
        
        m_pClubNameInput.text = "";
        m_strNationCode = null;
        m_pScrollRect.transform.gameObject.SetActive(false);
        EnableConfirm(false);
    }

    void EnableConfirm(bool bEnable)
    {
        if(m_pCreateButton.enabled == bEnable) return;

        m_pCreateButton.transform.Find("on").gameObject.SetActive(bEnable);
        m_pCreateButton.transform.Find("off").gameObject.SetActive(!bEnable);
        m_pCreateButton.enabled = bEnable;
    }

    void InputSumit(TMPro.TMP_InputField input )
    {
        if(input == null) return;
        bool bOk = !string.IsNullOrEmpty(input.text);
        string msg = null;
        if(SingleFunc.IsMatchString(input.text,@"[\;\'\""\/\`\\\r\n]"))
        {
            msg = "MSG_TXT_STRING_ERROR";
            input.text = "";
            bOk = false;
        }
        else if (!bOk)
        {
            msg = "MSG_TXT_TRY_AGAIN";
        }
        else
        {
            if(input.characterLimit <= input.text.Length)
            {
                msg = "MSG_TXT_TRY_AGAIN";
                bOk = false;
            }
        }

        if(!string.IsNullOrEmpty(msg))
        {
            Director.StartCoroutine(ShowNextFrameToastMessage(msg));
        }
        
        EnableConfirm(bOk);
    }

    IEnumerator ShowNextFrameToastMessage(string msg)
    {
        yield return null;

        m_pMainScene.ShowToastMessage(GameContext.getCtx().GetLocalizingText(msg),null);
    }

    public void UpdateEmblemInfos()
    {
        m_pEmblem.SetupEmblemData(GameContext.getCtx().GetEmblemInfo());
    }

    public void Close()
    {
    }

    void ClosePopup()
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
        if(sender.name == "create" )
        {
            if ( string.IsNullOrEmpty(m_pClubNameInput.text))
            {
                return;
            }

            GameContext pGameContext = GameContext.getCtx();
            if(SingleFunc.IsMatchString(m_pClubNameInput.text,@"[\;\'\""\/\`\\\r\n]")|| ALFUtils.IsFwordFilter(m_pClubNameInput.text))
            {
                m_pMainScene.ShowToastMessage(pGameContext.GetLocalizingText("MSG_TXT_STRING_ERROR"),null);
                m_pClubNameInput.text = "";
                EnableConfirm(false);
                return;
            }
            
            JObject pJObject = new JObject();
            pJObject["name"] = m_pClubNameInput.text;
            pJObject["nation"] = pGameContext.GetClubNation();
            pJObject["gender"] = pGameContext.GetUserGender();
            pJObject["age"] = pGameContext.GetUserAge();
            pJObject["emblem"] = Newtonsoft.Json.JsonConvert.SerializeObject(pGameContext.GetEmblemInfo().ToList(), Newtonsoft.Json.Formatting.None,new Newtonsoft.Json.JsonSerializerSettings { NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore});
            m_pMainScene.RequestAfterCall(E_REQUEST_ID.club_create,pJObject,ClosePopup);
            pGameContext.SendAdjustEvent("m1mpx6",true,true,-1); //CreateClub
        }
        else if( sender.name == "emblem")
        {
            m_pMainScene.ShowEditEmblemPopup();
        }
        else if( sender.name == "nationality")
        {
            bool bActive = m_pScrollRect.transform.gameObject.activeSelf;
            m_pScrollRect.transform.gameObject.SetActive(!bActive);
        }
        else if (sender.name == "back")
        {
            if (m_pScrollRect.transform.gameObject.activeSelf)
            {
                m_pScrollRect.transform.gameObject.SetActive(false);
            }
        }
    }
}
