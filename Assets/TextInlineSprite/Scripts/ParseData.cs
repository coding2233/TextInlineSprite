using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace EmojiUI
{

	#region generic
	public interface IFillData
	{
		string Tag { get; }
		int ID { get; }
		Vector3[] pos { get; set; }
		Vector2[] uv { get; set; }

		void Fill(VertexHelper filler);
	}

	public struct ParsedData : IEquatable<ParsedData>
	{
		public int atlasID;
		public string atlasTag;

		public bool Equals(ParsedData other)
		{
			if (this.atlasID != other.atlasID)
				return false;

			if (this.atlasTag != other.atlasTag)
				return false;

			return true;
		}
	}
	#endregion

	public class SpriteTagInfo:IFillData
	{
		private int _Position = -1;
		private const int dv = 100000;


		public int ID { get; set; }

		public Vector3[] pos { get; set; }

		public Vector2[] uv { get; set; }

		//custom

		public Vector2 Size { get; set; }

		public string Tag { get; set; }

		public void FillIdxAndPlaceHolder(int idx, int cnt)
		{
			_Position = cnt * dv + idx;
		}

		public int GetPlaceHolderCnt()
		{
			return _Position / dv;
		}

		public int GetPositionIdx()
		{
			return _Position % dv;
		}

	}
}
