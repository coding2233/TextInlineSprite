using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmojiUI
{
    public interface IParser
    {
        int Hot { get; set; }

        bool ParsetContent(string data);
    }

}

