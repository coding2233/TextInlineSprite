using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Text;

namespace EmojiUI
{
	public class EmojiParser : IParser
	{
		private static TextGenerator _UnderlineText;
		public int Hot { get; set; }

		public void DoFillMesh()
		{
			
		}

		public void RecordTextUpdate(InlineText text)
		{
			throw new System.NotImplementedException();
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


		public bool ParsetContent(InlineText text,StringBuilder textfiller, Match data,int matchindex)
		{

			string value = data.Value;
			if (!string.IsNullOrEmpty(value))
			{
				int index = value.IndexOf('#');
				int atlasId = 0;
				string tagKey = null;
				if (index != -1)
				{
					string subID = value.Substring(1, index - 1);
					if (subID.Length > 0 && !int.TryParse(subID, out atlasId))
					{
						Debug.LogErrorFormat("{0} convert failed ", subID);
					}
					else if (subID.Length > 0)
					{
						atlasId = -1;
					}
					else if (subID.Length == 0)
					{
						atlasId = 0;
					}

					tagKey = value.Substring(index + 1, value.Length - index - 2);

					parsedData.atlasID = atlasId;
					parsedData.atlasTag = tagKey;

				}
				else
				{
					tagKey = value.Substring(1, value.Length - 2);

					parsedData.atlasID = atlasId;
					parsedData.atlasTag = tagKey;

				}
				return true;
			}

			return false;
		}


	}
}


