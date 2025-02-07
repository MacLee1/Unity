using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ALF;
using ALF.LAYOUT;
using DATA;
using UnityEngine.EventSystems;
using Newtonsoft.Json.Linq;
using CONSTVALUE;

public class EditEmblem : IBaseUI
{
    enum E_TAB : byte { shape = 0,pattern,symbol,MAX}
    enum E_COLOR_TAB : byte { color1 = 0,color2,symbol,MAX}
    readonly string SCROLL_ITEM_NAME = "EmblemItem";
    
    EmblemBake m_pEmblem = null;
    MainScene m_pMainScene = null;
    ScrollRect m_pScrollRect = null;
    Button m_pOKButton = null;
    Transform m_pSelectColorMark = null;

    RawImage m_pColorImage = null;
    Vector2Int m_pixel = Vector2Int.zero;
    E_TAB m_eMainTab = E_TAB.MAX;
    E_COLOR_TAB m_eColorTab = E_COLOR_TAB.MAX;
    byte[] m_eEmblemInfos = null;
    byte[] m_colors = new byte[(int)E_COLOR_TAB.MAX];
    Transform[] m_pTabButtonList = new Transform[(int)E_TAB.MAX];
    Transform[] m_pColorTabButtonList = new Transform[(int)E_COLOR_TAB.MAX];
    EventTrigger m_pDragEventTrigger = null;
    public RectTransform MainUI { get; private set;}
    public bool Enable { set{ if (m_pScrollRect != null) m_pScrollRect.enabled = value; if (m_pDragEventTrigger != null) m_pDragEventTrigger.enabled = value; }}
    public EditEmblem(){}
    
    public void Dispose()
    {
        m_pColorImage = null;
        m_pEmblem.Dispose();
        m_pEmblem = null;
        ClearScroll();
        m_pScrollRect = null;
        m_pOKButton = null;
        m_pSelectColorMark = null;

        m_pMainScene = null;

        for(int n = 0; n < m_pDragEventTrigger.triggers.Count; ++n)
        {
            m_pDragEventTrigger.triggers[n].callback.RemoveAllListeners();
            m_pDragEventTrigger.triggers[n] = null;
        }
        m_pDragEventTrigger.triggers.Clear();
        m_pDragEventTrigger= null;
        MainUI = null;
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "EditEmblem : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "EditEmblem : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;

        float w = 0;
        float wh = 0;
        RectTransform item = null;
        Vector3 pos;
        Vector2 size;
        float ax = 0;
        int iTabIndex = -1;
        
        RectTransform ui = MainUI.Find("back").GetComponent<RectTransform>();
        LayoutManager.SetReciveUIButtonEvent(ui,this.ButtonEventCall);

        ui = MainUI.Find("root/tabs").GetComponent<RectTransform>();
        LayoutManager.SetReciveUIButtonEvent(ui,this.ButtonEventCall);

        w = (ui.rect.width / ui.childCount);
        wh = w * 0.5f;
        ax = ui.pivot.x * ui.rect.width;
        int n =0;
        for(n =0; n < ui.childCount; ++n)
        {
            item = ui.GetChild(n).GetComponent<RectTransform>();
            iTabIndex = (int)((E_TAB)Enum.Parse(typeof(E_TAB), item.gameObject.name));
            m_pTabButtonList[iTabIndex] = item;

            pos = item.localPosition;
            size = item.sizeDelta;
            size.x = w;
            pos.x = wh + (n * w) - ax;
            item.localPosition = pos;
            item.sizeDelta = size;
        }

        ui = MainUI.Find("root/colorTabs").GetComponent<RectTransform>();
        LayoutManager.SetReciveUIButtonEvent(ui,this.ButtonEventCall);

        w = (ui.rect.width / ui.childCount);
        wh = w * 0.5f;
        ax = ui.pivot.x * ui.rect.width;
        for(n =0; n < ui.childCount; ++n)
        {
            item = ui.GetChild(n).GetComponent<RectTransform>();
            iTabIndex = (int)((E_COLOR_TAB)Enum.Parse(typeof(E_COLOR_TAB), item.gameObject.name));
            m_pColorTabButtonList[iTabIndex] = item;

            pos = item.localPosition;
            size = item.sizeDelta;
            size.x = w;
            pos.x = wh + (n * w) - ax;
            item.localPosition = pos;
            item.sizeDelta = size;
        }
        
        m_pDragEventTrigger = MainUI.Find("root/color").GetComponent<EventTrigger>();
        m_pColorImage = m_pDragEventTrigger.GetComponent<RawImage>();
        
        Texture2D tex = (Texture2D)m_pColorImage.mainTexture;
        size = m_pColorImage.rectTransform.rect.size;
        m_pixel.x = (int)(size.x/16);
        m_pixel.y = (int)(size.y/16);

        EventTrigger.Entry pEntry = new EventTrigger.Entry();
        pEntry.eventID = EventTriggerType.PointerUp;
        pEntry.callback.AddListener(OnPointerUp);
        m_pDragEventTrigger.triggers.Add(pEntry);

        pEntry = new EventTrigger.Entry();
        pEntry.eventID = EventTriggerType.PointerDown;
        pEntry.callback.AddListener(OnPointerDown);
        m_pDragEventTrigger.triggers.Add(pEntry);

        pEntry = new EventTrigger.Entry();
        pEntry.eventID = EventTriggerType.Drag;
        pEntry.callback.AddListener(OnDrag);
        m_pDragEventTrigger.triggers.Add(pEntry);

        m_pSelectColorMark = m_pDragEventTrigger.transform.Find("select");

        m_pEmblem = MainUI.Find("root/emblem").GetComponent<EmblemBake>();
        m_pOKButton = MainUI.Find("root/confirm").GetComponent<Button>();
        TMPro.TMP_Text text = m_pOKButton.transform.Find("count").GetComponent<TMPro.TMP_Text>();
        text.gameObject.SetActive(GameContext.getCtx().IsLoadGameData());
        m_pOKButton.transform.Find("icon").gameObject.SetActive(text.gameObject.activeSelf);
        
        m_pOKButton.transform.Find("on").gameObject.SetActive(true);
        m_pOKButton.transform.Find("off").gameObject.SetActive(false);

        int cash = GameContext.getCtx().GetConstValue(E_CONST_TYPE.editEmblemTokenCost);
        text.SetText(cash.ToString());
        
        m_pScrollRect = MainUI.Find("root/scroll").GetComponent<ScrollRect>();

        ui = MainUI.Find("root/close").GetComponent<RectTransform>();
        LayoutManager.SetReciveUIButtonEvent(ui,this.ButtonEventCall);
        ui = MainUI.Find("root/confirm").GetComponent<RectTransform>();
        LayoutManager.SetReciveUIButtonEvent(ui,this.ButtonEventCall);
        ui = MainUI.Find("root/cancel").GetComponent<RectTransform>();
        LayoutManager.SetReciveUIButtonEvent(ui,this.ButtonEventCall);

        MainUI.gameObject.SetActive(false);
    }

    void OnPointerDown(BaseEventData e)
    {
        if(e is PointerEventData data)
        {
            if(data.pointerEnter != null && data.pointerEnter == m_pDragEventTrigger.gameObject)
            {
                m_pSelectColorMark.position = data.position;
                UpdateDragColor();
                UpdateColorScroll();
                m_pEmblem.UpdateEmblemColor(m_eEmblemInfos);
            }
        }
    }

    void OnDrag(BaseEventData e)
    {
        if(e is PointerEventData data)
        {
            if(data.pointerEnter != null && data.pointerEnter == m_pDragEventTrigger.gameObject)
            {
                m_pSelectColorMark.position = data.position;
                UpdateDragColor();
                UpdateColorScroll();
                m_pEmblem.UpdateEmblemColor(m_eEmblemInfos);
            }
        }
    }

    void OnPointerUp(BaseEventData e)
    {
        if(e is PointerEventData data)
        {
            if(data.pointerEnter != null && data.pointerEnter == m_pDragEventTrigger.gameObject)
            {
                m_pSelectColorMark.position = data.position;
                
                UpdateDragColor();
                UpdateColorScroll();
                m_pEmblem.UpdateEmblemColor(m_eEmblemInfos);
                EnableConfirm(CheckEnableConfirm());
            }
        }
    }
    void SetupDragColorMark()
    {
        int colorPixel = (int)m_colors[(int)m_eColorTab];
        m_pSelectColorMark.localPosition = new Vector3((colorPixel / 16) * m_pixel.x,(colorPixel % 16) * m_pixel.y,0);

    }
    void UpdateDragColor()
    {
        Texture2D tex = (Texture2D)m_pColorImage.mainTexture;

        int x = Mathf.FloorToInt(m_pSelectColorMark.localPosition.x / m_pixel.x);
        int y = Mathf.FloorToInt(m_pSelectColorMark.localPosition.y / m_pixel.y);
        
        Color32 colorPixel = (Color32)tex.GetPixel(x, y);

        m_colors[(int)m_eColorTab] = (byte)(x * 16 + y);
        int startIndex = 3 + (int)m_eColorTab;
        m_eEmblemInfos[startIndex] = m_colors[(int)m_eColorTab];
    }


    void SetupScroll(E_TAB eTab)
    {
        ClearScroll();
        EmblemBake[] pEmblemList = null;
        RectTransform pItem = null;
        Vector2 size;
        float h = 0;

        int total = eTab == E_TAB.symbol ? 36 : 10;
        float count = total / 4f;
        int num = Mathf.FloorToInt(total / 4);
        if(count - num > 0)
        {
            ++num;
        }

        float w = 0;
        float wh = 0;
        float ax = 0;
        Vector3 pos;
        RectTransform tm = null;
        int n = 0;
        for(int i =0; i < num; ++i)
        {
            pItem = LayoutManager.Instance.GetItem<RectTransform>(SCROLL_ITEM_NAME);
                
            if(pItem)
            {
                pEmblemList = pItem.GetComponentsInChildren<EmblemBake>(true);
                
                for(n = 0; n < pEmblemList.Length; ++n)
                {
                    pEmblemList[n].transform.parent.Find("select").gameObject.SetActive(false);
                    if(total > 0)
                    {
                        pEmblemList[n].transform.parent.gameObject.SetActive(true);
                        pEmblemList[n].SetupEmblemData(m_eEmblemInfos);
                    }
                    else
                    {
                        pEmblemList[n].transform.parent.gameObject.SetActive(false);
                    }
                    
                    --total;
                    pEmblemList[n].transform.parent.gameObject.name = $"{total}"; 
                }

                pItem.SetParent(m_pScrollRect.content,false);
                pItem.localScale = Vector3.one;       
                pItem.anchoredPosition = new Vector2(0,-h);
                size = pItem.sizeDelta;
                h += size.y;
                size.x = 0;
                pItem.sizeDelta = size;

                w = (pItem.rect.width / pItem.childCount);
                wh = w * 0.5f;
                ax = pItem.pivot.x * pItem.rect.width;
                
                for(n =0; n < pItem.childCount; ++n)
                {
                    tm = pItem.GetChild(n).GetComponent<RectTransform>();
                    pos = tm.localPosition;
                    size = tm.sizeDelta;
                    size.x = w;
                    pos.x = wh + (n * w) - ax;
                    tm.localPosition = pos;
                    tm.sizeDelta = size;
                }
            }
        }

        size = m_pScrollRect.content.sizeDelta;
        size.y = h;
        m_pScrollRect.content.sizeDelta = size;

        m_pScrollRect.content.anchoredPosition = Vector2.zero; 
        LayoutManager.SetReciveUIScollViewEvent(m_pScrollRect,ScrollViewItemButtonEventCall);
    }

    void ClearScroll()
    {        
        int i = m_pScrollRect.content.childCount;
        RectTransform item = null;
        EmblemBake[] pEmblemList = null;
        while(i > 0)
        {
            --i;
            item = m_pScrollRect.content.GetChild(i).GetComponent<RectTransform>();
            pEmblemList = item.GetComponentsInChildren<EmblemBake>(true);
            for(int n =0; n < pEmblemList.Length; ++n)
            {
                pEmblemList[n].Dispose();
            }
            
            LayoutManager.Instance.AddItem(SCROLL_ITEM_NAME,item);
        }
        LayoutManager.SetReciveUIScollViewEvent(m_pScrollRect,null);
        m_pScrollRect.content.anchoredPosition = Vector2.zero;
        
        Vector2 size = m_pScrollRect.content.sizeDelta;
        size.y =0;
        m_pScrollRect.content.sizeDelta = size;
    }

    public byte[] GetEmblemInfoData()
    {
        return m_eEmblemInfos;
    }

    public void SetupEmblemInfo(byte[] pEmblemInfo)
    {
        m_eEmblemInfos = pEmblemInfo;
        m_pEmblem.SetupEmblemData(m_eEmblemInfos);
        
        m_colors[0] = m_eEmblemInfos[3];
        m_colors[1] = m_eEmblemInfos[4];
        m_colors[2] = m_eEmblemInfos[5];

        ShowTabUI(E_TAB.shape);
        ShowColorTabUI(E_COLOR_TAB.color1);
        EnableConfirm(CheckEnableConfirm());
    }

    void UpdateColorScroll()
    {
        EmblemBake[] list = m_pScrollRect.content.GetComponentsInChildren<EmblemBake>();

        for(int n =0; n < list.Length; ++n)
        {
            list[n].UpdateEmblemColor(m_eEmblemInfos);
        }
    }

    void UpdateTextureScroll()
    {
        EmblemBake[] list = m_pScrollRect.content.GetComponentsInChildren<EmblemBake>();
        Sprite pSprite = null;
        int id = 0;
        int iSelectId = m_eEmblemInfos[(int)m_eMainTab];

        bool bActive = false;
        for(int n =0; n < list.Length; ++n)
        {
            list[n].patternTexture = m_pEmblem.patternTexture;
            list[n].baseMaskTexture = m_pEmblem.baseMaskTexture;
            list[n].simbolTexture = m_pEmblem.simbolTexture;

            if (int.TryParse(list[n].transform.parent.gameObject.name, out id))
            {
                pSprite = ALF.AFPool.GetItem<Sprite>("Texture",$"{m_eMainTab}{id}");
                if(m_eMainTab == E_TAB.pattern)
                {
                    list[n].patternTexture = pSprite.texture;
                }
                else if(m_eMainTab == E_TAB.symbol)
                {
                    list[n].simbolTexture = pSprite.texture;
                }
                else
                {
                    list[n].baseMaskTexture = pSprite.texture;
                }
                list[n].Bake();
                bActive = id == iSelectId;
                list[n].transform.parent.Find("select").gameObject.SetActive(bActive);
            }
        }
    }

    void ShowColorTabUI(E_COLOR_TAB eTab)
    {
        m_eColorTab = eTab;
        for(int i = 0; i < m_pColorTabButtonList.Length; ++i)
        {
            m_pColorTabButtonList[i].Find("on").gameObject.SetActive((int)eTab == i);
            m_pColorTabButtonList[i].Find("title").GetComponent<Graphic>().color = (int)eTab == i ? Color.white : GameContext.GRAY;
        }
        SetupDragColorMark();
        UpdateColorScroll();
    }

    void ShowTabUI(E_TAB eTab)
    {
        m_eMainTab = eTab;

        for(int i = 0; i < m_pTabButtonList.Length; ++i)
        {
            m_pTabButtonList[i].Find("on").gameObject.SetActive((int)eTab == i);
            m_pTabButtonList[i].Find("title").GetComponent<Graphic>().color = (int)eTab == i ? Color.white : GameContext.GRAY;
        }

        SetupScroll(m_eMainTab);
        UpdateTextureScroll();
    }

    bool CheckEnableConfirm()
    {
        GameContext pGameContext = GameContext.getCtx();
        if(pGameContext.IsLoadGameData())
        {
            if(pGameContext.GetTotalCash() >= pGameContext.GetConstValue(E_CONST_TYPE.editEmblemTokenCost))
            {
                byte[] info = pGameContext.GetEmblemInfo();
                for(int i =0; i < info.Length; ++i)
                {
                    if(info[i] != m_eEmblemInfos[i])
                    {
                        return true;
                    }
                }
            }
        }
        else
        {
            return true;
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
    void SelectScrollItem(GameObject sender)
    {
        bool bActive = false;
        EmblemBake[] list = m_pScrollRect.content.GetComponentsInChildren<EmblemBake>();
        for(int n =0; n < list.Length; ++n)
        {
            bActive = sender == list[n].transform.parent.gameObject;
            list[n].transform.parent.Find("select").gameObject.SetActive(bActive);
        }
    }
    
    void ScrollViewItemButtonEventCall(ScrollRect root, Transform tm,GameObject sender)
    {
        int id = 0;
        if (int.TryParse(sender.name, out id))
        {
            m_eEmblemInfos[(int)m_eMainTab] = (byte)id;
            SelectScrollItem(sender);
            Sprite pSprite = ALF.AFPool.GetItem<Sprite>("Texture",$"{m_eMainTab}{id}");
            if(m_eMainTab == E_TAB.pattern)
            {
                m_pEmblem.patternTexture = pSprite.texture;
            }
            else if(m_eMainTab == E_TAB.symbol)
            {
                m_pEmblem.simbolTexture = pSprite.texture;
            }
            else
            {
                m_pEmblem.baseMaskTexture = pSprite.texture;
            }
            m_pEmblem.Bake();
            EnableConfirm(CheckEnableConfirm());
        }
    }

    public void Close()
    {
        SingleFunc.HideAnimationDailog(MainUI,()=>{
            LayoutManager.Instance.InteractableEnabledAll();
            ClearScroll();
            MainUI.gameObject.SetActive(false);
            MainUI.Find("root").localScale = Vector3.one;
        });
    }

    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        if (root.gameObject.name == "tabs")
        {
            ShowTabUI((E_TAB)Enum.Parse(typeof(E_TAB), sender.name));
            return;
        }
        else if (root.gameObject.name == "colorTabs")
        {
            ShowColorTabUI((E_COLOR_TAB)Enum.Parse(typeof(E_COLOR_TAB), sender.name));
            return;
        }
        else if (root.gameObject.name == "confirm")
        {
            GameContext pGameContext = GameContext.getCtx();
            if(pGameContext.IsLoadGameData())
            {
                m_pMainScene.ShowConfirmPopup(pGameContext.GetLocalizingText("DIALOG_EMBLEMEDIT_TITLE"),string.Format(pGameContext.GetLocalizingText("DIALOG_EMBLEMEDIT_TXT"),pGameContext.GetConstValue(E_CONST_TYPE.editEmblemTokenCost)),pGameContext.GetLocalizingText("DIALOG_BTN_CONFIRM"),pGameContext.GetLocalizingText("DIALOG_BTN_CALCEL"),false,() =>{
                    JObject pJObject = new JObject();
                    pJObject["emblem"] = Newtonsoft.Json.JsonConvert.SerializeObject(m_eEmblemInfos.ToList(), Newtonsoft.Json.Formatting.None,new Newtonsoft.Json.JsonSerializerSettings { NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore});
                    m_pMainScene.RequestAfterCall(E_REQUEST_ID.club_changeEmblem,pJObject,Close);
                } );
                return;
            }
            else
            {
                pGameContext.SetEmblem(m_eEmblemInfos);
                if(m_pMainScene.IsShowInstance<CreateClub>())
                {
                    m_pMainScene.GetInstance<CreateClub>().UpdateEmblemInfos();
                }
            }
        }
        
        Close();
    }
}
