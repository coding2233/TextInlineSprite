using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(SpriteAsset))]
public class SpriteAssetEditor : Editor
{
    private SpriteAsset spriteAsset;
    public void OnEnable()
    {
        spriteAsset = (SpriteAsset)target;
    }
    private Vector2 ve2ScorllView;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        ve2ScorllView = GUILayout.BeginScrollView(ve2ScorllView);
      
         GUILayout.EndScrollView();
        //  EditorUtility.SetDirty(spriteAsset);
    }

}