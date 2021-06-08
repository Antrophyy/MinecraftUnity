using UnityEngine;

/// <summary>
/// The class is used for terrain height differences generation. This can
/// be later on improved to generate 3D Perlin noise that can be used to generate
/// a random blocks inside the terrain. E.g. sands within stone biom.
/// </summary>
public static class TerrainHeightRandomizer
{
    public static int MaximumPerlinNoise = 78;
    public static float PerlinNoise(Vector2 position, float offset, float scale)
    {
        return Mathf.PerlinNoise((position.x + 0.1f) / VoxelData.ChunkWidth * scale + offset, (position.y + 0.1f) / VoxelData.ChunkWidth * scale + offset);
    }
}
