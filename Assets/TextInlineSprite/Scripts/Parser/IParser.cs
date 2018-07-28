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

		bool ParsetContent(InlineText text,StringBuilder textfiller, Match data,int matchindex);

	}

}

