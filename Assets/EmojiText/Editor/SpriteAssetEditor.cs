using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace EmojiText.Taurus
{
    [CustomEditor(typeof(SpriteAsset))]
    public class SpriteAssetEditor : Editor
    {
        private SpriteAsset _spriteAsset;
        private DrawSpriteAsset _drawSpriteAsset;

        public void OnEnable()
        {
            _spriteAsset = (SpriteAsset)target;

            if(_spriteAsset)
                SetDrawSpriteAsset(_spriteAsset);
        }

        public void OnDisable()
        {
        }

        public override void OnInspectorGUI()
        {
            if (_spriteAsset == null)
                return;

            if (_drawSpriteAsset != null)
            {
                _drawSpriteAsset.Draw();
                _drawSpriteAsset.UpdateSpriteGroup();
            }

            EditorUtility.SetDirty(_spriteAsset);
        }

        //开启预览窗口
        public override bool HasPreviewGUI()
        {
            return true;
        }

        //标题
        public override GUIContent GetPreviewTitle()
        {
            return new GUIContent("Texture Preview");
        }

        //预览上面的按钮
        public override void OnPreviewSettings()
        {
            //  GUILayout.Label("文本", "preLabel");
            if (GUILayout.Button("Open Asset Window", "preButton")&& _spriteAsset!=null)
            {
                CreateSpriteAsset.OpenAsset(_spriteAsset);
            }
        }

        //重新绘制预览界面
        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            // base.OnPreviewGUI(r, background);
            
            if (_spriteAsset && _spriteAsset.TexSource)
            {
                Rect drawRect = r;
                float ratio = drawRect.height / (float)_spriteAsset.TexSource.height;
                float width = ratio * r.width;
                drawRect.x = r.width * 0.5f - width;
                GUI.Label(drawRect, _spriteAsset.TexSource);

                //绘制线
               // DrawTextureLines(drawRect);
            }
        }


        //绘制信息的类
        private void SetDrawSpriteAsset(SpriteAsset spriteAsset)
        {
            //添加
            if (_drawSpriteAsset == null)
                _drawSpriteAsset = new DrawSpriteAsset(spriteAsset);
            else
                _drawSpriteAsset.SetSpriteAsset(spriteAsset);
        }

        //这块窗口的属性  有点乱七八糟
        ////绘制纹理上的线
        //private void DrawTextureLines(Rect rect)
        //{
        //    if (_spriteAsset)
        //    {
        //        Vector2 endPos = rect.position + rect.size;
        //        Handles.BeginGUI();
        //        //行 - line 
        //        if (_spriteAsset.Row > 0)
        //        {
        //            Handles.color = _spriteAsset.IsStatic ? Color.green : Color.red;
        //            float interval = rect.height / _spriteAsset.Row;
        //            for (int i = 0; i <= _spriteAsset.Row; i++)
        //            {
        //                float h = rect.position.y+(interval * i);
        //                Handles.DrawLine(new Vector3(rect.position.x, h), new Vector3(endPos.x, h));
        //            }
        //        }
        //        //列 - line
        //        if (_spriteAsset.Column > 0)
        //        {
        //            Handles.color = Color.green;
        //            float interval = (rect.width* 2.0f) / _spriteAsset.Column;
        //            for (int i = 0; i <= _spriteAsset.Column; i++)
        //            {
        //                float w = rect.position.x+(interval * i);
        //                Handles.DrawLine(new Vector3(w, rect.position.y), new Vector3(w, endPos.y));
        //            }
        //        }

        //        Handles.EndGUI();
        //    }
        //}


    }

}