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

using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets.ResourceLocators;


public class Download : IBaseUI
{
    // Start is called before the first frame update
    IntroScene m_pIntroScene = null;
    TMPro.TMP_Text m_pMessage = null;
    TMPro.TMP_Text m_pGaugeText = null;
    Image m_pGauge = null;

    GameObject m_pDownloadButton = null;
    GameObject m_pOKButton = null;
    
    System.Action m_pActionOK = null;
    
    IEnumerable m_pDownloadList = null;
    float m_fTotalDownloadSize = 0;
    
    public RectTransform MainUI { get; private set;}
    public Download(){}

    public void Dispose()
    {
        m_pIntroScene = null;
        MainUI = null;
        m_pMessage = null;
        m_pGaugeText = null;
        m_pActionOK = null;
        m_pOKButton = null;
        m_pDownloadButton = null;
        m_pGauge = null;
        m_pDownloadList = null;
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pMainUI != null, "Download : targetUI is null!!");

        m_pIntroScene = (IntroScene)pBaseScene;
        MainUI = pMainUI;

        m_pMessage = MainUI.Find("root/bg/message").GetComponent<TMPro.TMP_Text>();
        m_pGauge  = MainUI.Find("root/bg/gauge/fill").GetComponent<Image>();
        m_pDownloadButton = MainUI.Find("root/bg/confirm").gameObject;
        m_pOKButton = MainUI.Find("root/bg/ok").gameObject;
        m_pGaugeText = m_pGauge.transform.parent.Find("message").GetComponent<TMPro.TMP_Text>();
        m_pGauge.transform.parent.gameObject.SetActive(false);
        MainUI.gameObject.SetActive(false);
        LayoutManager.SetReciveUIButtonEvent(MainUI,this.ButtonEventCall);
    }
    public void SetData(long iDownloadSize,IEnumerable list,System.Action pActionOK)
    {
        m_pDownloadButton.SetActive(true);
        m_pOKButton.SetActive(false);
        m_fTotalDownloadSize = iDownloadSize/1024.0f/1024.0f;
        m_pMessage.SetText(string.Format(GameContext.getCtx().GetLocalizingText("DIALOG_RESOURCEDOWNLOAD_TXT"),string.Format("{0:0.0#} MB",m_fTotalDownloadSize)));
        m_pActionOK = pActionOK;
        m_pDownloadList = list;
    }
    
    IEnumerator BundleUpdate() 
    {
        AsyncOperationHandle pAsyncOperationHandle = Addressables.DownloadDependenciesAsync(m_pDownloadList,Addressables.MergeMode.Union);
        pAsyncOperationHandle.Completed += op =>
        {
            GameContext pGameContext = GameContext.getCtx();
            if(op.Status == AsyncOperationStatus.Failed)
            {
                m_pGaugeText.SetText(pAsyncOperationHandle.OperationException.Message);
                m_pIntroScene.ShowConfirmPopup(pGameContext.GetLocalizingText("MSG_TXT_TRY_AGAIN"),pGameContext.GetLocalizingText("MSG_TXT_RESOURCE_DOWNLOAD_FAIL"),pGameContext.GetLocalizingText("DIALOG_BTN_CONFIRM"),null,false,()=>{
                    m_pGauge.fillAmount = 0;
                    m_pGaugeText.SetText(string.Format("0.0/{0:0.0#} MB",m_fTotalDownloadSize));
                    Director.StartCoroutine(BundleUpdate());
                });
            }
            else
            {
                m_pMessage.SetText(string.Format(pGameContext.GetLocalizingText("MSG_TXT_RESOURCE_DOWNLOAD_SUCCESS"),string.Format("{0:0.0#} MB",m_fTotalDownloadSize)));
                m_pGauge.fillAmount = 1;
                m_pGaugeText.SetText(string.Format("{0:0.0#}/{1:0.0#} MB",m_fTotalDownloadSize,m_fTotalDownloadSize));
                m_pOKButton.SetActive(true);
            }
            //다운로드가 끝나면 메모리 해제.
            Addressables.Release(op);
        };

        while(!pAsyncOperationHandle.IsDone)
        {
            m_pGauge.fillAmount = pAsyncOperationHandle.PercentComplete;
            m_pGaugeText.SetText(string.Format("{0:0.0#}/{1:0.0#} MB",m_fTotalDownloadSize * pAsyncOperationHandle.PercentComplete,m_fTotalDownloadSize));
            yield return null;
        }
    }


    public void Close()
    {
        if(!m_pGauge.transform.parent.gameObject.activeSelf)
        {
#if UNINTY_ANDROID
            Application.Quit();
#endif
        }
        
        m_pActionOK = null;
        m_pDownloadList = null;
        SingleFunc.HideAnimationDailog(MainUI);
    }
    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        if(sender.name == "confirm" )
        {
            m_pGauge.transform.parent.gameObject.SetActive(true);
            m_pGauge.fillAmount = 0;
            m_pGaugeText.SetText("0.0%");
            m_pDownloadButton.SetActive(false);
            GameContext.getCtx().SendAdjustEvent("tmizah",true,false,-1);
            Director.StartCoroutine(BundleUpdate());
            return;    
        }

        if(sender.name == "ok" )
        {
            GameContext.getCtx().BundleUpdate(m_pActionOK);
            m_pActionOK = null;
        }

        Close();
    }
    
}
