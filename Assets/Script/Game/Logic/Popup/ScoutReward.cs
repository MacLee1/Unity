using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ALF;

using ALF.STATE;
using ALF.CONDITION;
using ALF.LAYOUT;
using ALF.MACHINE;
using ALF.SOUND;
using DATA;
using USERDATA;
// using UnityEngine.EventSystems;
using STATEDATA;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;

public class ScoutReward : IBaseUI
{
    // Start is called before the first frame update
    MainScene m_pMainScene = null;
    Animation m_pScoutResult = null;
    ulong m_iPlayerID = 0;
    Transform m_pSkipButton = null;
    public RectTransform MainUI { get; private set;}

    public ScoutReward(){}
    
    public void Dispose()
    {
        m_pScoutResult.Stop();
        m_pMainScene = null;
        m_pSkipButton = null;
        MainUI = null;
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "ScoutReward : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "ScoutReward : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;
        m_pScoutResult = MainUI.Find("root/scoutResult").GetComponent<Animation>();
        m_pSkipButton = m_pScoutResult.transform.Find("skip");
        MainUI.gameObject.SetActive(false);
        LayoutManager.SetReciveUIButtonEvent(MainUI,this.ButtonEventCall);
    }

    void Clear()
    {
        Transform tm = m_pScoutResult.transform.Find("root/player");
        SingleFunc.ClearPlayerCard(tm);
    }

    void UpdateScoutResult(ulong id)
    {
        m_iPlayerID = id;
        GameContext pGameContext = GameContext.getCtx();
        PlayerT pPlayer = pGameContext.GetPlayerByID(m_iPlayerID);
        Transform tm = m_pScoutResult.transform.Find("root/player");
        SingleFunc.SetupPlayerCard(pPlayer,tm,E_ALIGN.Left);
        
        RectTransform tier = m_pScoutResult.transform.Find("root/tier").GetComponent<RectTransform>();
        SingleFunc.SetupQuality(pPlayer,tier,true);

        int n =0;
        Color color = Color.white;
        color.a = 0;
        for(n =0 ; n < tier.childCount; ++n)
        {
            tm = tier.GetChild(n);
            if(tm.gameObject.activeSelf)
            {
                if(tm.Find("h") != null)
                {
                    tm.Find("h").GetComponent<Graphic>().color = color;
                }
                if(tm.Find("on") != null)
                {
                    tm.Find("on").GetComponent<Graphic>().color = color;
                }
            }
        }

        tm = m_pScoutResult.transform.Find("root/name/nation");
        
        RawImage icon = tm.GetComponent<RawImage>();
        Sprite pSprite = AFPool.GetItem<Sprite>("Texture",pPlayer.Nation);
        icon.texture = pSprite.texture;
        TMPro.TMP_Text text = m_pScoutResult.transform.Find("root/name/text").GetComponent<TMPro.TMP_Text>();
        text.SetText($"{pPlayer.Forename} {pPlayer.Surname}");

        text = m_pScoutResult.transform.Find("root/age/text").GetComponent<TMPro.TMP_Text>();
        byte age = pGameContext.GetPlayerAge(pPlayer);
        text.SetText(age > 40 ? "40+":age.ToString());

        text = m_pScoutResult.transform.Find("root/value/text").GetComponent<TMPro.TMP_Text>();
        text.SetText(ALFUtils.NumberToString(pPlayer.Price));

        tm = m_pScoutResult.transform.Find("root/roles/roles");
        
        
        for(n = 0; n < tm.childCount; ++n)
        {
            tm.GetChild(n).gameObject.SetActive(false);
        }
        
        List<string> locList= new List<string>();
        
        for(n = 0; n < pPlayer.PositionFamiliars.Count; ++n)
        {
            if(pPlayer.PositionFamiliars[n] >= 80)
            {
                locList.Add(pGameContext.GetDisplayLocationName(n));
            }
        }
        Transform item = null;
        for(n = 0; n < locList.Count; ++n)
        {
            if(tm.childCount > n)
            {
                item = tm.GetChild(n);
                icon = item.GetComponent<RawImage>();
                icon.gameObject.SetActive(true);
                pSprite = AFPool.GetItem<Sprite>("Texture",pGameContext.GetDisplayCardFormationByLocationName(locList[n]));
                icon.texture = pSprite.texture;
                text = icon.transform.Find("text").GetComponent<TMPro.TMP_Text>();
                text.SetText(locList[n]);
            }
        }
    }

    public void SetupScoutData(JObject data)
    {
        if(data.ContainsKey("players"))
        {            
            JArray pArray = (JArray)data["players"];
            JObject item = null;
            
            for(int i= 0; i < pArray.Count; ++i)
            {
                item = (JObject)pArray[i];
                UpdateScoutResult((ulong)item["id"]);
            }
        }
        m_pScoutResult.Play();
        m_pScoutResult.gameObject.SetActive(false);
    }

    bool executeChangeStarCallback(IState state,float dt,bool bEnd)
    {
        if( state.StateData is ChangeStarStateData data)
        {
            if(!data.CurrentAnimation.IsPlaying(data.CurrentAnimation.clip.name))
            {
                if(data.AnimationList.Count == 0)
                {
                    return true;
                }

                data.CurrentAnimation = data.AnimationList[0];
                data.CurrentAnimation.gameObject.SetActive(true);
                data.AnimationList.RemoveAt(0);
                
                if(data.AnimationList.Count == 0)
                {
                    data.CurrentAnimation.Play("scout_end");
                    ShowActiveStar();
                }
                else
                {
                    if(data.CurrentAnimation != m_pScoutResult)
                    {
                        if(data.CurrentAnimation["star"].speed == 2f)
                        {
                            SoundManager.Instance.PlaySFX("sfx_scout_star", 0.5f);
                        }
                        else
                        {
                            SoundManager.Instance.PlaySFX("sfx_scout_star2", 0.8f);
                        }
                    }
                    data.CurrentAnimation.Play();
                }
            }
        }

        return bEnd;
    }

    public void Play()
    {
        m_pScoutResult.gameObject.SetActive(true);
        m_pScoutResult.Play();

        ChangeStarStateData pChangeStarStateData = new ChangeStarStateData();
        Animation[] list = m_pScoutResult.transform.Find("root/tier").GetComponentsInChildren<Animation>(true);
        int count = 0;
        for(int n =0; n < list.Length; ++n)
        {
            if(list[n].gameObject.activeSelf)
            {
                ++count;
                list[n]["star"].speed = count > 3 ? 1f - (count - 3) * 0.1f : 2f;
                pChangeStarStateData.AnimationList.Add(list[n]);
                list[n].gameObject.SetActive(false);
            }
        }

        BaseState pBaseState = BaseState.GetInstance(new BaseStateTarget(m_pScoutResult),-1, (uint)E_STATE_TYPE.Timer, null, executeChangeStarCallback);
        
        pChangeStarStateData.CurrentAnimation = m_pScoutResult;
        pChangeStarStateData.AnimationList.Add(m_pScoutResult);
        pBaseState.StateData = pChangeStarStateData;
        StateMachine.GetStateMachine().AddState(pBaseState);
    }
    
    void Skip()
    {
        List<BaseState> list = StateMachine.GetStateMachine().GetCurrentTargetStates<BaseState>(m_pScoutResult);
        if(list.Count > 0)
        {
            if(list[0].StateData is ChangeStarStateData data)
            {
                int i = data.AnimationList.Count;
                while(i > 0)
                {
                    --i;
                    data.AnimationList[i].gameObject.SetActive(true);
                    data.AnimationList.RemoveAt(i);
                }
                
                Animation[] pAnimation = m_pScoutResult.transform.Find("root/tier").GetComponentsInChildren<Animation>(true);
                for(int n =0; n < pAnimation.Length; ++n)
                {
                    pAnimation[n]["star"].time = pAnimation[n]["star"].length;
                }
            }
        }
        
        if(m_pScoutResult.IsPlaying("scout_start"))
        {
            m_pScoutResult["scout_start"].time = m_pScoutResult["scout_start"].length;
        }
        
        if( !m_pScoutResult.IsPlaying("scout_end"))
        {
            m_pScoutResult.Play("scout_end");
        }
        m_pScoutResult["scout_end"].time = m_pScoutResult["scout_end"].length;
        ShowActiveStar();
        m_pSkipButton.gameObject.SetActive(false);
    }
    
    void ShowActiveStar()
    {
        Animation[] animationList = m_pScoutResult.transform.Find("root/tier").GetComponentsInChildren<Animation>(true);
                    
        for(int n =0; n < animationList.Length; ++n)
        {
            if(animationList[n].gameObject.activeSelf)
            {
                Graphic[] graphicList = animationList[n].GetComponentsInChildren<Graphic>(true);
                for(int i =0; i < graphicList.Length; ++i)
                {
                    graphicList[i].color = Color.white;
                }
            }
        }
    }

    public void Close()
    {
        List<BaseState> list = StateMachine.GetStateMachine().GetCurrentTargetStates<BaseState>(m_pScoutResult);
        if(list.Count > 0)
        {
            Skip();
            return;
        }
        
        SingleFunc.HideAnimationDailog(MainUI,()=>{Clear(); MainUI.gameObject.SetActive(false);MainUI.Find("root").localScale = Vector3.one; LayoutManager.Instance.InteractableEnabledAll();});
    }

    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        if(sender.name == "skip")
        {
            Skip();
            return;
        }
        else if(sender.name == "info")
        {
            GameContext pGameContext = GameContext.getCtx();
            PlayerT pPlayer = pGameContext.GetPlayerByID(m_iPlayerID);
            if(pPlayer != null)
            {
                PlayerInfo pPlayerInfo = m_pMainScene.ShowPlayerInfoPopup();
                pPlayerInfo.SetupPlayerInfoData(E_PLAYER_INFO_TYPE.my,pPlayer);
                pPlayerInfo.SetupQuickPlayerInfoData(pGameContext.GetTotalPlayerList());
                pPlayerInfo.ShowTabUI(E_PLAYER_INFO_TAB.information);
            }

            return;
        }
        
        Close();
    }
    
}
