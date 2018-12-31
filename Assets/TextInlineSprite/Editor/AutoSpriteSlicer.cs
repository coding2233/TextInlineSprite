using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class AutoSpriteSlicer
{
	public static string ProcessTexture(Texture2D texture, Vector2 size, string[] spriteNames = null)
	{
		string path = AssetDatabase.GetAssetPath(texture);
		var importer = AssetImporter.GetAtPath(path) as TextureImporter;

		//importer.isReadable = true;
		importer.textureType = TextureImporterType.Sprite;
		importer.spriteImportMode = SpriteImportMode.Multiple;
		importer.mipmapEnabled = false;
		importer.filterMode = FilterMode.Point;
		importer.spritePivot = new Vector2(0.5f, 0.5f);
		importer.textureCompression = TextureImporterCompression.Uncompressed;

		var textureSettings = new TextureImporterSettings();
		importer.ReadTextureSettings(textureSettings);
		textureSettings.spriteMeshType = SpriteMeshType.Tight;
		textureSettings.spriteExtrude = 0;

		importer.SetTextureSettings(textureSettings);

        //int minimumSpriteSize = 32;
        //int extrudeSize = 0;
        //Rect[] rects = InternalSpriteUtility.GenerateAutomaticSpriteRectangles(texture, minimumSpriteSize, extrudeSize);
        Vector2 offset = Vector2.zero;
        Vector2 padding = Vector2.zero;
        Rect[] rects = InternalSpriteUtility.GenerateGridSpriteRectangles(texture, offset, size, padding);
        var rectsList = new List<Rect>(rects);
		rectsList = SortRects(rectsList, texture.width);

		string filenameNoExtension = Path.GetFileNameWithoutExtension(path);
		var metas = new List<SpriteMetaData>();
		int rectNum = 0;

		foreach (Rect rect in rectsList)
		{
            var meta = new SpriteMetaData
            {
                pivot = new Vector2(0.5f, 0.5f),
                alignment = (int)SpriteAlignment.Center,
                rect = rect,
            };
            if (null == spriteNames || spriteNames.Length <= 0)
            {
                meta.name = filenameNoExtension + "_" + rectNum;
            }
            else
            {
                meta.name = spriteNames[rectNum];
            }
            rectNum++;
            metas.Add(meta);
		}

		importer.spritesheet = metas.ToArray();

		AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        return path;
	}

	static List<Rect> SortRects(List<Rect> rects, float textureWidth)
	{
		List<Rect> list = new List<Rect>();
		while (rects.Count > 0)
		{
			Rect rect = rects[rects.Count - 1];
			Rect sweepRect = new Rect(0f, rect.yMin, textureWidth, rect.height);
			List<Rect> list2 = RectSweep(rects, sweepRect);
			if (list2.Count <= 0)
			{
				list.AddRange(rects);
				break;
			}
			list.AddRange(list2);
		}
		return list;
	}

	static List<Rect> RectSweep(List<Rect> rects, Rect sweepRect)
	{
		List<Rect> result;
		if (rects == null || rects.Count == 0)
		{
			result = new List<Rect>();
		}
		else
		{
			List<Rect> list = new List<Rect>();
			foreach (Rect current in rects)
			{
				if (current.Overlaps(sweepRect))
				{
					list.Add(current);
				}
			}
			foreach (Rect current2 in list)
			{
				rects.Remove(current2);
			}
			list.Sort((a, b) => a.x.CompareTo(b.x));
			result = list;
		}
		return result;
	}
}