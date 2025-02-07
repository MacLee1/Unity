using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EmblemBake : RawImage
{
    Texture2D m_pColorTexutre = null;
    public Texture baseMaskTexture = null;
    public Texture patternTexture = null;
    public Texture simbolTexture = null;

    public Color pattern1Color = Color.white;
    public Color pattern2Color = Color.white;
    public Color simbolColor = Color.white;

    public void Dispose()
    {   
        m_pColorTexutre = null;
        if(m_Material != null)
        {
            m_Material.SetTexture("_MainTex",null);
            m_Material.SetTexture("_PatternTex",null);
            m_Material.SetTexture("_SimbolTex",null);
            m_Material = null;
        }
        
        baseMaskTexture = null;
        patternTexture = null;
        simbolTexture = null;
    }

    public void UpdateEmblemColor(byte[] datas)
    {
        if(datas != null)
        {
            if(m_pColorTexutre == null)
            {
                m_pColorTexutre = ALF.AFPool.GetItem<Texture2D>("Texture","palette");
            }
            int x = (int)datas[3] / 16;
            int y = (int)datas[3] % 16;
            
            Color colorPixel = m_pColorTexutre.GetPixel(x, y);

            pattern1Color.r = colorPixel.r;
            pattern1Color.g = colorPixel.g;
            pattern1Color.b = colorPixel.b;

            x = (int)datas[4] / 16;
            y = (int)datas[4] % 16;
            colorPixel = m_pColorTexutre.GetPixel(x, y);

            pattern2Color.r = colorPixel.r;
            pattern2Color.g = colorPixel.g;
            pattern2Color.b = colorPixel.b;

            x = (int)datas[5] / 16;
            y = (int)datas[5] % 16;
            colorPixel = m_pColorTexutre.GetPixel(x, y);

            simbolColor.r = colorPixel.r;
            simbolColor.g = colorPixel.g;
            simbolColor.b = colorPixel.b;
            Bake();
        }
    }

    public void SetupEmblemData(byte[] datas)
    {   
        if(datas != null)
        {            
            Sprite pSprite = ALF.AFPool.GetItem<Sprite>("Texture",$"shape{datas[0]}");
            ALF.ALFUtils.Assert(pSprite != null, $"shape{datas[0]}");
            baseMaskTexture = pSprite.texture;
            pSprite = ALF.AFPool.GetItem<Sprite>("Texture",$"pattern{datas[1]}");
            ALF.ALFUtils.Assert(pSprite != null, $"shape{datas[1]}");
            patternTexture = pSprite.texture;
            pSprite = ALF.AFPool.GetItem<Sprite>("Texture",$"symbol{datas[2]}");
            ALF.ALFUtils.Assert(pSprite != null, $"shape{datas[2]}");
            simbolTexture = pSprite.texture;
            UpdateEmblemColor(datas);
            
            int star = 0;
            if(datas.Length > 6)
            {
                star = datas[6];
            }
            float width = 0;
            int i = 0;
            for(i = 0; i < transform.childCount; ++i)
            {
                transform.GetChild(i).gameObject.SetActive(false);
                width = transform.GetChild(i).GetComponent<RectTransform>().rect.width;
            }
            
            if(width == 0 || star == 0) return;

            star = Mathf.Min(star,5);

            while(star > 0)
            {
                --star;
                transform.GetChild(star).gameObject.SetActive(true);
            }
            
            AlignStar();
        }
    }

    void AlignStar()
    {
        float width = transform.GetChild(0).GetComponent<RectTransform>().rect.width;
        
        RectTransform item = null;
        float w = 0;
        Vector2 pos;
        int i =0;
        for(i = 0; i < transform.childCount; ++i)
        {
            item = (RectTransform)transform.GetChild(i);
            
            if(item.gameObject.activeSelf)
            {
                pos = item.anchoredPosition3D;
                pos.x = w;
                item.anchoredPosition3D = pos;
                w += width;        
            }
        }
        w = (w * -0.5f) + (width * 0.5f);

        for(i = 0; i < transform.childCount; ++i)
        {
            item = (RectTransform)transform.GetChild(i);
            pos = item.anchoredPosition3D;
            pos.x += w;
            item.anchoredPosition3D = pos;
        }

    }

    public void CopyPoint(EmblemBake src)
    {
        material = src.material;

        if(src.transform.childCount > 0 && transform.childCount == src.transform.childCount)
        {
            Transform tm = null;
            Transform item = null;
            for(int i = 0; i < transform.childCount; ++i)
            {
                tm = transform.GetChild(i);
                item = src.transform.GetChild(i);
                tm.gameObject.SetActive(item.gameObject.activeSelf);
            }

            AlignStar();
        }
        else
        {
            for(int i = 0; i < transform.childCount; ++i)
            {
                transform.GetChild(i).gameObject.SetActive(false);    
            }
        }
    }

    public void Bake()
    {
        if (m_Material == null)
        {
            m_Material = new Material(Shader.Find("ALF/UI/EmblemBake"));
        }
        
        if(m_Material != null && baseMaskTexture != null && patternTexture != null && simbolTexture != null)
        {
            m_Material.SetTexture("_MainTex",baseMaskTexture);
            m_Material.SetTexture("_PatternTex",patternTexture);
            m_Material.SetTexture("_SimbolTex",simbolTexture);
            m_Material.SetColor("_Pattern1Color", pattern1Color);
            m_Material.SetColor("_Pattern2Color", pattern2Color);
            m_Material.SetColor("_SimbolColor", simbolColor);
            SetAllDirty();
            gameObject.SetActive(false);
            gameObject.SetActive(true);
        }
    }
}


// using System.Collections.Generic;
// using UnityEngine.Serialization;

// namespace UnityEngine.UI
// {
//     /// <summary>
//     /// If you don't have or don't wish to create an atlas, you can simply use this script to draw a texture.
//     /// Keep in mind though that this will create an extra draw call with each RawImage present, so it's
//     /// best to use it only for backgrounds or temporary visible graphics.
//     /// </summary>
//     [AddComponentMenu("UI/Raw Image", 12)]
//     public class RawImage : MaskableGraphic
//     {
//         [FormerlySerializedAs("m_Tex")]
//         [SerializeField] Texture m_Texture;
//         [SerializeField] Rect m_UVRect = new Rect(0f, 0f, 1f, 1f);

//         protected RawImage()
//         { }

//         /// <summary>
//         /// Returns the texture used to draw this Graphic.
//         /// </summary>
//         public override Texture mainTexture
//         {
//             get
//             {
//                 return m_Texture == null ? s_WhiteTexture : m_Texture;
//             }
//         }

//         /// <summary>
//         /// Texture to be used.
//         /// </summary>
//         public Texture texture
//         {
//             get
//             {
//                 return m_Texture;
//             }
//             set
//             {
//                 if (m_Texture == value)
//                     return;

//                 m_Texture = value;
//                 SetVerticesDirty();
//                 SetMaterialDirty();
//             }
//         }

//         /// <summary>
//         /// UV rectangle used by the texture.
//         /// </summary>
//         public Rect uvRect
//         {
//             get
//             {
//                 return m_UVRect;
//             }
//             set
//             {
//                 if (m_UVRect == value)
//                     return;
//                 m_UVRect = value;
//                 SetVerticesDirty();
//             }
//         }

//         /// <summary>
//         /// Adjust the scale of the Graphic to make it pixel-perfect.
//         /// </summary>

//         public override void SetNativeSize()
//         {
//             Texture tex = mainTexture;
//             if (tex != null)
//             {
//                 int w = Mathf.RoundToInt(tex.width * uvRect.width);
//                 int h = Mathf.RoundToInt(tex.height * uvRect.height);
//                 rectTransform.anchorMax = rectTransform.anchorMin;
//                 rectTransform.sizeDelta = new Vector2(w, h);
//             }
//         }

//         /// <summary>
//         /// Update all renderer data.
//         /// </summary>
//         protected override void OnFillVBO(List<UIVertex> vbo)
//         {
//             Texture tex = mainTexture;

//             if (tex != null)
//             {
//                 Vector4 v = Vector4.zero;

//                 int w = Mathf.RoundToInt(tex.width * uvRect.width);
//                 int h = Mathf.RoundToInt(tex.height * uvRect.height);

//                 float paddedW = ((w & 1) == 0) ? w : w + 1;
//                 float paddedH = ((h & 1) == 0) ? h : h + 1;

//                 v.x = 0f;
//                 v.y = 0f;
//                 v.z = w / paddedW;
//                 v.w = h / paddedH;

//                 v.x -= rectTransform.pivot.x;
//                 v.y -= rectTransform.pivot.y;
//                 v.z -= rectTransform.pivot.x;
//                 v.w -= rectTransform.pivot.y;

//                 v.x *= rectTransform.rect.width;
//                 v.y *= rectTransform.rect.height;
//                 v.z *= rectTransform.rect.width;
//                 v.w *= rectTransform.rect.height;

//                 vbo.Clear();

//                 var vert = UIVertex.simpleVert;

//                 vert.position = new Vector2(v.x, v.y);
//                 vert.uv0 = new Vector2(m_UVRect.xMin, m_UVRect.yMin);
//                 vert.color = color;
//                 vbo.Add(vert);

//                 vert.position = new Vector2(v.x, v.w);
//                 vert.uv0 = new Vector2(m_UVRect.xMin, m_UVRect.yMax);
//                 vert.color = color;
//                 vbo.Add(vert);

//                 vert.position = new Vector2(v.z, v.w);
//                 vert.uv0 = new Vector2(m_UVRect.xMax, m_UVRect.yMax);
//                 vert.color = color;
//                 vbo.Add(vert);

//                 vert.position = new Vector2(v.z, v.y);
//                 vert.uv0 = new Vector2(m_UVRect.xMax, m_UVRect.yMin);
//                 vert.color = color;
//                 vbo.Add(vert);
//             }
//         }
//     }
// }