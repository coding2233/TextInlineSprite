using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace EmojiUI
{
	#region sprites
	[System.Serializable]
	public class SpriteInfo
	{
		/// <summary>
		/// 精灵
		/// </summary>
		public Sprite sprite;
		/// <summary>
		/// 标签
		/// </summary>
		public string tag;
		/// <summary>
		/// uv
		/// </summary>
		public Vector2[] uv;
	}

	[System.Serializable]
	public class SpriteInfoGroup
	{
		public string tag = "";

		public float width = 1.0f;
		public float size = 24.0f;

		public float x;
		public float y;

		public List<SpriteInfo> spritegroups = new List<SpriteInfo>();
	}

	#endregion
}

