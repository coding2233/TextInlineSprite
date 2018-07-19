using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EmojiUI
{
	public enum EmojiRenderType
	{
		RenderGroup,
		RenderUnit,
		RenderBoth,
	}

	public interface IEmojiRender
	{
		EmojiRenderType renderType { get; }

		float Speed { get; set; }

		List<InlineText> GetAllRenders();

		List<SpriteAsset> GetAllRenderAtlas();

		Texture getRenderTexture(SpriteGraphic graphic);

		bool isRendingAtlas(SpriteAsset asset);

		void PrepareAtlas(SpriteAsset asset);

		bool TryRendering(InlineText text);

		void DisRendering(InlineText text);

		void Clear();

		void Release(Graphic graphic);

		void FillMesh(Graphic graphic, VertexHelper vh);

		void LateUpdate();

		void DrawGizmos(Graphic graphic);

		void Dispose();
	}
}


