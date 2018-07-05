using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        public void DoStep()
        {
            bool needfix = false;
            for (int i = 0; i < parsers.Count;++i)
            {
                var data = parsers[i];

            }

            //reset
            
        }
    }

}

