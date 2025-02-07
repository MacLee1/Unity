using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ALF;
using ALF.NETWORK;
using ALF.LAYOUT;
using ALF.SOUND;
using USERDATA;
using DATA;
using ITEM;

public class ItemTip : IBaseUI
{
    const string TOKEN = "<size=32> <color=yellow>{0}</color></size>\n{1}";
    MainScene m_pMainScene = null;
    TMPro.TMP_Text m_pMessageText = null;
    RectTransform m_pMessageRect = null;
    RectTransform m_pMessageAnchor = null;
    Rect m_ScreenRect;

    public RectTransform MainUI { get; private set;}

    
    public ItemTip(){}
    
    public void Dispose()
    {
        m_pMessageText = null;
        m_pMessageRect = null;
        m_pMessageAnchor = null;
        MainUI = null;
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "ItemTip : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "ItemTip : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;
        m_ScreenRect = MainUI.rect;
        m_ScreenRect.position += new Vector2(40,40);
        m_ScreenRect.size -= new Vector2(80,80);

        m_pMessageRect = MainUI.Find("root").GetComponent<RectTransform>();
        m_pMessageAnchor = m_pMessageRect.Find("ac").GetComponent<RectTransform>();
        m_pMessageText = MainUI.Find("root/text").GetComponent<TMPro.TMP_Text>();
        LayoutManager.SetReciveUIButtonEvent(MainUI,ButtonEventCall);
        MainUI.gameObject.SetActive(false);
    }

    public void SetupData(RectTransform tm, uint id)
    {
        if(tm != null)
        {
            Vector2 pos = (Vector2)MainUI.InverseTransformPoint( tm.position );
            pos.x += (0.5f - tm.pivot.x) * tm.rect.width;
            m_pMessageRect.anchoredPosition = pos;

            GameContext pGameContext = GameContext.getCtx();
            ItemList pItemList = pGameContext.GetFlatBufferData<ItemList>(E_DATA_TYPE.ItemData);
            Item? pItem = pItemList.ItemByKey(id);
            if(pItem != null)
            {
                m_pMessageText.SetText(string.Format(TOKEN,pGameContext.GetLocalizingText(pItem.Value.Name),pGameContext.GetLocalizingText(pItem.Value.Description)));

                Vector2 size = m_pMessageRect.sizeDelta;
                size.y = m_pMessageText.preferredHeight + 30;
                size.x = m_pMessageText.preferredWidth + 40;
                m_pMessageRect.sizeDelta = size;
                m_pMessageAnchor.anchorMax = new Vector2(0.5f,0);
                m_pMessageAnchor.localScale  = new Vector3(1,-1,1);

                float y = (tm.rect.height + size.y )* 0.5f + 20;
                if(pos.y > 0)
                {
                    float yl = (pos.y - (tm.rect.yMin - 20) + m_ScreenRect.yMin) * -1;
                    if(yl < size.y)
                    {
                        y = (tm.rect.height + size.y )* -0.5f - 20;
                        m_pMessageAnchor.anchorMax = new Vector2(0.5f,1);
                        m_pMessageAnchor.localScale  = new Vector3(1,1,1);
                    }
                }
                
                m_pMessageAnchor.anchorMin = m_pMessageAnchor.anchorMax;
                m_pMessageAnchor.anchoredPosition = Vector2.zero;
                pos.y += y;

                float xl = pos.x + m_pMessageRect.rect.xMin;
                float xr = pos.x + m_pMessageRect.rect.xMax;
                if(xl < m_ScreenRect.xMin)
                {
                    float xw = m_ScreenRect.xMin - xl;
                    pos.x = pos.x + xw;
                    Vector2 p = m_pMessageAnchor.anchoredPosition;
                    p.x = -xw;
                    m_pMessageAnchor.anchoredPosition = p;
                }
                
                if(xr > m_ScreenRect.xMax)
                {
                    float xw = m_ScreenRect.xMax - xr;
                    pos.x = pos.x + xw;
                    Vector2 p = m_pMessageAnchor.anchoredPosition;
                    p.x = -xw;
                    m_pMessageAnchor.anchoredPosition = p;
                }
                m_pMessageRect.anchoredPosition = pos;
                
            }
        }
    }


    public void Close()
    {
        SingleFunc.HideAnimationDailog(MainUI);
    }

    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        Close();
    }
}
