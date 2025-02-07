using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MaskImage : Image
{
    public Vector4 maskBorder = Vector4.zero;
    static readonly Vector2[] s_VertScratch = new Vector2[4];
    static readonly Vector2[] s_UVScratch = new Vector2[4];

    public void SetMaskNode(RectTransform rect)
    {
        if(rect != null)
        {
            RectTransform tm = GetComponent<RectTransform>();
            
            maskBorder.x = rect.anchoredPosition.x - (rect.rect.width * rect.pivot.x);
            maskBorder.y = rect.anchoredPosition.y - (rect.rect.height * rect.pivot.y);
            maskBorder.z = tm.rect.width - (maskBorder.x + rect.rect.width);
            maskBorder.w = tm.rect.height - (maskBorder.y + rect.rect.height);
        }
    }
    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        if (overrideSprite == null)
        {
            base.OnPopulateMesh(toFill);
            return;
        }

        if (type == Type.Sliced)
        {
            GenerateSlicedSprite(toFill);
        }
    }
    private Vector4 GetDrawingDimensions(bool shouldPreserveAspect)
    {
        var padding = overrideSprite == null ? Vector4.zero : UnityEngine.Sprites.DataUtility.GetPadding(overrideSprite);
        var size = overrideSprite == null ? Vector2.zero : new Vector2(overrideSprite.rect.width, overrideSprite.rect.height);

        Rect r = GetPixelAdjustedRect();
    
        int spriteW = Mathf.RoundToInt(size.x);
        int spriteH = Mathf.RoundToInt(size.y);

        var v = new Vector4(
                padding.x / spriteW,
                padding.y / spriteH,
                (spriteW - padding.z) / spriteW,
                (spriteH - padding.w) / spriteH);

        if (shouldPreserveAspect && size.sqrMagnitude > 0.0f)
        {
            var spriteRatio = size.x / size.y;
            var rectRatio = r.width / r.height;

            if (spriteRatio > rectRatio)
            {
                var oldHeight = r.height;
                r.height = r.width * (1.0f / spriteRatio);
                r.y += (oldHeight - r.height) * rectTransform.pivot.y;
            }
            else
            {
                var oldWidth = r.width;
                r.width = r.height * spriteRatio;
                r.x += (oldWidth - r.width) * rectTransform.pivot.x;
            }
        }

        v = new Vector4(
                r.x + r.width * v.x,
                r.y + r.height * v.y,
                r.x + r.width * v.z,
                r.y + r.height * v.w
                );

        return v;
    }
    Vector4 getAdjustedBorders(Vector4 border, Rect rect)
    {
        for (int axis = 0; axis <= 1; axis++)
        {
            // If the rect is smaller than the combined borders, then there's not room for the borders at their normal size.
            // In order to avoid artefacts with overlapping borders, we scale the borders down to fit.
            float combinedBorders = border[axis] + border[axis + 2];
            if (rect.size[axis] < combinedBorders && combinedBorders != 0)
            {
                float borderScaleRatio = rect.size[axis] / combinedBorders;
                border[axis] *= borderScaleRatio;
                border[axis + 2] *= borderScaleRatio;
            }
        }
        return border;
    }
    void GenerateSimpleSprite(VertexHelper vh, bool lPreserveAspect)
    {
        Vector4 v = GetDrawingDimensions(lPreserveAspect);
        var uv = (overrideSprite != null) ? UnityEngine.Sprites.DataUtility.GetOuterUV(overrideSprite) : Vector4.zero;

        var color32 = color;
        vh.Clear();
        vh.AddVert(new Vector3(v.x, v.y), color32, new Vector2(uv.x, uv.y));
        vh.AddVert(new Vector3(v.x, v.w), color32, new Vector2(uv.x, uv.w));
        vh.AddVert(new Vector3(v.z, v.w), color32, new Vector2(uv.z, uv.w));
        vh.AddVert(new Vector3(v.z, v.y), color32, new Vector2(uv.z, uv.y));

        vh.AddTriangle(0, 1, 2);
        vh.AddTriangle(2, 3, 0);
    }
    private void GenerateSlicedSprite(VertexHelper toFill)
    {
        if (!hasBorder)
        {
            GenerateSimpleSprite(toFill, false);
            return;
        }

        Vector4 outer, inner, padding, border;

        if (overrideSprite != null)
        {
            outer = UnityEngine.Sprites.DataUtility.GetOuterUV(overrideSprite);
            inner = UnityEngine.Sprites.DataUtility.GetInnerUV(overrideSprite);
            padding = UnityEngine.Sprites.DataUtility.GetPadding(overrideSprite);
            // border = overrideSprite.border;
            
            border = maskBorder;
            // maskBorder = border;
        }
        else
        {
            outer = Vector4.zero;
            inner = Vector4.zero;
            padding = Vector4.zero;
            border = Vector4.zero;
        }

        Rect rect = GetPixelAdjustedRect();
        border = getAdjustedBorders(border / pixelsPerUnit, rect);
        padding = padding / pixelsPerUnit;

        s_VertScratch[0] = new Vector2(padding.x, padding.y);
        s_VertScratch[3] = new Vector2(rect.width - padding.z, rect.height - padding.w);

        s_VertScratch[1].x = border.x;
        s_VertScratch[1].y = border.y;
        s_VertScratch[2].x = rect.width - border.z;
        s_VertScratch[2].y = rect.height - border.w;

        for (int i = 0; i < 4; ++i)
        {
            s_VertScratch[i].x += rect.x;
            s_VertScratch[i].y += rect.y;
        }

        s_UVScratch[0] = new Vector2(outer.x, outer.y);
        s_UVScratch[1] = new Vector2(inner.x, inner.y);
        s_UVScratch[2] = new Vector2(inner.z, inner.w);
        s_UVScratch[3] = new Vector2(outer.z, outer.w);

        toFill.Clear();

        for (int x = 0; x < 3; ++x)
        {
            int x2 = x + 1;

            for (int y = 0; y < 3; ++y)
            {
                if (!fillCenter && x == 1 && y == 1)
                    continue;

                int y2 = y + 1;
                
                int startIndex = toFill.currentVertCount;

                toFill.AddVert(new Vector3(s_VertScratch[x].x, s_VertScratch[y].y, 0), color, new Vector2(s_UVScratch[x].x, s_UVScratch[y].y));
                toFill.AddVert(new Vector3(s_VertScratch[x].x, s_VertScratch[y2].y, 0), color, new Vector2(s_UVScratch[x].x, s_UVScratch[y2].y));
                toFill.AddVert(new Vector3(s_VertScratch[x2].x, s_VertScratch[y2].y, 0), color, new Vector2(s_UVScratch[x2].x, s_UVScratch[y2].y));
                toFill.AddVert(new Vector3(s_VertScratch[x2].x, s_VertScratch[y].y, 0), color, new Vector2(s_UVScratch[x2].x, s_UVScratch[y].y));

                toFill.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
                toFill.AddTriangle(startIndex + 2, startIndex + 3, startIndex);

            }
        }
    }
}
