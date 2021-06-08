using System.Collections.Generic;
using UnityEngine;

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

    public VoxelDetails GetVoxelDetails(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        x -= Mathf.FloorToInt(chunkObject.transform.position.x);
        z -= Mathf.FloorToInt(chunkObject.transform.position.z);

        var blockType = (BlockType)VoxelMap[x, y, z].BlockTypeId;

        for (int i = 0; i < world.BlockTypes.GetLength(0); i++)
        {
            if (world.BlockTypes[i].BlockType.Equals(blockType))
                return world.BlockTypes[i];
        }
        return null;
    }

    public Voxel GetVoxelFromGlobalVector3(Vector3 pos)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
        zCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);

        return VoxelMap[xCheck, yCheck, zCheck];
    }

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

    void PopulateVoxelMap()
    {
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    VoxelMap[x, y, z] = new Voxel(new Vector3Int(x, y, z) + WorldPosition, biome);
                }
            }
        }
        IsVoxelMapPopulated = true;
    }

    void UpdateChunk()
    {
        ClearMeshData();
        
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    if (world.BlockTypes[VoxelMap[x, y, z].BlockTypeId].IsSolid)
                        UpdateMeshData(new Vector3(x, y, z));
                }
            }
        }

        CreateMesh();
    }

    void ClearMeshData()
    {
        vertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
    }

    bool CheckVoxel(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        if (!IsVoxelInChunk(x, y, z))
            return world.VoxelExistsAndIsSolid(pos + WorldPosition);

        return world.BlockTypes[VoxelMap[x, y, z].BlockTypeId].IsSolid;
    }

    bool IsVoxelInChunk(int x, int y, int z)
    {
        if (x < 0 || x > VoxelData.ChunkWidth - 1 || y < 0 || y > VoxelData.ChunkHeight - 1 || z < 0 || z > VoxelData.ChunkWidth - 1)
            return false;
        else
            return true;
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

                vertices.Add(pos + VoxelData.VoxelVertices[VoxelData.VoxelSides[p, 0]]);
                vertices.Add(pos + VoxelData.VoxelVertices[VoxelData.VoxelSides[p, 1]]);
                vertices.Add(pos + VoxelData.VoxelVertices[VoxelData.VoxelSides[p, 2]]);
                vertices.Add(pos + VoxelData.VoxelVertices[VoxelData.VoxelSides[p, 3]]);

                AddTexture(world.BlockTypes[blockID].GetTextureID(p));

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

    public void EditVoxel(Vector3 pos, Voxel newVoxel)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        x -= Mathf.FloorToInt(chunkObject.transform.position.x);
        z -= Mathf.FloorToInt(chunkObject.transform.position.z);

        VoxelMap[x, y, z] = newVoxel;

        UpdateSurroundingVoxels(x, y, z);
        UpdateChunk();
    }

    void UpdateSurroundingVoxels(int x, int y, int z)
    {
        Vector3 voxel = new Vector3(x, y, z);

        for (int i = 0; i < 6; i++)
        {
            Vector3 currentVoxel = voxel + VoxelData.SideChecks[i];

            if (!IsVoxelInChunk((int)currentVoxel.x, (int)currentVoxel.y, (int)currentVoxel.z))
                world.GetChunkFromVector3(currentVoxel + WorldPosition).UpdateChunk();
        }
    }
}
