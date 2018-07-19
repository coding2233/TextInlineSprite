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

		public void DoFillText(InlineText text, StringBuilder stringBuilder, Match match, int Index, ParsedData tagInfo)
		{
			
		}

		public void RecordTextUpdate(InlineText text)
		{
			throw new System.NotImplementedException();
		}

		public bool ParsetContent(Match data, ref ParsedData parsedData)
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


