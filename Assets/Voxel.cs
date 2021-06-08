using UnityEngine;

/// <summary>
/// Voxel is a different name for cubes in the game. 
/// This voxel allows us to hide a different sides of the cube or entire cubes
/// in order to get better performance.
/// </summary>
[System.Serializable]
public class Voxel
{
    [Header("Voxel Details")]
    public BlockType BlockType;
    public bool IsSolid;
    public float TimeToDestroy;
    public byte BlockTypeId { get => (byte)BlockType; }

    [Header("2D Side Textures")]
    [SerializeField]
    int sideTexture;
    [SerializeField]
    int topTexture;
    [SerializeField]
    int bottomTexture;

    readonly Vector3 position;
    readonly BiomeAttributes biome;
    readonly World world;

    public Voxel(Vector3 position, BiomeAttributes biome, World world)
    {
        this.position = position;
        this.biome = biome;
        this.world = world;

        BlockType = DetermineBlockTypeBasedOnHeight();
        AssignPropertiesBasedOnType();
    }

    public Voxel(BlockType blockType)
    {
        BlockType = blockType;
    }

    public bool IsVoxelInWorld(Vector3 position)
    {
        return position.x >= 0 && position.x < World.WorldSizeInVoxels
            && position.y >= 0 && position.y < VoxelData.ChunkHeight
            && position.z >= 0 && position.z < World.WorldSizeInVoxels;
    }

    /// <summary>
    /// In order to be able to set the values from the inspector and not have them hard coded,
    /// this method takes values from inspector array and copies them over to the created voxels 
    /// within code.
    /// </summary>
    void AssignPropertiesBasedOnType()
    {
        foreach (var voxel in world.VoxelTypes)
        {
            if (voxel.BlockType == BlockType)
            {
                IsSolid = voxel.IsSolid;
                TimeToDestroy = voxel.TimeToDestroy;
            }
        }
    }

    /// <summary>
    /// The method uses PerlinNoise to generate a height differences in terrain. Based on this value
    /// and the Y coordinate of the voxel, it will be decided what kind of block will it be.
    /// </summary>
    /// <returns>BlockType that will be placed into the world.</returns>
    BlockType DetermineBlockTypeBasedOnHeight()
    {
        int positionY = Mathf.FloorToInt(position.y);
        int highestSolidGroundLevel = Mathf.FloorToInt(biome.TerrainHeight * TerrainHeightRandomizer.PerlinNoise(new Vector2(position.x, position.z), 0, biome.TerrainScale)) + biome.SolidGroundHeight;

        if (!IsVoxelInWorld(position))
            return BlockType.AirBlock;
        if (positionY < 5)
            return BlockType.BedrockBlock;
        if (positionY > TerrainHeightRandomizer.MaximumPerlinNoise - 5 && positionY <= highestSolidGroundLevel)
            return BlockType.SnowBlock;
        if (positionY > TerrainHeightRandomizer.MaximumPerlinNoise - 10 && positionY == highestSolidGroundLevel)
            return BlockType.SnowDirtBlock;
        if (positionY == highestSolidGroundLevel)
            return BlockType.GrassBlock;
        else if (positionY < highestSolidGroundLevel && positionY > highestSolidGroundLevel - 5)
            return BlockType.DirtBlock;
        else if (positionY > highestSolidGroundLevel)
            return BlockType.AirBlock;
        else
            return BlockType.StoneBlock;
    }

    public int GetTextureID(int sideIndex)
    {
        return sideIndex switch
        {
            0 => sideTexture,
            1 => sideTexture,
            2 => topTexture,
            3 => bottomTexture,
            4 => sideTexture,
            5 => sideTexture,
            _ => sideTexture,
        };
    }
}
