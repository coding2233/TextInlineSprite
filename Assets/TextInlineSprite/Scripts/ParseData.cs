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

		int Fill(UIVertex[] filler);

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
		
		public int Fill(UIVertex[] filler)
		{
			int index = GetPositionIdx();
			if (index >= 0)
			{
				pos[0] = filler[0].position;
				pos[1] = filler[1].position;
				pos[2] = filler[2].position;
				pos[3] = filler[3].position;

			}

			return -1;
		}

	}
}
