using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Text;

namespace EmojiUI
{
	public interface IParser
	{
		int Hot { get; set; }

		bool ParsetContent(Match data, ref ParsedData parsedData);

		void DoFillText(InlineText text, StringBuilder stringBuilder, Match match, int Index, ParsedData tagInfo);

		void DoFillMesh();

		void RecordTextUpdate(InlineText text);
	}

}

