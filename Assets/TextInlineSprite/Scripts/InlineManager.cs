/// ========================================================
/// file：InlineManager.cs
/// brief：
/// author： coding2233
/// date：
/// version：v1.0
/// ========================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EmojiUI
{
    public class InlineManager : MonoBehaviour
    {
        private List<SpriteAsset> sharedAtlases = new List<SpriteAsset>();

        private Dictionary<string, SpriteInfoGroup> alltags = new Dictionary<string, SpriteInfoGroup>();

        private IEmojiRender _Render;

        public List<string> PreparedAtlas = new List<string>();

#if UNITY_EDITOR
        [SerializeField]
        private bool _openDebug;
        public bool OpenDebug
        {
            get
            {
                return _openDebug;
            }
            set
            {
                if(_openDebug != value)
                {
                    _openDebug = value;
                    if(Application.isPlaying)
                    {
                        if (value)
                        {
                            EmojiTools.StartDumpGUI();
                        }
                        else
                        {
                            EmojiTools.EndDumpGUI();
                        }
                    }
                }
            }
        }

        private List<SpriteAsset> unityallAtlases;

        private List<string> lostAssets;
#endif
        [SerializeField]
        private float _animationspeed =5f;
        public float AnimationSpeed
        {
            get
            {
                return _animationspeed;
            }
            set
            {
                if(_Render != null)
                {
                    _Render.Speed = value;
                }
                _animationspeed = value;
            }
        }

        [SerializeField]
        private EmojiRenderType _renderType = EmojiRenderType.RenderUnit;
        public EmojiRenderType renderType
        {
            get
            {
                return _renderType;
            }
            set
            {
                if (_renderType != value)
                {
                    _renderType = value;
                    InitRender();
                }
            }
        }

        void Awake()
        {
#if UNITY_EDITOR
            if(OpenDebug)
            {
                EmojiTools.StartDumpGUI();
            }
#endif

            EmojiTools.BeginSample("Emoji_Init");
            Initialize();
            EmojiTools.EndSample();

            EmojiTools.AddUnityMemory(this);
        }

        void Initialize()
        {
#if UNITY_EDITOR
            string[] result = AssetDatabase.FindAssets(string.Format("t:{0}",typeof(SpriteAsset).FullName));

            if (result.Length > 0 && unityallAtlases == null)
            {
                unityallAtlases = new List<SpriteAsset>(result.Length);
                for (int i = 0; i < result.Length; ++i)
                {
                    string path = AssetDatabase.GUIDToAssetPath(result[i]);
                    SpriteAsset asset = AssetDatabase.LoadAssetAtPath<SpriteAsset>(path);
                    if (asset)
                    {
                        unityallAtlases.Add(asset);
                    }
                }
            }
            Debug.LogFormat("find :{0} atlas resource", result.Length);
            Debug.LogWarning("if your asset not in the resources please override InstantiateSpriteAsset");
#endif


            EmojiTools.BeginSample("Emoji_Init");
            InitRender();
            EmojiTools.EndSample();

            EmojiTools.BeginSample("Emoji_preLoad");
            PreLoad();
            EmojiTools.EndSample();

            RebuildTagList();

            ForceRebuild();
        }

        void LateUpdate()
        {
            EmojiTools.BeginSample("Emoji_LateUpdate");
            if(_Render != null)
            {
                _Render.LateUpdate();
            }
            EmojiTools.EndSample();
        }

        private void OnDestroy()
        {
#if UNITY_EDITOR
            if (lostAssets != null)
            {
                for (int i = 0; i < lostAssets.Count; ++i)
                {
                    string asset = lostAssets[i];
                    Debug.LogError(string.Format("not prepred atlasAsset named :{0}", asset));
                }
            }
#endif
            if (_Render != null)
            {
                _Render.Dispose();
            }

            _Render = null;
            EmojiTools.RemoveUnityMemory(this);
        }

        protected virtual SpriteAsset InstantiateSpriteAsset(string filepath)
        {
            return Resources.Load<SpriteAsset>(filepath);
        }

        void InitRender()
        {
            if (_Render == null || _Render.renderType != renderType)
            {

                if (renderType == EmojiRenderType.RenderGroup)
                {
                    EmojiRenderGroup newRender = new EmojiRenderGroup(this);
                    newRender.Speed = AnimationSpeed;

                    if(_Render != null)
                    {
                        List<InlineText> list = _Render.GetAllRenders();
                        if(list != null)
                        {
                            for (int i = 0; i < list.Count; ++i)
                            {
                                InlineText text = list[i];
                                if(text != null)
                                    newRender.TryRendering(text);
                            }
                        }

                        List<SpriteAsset> atlaslist = _Render.GetAllRenderAtlas();
                        if (atlaslist != null)
                        {
                            for (int i = 0; i < atlaslist.Count; ++i)
                            {
                                SpriteAsset atlas = atlaslist[i];
                                if (atlas != null)
                                    newRender.PrepareAtlas(atlas);
                            }
                        }
                        _Render.Dispose();
                    }

                    _Render = newRender;

                }
                else if (renderType == EmojiRenderType.RenderUnit)
                {
                    UnitRender newRender = new UnitRender(this);
                    newRender.Speed = AnimationSpeed;

                    if (_Render != null)
                    {
                        List<InlineText> list = _Render.GetAllRenders();
                        if (list != null)
                        {
                            for (int i = 0; i < list.Count; ++i)
                            {
                                InlineText text = list[i];
                                if (text != null)
                                    newRender.TryRendering(text);
                            }
                        }

                        List<SpriteAsset> atlaslist = _Render.GetAllRenderAtlas();
                        if (atlaslist != null)
                        {
                            for (int i = 0; i < atlaslist.Count; ++i)
                            {
                                SpriteAsset atlas = atlaslist[i];
                                if (atlas != null)
                                    newRender.PrepareAtlas(atlas);
                            }
                        }

                        _Render.Dispose();
                    }

                    _Render = newRender;
                }
                else
                {
                    Debug.LogError("not support yet");
                    this.enabled = false;
                }
            }
        }

        void PreLoad()
        {
            for (int i = 0; i < PreparedAtlas.Count; ++i)
            {
                string atlasname = PreparedAtlas[i];
                string fixname = System.IO.Path.GetFileNameWithoutExtension(atlasname);
                SpriteAsset _spriteAsset = FindAtlas(fixname);
                PushRenderAtlas(_spriteAsset);
            }
        }

        void RebuildTagList()
        {
            EmojiTools.BeginSample("Emoji_rebuildTags");
            alltags.Clear();
#if UNITY_EDITOR
            if(unityallAtlases != null)
            {
                for (int i = 0; i < unityallAtlases.Count; ++i)
                {
                    SpriteAsset asset = unityallAtlases[i];
                    for (int j = 0; j < asset.listSpriteGroup.Count; ++j)
                    {
                        SpriteInfoGroup infogroup = asset.listSpriteGroup[j];
                        SpriteInfoGroup group;
                        if (alltags.TryGetValue(infogroup.tag, out group))
                        {
                            Debug.LogErrorFormat("already exist :{0} ", infogroup.tag);
                        }

                        alltags[infogroup.tag] = infogroup;
                    }
                }
            }

#else
            for (int i = 0; i < sharedAtlases.Count; ++i)
            {
                SpriteAsset asset = sharedAtlases[i];
                for (int j = 0; j < asset.listSpriteGroup.Count; ++j)
                {
                    SpriteInfoGroup infogroup = asset.listSpriteGroup[j];
                    SpriteInfoGroup group;
                    if (alltags.TryGetValue(infogroup.tag, out group))
                    {
                        Debug.LogErrorFormat("already exist :{0} ", infogroup.tag);
                    }

                    alltags[infogroup.tag] = infogroup;
                }
            }
#endif
            EmojiTools.EndSample();
        }

        public IEmojiRender Register(InlineText _key)
        {
            EmojiTools.BeginSample("Emoji_Register");
            if (_Render != null)
            {
                if (_Render.TryRendering(_key))
                {
                    EmojiTools.EndSample();
                    return _Render;
                }
            }
            EmojiTools.EndSample();
            return null;
        }


        /// <summary>
        /// 移除文本 
        /// </summary>
        /// <param name="_id"></param>
        /// <param name="_key"></param>
        public void UnRegister(InlineText _key)
        {
            EmojiTools.BeginSample("Emoji_UnRegister");
            if (_Render != null)
            {
                _Render.DisRendering(_key);
            }
            EmojiTools.EndSample();
        }

        public void ForceRebuild()
        {
            EmojiTools.BeginSample("Emoji_ForceRebuild");
            InlineText[] alltexts = GetComponentsInChildren<InlineText>();
            for (int i = 0; i < alltexts.Length; i++)
            {
                alltexts[i].SetVerticesDirty();
            }
            EmojiTools.EndSample();
        }

        /// <summary>
        /// 清除所有的精灵
        /// </summary>
        public void ClearAllSprites()
        {
            EmojiTools.BeginSample("Emoji_ClearAll");
            if(_Render!= null)
            {
                _Render.Clear();
            }
            EmojiTools.EndSample();
        }

        public bool isRendering(SpriteAsset _spriteAsset)
        {
            return _spriteAsset != null && _Render != null && _Render.isRendingAtlas(_spriteAsset);
        }

        public bool CanRendering(string tagName)
        {
            return alltags.ContainsKey(tagName);
        }

        public bool CanRendering(int atlasId)
        {

#if UNITY_EDITOR
            if (unityallAtlases != null)
            {
                for (int i = 0; i < unityallAtlases.Count; ++i)
                {
                    SpriteAsset asset = unityallAtlases[i];
                    if (asset.ID == atlasId)
                    {
                        return true;
                    }
                }
            }

            return false;
#else
            for (int i = 0; i < sharedAtlases.Count; ++i)
            {
                SpriteAsset asset = sharedAtlases[i];
                if (asset.ID == atlasId)
                {
                    return true;
                }
            }
            return false;
#endif
        }

        public void PushRenderAtlas(SpriteAsset _spriteAsset)
        {
            EmojiTools.BeginSample("Emoji_PushRenderAtlas");
            if (!isRendering(_spriteAsset) && _spriteAsset != null)
            {
                _Render.PrepareAtlas(_spriteAsset);

                if (!sharedAtlases.Contains(_spriteAsset))
                {
                    sharedAtlases.Add(_spriteAsset);
                }
            }
            EmojiTools.EndSample();
        }

        public SpriteInfoGroup FindSpriteGroup(string TagName,out SpriteAsset resultatlas)
        {
            EmojiTools.BeginSample("Emoji_FindSpriteGroup");
#if UNITY_EDITOR

            resultatlas = null;

            SpriteInfoGroup result = null;
            if (unityallAtlases != null)
            {
                for (int i = 0; i < unityallAtlases.Count; ++i)
                {
                    SpriteAsset asset = unityallAtlases[i];
                    for (int j = 0; j < asset.listSpriteGroup.Count; ++j)
                    {
                        SpriteInfoGroup group = asset.listSpriteGroup[j];
                        if (group.tag.Equals(TagName))
                        {
                            result = group;
                            resultatlas = asset;
                            break;
                        }
                    }
                }
            }

            if (lostAssets == null)
                lostAssets = new List<string>();

            if (resultatlas!= null && !PreparedAtlas.Contains(resultatlas.AssetName) )
            {
                if(!lostAssets.Contains(resultatlas.AssetName))
                    lostAssets.Add(resultatlas.AssetName);
            }
            EmojiTools.EndSample();
            return result;
#else
            resultatlas = null;

            SpriteInfoGroup result = null;
            if(sharedAtlases != null)
            {
                for (int i = 0; i < sharedAtlases.Count; ++i)
                {
                    SpriteAsset asset = sharedAtlases[i];
                    for (int j = 0; j < asset.listSpriteGroup.Count; ++j)
                    {
                        SpriteInfoGroup group = asset.listSpriteGroup[j];
                        if (group.tag.Equals(TagName))
                        {
                            result = group;
                            resultatlas = asset;
                            break;
                        }
                    }
                }
            }
            EmojiTools.EndSample();
            return result;
#endif
        }

        public SpriteAsset FindAtlas(int atlasID)
        {
            EmojiTools.BeginSample("Emoji_FindAtlas");
#if UNITY_EDITOR
            SpriteAsset result = null;
            if(unityallAtlases != null)
            {
                for (int i = 0; i < unityallAtlases.Count; ++i)
                {
                    SpriteAsset asset = unityallAtlases[i];
                    if (asset.ID.Equals(atlasID))
                    {
                        result = asset;
                        break;
                    }
                }
            }

            if (lostAssets == null)
                lostAssets = new List<string>();

            if (result != null && !PreparedAtlas.Contains(result.AssetName))
            {
                if (!lostAssets.Contains(result.AssetName))
                    lostAssets.Add(result.AssetName);
            }
            EmojiTools.EndSample();
            return result;
#else
            for (int i = 0; i < sharedAtlases.Count; ++i)
            {
                SpriteAsset asset = sharedAtlases[i];
                if (asset.ID.Equals(atlasID))
                {
                    EmojiTools.EndSample();
                    return asset;
                }
            }
            EmojiTools.EndSample();
            return null;
#endif
        }


        public SpriteAsset FindAtlas(string atlasname)
        {
            EmojiTools.BeginSample("FindAtlas");
#if UNITY_EDITOR
            SpriteAsset result = null;
            if (unityallAtlases != null)
            {
                for (int i = 0; i < unityallAtlases.Count; ++i)
                {
                    SpriteAsset asset = unityallAtlases[i];
                    if (asset.AssetName.Equals(atlasname))
                    {
                        result = asset;
                        break;
                    }
                }
            }

            if (lostAssets == null)
                lostAssets = new List<string>();

            if (!PreparedAtlas.Contains(atlasname))
            {
                if (!lostAssets.Contains(atlasname))
                    lostAssets.Add(atlasname);
            }
            EmojiTools.EndSample();
            return result;
#else
            for (int i = 0; i < sharedAtlases.Count; ++i)
            {
                SpriteAsset asset = sharedAtlases[i];
                if (asset.AssetName.Equals(atlasname))
                {
                    EmojiTools.EndSample();
                    return asset;
                }
            }

            SpriteAsset newasset = InstantiateSpriteAsset(atlasname);
            if(newasset != null)
            {
                sharedAtlases.Add(newasset);
            }
            EmojiTools.EndSample();
            return newasset;
#endif

        }
    }
}


