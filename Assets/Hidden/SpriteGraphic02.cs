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

	//测试索引
	[SerializeField]
	[Range(0,19)]
	private int _testIndex=1;

	//分割数量
	[SerializeField]
	private int _cellAmount=1;
	//动画速度
	[SerializeField]
	private float _speed;

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
				_defaultMater.SetFloat("_CellAmount", _cellAmount);
				_defaultMater.SetFloat("_Speed", _speed);
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
		//base.OnPopulateMesh(h);

		if (m_spriteAsset != null&& h.vertexCount==0)
		{
			var r = GetPixelAdjustedRect();
			var v = new Vector4(r.x, r.y, r.x + r.width, r.y + r.height);
			h.vertices = new Vector3[4] { new Vector3(v.x, v.y), new Vector3(v.x, v.w), new Vector3(v.z, v.w), new Vector3(v.z, v.y) };
			h.colors = new Color[4] { color, color, color, color };
			h.triangles = new int[6] {0,1,2,2,3,0 };
			
			List<SpriteInfor> spriteInfors = m_spriteAsset.ListSpriteGroup[_testIndex].ListSpriteInfor;

			h.uv = spriteInfors[0].Uv;

			//--------------------------------------------------------------------------------------
			//看到unity 的mesh支持多层uv  还在想shader渲染动图有思路了呢
			//结果调试shader的时候发现uv1-uv3的值跟uv0一样
			//意思就是 unity canvasrender  目前的设计，为了优化性能,不支持uv1-3,并不是bug,所以没法存多套uv。。。
			//https://issuetracker.unity3d.com/issues/canvasrenderer-dot-setmesh-does-not-seem-to-support-more-than-one-uv-set
			//不知道后面会不会更新 ------  于是现在还是用老办法吧， 规则图集 --> uv移动 
			//--------------------------------------------------------------------------------------

			//h.uv2 = spriteInfors[1].Uv;
			//h.uv3 = spriteInfors[2].Uv;
			//h.uv4 = spriteInfors[3].Uv;

		}
	}
	protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper vh)
	{
		//	base.OnPopulateMesh(vh);
	}
}
