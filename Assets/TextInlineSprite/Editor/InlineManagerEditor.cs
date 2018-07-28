using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace EmojiUI
{
    [CustomEditor(typeof(InlineManager),true)]
    [CanEditMultipleObjects]
    public class InlineManagerEditor : Editor
    {
        SerializedProperty m_Script;
    
        private bool foldout =true;

        private Dictionary<string, SpriteAsset> assetDic = new Dictionary<string, SpriteAsset>();

        protected virtual void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");
        }

        public override void OnInspectorGUI()
        {
            InlineManager manager = target as InlineManager;
            EditorGUILayout.PropertyField(m_Script);

            EditorGUILayout.Space();
            serializedObject.Update();

            manager.OpenDebug =EditorGUILayout.Toggle("Debug", manager.OpenDebug);
            manager.RenderType = (EmojiRenderType)EditorGUILayout.EnumPopup("Rendetype", manager.RenderType);
            manager.AnimationSpeed = EditorGUILayout.Slider("AnimationSpeed", manager.AnimationSpeed, 0, 100);

            foldout = EditorGUILayout.Foldout(foldout, "prepared:"+manager.PreparedAtlas.Count);
            if(foldout)
            {
                EditorGUI.indentLevel++;
                for (int i =0; i < manager.PreparedAtlas.Count;++i)
                {
                    string resName = manager.PreparedAtlas[i];
                    SpriteAsset asset = null;
                    if(!string.IsNullOrEmpty(resName))
                    {
                        if (!assetDic.ContainsKey(resName))
                        {

                            string fixname = System.IO.Path.GetFileNameWithoutExtension(resName);
                            string[] allassets = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(SpriteAsset).FullName));
                            for (int j = 0; j < allassets.Length;++j)
                            {
                                string path = AssetDatabase.GUIDToAssetPath( allassets[j]);
                                string subname = System.IO.Path.GetFileNameWithoutExtension(path);
                                if(subname.Equals(fixname))
                                {
                                    asset = assetDic[resName] = AssetDatabase.LoadAssetAtPath<SpriteAsset>(path);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            asset = assetDic[resName];
                        }
                    }

                    SpriteAsset newasset = (SpriteAsset)EditorGUILayout.ObjectField(i.ToString(), asset, typeof(SpriteAsset),false);
                    if(newasset != asset)
                    {
                        if(newasset == null)
                        {
                            manager.PreparedAtlas[i] = "";
                        }
                        else
                        {
                            manager.PreparedAtlas[i] = System.IO.Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(newasset));
                            assetDic[manager.PreparedAtlas[i]] = newasset;
                        }
                        EditorUtility.SetDirty(manager);
                    }

                }

                EditorGUI.indentLevel--;

                EditorGUILayout.BeginHorizontal();

                if(GUILayout.Button("add",GUILayout.Width(100)))
                {
                    manager.PreparedAtlas.Add("");
                }

                if (GUILayout.Button("remove", GUILayout.Width(100)))
                {
                    if (manager.PreparedAtlas.Count > 0)
                        manager.PreparedAtlas.RemoveAt(manager.PreparedAtlas.Count - 1);
                }
                EditorGUILayout.EndHorizontal();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}


