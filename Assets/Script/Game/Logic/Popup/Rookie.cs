using UnityEngine;
using ALF;
using ALF.LAYOUT;

public class Rookie : IBaseUI
{
    MainScene m_pMainScene = null;
    public RectTransform MainUI { get; private set;}
    
    public Rookie(){}
    
    public void Dispose()
    {
        m_pMainScene = null;
        MainUI = null;
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {   
        ALFUtils.Assert(pBaseScene != null, "Rookie : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "Rookie : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;

        LayoutManager.SetReciveUIButtonEvent(MainUI,ButtonEventCall);
        MainUI.gameObject.SetActive(false);
    }

    // public RectTransform GetTutorailUI()
    // {
    //     return MainUI.Find("root").GetComponent<RectTransform>();
    // }

    public void Close()
    {
        SingleFunc.HideAnimationDailog(MainUI);
    }

    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        Close();
    }
}
