using UnityEngine;

public static class TerrainHeightRandomizer
{
    public static int MaximumPerlinNoise = 78;
    public static float PerlinNoise(Vector2 position, float offset, float scale)
    {
        return Mathf.PerlinNoise((position.x + 0.1f) / VoxelData.ChunkWidth * scale + offset, (position.y + 0.1f) / VoxelData.ChunkWidth * scale + offset);
    }
}
