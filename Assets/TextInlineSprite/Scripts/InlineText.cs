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
		/// <summary>
		/// Security usually means additional performance overhead. If you control the bounding box yourself, this calculation may be redundant.
		/// </summary>
		public static bool safeMode = true;
		
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
		private float? _pw;

		public override float preferredWidth
		{
			get
			{
				if (_pw == null)
				{
					//its override from uGUI Code ,but has bug?

					//var settings = GetGenerationSettings(Vector2.zero);
					//return cachedTextGeneratorForLayout.GetPreferredWidth(_OutputText, settings) / pixelsPerUnit;

					//next idea
					Vector2 extents = rectTransform.rect.size;

					var settings = GetGenerationSettings(extents);
					cachedTextGenerator.Populate(_outputText, settings);

					if (cachedTextGenerator.lineCount > 1)
					{
						float? minx = null;
						float? maxx = null;
						IList<UIVertex> verts = cachedTextGenerator.verts;
						int maxIndex = cachedTextGenerator.lines[1].startCharIdx;

						for (int i = 0, index = 0; i < verts.Count; i += 4, index++)
						{
							UIVertex v0 = verts[i];
							UIVertex v2 = verts[i + 1];
							float min = v0.position.x;
							float max = v2.position.x;

							if (minx.HasValue == false)
							{
								minx = min;
							}
							else
							{
								minx = Mathf.Min(minx.Value, min);
							}

							if (maxx.HasValue == false)
							{
								maxx = max;
							}
							else
							{
								maxx = Mathf.Max(maxx.Value, max);
							}

							if (index > maxIndex)
							{
								break;
							}
						}

						_pw = (maxx - minx);
					}
					else
					{
						//_pw = cachedTextGeneratorForLayout.GetPreferredWidth(_OutputText, settings) / pixelsPerUnit;
						float? minx = null;
						float? maxx = null;
						IList<UIVertex> verts = cachedTextGenerator.verts;
						int maxIndex = cachedTextGenerator.characterCount;

						for (int i = 0, index = 0; i < verts.Count; i += 4, index++)
						{
							UIVertex v0 = verts[i];
							UIVertex v2 = verts[i + 1];
							float min = v0.position.x;
							float max = v2.position.x;

							if (minx.HasValue == false)
							{
								minx = min;
							}
							else
							{
								minx = Mathf.Min(minx.Value, min);
							}

							if (maxx.HasValue == false)
							{
								maxx = max;
							}
							else
							{
								maxx = Mathf.Max(maxx.Value, max);
							}

							if (index > maxIndex)
							{
								break;
							}
						}

						_pw = (maxx - minx);
					}

				}
				return _pw.Value;
			}
		}

		public override float preferredHeight
		{
			get
			{
				var settings = GetGenerationSettings(new Vector2(GetPixelAdjustedRect().size.x, 0.0f));
				return cachedTextGeneratorForLayout.GetPreferredHeight(_outputText, settings) / pixelsPerUnit;
			}
		}

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
			if (Application.isPlaying && this.isActiveAndEnabled)
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
			//

			if(safeMode)
			{
				CalBoundsInSafe();
			}

		}

		void CalBoundsInSafe()
		{
			if(_renderTagList != null && _renderTagList.Count>0)
			{
				Rect rect = rectTransform.rect;
				for (int i = _renderTagList.Count - 1; i >= 0; i--)
				{
					IFillData data = _renderTagList[i];
					if (rect.Contains(data.pos[1]) && rect.Contains(data.pos[3]))
					{
						data.ignore = false;
					}
					else
					{
						data.ignore = true;
					}
				}
			}
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





