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
		
		private TextGenerator _SpaceGen;
		private List<Vector3> _listVertsPos;
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

		List<IFillData> RenderTagList;

		private string _OutputText = "";
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
					cachedTextGenerator.Populate(_OutputText, settings);

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
				return cachedTextGeneratorForLayout.GetPreferredHeight(_OutputText, settings) / pixelsPerUnit;
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

#if UNITY_EDITOR
		protected override void OnValidate()
		{
			//do nothing
		}
#endif

		public override void SetVerticesDirty()
		{
			base.SetVerticesDirty();
			if (!Application.isPlaying || !Manager)
			{
				_pw = null;
				_OutputText = m_Text;
				return;
			}

			ParserTransmit.mIns.DoParser(this, _textBuilder, m_Text);

			_OutputText = _textBuilder.ToString();
			_textBuilder.Length = 0;
			Debug.LogError(_OutputText);

		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			if (RenderTagList != null)
			{
				ListPool<IFillData>.Release(RenderTagList);
				RenderTagList = null;
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
			cachedTextGenerator.PopulateWithErrors(_OutputText, settings, gameObject);

			// Apply the offset to the vertices
			IList<UIVertex> verts = cachedTextGenerator.verts;
			float unitsPerPixel = 1 / pixelsPerUnit;
			//Last 4 verts are always a new line... (\n)
			int vertCount = verts.Count - 4;
			Vector2 roundingOffset = new Vector2(verts[0].position.x, verts[0].position.y) * unitsPerPixel;
			roundingOffset = PixelAdjustPoint(roundingOffset) - roundingOffset;
			toFill.Clear();

			//ClearQuadUVs(verts);

			if (RenderTagList != null && RenderTagList.Count > 0)
			{
				if (_listVertsPos == null)
					_listVertsPos = new List<Vector3>();
				else
					_listVertsPos.Clear();
			}


			int NextSkipMin = -10;
			int NextSkipMax = -10;
			int TagIndex = 0;
			if (RenderTagList != null && RenderTagList.Count > 0)
			{
				var data = RenderTagList[TagIndex];
				NextSkipMin = data.GetPositionIdx();
				NextSkipMax = NextSkipMin + 4 * data.GetPlaceHolderCnt();
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

					if (NextSkipMin >= 0 && i >= NextSkipMin && i <= NextSkipMax)
					{

					}
					else
					{
						if (tempVertsIndex == 3)
							toFill.AddUIVertexQuad(m_TempVerts);
					}


					if (RenderTagList != null && RenderTagList.Count > 0)
					{
						if (i == NextSkipMax)
						{
							TagIndex++;
							if (RenderTagList.Count > TagIndex)
							{
								var data = RenderTagList[TagIndex];
								NextSkipMin = data.GetPositionIdx();
								NextSkipMax = NextSkipMin + 4 * data.GetPlaceHolderCnt();
							}
							else
							{
								NextSkipMin = -10;
							}
						}

						_listVertsPos.Add(m_TempVerts[tempVertsIndex].position);
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

					if (NextSkipMin >= 0 && i >= NextSkipMin && i <= NextSkipMax)
					{

					}
					else
					{
						if (tempVertsIndex == 3)
							toFill.AddUIVertexQuad(m_TempVerts);
					}

					if (RenderTagList != null && RenderTagList.Count > 0)
					{
						if (i == NextSkipMax)
						{
							TagIndex++;
							if (RenderTagList.Count > TagIndex)
							{
								var data = RenderTagList[TagIndex];
								NextSkipMin = data.GetPositionIdx();
								NextSkipMax = NextSkipMin + 4 * data.GetPlaceHolderCnt();
							}
							else
							{
								NextSkipMin = -10;
							}
						}

						_listVertsPos.Add(m_TempVerts[tempVertsIndex].position);
					}
				}
			}

			m_DisableFontTextureRebuiltCallback = false;
			needupdate = false;
		}


		public List<IFillData> PopEmojiData()
		{
			return RenderTagList;
		}


		internal void FillSpriteTag(StringBuilder stringBuilder, Match match, int Index, ParsedData tagInfo)
		{
			int Id = tagInfo.atlasID;
			string TagName = tagInfo.atlasTag;
			if (Manager != null && Manager.CanRendering(Id) && Manager.CanRendering(TagName))
			{
				SpriteAsset sprAsset;
				SpriteInfoGroup tagSprites = Manager.FindSpriteGroup(TagName, out sprAsset);
				if (tagSprites != null && tagSprites.spritegroups.Count > 0)
				{
					if (!Manager.isRendering(sprAsset))
					{
						Manager.PushRenderAtlas(sprAsset);
					}

					if (_SpaceGen == null)
					{
						_SpaceGen = new TextGenerator();
					}

					if (updatespace)
					{
						Vector2 extents = rectTransform.rect.size;
						TextGenerationSettings settings = GetGenerationSettings(extents);
						_SpaceGen.Populate(palceholder, settings);
						updatespace = false;
					}

					IList<UIVertex> spaceverts = _SpaceGen.verts;
					float spacewid = spaceverts[1].position.x - spaceverts[0].position.x;
					float spaceheight = spaceverts[0].position.y - spaceverts[3].position.y;


					float autosize = Mathf.Min(tagSprites.size, Mathf.Max(spacewid, spaceheight));
					float spacesize = Mathf.Max(spacewid, spaceheight);

					int fillspacecnt = Mathf.CeilToInt(autosize / spacesize);

					for (int i = 0; i < fillspacecnt; i++)
					{
						stringBuilder.Append(palceholder);
					}

					if (RenderTagList == null)
					{
						RenderTagList = ListPool<IFillData>.Get();
					}

					if (RenderTagList.Count > Index)
					{
						SpriteTagInfo _tempSpriteTag = RenderTagList[Index];
						_tempSpriteTag._ID = Id;
						_tempSpriteTag._Tag = TagName;
						_tempSpriteTag._Size = new Vector2(autosize, autosize);
						_tempSpriteTag.FillIdxAndPlaceHolder(match.Index, fillspacecnt);
						_tempSpriteTag._UV = tagSprites.spritegroups[0].uv;
					}
					else
					{
						SpriteTagInfo _tempSpriteTag = new SpriteTagInfo
						{
							_ID = Id,
							_Tag = TagName,
							_Size = new Vector2(autosize, autosize),
							_Pos = new Vector3[4],
							_UV = tagSprites.spritegroups[0].uv
						};

						_tempSpriteTag.FillIdxAndPlaceHolder(match.Index, fillspacecnt);

						RenderTagList.Add(_tempSpriteTag);
					}
				}

			}
		}


	}
}





