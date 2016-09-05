using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(InlineSpriteManager))]
public class InlineSpriteManagerEditor : Editor {
    InlineSpriteManager inlineManager;
    SpriteAsset spriteAsset;
    public void OnEnable()
    {
        inlineManager = (InlineSpriteManager)target;
        spriteAsset = inlineManager.m_spriteAsset;
    }

    private Vector2 ve2ScorllView;
    public override void OnInspectorGUI()
    {
        ve2ScorllView = GUILayout.BeginScrollView(ve2ScorllView);
        GUILayout.Label("Sprite ID:");
        if (spriteAsset.listSpriteInfor == null)
            return;
        for (int i = 0; i < spriteAsset.listSpriteInfor.Count; i++)
        {
            GUILayout.Label("\n");
            EditorGUILayout.IntField("ID:", spriteAsset.listSpriteInfor[i].ID);
            EditorGUILayout.ObjectField("", spriteAsset.listSpriteInfor[i].sprite, typeof(Sprite), false);
        }
        GUILayout.EndScrollView();
    }
}
