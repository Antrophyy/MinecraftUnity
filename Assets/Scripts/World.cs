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
    public VoxelDetails[] BlockTypes;

    [SerializeField]
    int GameSeed;
    [SerializeField]
    BiomeAttributes biome;

    readonly Chunk[,] chunksArray = new Chunk[WorldSizeInChunks, WorldSizeInChunks];
    readonly List<ChunkCoord> activeChunks = new List<ChunkCoord>();
    Vector3 spawnLocation;
    ChunkCoord playerLastChunkCoordinates;
    ChunkCoord playerChunkCoord;

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

    public Chunk GetChunkFromVector3(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(position.z / VoxelData.ChunkWidth);
        return chunksArray[x, z];
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
                chunksArray[x, z] = new Chunk(new ChunkCoord(x, z), this, biome);
                activeChunks.Add(new ChunkCoord(x, z));
            }
        }
    }

    public bool VoxelExistsAndIsSolid(Vector3 pos)
    {
        ChunkCoord thisChunk = new ChunkCoord(pos);

        if (!IsChunkInWorld(thisChunk) || pos.y < 0 || pos.y > VoxelData.ChunkHeight)
            return false;

        if (chunksArray[thisChunk.X, thisChunk.Z] != null && chunksArray[thisChunk.X, thisChunk.Z].IsVoxelMapPopulated)
            return BlockTypes[chunksArray[thisChunk.X, thisChunk.Z].GetVoxelFromGlobalVector3(pos).BlockTypeId].IsSolid;

        return BlockTypes[new Voxel(pos, biome).BlockTypeId].IsSolid;
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
                        chunksArray[x, z] = new Chunk(chunkCoord, this, biome);

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