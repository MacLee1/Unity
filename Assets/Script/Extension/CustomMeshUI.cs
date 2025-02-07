using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;


public class CustomMeshUI : MaskableGraphic
{
    [FormerlySerializedAs("m_Tex")]
    [SerializeField] Texture m_Texture;
    [SerializeField] Rect m_UVRect = new Rect(0f, 0f, 1f, 1f);
    [SerializeField] Mesh m_Mesh;


    public override Texture mainTexture
    {
        get
        {
            if (m_Texture == null)
            {
                if (material != null && material.mainTexture != null)
                {
                    return material.mainTexture;
                }
                return s_WhiteTexture;
            }

            return m_Texture;
        }
    }

    public Texture texture
    {
        get
        {
            return m_Texture;
        }
        set
        {
            if (m_Texture == value)
                return;

            m_Texture = value;
            SetVerticesDirty();
            SetMaterialDirty();
        }
    }

    public Rect uvRect
    {
        get
        {
            return m_UVRect;
        }
        set
        {
            if (m_UVRect == value)
                return;
            m_UVRect = value;
            SetVerticesDirty();
        }
    }

    // void LateUpdate()
    // {
    //     canvasRenderer.SetMesh(m_Mesh);
    //     canvasRenderer.SetTexture(texture);
    // }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if(m_Mesh != null)
        {
            
            // var mesh = smartMesh.mesh;
			// meshGenerator.FillVertexData(mesh);
			// if (updateTriangles) meshGenerator.FillTriangles(mesh);
			// meshGenerator.FillLateVertexData(mesh);

			// canvasRenderer.SetMesh(m_Mesh);
			// smartMesh.instructionUsed.Set(currentInstructions);

			// if (currentInstructions.submeshInstructions.Count > 0) {
			// 	var material = currentInstructions.submeshInstructions.Items[0].material;
			// 	if (material != null && baseTexture != material.mainTexture) {
			// 		baseTexture = material.mainTexture;
			// 		if (overrideTexture == null)
			// 			canvasRenderer.SetTexture(this.mainTexture);
			// 	}
			// }

            var color32 = color;
            int i = 0;
            
            while (i < m_Mesh.vertices.Length) 
            {
                vh.AddVert(m_Mesh.vertices[i], color32, new Vector2(m_Mesh.uv[i].x + m_UVRect.xMin, m_Mesh.uv[i].y + m_UVRect.yMin));
                ++i;
            }

            i = 0;
            
            while (i < m_Mesh.triangles.Length) 
            {
                vh.AddTriangle(m_Mesh.triangles[i], m_Mesh.triangles[i+1], m_Mesh.triangles[i+2]);
                i+=3;
            }

        }
        // if (tex != null)
        // {
        //     // var r = GetPixelAdjustedRect();
        //     // var v = new Vector4(r.x, r.y, r.x + r.width, r.y + r.height);
        //     // var scaleX = tex.width * tex.texelSize.x;
        //     // var scaleY = tex.height * tex.texelSize.y;
        //     // {
        //     //     var color32 = color;
        //     //     vh.AddVert(new Vector3(v.x, v.y), color32, new Vector2(m_UVRect.xMin * scaleX, m_UVRect.yMin * scaleY));
        //     //     vh.AddVert(new Vector3(v.x, v.w), color32, new Vector2(m_UVRect.xMin * scaleX, m_UVRect.yMax * scaleY));
        //     //     vh.AddVert(new Vector3(v.z, v.w), color32, new Vector2(m_UVRect.xMax * scaleX, m_UVRect.yMax * scaleY));
        //     //     vh.AddVert(new Vector3(v.z, v.y), color32, new Vector2(m_UVRect.xMax * scaleX, m_UVRect.yMin * scaleY));

        //     //     vh.AddTriangle(0, 1, 2);
        //     //     vh.AddTriangle(2, 3, 0);
        //     // }
        // }
    }

}
