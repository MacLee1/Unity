using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ALF;

// using ALF.STATE;
// using ALF.CONDITION;
using ALF.LAYOUT;
// using ALF.MACHINE;
using ALF.SOUND;
// using DATA;
// using UnityEngine.EventSystems;
using STATEDATA;


public class SelectFormation : IBaseUI
{
    const string SCROLL_ITEM_NAME = "FormatiomItem";
    MainScene m_pMainScene = null;
    ScrollRect m_pScrollRect = null;
    public RectTransform MainUI { get; private set;}
    public bool Enable {set{if (m_pScrollRect != null) m_pScrollRect.enabled = value;}}
    public SelectFormation(){}
    
    public void Dispose()
    {
        m_pMainScene = null;
        MainUI = null;
        if(m_pScrollRect != null)
        {
            ClearScroll();
        }
        m_pScrollRect = null;
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "SelectFormation : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "SelectFormation : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;
        MainUI.gameObject.SetActive(false);
        m_pScrollRect = MainUI.GetComponentInChildren<ScrollRect>(true);
        LayoutManager.SetReciveUIButtonEvent(MainUI,this.ButtonEventCall);
        SetupScroll(TacticsFormation.formation_preset,TacticsFormation.formation_preset_names);
    }

    public void SetupData()
    {
        m_pScrollRect.content.anchoredPosition = Vector2.zero;
    }

    void SetupScroll(List<byte[]> list,string[] namse)
    {
        TMPro.TMP_Text text = null;
        RectTransform pItem = null;
        Vector2 size;
        float h = 0;
        int total = 0;
        int n =0;
        Transform tm = null;
        for(int i =0; i < list.Count;)
        {
            i +=2;
            pItem = LayoutManager.Instance.GetItem<RectTransform>(SCROLL_ITEM_NAME);
            
            if(pItem)
            {
                pItem.gameObject.name = $"{SCROLL_ITEM_NAME}_{total}";
                tm = pItem.Find("formation_l");
                tm.gameObject.SetActive(true);
                
                text = tm.Find("text").GetComponent<TMPro.TMP_Text>();
                text.SetText(GameContext.getCtx().GetLocalizingText(namse[i - 2]));
                for(n = 1; n < 26; ++n)
                {
                    tm.Find(n.ToString()).gameObject.SetActive(false);
                }
                
                for(n =1; n < list[i -2].Length; ++n)
                {
                    tm.Find((list[i -2][n]).ToString()).gameObject.SetActive(true);
                }

                tm = pItem.Find("formation_r");
                tm.gameObject.SetActive(!(i > list.Count));

                if(tm.gameObject)
                {
                    text = tm.Find("text").GetComponent<TMPro.TMP_Text>();
                    text.SetText(GameContext.getCtx().GetLocalizingText(namse[i - 1]));
                    for(n = 1; n < 26; ++n)
                    {
                        tm.Find(n.ToString()).gameObject.SetActive(false);
                    }

                    for(n =1; n < list[i -1].Length; ++n)
                    {
                        tm.Find((list[i -1][n]).ToString()).gameObject.SetActive(true);
                    }
                }

                pItem.SetParent(m_pScrollRect.content,false);
                
                pItem.localScale = Vector3.one;       
                pItem.anchoredPosition = new Vector2(0,-h);
                size = pItem.sizeDelta;
                h += size.y;
            }
            ++total;
        }

        size = m_pScrollRect.content.sizeDelta;
        size.y = h;
        m_pScrollRect.content.sizeDelta = size;
        LayoutManager.SetReciveUIScollViewEvent(m_pScrollRect,ScrollViewItemButtonEventCall);
    }

    void ClearScroll()
    {
        LayoutManager.SetReciveUIScollViewEvent(m_pScrollRect,null);
        int i = m_pScrollRect.content.childCount;
        while(i > 0)
        {
            --i;
            LayoutManager.Instance.AddItem(SCROLL_ITEM_NAME,m_pScrollRect.content.GetChild(i).GetComponent<RectTransform>());
        }
        m_pScrollRect.content.anchoredPosition = Vector2.zero;
        Vector2 size = m_pScrollRect.content.sizeDelta;
        size.y =0;
        m_pScrollRect.content.sizeDelta = size;
    }

    void ScrollViewItemButtonEventCall(ScrollRect root, Transform tm,GameObject sender)
    {
        for(int i =0; i < root.content.childCount; ++i)
        {
            if(tm == root.content.GetChild(i))
            {
                i = i *2 + (sender.name == "formation_l" ? 0 : 1);
                m_pMainScene.UpdateFormationFromPresetData(GameContext.getCtx().GetActiveTacticsType(),i);
                ButtonEventCall(null,null);
                return;
            }
        }
    }
    public void Close()
    {
        Enable = false;
        SingleFunc.HideAnimationDailog(MainUI);
    }
    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        Close();
    }
    
}
