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
            //     EditorGUILayout.ObjectField("", spriteAsset.listSpriteInfor[i].sprite, typeof(Sprite),false);
            EditorGUILayout.IntField("ID:", spriteAsset.listSpriteInfor[i].ID);
            spriteAsset.listSpriteInfor[i].tag=EditorGUILayout.TextField("标签:", spriteAsset.listSpriteInfor[i].tag);
            EditorGUILayout.TextField("name:", spriteAsset.listSpriteInfor[i].name);
         //   EditorGUILayout.Vector2Field("povit:", spriteAsset.listSpriteInfor[i].pivot);
      //      EditorGUILayout.RectField("rect:", spriteAsset.listSpriteInfor[i].rect);
        //    GUILayout.Label("\n");
        }
        //if (EditorGUI.EndChangeCheck())
        //{
          
        //    // 如果我们直接修改属性，而没有通过serializedObject，那么Unity并不会保存这些数据，Unity只会保存那些标识为dirty的属性
        //    //     EditorUtility.SetDirty(spriteAsset);

        //}

        GUILayout.EndScrollView();
        SerializedObject _so = new SerializedObject(spriteAsset);
        _so.ApplyModifiedProperties();
    }
}