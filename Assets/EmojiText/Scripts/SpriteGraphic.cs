using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace EmojiText.Taurus
{
	[ExecuteInEditMode]
	public class SpriteGraphic : MaskableGraphic
	{
		#region 属性
		//默认shader
		private const string _defaultShader = "Hidden/UI/Emoji";
		private Material _defaultMater = null;

		public SpriteAsset m_spriteAsset;

		////分割数量
		//[SerializeField]
		//private int _cellAmount = 1;
		////动画速度
		//[SerializeField]
		//private float _speed;
		//顶点缓存数据
		readonly UIVertex[] _tempVerts = new UIVertex[4];

		//模型数据
		private MeshInfo _meshInfo;
		public MeshInfo MeshInfo
		{
			get { return _meshInfo; }
			set
			{
				if (value == null && _meshInfo != null)
				{
					_meshInfo.Clear();
				}
				else
					_meshInfo = value;

				SetAllDirty();
			}
		}

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
				if (_defaultMater == null&& m_spriteAsset!=null)
				{
					_defaultMater = new Material(Shader.Find(_defaultShader));
                    //是否开启动画
                    if (m_spriteAsset.IsStatic)
                        _defaultMater.DisableKeyword("EMOJI_ANIMATION");
                    else
                    {
                        _defaultMater.EnableKeyword("EMOJI_ANIMATION");
                        _defaultMater.SetFloat("_CellAmount", m_spriteAsset.Column);
                        _defaultMater.SetFloat("_Speed", m_spriteAsset.Speed);
                    }
				}
				return _defaultMater;
			}
		}
		#endregion

		protected override void OnPopulateMesh(VertexHelper vh)
		{
			vh.Clear();
			//在这里可以做一个数据判断，如果数据一样 就不再刷新
			if (_meshInfo != null)
			{
				for (int i = 0; i < _meshInfo.Vertices.Count; i++)
				{
					int tempVertsIndex = i & 3;
					_tempVerts[tempVertsIndex].position = _meshInfo.Vertices[i];// Utility.TransformWorld2Point(transform, _meshInfo.Vertices[i]);
					_tempVerts[tempVertsIndex].uv0 = _meshInfo.UVs[i];
					_tempVerts[tempVertsIndex].color = color;
					if (tempVertsIndex == 3)
						vh.AddUIVertexQuad(_tempVerts);
				}
			}
		}
	}

}