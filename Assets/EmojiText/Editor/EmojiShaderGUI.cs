using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace EmojiText.Taurus
{
	public class EmojiShaderGUI : ShaderGUI
	{

		public override void OnGUI(UnityEditor.MaterialEditor materialEditor, UnityEditor.MaterialProperty[] properties)
		{
			//base.OnGUI(materialEditor, properties);

			//Material targetMat = materialEditor.target as Material;

			//// see if redify is set, and show a checkbox
			//bool redify = Array.IndexOf(targetMat.shaderKeywords, "EMOJI_ANIMATION") != -1;
			//EditorGUI.BeginChangeCheck();
			//redify = EditorGUILayout.Toggle("Emoji Animation", redify);
			//if (EditorGUI.EndChangeCheck())
			//{
			//	// enable or disable the keyword based on checkbox
			//	if (redify)
			//		targetMat.EnableKeyword("EMOJI_ANIMATION");
			//	else
			//		targetMat.DisableKeyword("EMOJI_ANIMATION");
			//}
		}
	}

}