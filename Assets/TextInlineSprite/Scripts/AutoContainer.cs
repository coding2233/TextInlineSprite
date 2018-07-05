using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
namespace EmojiUI
{
    public class AutoContainer 
    {
#if UNITY_EDITOR
        public static int hot = 100;
#else
        public const int hot = 100;
#endif
        private List<IParser> parsers = new List<IParser>(8);

        private Regex TagRegex = new Regex(@"{\S+}");

        private List<SpriteTagInfo> tagList;

        public void Add(IParser data)
        {
#if UNITY_EDITOR
            if(parsers.Contains(data))
            {
                Debug.LogErrorFormat("has contains it  :{0}", data);
            }
#endif
            parsers.Add(data);
        }

        public bool Remove(IParser data)
        {
            return parsers.Remove(data);
        }

        public void DoParser(string content)
        {
            if (tagList == null)
                tagList = new List<SpriteTagInfo>(8);
            else
                tagList.Clear();

            for (int i = 0; i < parsers.Count;++i)
            {
                var data = parsers[i];

            }
        }
    }

}

