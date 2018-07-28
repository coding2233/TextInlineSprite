#define EMOJI_RUNTIME
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
		private readonly List<SpriteAsset> _sharedAtlases = new List<SpriteAsset>();

		private readonly Dictionary<string, SpriteInfoGroup> _alltags = new Dictionary<string, SpriteInfoGroup>();

		private readonly Dictionary<string, KeyValuePair<SpriteAsset, SpriteInfoGroup>> _spritemap = new Dictionary<string, KeyValuePair<SpriteAsset, SpriteInfoGroup>>();

		private IEmojiRender _render;

		public List<string> PreparedAtlas = new List<string>();

		public bool HasInit { get; private set; }

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
				if (_openDebug != value)
				{
					_openDebug = value;
					if (Application.isPlaying)
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

		private List<SpriteAsset> _unityallAtlases;

		private List<string> _lostAssets;
#endif
		[SerializeField]
		private float _animationspeed = 5f;
		public float AnimationSpeed
		{
			get
			{
				return _animationspeed;
			}
			set
			{
				if (_render != null)
				{
					_render.Speed = value;
				}
				_animationspeed = value;
			}
		}

		[SerializeField]
		private EmojiRenderType _renderType = EmojiRenderType.RenderUnit;
		public EmojiRenderType RenderType
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
			if (OpenDebug)
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
			HasInit = true;
#if UNITY_EDITOR
			string[] result = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(SpriteAsset).FullName));

			if (result.Length > 0 && _unityallAtlases == null)
			{
				_unityallAtlases = new List<SpriteAsset>(result.Length);
				for (int i = 0; i < result.Length; ++i)
				{
					string path = AssetDatabase.GUIDToAssetPath(result[i]);
					SpriteAsset asset = AssetDatabase.LoadAssetAtPath<SpriteAsset>(path);
					if (asset)
					{
						_unityallAtlases.Add(asset);
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
			if (_render != null)
			{
				_render.LateUpdate();
			}
			EmojiTools.EndSample();
		}

		private void OnDestroy()
		{
#if UNITY_EDITOR
			if (_lostAssets != null)
			{
				for (int i = 0; i < _lostAssets.Count; ++i)
				{
					string asset = _lostAssets[i];
					Debug.LogError(string.Format("not prepred atlasAsset named :{0}", asset));
				}
			}
#endif
			if (_render != null)
			{
				_render.Dispose();
			}

			_render = null;
			EmojiTools.RemoveUnityMemory(this);
		}

		protected virtual SpriteAsset InstantiateSpriteAsset(string filepath)
		{
			return Resources.Load<SpriteAsset>(filepath);
		}

		void InitRender()
		{
			if (_render == null || _render.renderType != RenderType)
			{

				if (RenderType == EmojiRenderType.RenderGroup)
				{
					EmojiRenderGroup newRender = new EmojiRenderGroup(this);
					newRender.Speed = AnimationSpeed;

					if (_render != null)
					{
						List<InlineText> list = _render.GetAllRenders();
						if (list != null)
						{
							for (int i = 0; i < list.Count; ++i)
							{
								InlineText text = list[i];
								if (text != null)
									newRender.TryRendering(text);
							}
						}

						List<SpriteAsset> atlaslist = _render.GetAllRenderAtlas();
						if (atlaslist != null)
						{
							for (int i = 0; i < atlaslist.Count; ++i)
							{
								SpriteAsset atlas = atlaslist[i];
								if (atlas != null)
									newRender.PrepareAtlas(atlas);
							}
						}
						_render.Dispose();
					}

					_render = newRender;

				}
				else if (RenderType == EmojiRenderType.RenderUnit)
				{
					UnitRender newRender = new UnitRender(this);
					newRender.Speed = AnimationSpeed;

					if (_render != null)
					{
						List<InlineText> list = _render.GetAllRenders();
						if (list != null)
						{
							for (int i = 0; i < list.Count; ++i)
							{
								InlineText text = list[i];
								if (text != null)
									newRender.TryRendering(text);
							}
						}

						List<SpriteAsset> atlaslist = _render.GetAllRenderAtlas();
						if (atlaslist != null)
						{
							for (int i = 0; i < atlaslist.Count; ++i)
							{
								SpriteAsset atlas = atlaslist[i];
								if (atlas != null)
									newRender.PrepareAtlas(atlas);
							}
						}

						_render.Dispose();
					}

					_render = newRender;
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
			_alltags.Clear();
			_spritemap.Clear();
#if UNITY_EDITOR && !EMOJI_RUNTIME
			if (_unityallAtlases != null)
			{
				for (int i = 0; i < _unityallAtlases.Count; ++i)
				{
					SpriteAsset asset = _unityallAtlases[i];
					for (int j = 0; j < asset.listSpriteGroup.Count; ++j)
					{
						SpriteInfoGroup infogroup = asset.listSpriteGroup[j];
						SpriteInfoGroup group;
						if (_alltags.TryGetValue(infogroup.tag, out group))
						{
							Debug.LogErrorFormat("already exist :{0} ", infogroup.tag);
						}

						_alltags[infogroup.tag] = infogroup;
					}
				}
			}

#else
			for (int i = 0; i < _sharedAtlases.Count; ++i)
			{
				SpriteAsset asset = _sharedAtlases[i];
				for (int j = 0; j < asset.listSpriteGroup.Count; ++j)
				{
					SpriteInfoGroup infogroup = asset.listSpriteGroup[j];
					SpriteInfoGroup group;
					if (_alltags.TryGetValue(infogroup.tag, out group))
					{
						Debug.LogErrorFormat("already exist :{0} ", infogroup.tag);
					}

					_alltags[infogroup.tag] = infogroup;
					_spritemap[infogroup.tag] = new KeyValuePair<SpriteAsset, SpriteInfoGroup>(asset, infogroup);
				}
			}
#endif
			EmojiTools.EndSample();
		}

		public IEmojiRender Register(InlineText _key)
		{
			EmojiTools.BeginSample("Emoji_Register");
			if (_render != null)
			{
				if (_render.TryRendering(_key))
				{
					EmojiTools.EndSample();
					return _render;
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
			if (_render != null)
			{
				_render.DisRendering(_key);
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
			if (_render != null)
			{
				_render.Clear();
			}
			EmojiTools.EndSample();
		}

		public bool isRendering(SpriteAsset _spriteAsset)
		{
			return _spriteAsset != null && _render != null && _render.isRendingAtlas(_spriteAsset);
		}

		public bool CanRendering(string tagName)
		{
			return _alltags != null && _alltags.ContainsKey(tagName);
		}

		public bool CanRendering(int atlasId)
		{

#if UNITY_EDITOR && !EMOJI_RUNTIME
			if (_unityallAtlases != null)
			{
				for (int i = 0; i < _unityallAtlases.Count; ++i)
				{
					SpriteAsset asset = _unityallAtlases[i];
					if (asset.ID == atlasId)
					{
						return true;
					}
				}
			}

			return false;
#else
			for (int i = 0; i < _sharedAtlases.Count; ++i)
			{
				SpriteAsset asset = _sharedAtlases[i];
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
				_render.PrepareAtlas(_spriteAsset);

				if (!_sharedAtlases.Contains(_spriteAsset))
				{
					_sharedAtlases.Add(_spriteAsset);
				}
			}
			EmojiTools.EndSample();
		}

		public SpriteInfoGroup FindSpriteGroup(string TagName, out SpriteAsset resultatlas)
		{
			EmojiTools.BeginSample("Emoji_FindSpriteGroup");
#if UNITY_EDITOR && !EMOJI_RUNTIME

			resultatlas = null;
			SpriteInfoGroup result = null;
			if (_unityallAtlases != null)
			{
				for (int i = 0; i < _unityallAtlases.Count; ++i)
				{
					SpriteAsset asset = _unityallAtlases[i];
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

			if (_lostAssets == null)
				_lostAssets = new List<string>();

			if (resultatlas != null && !PreparedAtlas.Contains(resultatlas.AssetName))
			{
				if (!_lostAssets.Contains(resultatlas.AssetName))
					_lostAssets.Add(resultatlas.AssetName);
			}
			EmojiTools.EndSample();
			return result;
#else
			resultatlas = null;
			SpriteInfoGroup result = null;
			KeyValuePair<SpriteAsset, SpriteInfoGroup> data;
			if (_spritemap.TryGetValue(TagName,out data))
			{
				result = data.Value;
				resultatlas = data.Key;
			}
			EmojiTools.EndSample();
			return result;
#endif
		}

		public SpriteAsset FindAtlas(int atlasID)
		{
			EmojiTools.BeginSample("Emoji_FindAtlas");
#if UNITY_EDITOR && !EMOJI_RUNTIME
			SpriteAsset result = null;
			if (_unityallAtlases != null)
			{
				for (int i = 0; i < _unityallAtlases.Count; ++i)
				{
					SpriteAsset asset = _unityallAtlases[i];
					if (asset.ID.Equals(atlasID))
					{
						result = asset;
						break;
					}
				}
			}

			if (_lostAssets == null)
				_lostAssets = new List<string>();

			if (result != null && !PreparedAtlas.Contains(result.AssetName))
			{
				if (!_lostAssets.Contains(result.AssetName))
					_lostAssets.Add(result.AssetName);
			}
			EmojiTools.EndSample();
			return result;
#else
			for (int i = 0; i < _sharedAtlases.Count; ++i)
			{
				SpriteAsset asset = _sharedAtlases[i];
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
#if UNITY_EDITOR && !EMOJI_RUNTIME
			SpriteAsset result = null;
			if (_unityallAtlases != null)
			{
				for (int i = 0; i < _unityallAtlases.Count; ++i)
				{
					SpriteAsset asset = _unityallAtlases[i];
					if (asset.AssetName.Equals(atlasname))
					{
						result = asset;
						break;
					}
				}
			}

			if (_lostAssets == null)
				_lostAssets = new List<string>();

			if (!PreparedAtlas.Contains(atlasname))
			{
				if (!_lostAssets.Contains(atlasname))
					_lostAssets.Add(atlasname);
			}
			EmojiTools.EndSample();
			return result;
#else
			for (int i = 0; i < _sharedAtlases.Count; ++i)
			{
				SpriteAsset asset = _sharedAtlases[i];
				if (asset.AssetName.Equals(atlasname))
				{
					EmojiTools.EndSample();
					return asset;
				}
			}

			SpriteAsset newasset = InstantiateSpriteAsset(atlasname);
			if (newasset != null)
			{
				_sharedAtlases.Add(newasset);
			}
			EmojiTools.EndSample();
			return newasset;
#endif

		}
	}
}


