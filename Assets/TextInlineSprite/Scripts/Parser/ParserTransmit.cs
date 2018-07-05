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

        public static ParserTransmit mIns{
            get
            {
                if(_mins == null)
                {
                    _mins = new ParserTransmit();
                }
                return _mins;
            }
        }

        private const string palceholder = "1";

        public static int hot = 100;

        private List<IParser> parsers = new List<IParser>(8);

        private Regex TagRegex = new Regex(@"{\S+}");

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

                        fillbuilder.Append(content.Substring(index, matchstr.Index));

                        index = matchstr.Index + matchstr.Length;

                        for (int i = 0; i < parsers.Count; ++i)
                        {
                            var parser = parsers[i];

                            SpriteTagInfo tagInfo;
                            if (parser.ParsetContent(matchstr, out tagInfo))
                            {
                                parser.Hot++;
                                text.FillSpriteTag(tagInfo);

                                if(parser.Hot > hot)
                                {
                                    needfix = true;
                                }
                            }
                        }
                    }

                    //reset and fix
                    if (needfix)
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
                Debug.LogError("no parse job");
            }


        }

        int SortList(IParser lf,IParser rt)
        {
            return  -lf.Hot + rt.Hot;
        }
    }
}


