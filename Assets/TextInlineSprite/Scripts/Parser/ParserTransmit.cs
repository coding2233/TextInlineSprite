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



		private const string ParsetLeft = "\\[";

		private const string ParsetRight = "\\]";

		public static int hot = 100;

		private readonly List<IParser> _parsers = new List<IParser>(8);

		private readonly Regex _tagRegex = new Regex(string.Format(@"{0}(\-{{0,1}}\d{{0,}})#(.+?){1}", ParsetLeft, ParsetRight), RegexOptions.Singleline);

		private ParserTransmit()
		{
			//default
			AddParser(new EmojiParser());
		}

		public void AddParser(IParser parser)
		{
			if (Application.isEditor && _parsers.Contains(parser))
			{
				Debug.LogErrorFormat("has contains it  :{0}", parser);
			}

			_parsers.Add(parser);
		}

		public bool RemoveParser(IParser parser)
		{
			return _parsers.Remove(parser);
		}

		public void DoParse(InlineText text, StringBuilder fillbuilder, string content)
		{

			if (_parsers.Count > 0)
			{
				MatchCollection matches = _tagRegex.Matches(content);
				if (matches.Count > 0)
				{
					bool needfix = false;
					int index = 0;
					for (int m = 0; m < matches.Count; ++m)
					{
						Match matchstr = matches[m];

						fillbuilder.Append(content.Substring(index, matchstr.Index - index));

						index = matchstr.Index + matchstr.Length;

						for (int i = 0; i < _parsers.Count; ++i)
						{
							var parser = _parsers[i];

							if (parser.ParsetContent(text,fillbuilder,matchstr,m))
							{
								parser.Hot++;
								if (parser.Hot > hot)
								{
									needfix = true;
								}
							}
						}
						
						if (m == matches.Count - 1)
						{
							fillbuilder.Append(content.Substring(matchstr.Index + matchstr.Length));
						}
					}

					//reset and fix
					if (needfix && this._parsers.Count >1)
						this._parsers.Sort(SortList);

					//
					for (int i = 0; i < _parsers.Count; ++i)
					{
						var parser = _parsers[i];
						parser.Hot = 0;
					}
				}
				else
				{
					fillbuilder.Append(content);
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


