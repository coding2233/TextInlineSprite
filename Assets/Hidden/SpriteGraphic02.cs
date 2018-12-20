using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpriteGraphic02 : MaskableGraphic
{
	#region 属性
	//默认shader
	private const string _defaultShader = "Hidden/UI/Default";
	private Material _defaultMater = null;

	public SpriteAsset m_spriteAsset;

	public override Texture mainTexture
	{
		get
		{
			if (m_spriteAsset == null || m_spriteAsset.TexSource == null)
				return base.mainTexture;
			else
				return m_spriteAsset.TexSource;
		}
	}

	public override Material material
	{
		get
		{
			if (_defaultMater == null)
			{
				_defaultMater = new Material(Shader.Find(_defaultShader));
			}
			return _defaultMater;
		}
	}



	#endregion



	public override UnityEngine.Material GetModifiedMaterial(UnityEngine.Material baseMaterial)
	{
		return base.GetModifiedMaterial(baseMaterial);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
	}

	
#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
	}
#endif

	public override void SetMaterialDirty()
	{
		base.SetMaterialDirty();
	}

	protected override void UpdateMaterial()
	{
		base.UpdateMaterial();
	}

	protected override void OnPopulateMesh(Mesh h)
	{
		//if (s_Mesh != null&&s_Mesh.vertexCount==0)
		//{
		//	//s_Mesh
		//}
		//if (workerMesh != null || s_Mesh != null)
		//{
		//	Debug.Log("workmesh count:");
		//}
		//base.OnPopulateMesh(h);

		if (m_spriteAsset != null&& h.vertexCount==0)
		{
		//	h.Clear();
			var r = GetPixelAdjustedRect();
			var v = new Vector4(r.x, r.y, r.x + r.width, r.y + r.height);
			h.vertices = new Vector3[4] { new Vector3(v.x, v.y), new Vector3(v.x, v.w), new Vector3(v.z, v.w), new Vector3(v.z, v.y) };
			h.colors = new Color[4] { color, color, color, color };
			h.triangles = new int[6] {0,1,2,2,3,0 };
			
			List<SpriteInfor> spriteInfors = m_spriteAsset.ListSpriteGroup[1].ListSpriteInfor;

			h.uv = spriteInfors[0].Uv;
			h.uv2 = spriteInfors[1].Uv;
			h.uv3 = spriteInfors[2].Uv;
			h.uv4 = spriteInfors[3].Uv;

		

			//Color32 color32 = color;
			//vh.Clear();
			//vh.AddVert(new Vector3(v.x, v.y), color32, new Vector2(0f, 0f));
			//vh.AddVert(new Vector3(v.x, v.w), color32, new Vector2(0f, 1f));
			//vh.AddVert(new Vector3(v.z, v.w), color32, new Vector2(1f, 1f));
			//vh.AddVert(new Vector3(v.z, v.y), color32, new Vector2(1f, 0f));

			//vh.AddTriangle(0, 1, 2);
			//vh.AddTriangle(2, 3, 0);

			//canvasRenderer.SetMesh(h);
		}
	}
	protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper vh)
	{
		//	base.OnPopulateMesh(vh);

	
	}
}
