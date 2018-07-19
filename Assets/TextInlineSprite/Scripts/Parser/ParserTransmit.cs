using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Text.RegularExpressions;


namespace EmojiUI
{
	public class ParserTransmit
	{

		private static ParserTransmit _mins;

		public static ParserTransmit mIns
		{
			get
			{
				if (_mins == null)
				{
					_mins = new ParserTransmit();
				}
				return _mins;
			}
		}

		private const string palceholder = "1";

		private const string ParsetLeft = "\\[";

		private const string ParsetRight = "\\]";

		public static int hot = 100;

		private List<IParser> parsers = new List<IParser>(8);

		private Regex TagRegex = new Regex(string.Format(@"{0}(\-{{0,1}}\d{{0,}})#(.+?){1}", ParsetLeft, ParsetRight), RegexOptions.Singleline);

		private ParserTransmit()
		{
			//default
			AddParser(new EmojiParser());
		}

		public void AddParser(IParser parser)
		{
			if (Application.isEditor && parsers.Contains(parser))
			{
				Debug.LogErrorFormat("has contains it  :{0}", parser);
			}

			parsers.Add(parser);
		}

		public bool RemoveParser(IParser parser)
		{
			return parsers.Remove(parser);
		}

		public void DoParser(InlineText text, StringBuilder fillbuilder, string content)
		{
			if (parsers.Count > 0)
			{
				MatchCollection matches = TagRegex.Matches(content);
				if (matches.Count > 0)
				{
					bool needfix = false;
					int index = 0;
					for (int m = 0; m < matches.Count; ++m)
					{
						Match matchstr = matches[m];

						fillbuilder.Append(content.Substring(index, matchstr.Index - index));

						if (m == matches.Count - 1)
						{
							fillbuilder.Append(content.Substring(matchstr.Index + matchstr.Length));
						}

						index = matchstr.Index + matchstr.Length;

						for (int i = 0; i < parsers.Count; ++i)
						{
							var parser = parsers[i];

							ParsedData tagInfo = new ParsedData();
							if (parser.ParsetContent(matchstr, ref tagInfo))
							{
								parser.Hot++;

								parser.DoFillText(fillbuilder, matchstr, m, tagInfo);
		
								if (parser.Hot > hot)
								{
									needfix = true;
								}
							}
						}
					}

					//reset and fix
					if (needfix && this.parsers.Count >1)
						this.parsers.Sort(SortList);

					//
					for (int i = 0; i < parsers.Count; ++i)
					{
						var parser = parsers[i];
						parser.Hot = 0;
					}
				}
			}
			else
			{
				fillbuilder.Append(content);
			}
		}

		int SortList(IParser lf, IParser rt)
		{
			return -lf.Hot + rt.Hot;
		}
	}
}


