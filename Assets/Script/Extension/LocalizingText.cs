
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.Globalization;


namespace ALF
{
    public class LocalizingText : TMPro.TextMeshProUGUI
    {
        public string Key = null;
        public bool IsLocalizing = false;
        
        public void UpdateLocalizing()
        {
            if(IsLocalizing)
            {
                SetText(GameContext.getCtx().GetLocalizingText(Key));
            }
            // m_pText?.SetText(GameContext.getCtx().GetLocalizingText(Key));

            // if(m_pGraphic is RawImage rawImage)
            // {
            //     rawImage.texture = AFPool.GetItem<Sprite>("Texture",Key).texture;
            // }
            // else if(m_pGraphic is Image image)
            // {
            //     image.sprite = AFPool.GetItem<Sprite>("Texture",Key);
            // }
        }

        // void OnDestroy() 
        // {
        //     Dispose();
        // }

        // public void Dispose()
        // {
        //     m_pText = null;
        //     m_pGraphic = null;
        // }
    }
}