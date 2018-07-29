#define DebugInfo
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Profiling;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System;

namespace EmojiUI
{
	internal static class EmojiTools
	{
#if DebugInfo

		private static long unitymemorysize;

		private static long? last;

		private static DumpClass dumptarget;

		private class DumpClass : MonoBehaviour
		{
			private void OnGUI()
			{
				GUILayout.Label(DumpDebugInfo());
			}
		}

#endif

		public static void StartDumpGUI()
		{

#if DebugInfo
			if (dumptarget == null)
			{
				Camera camera = GameObject.FindObjectOfType<Camera>();
				if (camera != null)
				{
					dumptarget = camera.GetComponent<DumpClass>();
					if (dumptarget == null)
						dumptarget = camera.gameObject.AddComponent<DumpClass>();
					else
						dumptarget.enabled = true;
				}
			}
#endif
		}

		public static void EndDumpGUI()
		{

#if DebugInfo
			if (dumptarget != null)
			{
				Camera camera = GameObject.FindObjectOfType<Camera>();
				if (camera != null)
				{
					dumptarget = camera.GetComponent<DumpClass>();
					if (dumptarget != null)
					{
						dumptarget.enabled = false;
					}
				}
			}
#endif
		}

		static String[] units = new String[] { "B", "KB", "MB", "GB", "TB", "PB" };
		static String Getsize(double size)
		{
			double mod = 1024;
			int i = 0;
			while (size >= mod)
			{
				size /= mod;
				i++;
			}
			return String.Format("{0:0.##} {1}", size, units[i]);
		}

		public static string DumpDebugInfo()
		{

#if DebugInfo
			string result = string.Format("<color=#ff0000ff>c# sharpMemroy :{0} Unity emoji ManagedMemory:{1}</color>\n",
								Getsize(GC.GetTotalMemory(false)),
								Getsize(unitymemorysize));
			Debug.Log(result);
			return result;
#endif
		}

		public static void AddUnityMemory(UnityEngine.Object obj)
		{
#if DebugInfo
			if (obj != null)
				unitymemorysize += Profiler.GetRuntimeMemorySizeLong(obj);
#endif

		}

		public static void RemoveUnityMemory(UnityEngine.Object obj)
		{
#if DebugInfo
			if (obj != null)
				unitymemorysize -= Profiler.GetRuntimeMemorySizeLong(obj);
#endif
		}

		public static void BeginSample(string key)
		{
#if UNITY_EDITOR
			Profiler.BeginSample(key);
#endif
		}

		public static void EndSample()
		{
#if UNITY_EDITOR
			Profiler.EndSample();
#endif
		}
	}
}


