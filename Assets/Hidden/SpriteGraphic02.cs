using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpriteGraphic02 : MaskableGraphic
{
	#region 属性
	//默认shader
	private const string _defaultShader = "Hidden/UI/Emoji";
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

	readonly UIVertex[] _tempVerts = new UIVertex[4];


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
				_defaultMater.EnableKeyword("EMOJI_ANIMATION");
			}
			return _defaultMater;
		}
	}
	#endregion

	protected InlineManager _inlineManager;

	protected override void OnEnable()
	{
		base.OnEnable();
		_inlineManager = GetComponentInParent<InlineManager>();
	}

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		if (_inlineManager == null|| m_spriteAsset==null|| !_inlineManager.GetMeshInfo.ContainsKey(m_spriteAsset.Id))
			return;

		SpriteTagInfo meshInfo = _inlineManager.GetMeshInfo[m_spriteAsset.Id];
		if (meshInfo == null || meshInfo.Pos == null || meshInfo.Pos.Length == 0)
		{
			vh.Clear();
			//base.OnPopulateMesh(vh);
		}
		else
		{
			vh.Clear();
			for (int i = 0; i < meshInfo.Pos.Length; i++)
			{
				int tempVertsIndex = i & 3;
				_tempVerts[tempVertsIndex].position = meshInfo.Pos[i];
				_tempVerts[tempVertsIndex].uv0 = meshInfo.Uv[i];
				_tempVerts[tempVertsIndex].color = color;
				if (tempVertsIndex == 3)
					vh.AddUIVertexQuad(_tempVerts);

				//		//--------------------------------------------------------------------------------------
				//		//看到unity 的mesh支持多层uv  还在想shader渲染动图有思路了呢
				//		//结果调试shader的时候发现uv1-uv3的值跟uv0一样
				//		//意思就是 unity canvasrender  目前的设计，为了优化性能,不支持uv1-3,并不是bug,所以没法存多套uv。。。
				//		//https://issuetracker.unity3d.com/issues/canvasrenderer-dot-setmesh-does-not-seem-to-support-more-than-one-uv-set
				//		//不知道后面会不会更新 ------  于是现在还是用老办法吧， 规则图集 --> uv移动 
				//		//--------------------------------------------------------------------------------------

				//		//h.uv2 = spriteInfors[1].Uv;
				//		//h.uv3 = spriteInfors[2].Uv;
				//		//h.uv4 = spriteInfors[3].Uv;
			}
		}
	}
}
