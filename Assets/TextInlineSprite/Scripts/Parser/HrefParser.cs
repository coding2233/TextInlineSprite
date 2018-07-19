using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Text;

namespace EmojiUI
{
	public class HrefParser : IParser
	{

		public int Hot { get; set; }

		public void DoFillMesh()
		{
			throw new System.NotImplementedException();
		}

		public void RecordTextUpdate(InlineText text)
		{
			throw new System.NotImplementedException();
		}

		public void DoFillText(InlineText text, StringBuilder stringBuilder, Match match, int Index, ParsedData tagInfo)
		{
			throw new System.NotImplementedException();
		}

		public bool ParsetContent(Match data, ref ParsedData parsedData)
		{
			return false;
		}

	}

}
