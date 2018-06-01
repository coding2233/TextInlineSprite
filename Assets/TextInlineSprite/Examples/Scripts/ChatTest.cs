/// ========================================================
/// file：ChatTest.cs
/// brief：
/// author： coding2233
/// date：
/// version：v1.0
/// ========================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatTest : MonoBehaviour {

    [SerializeField]
    private GameObject _PreChatItem;
    [SerializeField]
    private RectTransform _ChatParent;
    [SerializeField]
    private RectTransform _ViewContent;
    [SerializeField]
    private InputField _InputText;
    
    Vector2 _ChatTextSize = new Vector2(330.0f, 26.0f);
    float _ViewHight = 0.0f;
	
    #region 点击发送
    public void OnClickSend()
    {
        string _chatString = _InputText.text;
        if (string.IsNullOrEmpty(_chatString))
            return;

        GameObject _chatClone = Instantiate(_PreChatItem);
        _chatClone.transform.SetParent( _ChatParent);
        InlineText _chatText = _chatClone.transform.Find("Text").GetComponent<InlineText>();
        Image _chatImage= _chatClone.transform.Find("BG").GetComponent<Image>();
        _chatText.text = _chatString;
      //  _chatText.ActiveText();
        Vector2 _imagSize = _ChatTextSize;
        if (_chatText.preferredWidth < _ChatTextSize.x)
            _imagSize.x = _chatText.preferredWidth+0.3f;
        if(_chatText.preferredHeight> _ChatTextSize.y)
            _imagSize.y = _chatText.preferredHeight+0.8f;
        _chatImage.rectTransform.sizeDelta = _imagSize;
        Vector2 _pos = new Vector2(0.0f, _ViewHight);
        _chatClone.GetComponent<RectTransform>().anchoredPosition= _pos;
    
        _ViewHight += -_imagSize.y - 20.0f;
        _ViewContent.sizeDelta = new Vector2(_ViewContent.sizeDelta.x,Mathf.Abs( _ViewHight));
    }
    #endregion
}
