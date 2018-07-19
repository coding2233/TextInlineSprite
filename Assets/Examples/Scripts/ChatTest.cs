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
using EmojiUI;

public class ChatTest : MonoBehaviour
{

	[SerializeField]
	private GameObject _PreChatItem;
	[SerializeField]
	private RectTransform _ChatParent;
	[SerializeField]
	private RectTransform _ViewContent;
	[SerializeField]
	private InputField _InputText;

	public int testcnt;

	#region 点击发送
	public void OnClickSend()
	{
		string _chatString = _InputText.text;
		if (string.IsNullOrEmpty(_chatString))
			return;

		for (int i = 0; i < testcnt; ++i)
		{
			GameObject _chatClone = Instantiate(_PreChatItem);
			_chatClone.transform.SetParent(_ChatParent);

			InlineText _chatText = _chatClone.GetComponentInChildren<InlineText>();
			if (_chatText != null)
			{
				_chatText.text = _chatString;
			}
		}

	}
	#endregion
}
