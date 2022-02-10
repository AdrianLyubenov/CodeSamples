using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Megalith
{
    public class MegalithGpuPainter
    {
        private static ComputeShader computeShader;
        private static Texture2D     test;
        private static RenderTexture maskArrayRenderTexture;

        public struct PaintVars
        {
            public readonly Terrain        terrain;
            public readonly Texture2DArray maskArray;

            public string kernelName;

            public readonly TextureLayerModel textureLayer;

            public readonly Texture2D  brush;
            public readonly Vector2Int brushSize;
            public readonly Vector2Int offset;
            public readonly Vector2    uv;

            public readonly float blendingAmountForFill;
            public readonly int   normalMapSampleLevelForFill;

            public readonly float noiseScale;
            public readonly int   noiseIterations;
            public readonly float noiseStrength;

            public readonly RectInt region;
            public readonly int     origWidth;
            public readonly int     origHeight;

            public PaintVars(TextureStampingModel textureStampingModel, MegalithTerrainRenderer terrainRenderer, Texture2DArray maskArray, TextureLayerModel textureLayer,
                             string               kernelName) :
                this(textureStampingModel, terrainRenderer, maskArray, textureLayer, kernelName, Vector2.zero, Texture2D.whiteTexture, Vector2Int.zero)
            {
                this.origWidth  = maskArray.width;
                this.origHeight = maskArray.height;
                this.region     = new RectInt(0, 0, origWidth, origHeight);
            }

            public PaintVars(TextureStampingModel textureStampingModel, MegalithTerrainRenderer terrainRenderer, Texture2DArray maskArray, TextureLayerModel textureLayer,
                             string               kernelName,           Vector2                 uv,
                             Texture2D            brush,                Vector2Int              brushSize)
            {
                this.maskArray    = maskArray;
                this.textureLayer = textureLayer;
                this.kernelName   = kernelName;

                //   brush = textureLayer.brush;

                this.terrain = terrainRenderer.terrain;

                this.noiseScale      = textureStampingModel.NoiseScale;
                this.noiseIterations = textureStampingModel.NoiseIterations;
                this.noiseStrength   = textureStampingModel.NoiseStrength;

                this.blendingAmountForFill       = textureStampingModel.blendingAmountForFill;
                this.normalMapSampleLevelForFill = textureStampingModel.normalMapSampleLevelForFill;


                this.brush     = brush;
                this.uv        = uv;
                this.brushSize = brushSize;

                this.region = GetModifiedRegion(uv, brushSize, maskArray.width, terrain.terrainData, out int origRegionWidth, out int origRegionHeight, out Vector2Int offset);

                this.origWidth  = origRegionWidth;
                this.origHeight = origRegionHeight;
                this.offset     = offset;
            }

            public override string ToString()
            {
                return
                    $"{nameof(terrain)}: {terrain}, {nameof(maskArray)}: {maskArray}, {nameof(kernelName)}: {kernelName}, {nameof(textureLayer)}: {textureLayer}, " +
                    $"{nameof(brush)}: {brush}, {nameof(brushSize)}: {brushSize}, {nameof(offset)}: {offset}, {nameof(uv)}: {uv}, {nameof(blendingAmountForFill)}:" +
                    $" {blendingAmountForFill}, {nameof(normalMapSampleLevelForFill)}: {normalMapSampleLevelForFill}, {nameof(noiseScale)}: {noiseScale}, {nameof(noiseIterations)}: {noiseIterations}, {nameof(noiseStrength)}:" +
                    $" {noiseStrength}, {nameof(region)}: {region}, {nameof(origWidth)}: {origWidth}, {nameof(origHeight)}: {origHeight}";
            }
        }


        private static void SetComputeShader()
        {
            computeShader = (ComputeShader) Resources.Load("Compute/TexturePaint");
        }

        public static void Clear()
        {
            if (maskArrayRenderTexture != null)
            {
                maskArrayRenderTexture.Release();
                GameObject.DestroyImmediate(maskArrayRenderTexture);
                maskArrayRenderTexture = null;
            }

            test          = null;
            computeShader = null;
        }

        public static void FillMask(PaintVars paintVars)
        {
#if UNITY_EDITOR
            Undo.RegisterCompleteObjectUndo(paintVars.maskArray, "Fill Terrain");
#endif

            PaintMask(paintVars);

            Clear();
        }


        public static void PaintMask(PaintVars paintVars, RenderTexture normalMap = null)
        {
            if (computeShader == null)
                SetComputeShader();

            if (computeShader == null)
            {
                Debug.LogError("Compute shader not found unable to fill mask");
                return;
            }


            //possible when painting across terrain boundaries
            if (paintVars.region.width == 0 || paintVars.region.height == 0)
                return;

            var terrainData = paintVars.terrain.terrainData;
            var heightMap   = terrainData.heightmapTexture;

            int kernelHandle = computeShader.FindKernel(paintVars.kernelName);
            int maskIndex    = paintVars.textureLayer.layerIndex / 4;
            int channel      = paintVars.textureLayer.layerIndex % 4;

            var origInstanced = paintVars.terrain.drawInstanced;

            // refresh needed for normal map, cache as texture instead
            if (true)
                RefreshTerrainMaps(kernelHandle, heightMap, normalMap, paintVars.terrain);


            CreateRenderTextureIfNeeded(paintVars);


            for (int i = 0; i < paintVars.maskArray.depth; i++)
            {
                Graphics.CopyTexture(
                    paintVars.maskArray, i, 0, paintVars.region.x, paintVars.region.y, paintVars.region.width, paintVars.region.height,
                    maskArrayRenderTexture, i, 0, 0, 0
                );
            }

            computeShader.SetTexture(kernelHandle, "mask_result", maskArrayRenderTexture);


            computeShader.SetTexture(kernelHandle, "brush", paintVars.brush);
            computeShader.SetInt("channel",    channel);
            computeShader.SetInt("mask_index", maskIndex);

            var terrainRegion = GetModifiedRegion(paintVars.uv, paintVars.brushSize, heightMap.width, terrainData, out int t, out int g, out Vector2Int o);

            computeShader.SetInts("terrain_map_region_start", terrainRegion.x, terrainRegion.y);
            computeShader.SetInts("terrain_map_res",          heightMap.width, heightMap.height);

            computeShader.SetFloat("mask_to_terrain_map_ratio", paintVars.maskArray.width / (float) heightMap.width);
            computeShader.SetFloat("region_width",              paintVars.origWidth);
            computeShader.SetFloat("region_height",             paintVars.origHeight);

            computeShader.SetInt("normal_map_sample_level", paintVars.normalMapSampleLevelForFill);

            computeShader.SetInt("mask_array_depth", paintVars.maskArray.depth);
            computeShader.SetInt("t_width",          (int) paintVars.terrain.terrainData.size.x);
            computeShader.SetInt("t_height",         (int) paintVars.terrain.terrainData.size.y);
            computeShader.SetInt("t_length",         (int) paintVars.terrain.terrainData.size.z);
            computeShader.SetInt("offset_x",         paintVars.offset.x);
            computeShader.SetInt("offset_y",         paintVars.offset.y);

            var strength = paintVars.textureLayer.brushSettings.Opacity;

            if (paintVars.textureLayer.brushSettings.texturingBrushType == ETexturingBrushType.Delete) strength *= -1;

            //noise
            if (paintVars.noiseScale > 0)
            {
                computeShader.SetBool("ApplyNoise", true);
                computeShader.SetFloat("NoiseFreq",       paintVars.noiseScale);
                computeShader.SetFloat("NoiseSeed",       Random.Range(0, 1000));
                computeShader.SetFloat("NoiseIterations", paintVars.noiseIterations);
                computeShader.SetFloat("NoiseStrength",   paintVars.noiseStrength);
                computeShader.SetFloat("NoiseBlending",   paintVars.blendingAmountForFill);
            }
            else
            {
                computeShader.SetBool("ApplyNoise", false);
            }

            computeShader.SetBool("is_grass", paintVars.textureLayer.textureType == ETextureType.TriGrass || paintVars.textureLayer.textureType == ETextureType.QuadGrass);

            computeShader.SetFloat("strength", strength);

            computeShader.SetFloat("slope_min", paintVars.textureLayer.brushSettings.SlopeConstraints.x);
            computeShader.SetFloat("slope_max", paintVars.textureLayer.brushSettings.SlopeConstraints.y);


            var heightMin = paintVars.textureLayer.brushSettings.HeightConstraints.x / terrainData.size.y;
            var heightMax = paintVars.textureLayer.brushSettings.HeightConstraints.y / terrainData.size.y;

            heightMin = Mathf.Min(heightMin, 1);
            heightMax = Mathf.Min(heightMax, 1);

            computeShader.SetFloat("height_min", heightMin);
            computeShader.SetFloat("height_max", heightMax);


            var blending = Mathf.Min(paintVars.blendingAmountForFill / 40f, (heightMax - heightMin) / 4f);
            computeShader.SetFloat("blending", blending);

            computeShader.GetKernelThreadGroupSizes(kernelHandle, out uint xThreadCount, out uint yThreadCount, out uint zThreadCount);

            var groupsX = Mathf.CeilToInt(maskArrayRenderTexture.width / (float) xThreadCount);
            var groupsY = Mathf.CeilToInt(maskArrayRenderTexture.height / (float) yThreadCount);
            // var groupsZ = Mathf.CeilToInt(maskArrayRenderTexture.volumeDepth / (float) zThreadCount);

            var groupsZ = 1;

            computeShader.Dispatch(kernelHandle, Mathf.Max(groupsX, 1), Mathf.Max(groupsY, 1), Mathf.Max(groupsZ, 1));

            UpdateMaskArrayCustom(paintVars);
            //UpdateMaskArraysAsync(paintVars);


            // test                   = null;
            // maskArrayRenderTexture = null;
            // slice                  = null;

            paintVars.terrain.drawInstanced = origInstanced;
        }

        private static void CreateRenderTextureIfNeeded(PaintVars paintVars)
        {
            if (maskArrayRenderTexture == null ||
                maskArrayRenderTexture.width != paintVars.region.width || maskArrayRenderTexture.height != paintVars.region.height ||
                maskArrayRenderTexture.volumeDepth != paintVars.maskArray.depth)
            {
                maskArrayRenderTexture = new RenderTexture(paintVars.region.width, paintVars.region.height, 8)
                                         {
                                             enableRandomWrite = true, volumeDepth = paintVars.maskArray.depth, dimension = TextureDimension.Tex2DArray
                                         };
            }
        }

        private static void UpdateMaskArraysAsync(PaintVars paintVars)
        {
            test = new Texture2D(paintVars.region.width, paintVars.region.height, paintVars.maskArray.format, false, true);
            AsyncGPUReadback.Request(maskArrayRenderTexture, 0, 0, paintVars.region.width, 0, paintVars.region.height, 0, maskArrayRenderTexture.volumeDepth, request =>
            {
                if (request.hasError)
                {
                    return;
                }

                for (var i = 0; i < request.layerCount; i++)
                {
                    test.SetPixels32(request.GetData<Color32>(i).ToArray());
                    Graphics.CopyTexture(
                        test, 0, 0, 0, 0, paintVars.region.width, paintVars.region.height,
                        paintVars.maskArray, i, 0, paintVars.region.x, paintVars.region.y
                    );
                }
            }).WaitForCompletion();

            paintVars.maskArray.Apply();
        }

        private static void UpdateMaskArrayCustom(PaintVars paintVars)
        {
            test = new Texture2D(paintVars.region.width, paintVars.region.height, paintVars.maskArray.format, false, true);

            // slice = new RenderTexture(paintVars.region.width, paintVars.region.height, 8);
            //RenderTexture.active = slice;

            var slice = RenderTexture.GetTemporary(paintVars.region.width, paintVars.region.height);
            RenderTexture.active = slice;

            for (int i = 0; i < paintVars.maskArray.depth; i++)
            {
                Graphics.CopyTexture(
                    maskArrayRenderTexture, i, 0, 0, 0, paintVars.region.width, paintVars.region.height,
                    slice, 0, 0, 0, 0
                );


                test.ReadPixels(new Rect(0, 0, paintVars.region.width, paintVars.region.height), 0, 0, false);
                test.Apply();

                Graphics.CopyTexture(
                    test, 0, 0, 0, 0, paintVars.region.width, paintVars.region.height,
                    paintVars.maskArray, i, 0, paintVars.region.x, paintVars.region.y
                );
            }


            RenderTexture.ReleaseTemporary(slice);
            RenderTexture.active = null;
        }

        private static void RefreshTerrainMaps(int kernelHandle, RenderTexture heightMap, RenderTexture normalMap, Terrain terrain)
        {
            //  if (terrain.drawInstanced == false)


            if (normalMap == null)
            {
                terrain.drawInstanced = true;
                normalMap             = terrain.normalmapTexture;
            }

            computeShader.SetTexture(kernelHandle, "terrain_height_map", heightMap);
            computeShader.SetTexture(kernelHandle, "terrain_normal_map", normalMap);
        }


        public static RectInt GetModifiedRegion(Vector2 uv, Vector2Int brushSize, int res, TerrainData terrainData, out int origRegionWidth, out int origRegionHeight, out Vector2Int offset)
        {
            int x = (int) (res * uv.x);
            int y = (int) (res * uv.y);

            origRegionWidth  = (int) ((brushSize.x * res) / terrainData.size.x);
            origRegionHeight = (int) ((brushSize.y * res) / terrainData.size.z);

            var region = new RectInt {width = origRegionWidth, height = origRegionHeight};

            x -= Mathf.CeilToInt(region.width / 2f);
            y -= Mathf.CeilToInt(region.height / 2f);

            offset = new Vector2Int();

            if (x < 0)
            {
                offset.x     =  x;
                region.width += offset.x;
                x            =  0;
            }
            else if (x + region.width > res)
            {
                region.width += res - (x + region.width);
            }

            if (y < 0)
            {
                offset.y      =  y;
                region.height += offset.y;
                y             =  0;
            }
            else if (y + region.height > res)
            {
                region.height += res - (y + region.height);
            }

            region.x = x;
            region.y = y;

            return region;
        }
    }
}