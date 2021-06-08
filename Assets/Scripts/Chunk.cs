using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Chunk contains a VoxelMap. Default block values are 128x16. There are many chunks
/// throughout the game and all chunks contain a specific amount of voxels. This is 
/// done to optimize the final game, where one gameobject is one chunk that contains multiple 
/// voxels.
/// </summary>
public class Chunk
{
    public bool IsActive { get => chunkObject.activeSelf; set => chunkObject.SetActive(value); }
    public Vector3 WorldPosition => chunkObject.transform.position;
    public readonly Voxel[,,] VoxelMap = new Voxel[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];
    public bool IsVoxelMapPopulated;
    readonly List<Vector3> vertices = new List<Vector3>();
    readonly List<int> triangles = new List<int>();
    readonly List<Vector2> uvs = new List<Vector2>();
    readonly World world;
    readonly BiomeAttributes biome;
    int vertexIndex = 0;
    GameObject chunkObject;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    MeshCollider meshCollider;

    public Chunk(ChunkCoord chunkCoordinates, World world, BiomeAttributes biome)
    {
        this.world = world;
        this.biome = biome;

        SetChunkGameObject(chunkCoordinates);
        PopulateVoxelMap();
        UpdateChunk();
    }

    /// <summary>
    /// Takes any world transform position value and finds the voxel out of that location.
    /// </summary>
    /// <param name="pos">Any transform.position of the focused position.</param>
    /// <returns>Voxel that is present at the position.</returns>
    public Voxel GetVoxelFromGlobalVector3(Vector3 pos)
    {
        Vector3Int newPost = Vector3ToVector3IntFloored(pos);

        newPost.x -= Mathf.FloorToInt(chunkObject.transform.position.x);
        newPost.z -= Mathf.FloorToInt(chunkObject.transform.position.z);

        return VoxelMap[newPost.x, newPost.y, newPost.z];
    }

    /// <summary>
    /// Prepares the chunk gameobject that is to be created.
    /// </summary>
    /// <param name="chunkCoordinates">Coordinates of the chunk after its creation.</param>
    void SetChunkGameObject(ChunkCoord chunkCoordinates)
    {
        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshCollider = chunkObject.AddComponent<MeshCollider>();
        meshRenderer.material = world.Material;
        chunkObject.transform.SetParent(world.transform);
        chunkObject.transform.position = new Vector3(chunkCoordinates.X * VoxelData.ChunkWidth, 0f, chunkCoordinates.Z * VoxelData.ChunkWidth);
        chunkObject.name = $"Chunk {chunkCoordinates.X}, {chunkCoordinates.Z}";
        chunkObject.layer = LayerMask.NameToLayer("Ground");
        IsActive = true;
    }

    /// <summary>
    /// Fills up the chunk with the Voxels.
    /// </summary>
    void PopulateVoxelMap()
    {
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    VoxelMap[x, y, z] = new Voxel(new Vector3Int(x, y, z) + WorldPosition, biome, world);
                }
            }
        }
        IsVoxelMapPopulated = true;
    }

    /// <summary>
    /// Rebuilds the current chunk.
    /// </summary>
    void UpdateChunk()
    {
        ClearMeshData();

        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    if (world.VoxelTypes[VoxelMap[x, y, z].BlockTypeId].IsSolid)
                        UpdateMeshData(new Vector3(x, y, z));
                }
            }
        }

        CreateMesh();
    }

    /// <summary>
    /// Clears mesh data so that the chunk can be rebuilt.
    /// </summary>
    void ClearMeshData()
    {
        vertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
    }

    /// <summary>
    /// Checks whether the voxel is a solid or if it's just air.
    /// </summary>
    /// <param name="pos"></param>
    /// <returns>True if the voxel is solid and false if it's air.</returns>
    bool CheckVoxel(Vector3 pos)
    {
        Vector3Int newPos = Vector3ToVector3IntFloored(pos);

        if (IsVoxelOutsideChunk(newPos))
            return world.VoxelExistsAndIsSolid(pos + WorldPosition);

        return world.VoxelTypes[VoxelMap[newPos.x, newPos.y, newPos.z].BlockTypeId].IsSolid;
    }

    /// <summary>
    /// Checks whether the position of Voxel given is outside this chunk.
    /// </summary>
    /// <param name="pos">The position of Vexel to be checked.</param>
    /// <returns>True if it is outside this chunk and False if it is not.</returns>
    bool IsVoxelOutsideChunk(Vector3Int pos)
    {
        return pos.x < 0 || pos.x > VoxelData.ChunkWidth - 1 || pos.y < 0 || pos.y > VoxelData.ChunkHeight - 1 || pos.z < 0 || pos.z > VoxelData.ChunkWidth - 1;
    }

    /// <summary>
    /// The method goes over triangles and gets their vertexes,
    /// adds them to the list so that the mesh can map them.
    /// </summary>
    void UpdateMeshData(Vector3 pos)
    {
        for (int p = 0; p < 6; p++)
        {
            if (!CheckVoxel(pos + VoxelData.SideChecks[p]))
            {
                byte blockID = VoxelMap[(int)pos.x, (int)pos.y, (int)pos.z].BlockTypeId;

                for (int i = 0; i < 4; i++)
                    vertices.Add(pos + VoxelData.VoxelVertices[VoxelData.VoxelSides[p, i]]);

                AddTexture(world.VoxelTypes[blockID].GetTextureID(p));

                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 3);
                vertexIndex += 4;
            }
        }
    }

    /// <summary>
    /// Creates a mesh for the chunk.
    /// </summary>
    void CreateMesh()
    {
        Mesh mesh = new Mesh
        {
            vertices = vertices.ToArray(),
            triangles = triangles.ToArray(),
            uv = uvs.ToArray()
        };
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = meshFilter.mesh;
    }

    /// <summary>
    /// The method adds textures from the blocks map and adds them to the UVs which makes all
    /// the sides of voxel to have a texture. Positions of the sides are normalized, i.e. 
    /// you can reference a picture by its index.
    /// </summary>
    /// <param name="textureID">Index of the picture you want to add.</param>
    void AddTexture(int textureID)
    {
        float y = textureID / VoxelData.TextureAliasSizeInBlocks;
        float x = textureID - (y * VoxelData.TextureAliasSizeInBlocks);

        x *= VoxelData.NormalizedBlockTexturesSize;
        y *= VoxelData.NormalizedBlockTexturesSize;

        y = 1f - y - VoxelData.NormalizedBlockTexturesSize;

        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y + VoxelData.NormalizedBlockTexturesSize));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTexturesSize, y));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTexturesSize, y + VoxelData.NormalizedBlockTexturesSize));
    }

    /// <summary>
    /// The method converts world transform position to chunk position
    /// in order to find the voxel that is to be modified.
    /// </summary>
    /// <param name="pos">World transform position that should point to voxel.</param>
    /// <param name="newVoxel">A new voxel that you want to place instead.</param>
    public void ModifyVoxel(Vector3 pos, BlockType newVoxel)
    {
        Vector3Int newPos = Vector3ToVector3IntFloored(pos);

        newPos.x -= Mathf.FloorToInt(chunkObject.transform.position.x);
        newPos.z -= Mathf.FloorToInt(chunkObject.transform.position.z);

        VoxelMap[newPos.x, newPos.y, newPos.z].ModifyVoxel(newVoxel);

        UpdateSurroundingChunks(new Vector3Int(newPos.x, newPos.y, newPos.z));
        UpdateChunk();
    }

    /// <summary>
    /// Checks each side of the voxel, if the neighboring voxel is part of any other chunk but this one,
    /// said chunk will get updated.
    /// </summary>
    /// <param name="voxel">Vector3Int coordinates of the voxel to be checked.</param>
    void UpdateSurroundingChunks(Vector3Int voxel)
    {
        for (int i = 0; i < 6; i++)
        {
            Vector3 currentVoxel = voxel + VoxelData.SideChecks[i];

            if (IsVoxelOutsideChunk(Vector3ToVector3IntFloored(currentVoxel)))
                world.GetChunkFromVector3(currentVoxel + WorldPosition).UpdateChunk();
        }
    }

    /// <summary>
    /// The method converts any Vector3 and uses FloorToInt method to round
    /// the coordinates and return Vector3Int.
    /// </summary>
    /// <param name="pos">Vector3 that you want to floor to int coordinates.</param>
    Vector3Int Vector3ToVector3IntFloored(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        return new Vector3Int(x, y, z);
    }
}
