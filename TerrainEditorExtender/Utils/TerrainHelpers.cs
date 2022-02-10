using UnityEngine;

namespace Megalith
{
    public class TerrainHelpers
    {
        public static Vertex[] TerrainBounds(Terrain terrain)
        {
            int terResX = terrain.terrainData.heightmapResolution;
            int terResZ = terrain.terrainData.heightmapResolution;

            return new Vertex[] 
            {
                new Vertex(0, 0f, 0, terrain),
                new Vertex(0, 1f, 0, terrain),

                new Vertex(0, 0f, terResZ, terrain),
                new Vertex(0, 1f, terResZ, terrain),

                new Vertex(terResX, 0f, terResZ, terrain),
                new Vertex(terResX, 1f, terResZ, terrain),

                new Vertex(terResX, 0f, 0, terrain),
                new Vertex(terResX, 1f, 0, terrain)
            };
        }

        public static float HeightmapEdgeLength(Terrain terrain)
        {
            return new Vertex(1, 0f, 0, terrain).WorldPos().x - new Vertex(0, 0f, 0, terrain).WorldPos().x;
        }

        public static float AlphamapEdgeLength(Terrain terrain)
        {
            return new Vertex(1, 0f, 0, terrain, EGridType.AlphaMap).WorldPos().x - new Vertex(0, 0f, 0, terrain, EGridType.AlphaMap).WorldPos().x;
        }
    }

}
