using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton class for a rendered world
/// </summary>
public class World : MonoBehaviour
{
    public Transform Player;
    public static readonly int WorldSizeInChunks = 50;
    public static int WorldSizeInVoxels => WorldSizeInChunks * VoxelData.ChunkWidth;
    public static readonly int ViewDistanceInChunks = 5;
    public Material Material;
    public VoxelSideType[] BlockTypes;

    [SerializeField]
    int GameSeed;
    [SerializeField]
    BiomeAttributes biome;

    readonly ChunkBuilder[,] chunksArray = new ChunkBuilder[WorldSizeInChunks, WorldSizeInChunks];
    readonly List<Vector2> activeChunks = new List<Vector2>();
    Vector3 spawnLocation;
    Vector2 playerLastChunkCoordinates;
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
        if (!playerLastChunkCoordinates.Equals(GetChunkPositionFromVector3(Player.position)))
            CheckViewDistance();
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
        if (yPosition > maximumPerlinNoise - 10 && yPosition <= terrainHeight)
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
        int y = Mathf.FloorToInt(position.z / VoxelData.ChunkWidth);
        return chunksArray[x, y];
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
            for (int y = (WorldSizeInChunks / 2) - ViewDistanceInChunks; y < (WorldSizeInChunks / 2) + ViewDistanceInChunks; y++)
            {
                CreateNewChunk(x, y);
            }
        }
    }

    void CreateNewChunk(int x, int y)
    {
        chunksArray[x, y] = new ChunkBuilder(new Vector2(x, y), this);
        activeChunks.Add(new Vector2(x, y));
    }
    void SpawnPlayer()
    {
        spawnLocation = new Vector3((WorldSizeInChunks * VoxelData.ChunkWidth) / 2f, VoxelData.ChunkHeight - 50, (WorldSizeInChunks * VoxelData.ChunkWidth) / 2f);

        Player.position = spawnLocation;
    }

    bool IsChunkInWorld(Vector2 chunkCoordinate)
    {
        return chunkCoordinate.x > 0 && chunkCoordinate.x < WorldSizeInChunks - 1
            && chunkCoordinate.y > 0 && chunkCoordinate.y < WorldSizeInChunks - 1;
    }

    Vector2 GetChunkPositionFromVector3(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x / VoxelData.ChunkWidth);
        int y = Mathf.FloorToInt(position.z / VoxelData.ChunkWidth);
        return new Vector2(x, y);
    }

    void CheckViewDistance()
    {
        Vector2 chunkCoordinates = GetChunkPositionFromVector3(Player.position);

        int chunkCoordinateX = (int)chunkCoordinates.x;
        int chunkCoordinateY = (int)chunkCoordinates.y;

        List<Vector2> previouslyActiveChunks = new List<Vector2>(activeChunks);

        for (int x = chunkCoordinateX - ViewDistanceInChunks; x < chunkCoordinates.x + ViewDistanceInChunks; x++)
        {
            for (int y = chunkCoordinateY - ViewDistanceInChunks; y < chunkCoordinates.y + ViewDistanceInChunks; y++)
            {
                if (IsChunkInWorld(new Vector2(x, y)))
                {
                    if (chunksArray[x, y] == null)
                        CreateNewChunk(x, y);
                    else if (!chunksArray[x, y].IsActive)
                    {
                        chunksArray[x, y].IsActive = true;
                        activeChunks.Add(new Vector2(x, y));
                    }
                }

                for (int i = 0; i < previouslyActiveChunks.Count; i++)
                {
                    if (previouslyActiveChunks[i].Equals(new Vector2(x, y)))
                    {
                        previouslyActiveChunks.RemoveAt(i);
                    }
                }
            }
        }

        foreach (Vector2 item in previouslyActiveChunks)
            chunksArray[(int)item.x, (int)item.y].IsActive = false;
    }
}