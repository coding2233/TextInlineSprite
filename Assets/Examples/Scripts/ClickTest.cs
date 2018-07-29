/// ========================================================
/// file：ClickTest.cs
/// brief：
/// author： coding2233
/// date：
/// version：v1.0
/// ========================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EmojiUI;

public class ClickTest : MonoBehaviour
{

	private InlineText _text;

	void Awake()
	{
		_text = GetComponent<InlineText>();
	}

	void OnEnable()
	{
		
	}

	void OnDisable()
	{
		
	}

	private void OnHrefClick(string hrefName, int id)
	{
		Debug.Log("点击了 " + hrefName + "  id:" + id);
		//  Application.OpenURL("www.baidu.com");  
	}
}
