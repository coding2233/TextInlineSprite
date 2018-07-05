using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

namespace EmojiUI
{
    public interface IParser
    {
        int Hot { get; set; }

        Regex regex { get; }

        bool ParsetContent(string data);
    }

}

