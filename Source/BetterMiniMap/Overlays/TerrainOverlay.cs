using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace BetterMiniMap.Overlays
{
    [StaticConstructorOnStartup]
    public static class TerrainColors
    {
        public static Dictionary<ushort, Color32> colorMapping;

        // NOTE: need to handle dynamic stuff (like fluffy's floor)
        // REFERENCE: https://support.unity3d.com/hc/en-us/articles/206486626-How-can-I-get-pixels-from-unreadable-textures-
        static TerrainColors()
        {
            colorMapping = new Dictionary<ushort, Color32>();
            Texture2D nonReadableTexture;
            ulong r, g, b;
            uint count;
            Color32 averagedColor;

            // stash current RenderTexture
            RenderTexture previous = RenderTexture.active;

            foreach (TerrainDef terrainDef in DefDatabase<TerrainDef>.AllDefs)
            {
                nonReadableTexture = ContentFinder<Texture2D>.Get(terrainDef.texturePath);

                RenderTexture tmp = RenderTexture.GetTemporary(nonReadableTexture.width, nonReadableTexture.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
                Graphics.Blit(nonReadableTexture, tmp);
                RenderTexture.active = tmp;

                Texture2D textureCopy = new Texture2D(nonReadableTexture.width, nonReadableTexture.height, TextureFormat.ARGB32, false);

                textureCopy.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
                textureCopy.Apply(false);

                count = 0;
                r = g = b = 0;
                foreach(Color32 color in textureCopy.GetPixels32())
                {
                    r += color.r;
                    g += color.g;
                    b += color.b;
                    count++;
                }
                averagedColor = new Color32((byte)Math.Min(255, r / count), (byte)Math.Min(255, g / count), (byte)Math.Min(255, b / count), 255);
                
                colorMapping.Add(terrainDef.shortHash, averagedColor);

                RenderTexture.active = null;
                RenderTexture.ReleaseTemporary(tmp);

                Texture2D.DestroyImmediate(textureCopy);
            }

            // restore
            RenderTexture.active = previous;
        }

    }

	public class TerrainOverlay : Overlay, IExposable
	{
        public TerrainOverlay(Map map, bool visible = true) : base(map, visible) { }

        public void ExposeData() => this.ExposeData("overlayTerrain");

		public override int GetUpdateInterval() => BetterMiniMapMod.modSettings.updatePeriods.terrain;

        public void Update() => base.Update(false);

        public override void Render()
		{
            Color32[] pixels = new Color32[this.map.cellIndices.NumGridCells];
			for (int i = 0; i < this.map.cellIndices.NumGridCells; i++)
			{
				Color curCol = this.map.terrainGrid.TerrainAt(i).color;
				if (curCol != Color.white)
					pixels[i] = curCol;
				else
					pixels[i] = TerrainColors.colorMapping[this.map.terrainGrid.TerrainAt(i).shortHash];
			}
            base.Texture.SetPixels32(pixels);
        }

    }
}
