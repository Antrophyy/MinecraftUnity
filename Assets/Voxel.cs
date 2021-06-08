using UnityEngine;

[System.Serializable]
public class Voxel
{
    public byte BlockTypeId { get; private set; }
    readonly Vector3 position;
    readonly BiomeAttributes biome;

    public Voxel(Vector3 position, BiomeAttributes biome)
    {
        this.position = position;
        this.biome = biome;

        BlockTypeId = DetermineBlockTypeBasedOnHeight();
    }

    public Voxel(BlockType blockType)
    {
        BlockTypeId = (byte)blockType;
    }

    public bool IsVoxelInWorld(Vector3 position)
    {
        return position.x >= 0 && position.x < World.WorldSizeInVoxels
            && position.y >= 0 && position.y < VoxelData.ChunkHeight
            && position.z >= 0 && position.z < World.WorldSizeInVoxels;
    }

    byte DetermineBlockTypeBasedOnHeight()
    {
        int positionY = Mathf.FloorToInt(position.y);
        int highestSolidGroundLevel = Mathf.FloorToInt(biome.TerrainHeight * TerrainHeightRandomizer.PerlinNoise(new Vector2(position.x, position.z), 0, biome.TerrainScale)) + biome.SolidGroundHeight;

        if (!IsVoxelInWorld(position))
            return (byte)BlockType.AirBlock;

        if (positionY < 5)
            return (byte)BlockType.BedrockBlock;

        if (positionY > TerrainHeightRandomizer.MaximumPerlinNoise - 5 && positionY <= highestSolidGroundLevel)
            return (byte)BlockType.SnowBlock;

        if (positionY > TerrainHeightRandomizer.MaximumPerlinNoise - 10 && positionY == highestSolidGroundLevel)
            return (byte)BlockType.SnowDirtBlock;

        if (positionY == highestSolidGroundLevel)
            return (byte)BlockType.GrassBlock;

        else if (positionY < highestSolidGroundLevel && positionY > highestSolidGroundLevel - 5)
            return (byte)BlockType.DirtBlock;

        else if (positionY > highestSolidGroundLevel)
            return (byte)BlockType.AirBlock;

        else
            return (byte)BlockType.StoneBlock;
    }
}
