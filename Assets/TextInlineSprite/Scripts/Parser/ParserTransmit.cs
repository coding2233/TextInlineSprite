using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        private struct ParserHistory
        {
            
        }

        public void AddParser(IParser parser)
        {
            
        }

        public void RemoveParser(IParser parser)
        {
            
        }
    }
}


