/// ========================================================
/// file：InlineText.cs
/// brief：
/// author： coding2233
/// date：
/// version：v1.0
/// ========================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.Text;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;

namespace EmojiUI
{
	public class InlineText : Text
	{
		private static StringBuilder _textBuilder = new StringBuilder();
		private static UIVertex[] m_TempVerts = new UIVertex[4];
		private static Vector3[] m_TagVerts = new Vector3[2];
		
		private TextGenerator _SpaceGen;
		private InlineManager _InlineManager;
		//文本表情管理器
		public InlineManager Manager
		{
			get
			{
				if (!_InlineManager && canvas != null)
				{
					_InlineManager = GetComponentInParent<InlineManager>();
					if (_InlineManager == null)
					{
						_InlineManager = canvas.gameObject.AddComponent<InlineManager>();
					}
				}
				return _InlineManager;
			}
		}

		List<IFillData> _renderTagList;
		private string _lasttext ;
		private string _outputText = "";

		void OnDrawGizmos()
		{
			Gizmos.color = Color.blue;

			var corners = new Vector3[4];
			rectTransform.GetWorldCorners(corners);

			Gizmos.DrawLine(corners[0], corners[1]);
			Gizmos.DrawLine(corners[1], corners[2]);
			Gizmos.DrawLine(corners[3], corners[3]);
			Gizmos.DrawLine(corners[3], corners[0]);
		}

		protected override void Start()
		{
			base.Start();

			EmojiTools.AddUnityMemory(this);
		}

		public override void SetVerticesDirty()
		{
			base.SetVerticesDirty();
			if (Application.isPlaying)
			{
				if (!Manager)
				{
					_outputText = m_Text;
				}
				else if(Manager.HasInit)
				{
					DoUpdateEmoji();
				}
				else
				{
					StartCoroutine(WaitManagerInited());
				}
			}
			else
			{
				_outputText = m_Text;
			}
		}

		IEnumerator WaitManagerInited()
		{
			while (Manager != null && !Manager.HasInit)
			{
				yield return null;
			}

			DoUpdateEmoji();
		}

		void DoUpdateEmoji()
		{
			if(m_Text != null && !m_Text.Equals(_lasttext) )
			{
				ClearFillData();
				_lasttext = m_Text;
				ParserTransmit.mIns.DoParse(this, _textBuilder, m_Text);

				_outputText = _textBuilder.ToString();
				_textBuilder.Length = 0;
			}	
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			if (_renderTagList != null)
			{
				ListPool<IFillData>.Release(_renderTagList);
				_renderTagList = null;
			}

			if (Manager)
			{
				Manager.UnRegister(this);
			}

			EmojiTools.RemoveUnityMemory(this);
		}

		protected override void OnPopulateMesh(VertexHelper toFill)
		{
			if (font == null)
				return;

			// We don't care if we the font Texture changes while we are doing our Update.
			// The end result of cachedTextGenerator will be valid for this instance.
			// Otherwise we can get issues like Case 619238.
			m_DisableFontTextureRebuiltCallback = true;

			Vector2 extents = rectTransform.rect.size;

			var settings = GetGenerationSettings(extents);
			cachedTextGenerator.PopulateWithErrors(_outputText, settings, gameObject);

			// Apply the offset to the vertices
			IList<UIVertex> verts = cachedTextGenerator.verts;
			float unitsPerPixel = 1 / pixelsPerUnit;
			//Last 4 verts are always a new line... (\n)
			int vertCount = verts.Count - 4;
			Vector2 roundingOffset = new Vector2(verts[0].position.x, verts[0].position.y) * unitsPerPixel;
			roundingOffset = PixelAdjustPoint(roundingOffset) - roundingOffset;
			toFill.Clear();

			
			int nextfilldata = -1;
			int fillindex = -1;
			int startfilldata = -1;
			
			if (_renderTagList != null && _renderTagList.Count  >0)
			{
				var data = _renderTagList[0];
				nextfilldata = data.GetPositionIdx() ;
				//at least one 
				startfilldata = nextfilldata - (data.GetFillCnt()-1) * 4-3;
				fillindex = 0;

				Manager.Register(this);
			}
			else
			{
				Manager.UnRegister(this);
			}
			
			if (roundingOffset != Vector2.zero)
			{
				for (int i = 0; i < vertCount; ++i)
				{
					int tempVertsIndex = i & 3;
					m_TempVerts[tempVertsIndex] = verts[i];
					m_TempVerts[tempVertsIndex].position *= unitsPerPixel;
					m_TempVerts[tempVertsIndex].position.x += roundingOffset.x;
					m_TempVerts[tempVertsIndex].position.y += roundingOffset.y;

					//first
					if (startfilldata >= 0 && i == startfilldata)
					{
						m_TagVerts[0]= m_TempVerts[tempVertsIndex].position;
					}

					//third
					if (nextfilldata >= 0 && i == nextfilldata - 1)
					{
						m_TagVerts[1] = m_TempVerts[tempVertsIndex].position;
					}

					if (tempVertsIndex == 3)
					{
						//skip
						if ( i < startfilldata || i > nextfilldata )
						{
							toFill.AddUIVertexQuad(m_TempVerts);
						}

						if (nextfilldata >=0 && i >= nextfilldata)
						{
							FillNextTag(ref startfilldata,ref nextfilldata,ref fillindex);
						}
					}
				}
			}
			else
			{
				for (int i = 0; i < vertCount; ++i)
				{
					int tempVertsIndex = i & 3;
					m_TempVerts[tempVertsIndex] = verts[i];
					m_TempVerts[tempVertsIndex].position *= unitsPerPixel;

					//first
					if (startfilldata >= 0 && i == startfilldata)
					{
						m_TagVerts[0] = m_TempVerts[tempVertsIndex].position;
					}

					//third
					if (nextfilldata >= 0 && i == nextfilldata - 1)
					{
						m_TagVerts[1] = m_TempVerts[tempVertsIndex].position;
					}

					if (tempVertsIndex == 3)
					{
						//skip
						if ( startfilldata == -1 || (startfilldata >=0 && i < startfilldata) ||  (nextfilldata >= 0 &&  i > nextfilldata ))
						{
							toFill.AddUIVertexQuad(m_TempVerts);
						}

						if (nextfilldata >=0 && i >= nextfilldata)
						{
							FillNextTag(ref startfilldata,ref nextfilldata,ref fillindex);
						}
					}
				}
			}

			m_DisableFontTextureRebuiltCallback = false;

		}

		void FillNextTag(ref int startfilldata,ref int nextfilldata,ref int fillindex)
		{
			if(_renderTagList != null && fillindex >=0)
			{
				//fill current
				var current = _renderTagList[fillindex];
				current.Fill(m_TagVerts[0],m_TagVerts[1]);

				fillindex++;
				if (fillindex < _renderTagList.Count)
				{
					//update next
					var data = _renderTagList[fillindex];
					nextfilldata = data.GetPositionIdx();
					startfilldata = nextfilldata - (data.GetFillCnt() - 1) * 4 - 3;
				}
				else
				{
					startfilldata = -1;
					nextfilldata = -1;
					fillindex = -1;
				}
			}
		}


		internal List<IFillData> PopEmojiData()
		{
			return _renderTagList;
		}

		void ClearFillData()
		{
			if(_renderTagList != null)
			{
				_renderTagList.Clear();
			}
		}

		internal void AddFillData(IFillData data)
		{
			if(_renderTagList == null)
			{
				_renderTagList = ListPool<IFillData>.Get();
			}
			this._renderTagList.Add(data);
		}

		internal void RemoveFillData(IFillData data)
		{
			if(_renderTagList != null)
			{
				_renderTagList.Remove(data);
			}
		}	
	}
}





