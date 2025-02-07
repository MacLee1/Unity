using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;
using ALF;
// using ALF.STATE;
// using ALF.CONDITION;
using ALF.LAYOUT;
// using ALF.MACHINE;
using ALF.SOUND;
using DATA;
using USERDATA;
// using UnityEngine.EventSystems;
// using STATEDATA;


public class HPRecovery : IBaseUI
{
    // Start is called before the first frame update
    MainScene m_pMainScene = null;
    TMPro.TMP_Text m_pItemText = null;
    TMPro.TMP_Text m_pMessageText = null;
    Button m_pConfirm = null;

    public RectTransform MainUI { get; private set;}
    public HPRecovery(){}
    
    public void Dispose()
    {
        m_pMainScene = null;
        MainUI = null;
        m_pItemText = null;
        m_pMessageText = null;
        m_pConfirm = null;
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "HPRecovery : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "HPRecovery : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;

        m_pItemText = MainUI.Find("root/confirm/text").GetComponent<TMPro.TMP_Text>();
        m_pMessageText = MainUI.Find("root/bg/message").GetComponent<TMPro.TMP_Text>();
        m_pConfirm = MainUI.Find("root/confirm").GetComponent<Button>();
        MainUI.gameObject.SetActive(false);
        LayoutManager.SetReciveUIButtonEvent(MainUI,this.ButtonEventCall);
    }

    public void SetupData()
    {
        uint count = 0;
        if(GetData(ref count) == null )
        {
            m_pConfirm.enabled = false;
            m_pConfirm.transform.Find("on").gameObject.SetActive(m_pConfirm.enabled);
            m_pConfirm.transform.Find("off").gameObject.SetActive(!m_pConfirm.enabled);
            m_pMainScene.ShowToastMessage(GameContext.getCtx().GetLocalizingText("ALERT_TXT_LINEUP_HP_RECOVERED"),null);
            Close();
        }
    }

    ulong[] GetData(ref uint count)
    {
        GameContext pGameContext = GameContext.getCtx();
        ulong[] pPlayerIDs = pGameContext.GetRecoverHpPlayerIDs();

        if(pPlayerIDs != null)
        {
            count = pGameContext.GetUseRecoverHPItemCount(pPlayerIDs);
            ulong iCount = pGameContext.GetItemCountByNO(GameContext.STAMINA_DRINK);
            m_pMessageText.SetText(string.Format(pGameContext.GetLocalizingText("DIALOG_LINEUPHPRECOVERY_TXT"),ALFUtils.NumberToString(iCount)));
            m_pItemText.SetText(ALFUtils.NumberToString(count));
            m_pConfirm.enabled = iCount >= count;
            m_pConfirm.transform.Find("on").gameObject.SetActive(m_pConfirm.enabled);
            m_pConfirm.transform.Find("off").gameObject.SetActive(!m_pConfirm.enabled);
            return pPlayerIDs;
        }

        return null;
    }

    public void Close()
    {
        SingleFunc.HideAnimationDailog(MainUI);
    }

    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        if(sender.name == "confirm" )
        {
            uint count = 0;
            ulong[] list = GetData(ref count);
            if(count > 0 )
            {
                JObject pJObject = new JObject();
                JArray pJArray = new JArray();
                for(int i =0; i < list.Length; ++i)
                {
                    pJArray.Add(list[i]);
                }
                pJObject["ids"] = pJArray;
                pJObject["amount"] = count;
                m_pMainScene.RequestAfterCall(E_REQUEST_ID.player_recoverHP,pJObject,Close);
                return;
            }
            else
            {
                m_pMainScene.ShowToastMessage(GameContext.getCtx().GetLocalizingText("ALERT_TXT_LINEUP_HP_RECOVERED"),null);
            }
        }
        Close();
    }    
}
