using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Sprites;

namespace EmojiUI
{
	public class SpriteGraphic : MaskableGraphic
	{
		IEmojiRender _Render;

		public override Texture mainTexture
		{
			get
			{
				if (_Render != null)
				{
					Texture texture = _Render.getRenderTexture(this);
					if (texture != null)
					{
						return texture;
					}
				}
				return s_WhiteTexture;
			}
		}

		public bool isDirty { get; protected set; }

		public void SetDirtyMask()
		{
			isDirty = true;
		}

		public void Draw(IEmojiRender rd)
		{
			_Render = rd;
		}

		protected override void Start()
		{
			base.Start();

			EmojiTools.AddUnityMemory(this);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			if (_Render != null)
			{
				_Render.Release(this);
			}
			_Render = null;
			EmojiTools.RemoveUnityMemory(this);
		}

		protected override void OnPopulateMesh(VertexHelper vh)
		{
			vh.Clear();
			if (_Render != null)
			{
				_Render.FillMesh(this, vh);
			}
			isDirty = false;
		}

		void OnDrawGizmos()
		{
			if (_Render != null)
			{
				_Render.DrawGizmos(this);
			}
		}
	}
}


