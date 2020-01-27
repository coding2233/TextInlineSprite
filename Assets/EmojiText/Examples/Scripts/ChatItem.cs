using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wanderer.EmojiText
{
    public class ChatItem : MonoBehaviour
    {

        [SerializeField]
        private InlineText _inlineText;
        [SerializeField]
        private RectTransform _itemRect;
        [SerializeField]
        private RectTransform _itemBgRect;

        public string Text
        {
            get { return _inlineText.text; }
            set
            {

                _inlineText.text = value;

                //加5.0f的偏移值 是为了更加美观-- 具体范围可以看scene视图的蓝色线框
                Vector2 size = new Vector2(_inlineText.preferredWidth + 5.0f, _inlineText.preferredHeight + 5.0f);

                _itemRect.sizeDelta = size;
                _itemBgRect.sizeDelta = size;
            }
        }

    }
}
