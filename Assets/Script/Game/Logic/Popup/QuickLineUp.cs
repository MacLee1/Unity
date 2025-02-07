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
using CONSTVALUE;
public class QuickLineUp : IBaseUI
{
    E_LOCATION m_eSelectLoction = E_LOCATION.LOC_END;
    MainScene m_pMainScene = null;
    PlayerT m_pSelectPlayerData = null;
    int m_iLineupPlayerIndex = -1;
    TMPro.TMP_Text m_pNotice = null;
    public RectTransform MainUI { get; private set;}

    public QuickLineUp(){}
    
    public void Dispose()
    {
        m_pNotice = null;
        m_pMainScene = null;
        m_pSelectPlayerData = null;
        MainUI = null;
    }

    public void OnInit(IBaseScene pBaseScene,RectTransform pMainUI)
    {
        ALFUtils.Assert(pBaseScene != null, "QuickLineUp : rootScene is null!!");
        ALFUtils.Assert(pMainUI != null, "QuickLineUp : targetUI is null!!");

        m_pMainScene = (MainScene)pBaseScene;
        MainUI = pMainUI;
        m_pNotice = MainUI.Find("root/noti/text").GetComponent<TMPro.TMP_Text>();
        LayoutManager.SetReciveUIButtonEvent(MainUI,this.ButtonEventCall);
        MainUI.gameObject.SetActive(false);   
    }

    public void SetupPlayerInfoData(PlayerT pPlayerData)
    {
        GameContext pGameContext = GameContext.getCtx();
        m_pSelectPlayerData = pPlayerData;
        m_eSelectLoction = (E_LOCATION)m_pSelectPlayerData.Position;
        Transform tm = MainUI.Find("root/select");
        UpdatePlayerInfo(tm,m_pSelectPlayerData,false,E_LOCATION.LOC_END);
        
        m_pNotice.SetText(pGameContext.GetLocalizingText(m_pSelectPlayerData.Position == (byte)E_LOCATION.LOC_GK ? "SQUADQUICKSUBSTITUTION_TXT_TIP_GOALKEEPER" : "SQUADQUICKSUBSTITUTION_TXT_TIP_FIELDPLAYER"));
        int i =0;
        m_iLineupPlayerIndex = -1;

        List<ulong> lineup = pGameContext.GetLineupPlayerIdListByIndex(pGameContext.GetActiveLineUpType());
        for(i =0; i < lineup.Count; ++i)
        {
            if(lineup[i] == m_pSelectPlayerData.Id)
            {
                m_iLineupPlayerIndex = i;
                break;
            }
        }
        
        bool bActive = (E_LOCATION)m_pSelectPlayerData.Position != E_LOCATION.LOC_GK;
        byte[] formations = pGameContext.GetFormationArrayByIndex(pGameContext.GetActiveTacticsType());
        
        tm = MainUI.Find("root/lineup");
        for(i =0; i < lineup.Count; ++i)
        {
            if(i > 10)
            {
                UpdatePlayerInfo(tm.GetChild(i),pGameContext.GetPlayerByID(lineup[i]),!bActive ? bActive : lineup[i] != m_pSelectPlayerData.Id,E_LOCATION.LOC_END);
            }
            else
            {
                UpdatePlayerInfo(tm.GetChild(i),pGameContext.GetPlayerByID(lineup[i]),!bActive ? bActive : lineup[i] != m_pSelectPlayerData.Id,(E_LOCATION)formations[i]);
            }
        }
        
        for(i =11; i < lineup.Count; ++i)
        {
            if(!bActive)
            {
                if((E_LOCATION)pGameContext.GetPlayerByID(lineup[i]).Position == E_LOCATION.LOC_GK)
                {
                    tm.GetChild(i).GetComponent<Button>().enabled = true;
                }
            }
            else
            {
                if((E_LOCATION)pGameContext.GetPlayerByID(lineup[i]).Position == E_LOCATION.LOC_GK)
                {
                    tm.GetChild(i).GetComponent<Button>().enabled = false;
                }
            }
        }

        if(!bActive)
        {
            if(lineup[0] != m_pSelectPlayerData.Id)
            {
                Button pButton = tm.GetChild(0).GetComponent<Button>();
                pButton.enabled = true;
                pButton.transform.Find("off").gameObject.SetActive(false);
                pButton.transform.Find("on").gameObject.SetActive(true);
            }
        }
        else
        {
            Button pButton = tm.GetChild(0).GetComponent<Button>();
            pButton.enabled = false;
            pButton.transform.Find("off").gameObject.SetActive(true);
            pButton.transform.Find("on").gameObject.SetActive(false);
        }
    }

    public void UpdatePlayerHP()
    {
        UpdateHP(MainUI.Find("root/select/hp/fill").GetComponent<Image>(),m_pSelectPlayerData);
        GameContext pGameContext = GameContext.getCtx();
        List<ulong> lineup = pGameContext.GetLineupPlayerIdListByIndex(pGameContext.GetActiveLineUpType());
        Transform tm = MainUI.Find("root/lineup");
        for(int i =0; i < lineup.Count; ++i)
        {
            UpdateHP(tm.GetChild(i).Find("hp/fill").GetComponent<Image>(),pGameContext.GetPlayerByID(lineup[i]));
        }
    }
    
    void UpdateHP(Image tm,PlayerT pPlayer)
    {
        float hp = (float)pPlayer.Hp / 100f;
        tm.fillAmount = hp;

        if(hp < 0.5f)
        {
            tm.color = GameContext.HP_L;
        }
        else if(hp < 0.7f)
        {
            tm.color = GameContext.HP_LH;
        }
        else if(hp < 0.9f)
        {
            tm.color = GameContext.HP_H;
        }
        else
        {
            tm.color = GameContext.HP_F;
        }

        tm.transform.Find("bar").gameObject.SetActive(hp > 0);
    }

    void UpdatePlayerInfo(Transform tm,PlayerT pPlayer,bool bActive,E_LOCATION eLoc)
    {
        GameContext pGameContext = GameContext.getCtx();
        int index = pGameContext.GetActiveLineUpType();
        int isPlay = pGameContext.IsLineupPlayer(index,pPlayer.Id);
        Button pButton = tm.GetComponent<Button>();
        TMPro.TMP_Text pText = tm.Find("form").GetComponent<TMPro.TMP_Text>();

        if(pButton != null)
        {
            pButton.enabled = bActive;
            tm.gameObject.name = pPlayer.Id.ToString();
            
            tm.Find("off").gameObject.SetActive(!bActive);
            tm.Find("on").gameObject.SetActive(bActive);
            if(isPlay == 2)
            {
                Transform item = tm.Find("on");
                for(int i = 0; i < item.childCount; ++i)
                {
                    item.GetChild(i).gameObject.SetActive(false);
                }
                
                item.Find(pGameContext.GetDisplayCardFormationByLocationName(pGameContext.GetDisplayLocationName(pPlayer.Position))).gameObject.SetActive(true);
            }
        }
        else
        {
            Sprite pSprite = null;

            if(isPlay == 2)
            {
                pSprite = AFPool.GetItem<Sprite>("Texture","S_"+pGameContext.GetDisplayCardFormationByLocationName(pGameContext.GetDisplayLocationName(pPlayer.Position)));
            }
            else
            {
                pSprite = AFPool.GetItem<Sprite>("Texture","S_S");
            }
            if(pSprite != null)
            {
                RawImage icon = tm.Find("icon").GetComponent<RawImage>();
                icon.texture = pSprite.texture;
            }
        }

        if(isPlay == 2)
        {
            pText.SetText(pGameContext.GetDisplayLocationName(pPlayer.Position));
        }
        else if(isPlay == 1)
        {
            pText.SetText($"S{pGameContext.IsSubstitutionPlayerIndex(index,pPlayer.Id) - GameContext.CONST_NUMSTARTING +1}");
        }
        else
        {
            pText.SetText("");
        }
        
        pText = tm.Find("name").GetComponent<TMPro.TMP_Text>();
        pText.SetText($"{pPlayer.Forename[0]}. {pPlayer.Surname}");

        UpdateHP(tm.Find("hp/fill").GetComponent<Image>(),pPlayer);
        
        Transform pGroup = tm.Find("mark");
        if(pGroup != null)
        {
            for(int i =0; i < pGroup.childCount; ++i)
            {
                pGroup.GetChild(i).gameObject.SetActive(false);
            }

            index = pGameContext.ConvertPositionByTag(eLoc);
            byte value = pPlayer.PositionFamiliars[index];
            if(value > 0)
            {
                if(value >= 90)
                {
                    pGroup.Find("c4").gameObject.SetActive(true);
                    return;
                }
                if(value >= 60)
                {
                    pGroup.Find("c3").gameObject.SetActive(true);
                    return;
                }
                
                if(value >= 30)
                {
                    pGroup.Find("c2").gameObject.SetActive(true);
                    return;
                }
                
                pGroup.Find("c1").gameObject.SetActive(true);
            }
        }
    }

    public void Close()
    {
        SingleFunc.HideAnimationDailog(MainUI);
    }

    void ButtonEventCall(RectTransform root ,GameObject sender)
    {
        GameContext pGameContext = GameContext.getCtx();
        List<ulong> lineup = pGameContext.GetLineupPlayerIdListByIndex(pGameContext.GetActiveLineUpType());
        
        Transform tm = MainUI.Find("root/lineup");
        int i =0;
        for(i =0; i < tm.childCount; ++i)
        {
            if(tm.GetChild(i).gameObject == sender)
            {
                ulong id = lineup[i];
                lineup[i] =  m_pSelectPlayerData.Id;
                if(m_iLineupPlayerIndex > -1)
                {
                    lineup[m_iLineupPlayerIndex] = id;
                }

                if(pGameContext.CheckSameLineupData(lineup))
                {
                    JArray jArray = new JArray();
                    for(i = 0; i < lineup.Count; ++i)
                    {
                        jArray.Add(lineup[i]);
                    }
                    JObject pJObject = new JObject();
                    pJObject["type"] = pGameContext.GetActiveLineUpType();
                    pJObject["data"] =jArray.ToString(Newtonsoft.Json.Formatting.None);
                    pJObject["squadPower"] = pGameContext.GetTotalPlayerAbility(pGameContext.GetActiveLineUpType());
                    pJObject["totalValue"] = pGameContext.GetTotalPlayerValue(0);
                    pJObject["countQualified"] = pGameContext.GetTotalPlayerNAbilityTier(null,true);
                    pJObject["avgAge"] = pGameContext.GetPlayerAvgAge(null,true);
                    pJObject["playerCount"] = pGameContext.GetTotalPlayerCount();
                    m_pMainScene.RequestAfterCall(E_REQUEST_ID.lineup_put,pJObject);
                    m_pMainScene.UpdateScrollLineUpPlayer();
                }
                break;
            }
        }
        
        Close();
    }
}
