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
		
		private const string palceholder = "1";

		private TextGenerator _SpaceGen;

		private int cacheframecnt;

		private InlineText cachetext;

		public void DoFillMesh()
		{
			
		}

		public void RecordTextUpdate(InlineText text)
		{
			throw new System.NotImplementedException();
		}

		void FillSpriteTag(InlineText text,StringBuilder stringBuilder, Match match,int matchindex, int atlasId,string atlasTag)
		{

			if (text.Manager != null && text.Manager.CanRendering(atlasId) && text.Manager.CanRendering(atlasTag))
			{
				SpriteAsset sprAsset;
				SpriteInfoGroup tagSprites = text.Manager.FindSpriteGroup(atlasTag, out sprAsset);
				if (tagSprites != null && tagSprites.spritegroups.Count > 0)
				{
					if (!text.Manager.isRendering(sprAsset))
					{
						text.Manager.PushRenderAtlas(sprAsset);
					}

					if (_SpaceGen == null)
					{
						_SpaceGen = new TextGenerator();
					}

					if (cacheframecnt != Time.frameCount || cachetext != text)
					{
						cacheframecnt = Time.frameCount;
						cachetext = text;
						
						Vector2 extents = text.rectTransform.rect.size;
						TextGenerationSettings settings = text.GetGenerationSettings(extents);
						_SpaceGen.Populate(palceholder, settings);
	
					}

					IList<UIVertex> spaceverts = _SpaceGen.verts;
					float spacewid = spaceverts[1].position.x - spaceverts[0].position.x;
					float spaceheight = spaceverts[0].position.y - spaceverts[3].position.y;

					float autosize = Mathf.Min(tagSprites.size, Mathf.Max(spacewid, spaceheight));
					float spacesize = Mathf.Min(spacewid, spaceheight);

					int fillspacecnt = Mathf.RoundToInt(autosize / spacesize);

					for (int i = 0; i < fillspacecnt; i++)
					{
						stringBuilder.Append(palceholder);
					}

					if (fillspacecnt > 0)
					{
						SpriteTagInfo tempSpriteTag = new SpriteTagInfo();
						tempSpriteTag.ID = atlasId;
						tempSpriteTag.Tag = atlasTag;
						tempSpriteTag.Size = new Vector2(autosize, autosize);
						tempSpriteTag.pos = new Vector3[4];
						tempSpriteTag.uv = tagSprites.spritegroups[0].uv;
					
			
						tempSpriteTag.FillIdxAndPlaceHolder(stringBuilder.Length * 4-1,matchindex,fillspacecnt);

						text.AddFillData(tempSpriteTag);
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
					string subId = value.Substring(1, index - 1);
					if (subId.Length > 0 && int.TryParse(subId, out atlasId))
					{
						//Debug.LogErrorFormat("{0} convert success ", subId);
					}
					else if (subId.Length > 0)
					{
						atlasId = -1;
					}
					else if (subId.Length == 0)
					{
						atlasId = 0;
					}

					tagKey = value.Substring(index + 1, value.Length - index - 2);
					
					FillSpriteTag(text,textfiller,data,matchindex,atlasId,tagKey);


				}
				else
				{
					tagKey = value.Substring(1, value.Length - 2);
					FillSpriteTag(text,textfiller,data,matchindex,atlasId,tagKey);

				}
				return true;
			}

			return false;
		}


	}
}


