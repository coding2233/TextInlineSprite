using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(SpriteAsset))]
public class SpriteAssetEditor : Editor
{
    SpriteAsset spriteAsset;

    public void OnEnable()
    {
        spriteAsset = (SpriteAsset)target;
    }
    private Vector2 ve2ScorllView;
    public override void OnInspectorGUI()
    {
        ve2ScorllView = GUILayout.BeginScrollView(ve2ScorllView);
        GUILayout.Label("UGUI Sprite Asset");
        if (spriteAsset.listSpriteInfor == null)
            return;
        for (int i = 0; i < spriteAsset.listSpriteInfor.Count; i++)
        {
            GUILayout.Label("\n");
            EditorGUILayout.ObjectField("", spriteAsset.listSpriteInfor[i].sprite, typeof(Sprite),false);
            EditorGUILayout.IntField("ID:", spriteAsset.listSpriteInfor[i].ID);
            EditorGUILayout.LabelField("name:", spriteAsset.listSpriteInfor[i].name);
            EditorGUILayout.Vector2Field("povit:", spriteAsset.listSpriteInfor[i].pivot);
            EditorGUILayout.RectField("rect:", spriteAsset.listSpriteInfor[i].rect);
            GUILayout.Label("\n");
        }
        GUILayout.EndScrollView();
    }
}