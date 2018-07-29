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

		bool ignore { get; set; }

		int Fill(Vector3 start, Vector3 end);

		int GetFillCnt();

		int GetPositionIdx();

		int GetTagIndex();
	}

	#endregion

	public class SpriteTagInfo:IFillData
	{
		private int _position ;
		private const int Dv = 1000;
		private const int DV2 = Dv * Dv;
		
		public int ID { get; set; }

		public Vector3[] pos { get; set; }

		public Vector2[] uv { get; set; }

		public Vector2 Size { get; set; }

		public bool ignore { get; set; }

		public string Tag { get; set; }

		public void FillIdxAndPlaceHolder(int idx, int tagidx,int fillcnt)
		{
			_position =fillcnt *DV2 + tagidx * Dv + idx;
		}

		public int GetTagIndex()
		{
			return (_position / Dv) % Dv;
		}

		public int GetPositionIdx()
		{
			return _position % Dv;
		}

		public int GetFillCnt()
		{
			return _position / DV2;
		}
		
		public int Fill(Vector3 start,Vector3 end)
		{
			int index = GetPositionIdx();
			if (index >= 0)
			{
				Vector3 center = (start + end) / 2;
				pos[3] = center + new Vector3(-Size.x / 2,-Size.y / 2, 0);
				pos[2] = center + new Vector3(Size.x / 2, -Size.y / 2, 0);
				pos[1] = center + new Vector3(Size.x / 2, Size.y / 2, 0);
				pos[0] = center + new Vector3(-Size.x / 2, Size.y / 2, 0);
			}
			return -1;
		}

	}
}
