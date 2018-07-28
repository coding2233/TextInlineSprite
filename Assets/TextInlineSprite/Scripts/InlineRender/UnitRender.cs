using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EmojiUI
{
	public class UnitRender : IEmojiRender
	{
		public EmojiRenderType renderType { get { return EmojiRenderType.RenderUnit; } }

		public float Speed { get; set; }

		private InlineManager manager;

		private List<SpriteAsset> allatlas;

		private List<InlineText> alltexts;

		private Dictionary<InlineText, List<SpriteGraphic>> textGraphics;

		private Dictionary<Graphic, UnitMeshInfo> renderData;

		private List<InlineText> rebuildqueue;

		private UnitMeshInfo tempMesh;

		private float? _time;

		private static GameObject PoolObj;

		public UnitRender(InlineManager target)
		{
			manager = target;
		}

		public List<InlineText> GetAllRenders()
		{
			return alltexts;
		}

		public List<SpriteAsset> GetAllRenderAtlas()
		{
			return allatlas;
		}

		public void Clear()
		{
			if (alltexts != null)
			{
				alltexts.Clear();
			}

			if (renderData != null)
			{
				renderData.Clear();
			}

			if (allatlas != null)
			{
				allatlas.Clear();
			}

			if (rebuildqueue != null)
			{
				rebuildqueue.Clear();
			}

		}

		public bool isRendingAtlas(SpriteAsset asset)
		{
			if (allatlas != null)
				return allatlas.Contains(asset);
			return false;
		}

		public void PrepareAtlas(SpriteAsset asset)
		{
			if (allatlas == null)
			{
				allatlas = ListPool<SpriteAsset>.Get();
			}

			if (!allatlas.Contains(asset))
			{
				allatlas.Add(asset);
			}
		}

		public bool TryRendering(InlineText text)
		{
			AddInText(text);

			if (rebuildqueue == null)
			{
				rebuildqueue = ListPool<InlineText>.Get();
			}

			if (!rebuildqueue.Contains(text))
			{
				rebuildqueue.Add(text);
				return true;
			}

			return false;
		}

		public void DisRendering(InlineText text)
		{
			RemoveInText(text);

			if (rebuildqueue != null && !rebuildqueue.Contains(text))
			{
				rebuildqueue.Add(text);
			}
		}

		public Texture getRenderTexture(SpriteGraphic graphic)
		{
			if (graphic != null && renderData != null)
			{
				UnitMeshInfo info;
				if (renderData.TryGetValue(graphic, out info))
				{
					return info.getTexture();
				}
			}
			return null;
		}

		public void Dispose()
		{
			if (allatlas != null)
			{
				ListPool<SpriteAsset>.Release(allatlas);
				allatlas = null;
			}

			if (alltexts != null)
			{
				ListPool<InlineText>.Release(alltexts);
				alltexts = null;
			}

			if (tempMesh != null)
			{
				UnitMeshInfoPool.Release(tempMesh);
				tempMesh = null;
			}

			if (textGraphics != null)
			{
				var en = textGraphics.GetEnumerator();
				while (en.MoveNext())
				{
					var list = en.Current.Value;
					for (int i = 0; i < list.Count; ++i)
					{
						var target = list[i];
						if (target != null)
						{
							target.Draw(null);
							target.SetDirtyMask();
							target.SetVerticesDirty();
						}
					}

					ListPool<SpriteGraphic>.Release(en.Current.Value);
				}
				textGraphics = null;
			}

			if (rebuildqueue != null)
			{
				ListPool<InlineText>.Release(rebuildqueue);
				rebuildqueue = null;
			}

			if (renderData != null)
			{
				renderData.Clear();
				renderData = null;
			}

			if (tempMesh != null)
			{
				UnitMeshInfoPool.Release(tempMesh);
				tempMesh = null;
			}

			_time = null;
		}

		public void Release(Graphic graphic)
		{

			if (graphic is SpriteGraphic)
			{
				SpriteGraphic spriteGraphic = graphic as SpriteGraphic;
				if (renderData != null)
					renderData.Remove(spriteGraphic);

				if (textGraphics != null)
				{
					var en = textGraphics.GetEnumerator();
					while (en.MoveNext())
					{
						var list = en.Current.Value;
						for (int i = list.Count - 1; i >= 0; --i)
						{
							var target = list[i];

							if (UnityEngine.Object.ReferenceEquals(target, graphic))
							{
								list.RemoveAt(i);
								break;
							}
						}
					}
				}
			}

		}


		public void LateUpdate()
		{
			if (Application.isPlaying)
			{
				RenderRebuild();
				PlayAnimation();
			}

		}

		void RenderRebuild()
		{
			EmojiTools.BeginSample("Emoji_UnitRebuild");
			if (rebuildqueue != null && rebuildqueue.Count > 0)
			{
				for (int i = 0; i < rebuildqueue.Count; ++i)
				{
					InlineText text = rebuildqueue[i];
					List<IFillData> emojidata = text.PopEmojiData();
					if (emojidata != null && emojidata.Count > 0 && allatlas != null && allatlas.Count > 0)
					{
						if (tempMesh == null)
							tempMesh = UnitMeshInfoPool.Get();

						if (renderData == null)
							renderData = new Dictionary<Graphic, UnitMeshInfo>(emojidata.Count);

						for (int j = 0; j < emojidata.Count; ++j)
						{
							IFillData taginfo = emojidata[j];
							if (taginfo == null || taginfo.ignore)
								continue;
							Parse(text, taginfo);
						}

						List<SpriteGraphic> list;
						if (textGraphics != null && textGraphics.TryGetValue(text, out list))
						{
							for (int j = 0; j < list.Count; ++j)
							{
								SpriteGraphic graphic = list[j];
								//not render
								if (graphic != null && !graphic.isDirty)
								{
									graphic.Draw(null);
									graphic.SetDirtyMask();
									graphic.SetVerticesDirty();
								}
							}
						}
					}
					else
					{
						List<SpriteGraphic> list;
						if (textGraphics != null && textGraphics.TryGetValue(text, out list))
						{
							for (int j = 0; j < list.Count; ++j)
							{
								SpriteGraphic graphic = list[j];
								//not render
								if (graphic != null)
								{
									graphic.Draw(null);
									graphic.SetDirtyMask();
									graphic.SetVerticesDirty();
								}
							}
						}
					}
				}

				rebuildqueue.Clear();
			}
			EmojiTools.EndSample();
		}

		void PlayAnimation()
		{
			EmojiTools.BeginSample("Emoji_UnitAnimation");
			if (alltexts != null)
			{
				if (_time == null)
				{
					_time = Time.timeSinceLevelLoad;
				}
				else
				{
					float deltatime = Time.timeSinceLevelLoad - _time.Value;
					//at least one frame
					int framecnt = Mathf.RoundToInt(deltatime * Speed);
					if (framecnt > 0)
					{
						for (int i = 0; i < alltexts.Count; ++i)
						{
							InlineText text = alltexts[i];
							List<IFillData> emojidata = text.PopEmojiData();
							if (emojidata != null && emojidata.Count > 0 && allatlas != null && allatlas.Count > 0)
							{
								for (int j = 0; j < emojidata.Count; ++j)
								{
									IFillData taginfo = emojidata[j];
									if (taginfo == null || taginfo.ignore)
										continue;

									SpriteAsset asset = null;
									SpriteInfoGroup groupinfo = manager.FindSpriteGroup(taginfo.Tag, out asset);
									if (groupinfo != null && groupinfo.spritegroups.Count > 1)
									{
										int index = framecnt % groupinfo.spritegroups.Count;
										SpriteInfo sprInfo = groupinfo.spritegroups[index];
										taginfo.uv = sprInfo.uv;

										List<SpriteGraphic> list = null;
										SpriteGraphic target = FindGraphic(text, asset, out list);
										if (target)
										{
											if (tempMesh == null)
												tempMesh = UnitMeshInfoPool.Get();

											if (renderData == null)
												renderData = new Dictionary<Graphic, UnitMeshInfo>(emojidata.Count);

											RefreshSubUIMesh(text, target, asset, taginfo.pos, taginfo.uv);
										}
									}
								}
							}
						}
					}
				}
			}

			EmojiTools.EndSample();
		}

		SpriteGraphic Parse(InlineText text, IFillData taginfo)
		{
			if (taginfo != null)
			{
				return ParsePosAndUV(text, taginfo.ID, taginfo.pos, taginfo.uv);
			}
			return null;
		}

		SpriteGraphic ParsePosAndUV(InlineText text, int ID, Vector3[] Pos, Vector2[] UV)
		{
			EmojiTools.BeginSample("Emoji_UnitParsePosAndUV");
			SpriteAsset matchAsset = null;
			for (int i = 0; i < allatlas.Count; ++i)
			{
				SpriteAsset asset = allatlas[i];
				if (asset != null && asset.ID == ID)
				{
					matchAsset = asset;
					break;
				}
			}

			if (matchAsset && matchAsset.texSource != null)
			{
				List<SpriteGraphic> list = null;
				SpriteGraphic target = FindGraphic(text, matchAsset, out list);
				if (!target)
				{
					target = CreateSpriteRender(text.transform);
					list.Add(target);
				}

				RefreshSubUIMesh(text, target, matchAsset, Pos, UV);

				EmojiTools.EndSample();
				return target;
			}
			else
			{
				Debug.LogErrorFormat("missing {0} atlas", ID);
			}
			EmojiTools.EndSample();
			return null;
		}

		void RefreshSubUIMesh(InlineText text, SpriteGraphic target, SpriteAsset matchAsset, Vector3[] Pos, Vector2[] UV)
		{
			// set mesh
			tempMesh.SetAtlas(matchAsset);
			tempMesh.SetUVLen(UV.Length);
			tempMesh.SetVertLen(Pos.Length);

			for (int i = 0; i < Pos.Length; ++i)
			{
				//text object pos
				Vector3 value = Pos[i];
				Vector3 worldpos = text.transform.TransformPoint(value);
				Vector3 localpos = target.transform.InverseTransformPoint(worldpos);
				tempMesh.SetVert(i, localpos);
			}

			for (int i = 0; i < UV.Length; ++i)
			{
				Vector2 value = UV[i];
				tempMesh.SetUV(i, value);
			}

			//rendermesh
			UnitMeshInfo currentMesh = null;
			if (!this.renderData.TryGetValue(target, out currentMesh))
			{
				currentMesh = new UnitMeshInfo();
				renderData[target] = currentMesh;
			}

			if (!currentMesh.Equals(tempMesh))
			{
				if (target.isDirty)
				{
					currentMesh.AddCopy(tempMesh);
					tempMesh.Clear();
				}
				else
				{
					currentMesh.Copy(tempMesh);
				}
			}

			if (currentMesh.VertCnt() > 3 && currentMesh.UVCnt() > 3)
			{
				target.Draw(this);
				target.SetDirtyMask();
				target.SetVerticesDirty();
			}
			else
			{
				target.Draw(null);
				target.SetDirtyMask();
				target.SetVerticesDirty();
			}
		}

		public void FillMesh(Graphic graphic, VertexHelper vh)
		{
			UnitMeshInfo rendermesh;
			if (renderData != null && renderData.Count > 0 && renderData.TryGetValue(graphic, out rendermesh))
			{
				if (rendermesh != null && rendermesh.getTexture() != null)
				{
					int vertcnt = rendermesh.VertCnt();
					int uvcnt = rendermesh.UVCnt();
					if (vertcnt != uvcnt)
					{
						Debug.LogError("data error");
					}
					else
					{
						for (int i = 0; i < vertcnt; ++i)
						{
							vh.AddVert(rendermesh.GetVert(i), graphic.color, rendermesh.GetUV(i));
						}

						int cnt = vertcnt / 4;
						for (int i = 0; i < cnt; ++i)
						{
							int m = i * 4;

							vh.AddTriangle(m, m + 1, m + 2);
							vh.AddTriangle(m + 2, m + 3, m);
						}

						//vh.AddTriangle(0, 1, 2);
						//vh.AddTriangle(2, 3, 0);
					}
				}
			}
		}

		public void DrawGizmos(Graphic graphic)
		{
			UnitMeshInfo rendermesh;
			if (renderData != null && renderData.Count > 0 && renderData.TryGetValue(graphic, out rendermesh))
			{
				if (rendermesh != null && rendermesh.getTexture() != null)
				{
					Gizmos.color = Color.red;
					int vertcnt = rendermesh.VertCnt();
					int uvcnt = rendermesh.UVCnt();
					if (vertcnt != uvcnt)
					{
						Debug.LogError("data error");
					}
					else
					{
						for (int i = 0; i < vertcnt; i += 4)
						{
							Vector3 p1 = getPoint(graphic, rendermesh.GetVert(i));
							Vector3 p2 = getPoint(graphic, rendermesh.GetVert(i + 1));
							Vector3 p3 = getPoint(graphic, rendermesh.GetVert(i + 2));
							Vector3 p4 = getPoint(graphic, rendermesh.GetVert(i + 3));

							Gizmos.DrawLine(p1, p2);
							Gizmos.DrawLine(p2, p3);
							Gizmos.DrawLine(p3, p4);
							Gizmos.DrawLine(p4, p1);
						}
					}
				}
			}

		}

		Vector3 getPoint(Graphic graphic, Vector3 v)
		{
			Vector3 worldpos = graphic.transform.TransformPoint(v);
			return worldpos;
		}

		#region 考虑到后面测试和调整的时候数据类型的转换

		SpriteGraphic FindGraphic(InlineText text, int atlasID, out SpriteAsset matchAsset)
		{
			matchAsset = null;
			for (int i = 0; i < allatlas.Count; ++i)
			{
				SpriteAsset asset = allatlas[i];
				if (asset != null && asset.ID == atlasID)
				{
					matchAsset = asset;
					break;
				}
			}

			if (matchAsset && matchAsset.texSource != null)
			{
				List<SpriteGraphic> list = null;
				SpriteGraphic target = FindGraphic(text, matchAsset, out list);
				if (!target)
				{
					target = CreateSpriteRender(text.transform);
					list.Add(target);
				}
				return target;
			}
			return null;
		}


		SpriteGraphic FindGraphic(InlineText text, SpriteAsset matchAsset, out List<SpriteGraphic> list)
		{
			EmojiTools.BeginSample("Emoji_UnitFindGraphic");
			if (textGraphics == null)
				textGraphics = new Dictionary<InlineText, List<SpriteGraphic>>();

			if (!textGraphics.TryGetValue(text, out list))
			{
				list = ListPool<SpriteGraphic>.Get();
				textGraphics[text] = list;
			}

			SpriteGraphic target = null;
			for (int i = 0; i < list.Count; ++i)
			{
				SpriteGraphic graphic = list[i];
				if (graphic && UnityEngine.Object.ReferenceEquals(graphic.mainTexture, matchAsset.texSource))
				{
					target = graphic;
					break;
				}
			}
			EmojiTools.EndSample();
			return target;
		}

		void AddInText(InlineText text)
		{
			if (alltexts == null)
			{
				alltexts = ListPool<InlineText>.Get();
			}

			if (!alltexts.Contains(text))
			{
				alltexts.Add(text);
			}
		}

		void RemoveInText(InlineText text)
		{
			if (alltexts != null)
			{
				alltexts.Remove(text);
			}
		}

		#endregion

		SpriteGraphic CreateSpriteRender(Transform targetTrans)
		{
			if (PoolObj == null)
			{
				PoolObj = new GameObject("UnitRenderPool");
				SpriteGraphic target = null;
				for (int i = 0; i < 10; i++)
				{
					target = CreateInstance(PoolObj.transform);
				}

				target.transform.SetParent(targetTrans);
				return target;
			}
			else
			{
				int childcnt = PoolObj.transform.childCount;
				if (childcnt > 0)
				{
					Transform trans = PoolObj.transform.GetChild(0);
					trans.SetParent(targetTrans);
					return trans.GetComponent<SpriteGraphic>();
				}

				return CreateInstance(targetTrans);
			}
		}

		SpriteGraphic CreateInstance(Transform targetTrans)
		{
			GameObject newobject = new GameObject("pre_Sprite");
			newobject.transform.SetParent(targetTrans);
			newobject.transform.localPosition = Vector3.zero;
			newobject.transform.localScale = Vector3.one;
			newobject.transform.localRotation = Quaternion.identity;
			newobject.transform.gameObject.layer = targetTrans.gameObject.layer;
			newobject.transform.tag = targetTrans.gameObject.tag;
			//newobject.transform.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;

			SpriteGraphic spritegrahic = newobject.AddComponent<SpriteGraphic>();
			spritegrahic.raycastTarget = false;
			return spritegrahic;
		}

	}

}

