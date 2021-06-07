using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton class for a rendered world
/// </summary>
public class World : MonoBehaviour
{
    public Transform Player;
    public static readonly int WorldSizeInChunks = 100;
    public static int WorldSizeInVoxels => WorldSizeInChunks * VoxelData.ChunkWidth;
    public static readonly int ViewDistanceInChunks = 5;
    public Material Material;
    public VoxelSideType[] BlockTypes;

    [SerializeField]
    int GameSeed;
    [SerializeField]
    BiomeAttributes biome;

    readonly ChunkBuilder[,] chunksArray = new ChunkBuilder[WorldSizeInChunks, WorldSizeInChunks];
    readonly List<ChunkCoord> activeChunks = new List<ChunkCoord>();
    Vector3 spawnLocation;
    ChunkCoord playerLastChunkCoordinates;
    ChunkCoord playerChunkCoord;
    readonly int maximumPerlinNoise = 78;

    void Start()
    {
        Random.InitState(GameSeed);
        GenerateInitialWorld();
        SpawnPlayer();
        playerLastChunkCoordinates = GetChunkPositionFromVector3(Player.position);
    }

    void Update()
    {
        playerChunkCoord = GetChunkPositionFromVector3(Player.position);

        if (!playerLastChunkCoordinates.Equals(playerChunkCoord))
        {
            CheckViewDistance();
            playerLastChunkCoordinates = GetChunkPositionFromVector3(Player.position);
        }
    }

    public byte GetVoxel(Vector3 position)
    {
        int yPosition = Mathf.FloorToInt(position.y);

        // outside of world = air
        if (!IsVoxelInWorld(position))
            return (byte)BlockType.AirBlock;

        // bottom of chunk = bedrock
        if (yPosition < 5)
            return (byte)BlockType.BedrockBlock;

        int terrainHeight = Mathf.FloorToInt(biome.TerrainHeight * Noise.Get2DPerlin(new Vector2(position.x, position.z), 0, biome.TerrainScale)) + biome.SolidGroundHeight;

        if (yPosition > maximumPerlinNoise - 5 && yPosition <= terrainHeight)
            return (byte)BlockType.SnowBlock;
        if (yPosition > maximumPerlinNoise - 10 && yPosition == terrainHeight)
            return (byte)BlockType.SnowDirtBlock;
        if (yPosition == terrainHeight)
            return (byte)BlockType.GrassBlock;
        else if (yPosition < terrainHeight && yPosition > terrainHeight - 5)
            return (byte)BlockType.DirtBlock;
        else if (yPosition > terrainHeight)
            return (byte)BlockType.AirBlock;
        else
            return (byte)BlockType.StoneBlock;
    }

    public ChunkBuilder GetChunkFromVector3(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(position.z / VoxelData.ChunkWidth);
        return chunksArray[x, z];
    }

    public bool IsVoxelInWorld(Vector3 position)
    {
        return position.x >= 0 && position.x < WorldSizeInVoxels
            && position.y >= 0 && position.y < VoxelData.ChunkHeight
            && position.z >= 0 && position.z < WorldSizeInVoxels;
    }

    /// <summary>
    /// Building chunks only around a player to minimize building the whole world.
    /// The value rendered depends on the viewdistance that is set in the
    /// ViewDistanceInChunks variable.
    /// </summary>
    void GenerateInitialWorld()
    {
        for (int x = (WorldSizeInChunks / 2) - ViewDistanceInChunks; x < (WorldSizeInChunks / 2) + ViewDistanceInChunks; x++)
        {
            for (int z = (WorldSizeInChunks / 2) - ViewDistanceInChunks; z < (WorldSizeInChunks / 2) + ViewDistanceInChunks; z++)
            {
                chunksArray[x, z] = new ChunkBuilder(new ChunkCoord(x, z), this);
                activeChunks.Add(new ChunkCoord(x, z));
            }
        }
    }

    public bool CheckForVoxel(Vector3 pos)
    {
        ChunkCoord thisChunk = new ChunkCoord(pos);

        if (!IsChunkInWorld(thisChunk) || pos.y < 0 || pos.y > VoxelData.ChunkHeight)
            return false;

        if (chunksArray[thisChunk.X, thisChunk.Z] != null && chunksArray[thisChunk.X, thisChunk.Z].IsVoxelMapPopulated)
            return BlockTypes[chunksArray[thisChunk.X, thisChunk.Z].GetVoxelFromGlobalVector3(pos)].IsSolid;

        return BlockTypes[GetVoxel(pos)].IsSolid;
    }

    void SpawnPlayer()
    {
        spawnLocation = new Vector3(WorldSizeInVoxels / 2f, VoxelData.ChunkHeight - 50, (WorldSizeInVoxels) / 2f);

        Player.position = spawnLocation;
    }

    bool IsChunkInWorld(ChunkCoord chunkCoordinate)
    {
        return chunkCoordinate.X >= 0 && chunkCoordinate.X < WorldSizeInChunks
            && chunkCoordinate.Z >= 0 && chunkCoordinate.Z < WorldSizeInChunks;
    }

    ChunkCoord GetChunkPositionFromVector3(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(position.z / VoxelData.ChunkWidth);
        return new ChunkCoord(x, z);
    }

    void CheckViewDistance()
    {
        ChunkCoord chunkCoordinates = GetChunkPositionFromVector3(Player.position);
        playerLastChunkCoordinates = playerChunkCoord;

        List<ChunkCoord> previouslyActiveChunks = new List<ChunkCoord>(activeChunks);

        activeChunks.Clear();

        for (int x = chunkCoordinates.X - ViewDistanceInChunks; x < chunkCoordinates.X + ViewDistanceInChunks; x++)
        {
            for (int z = chunkCoordinates.Z - ViewDistanceInChunks; z < chunkCoordinates.Z + ViewDistanceInChunks; z++)
            {
                ChunkCoord chunkCoord = new ChunkCoord(x, z);

                if (IsChunkInWorld(chunkCoord))
                {
                    if (chunksArray[x, z] == null)
                        chunksArray[x, z] = new ChunkBuilder(chunkCoord, this);

                    chunksArray[x, z].IsActive = true;
                    activeChunks.Add(chunkCoord);
                }

                for (int i = 0; i < previouslyActiveChunks.Count; i++)
                {
                    if (previouslyActiveChunks[i].Equals(chunkCoord))
                        previouslyActiveChunks.RemoveAt(i);
                }
            }
        }

        foreach (ChunkCoord coord in previouslyActiveChunks)
            chunksArray[coord.X, coord.Z].IsActive = false;
    }
}