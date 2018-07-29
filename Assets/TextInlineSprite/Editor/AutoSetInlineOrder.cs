using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[InitializeOnLoad]
public class AutoSetInlineOrder : MonoBehaviour {


#if  UNITY_EDITOR
	static AutoSetInlineOrder()
	{

		foreach (MonoScript monoScript in MonoImporter.GetAllRuntimeMonoScripts())
		{
			if (monoScript.name == "InlineManager")
			{
				if(MonoImporter.GetExecutionOrder(monoScript) != 10)
					MonoImporter.SetExecutionOrder(monoScript, 10);
			}
		}
	}
#endif
}
