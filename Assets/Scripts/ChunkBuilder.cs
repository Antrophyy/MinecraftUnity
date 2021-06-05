using System.Collections.Generic;
using UnityEngine;

public class ChunkBuilder
{
    public bool IsActive { get => chunkObject.activeSelf; set => chunkObject.SetActive(value); }
    public Vector3 WorldPosition => chunkObject.transform.position;
    readonly GameObject chunkObject;
    readonly MeshFilter meshFilter;
    readonly MeshRenderer meshRenderer;
    readonly MeshCollider meshCollider;
    readonly List<Vector3> vertices = new List<Vector3>();
    readonly List<int> triangles = new List<int>();
    readonly List<Vector2> uvs = new List<Vector2>();
    readonly byte[,,] voxelMap = new byte[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];
    readonly World world;
    int vertexIndex = 0;

    public ChunkBuilder(Vector2 chunkCoordinates, World world)
    {
        this.world = world;
        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshCollider = chunkObject.AddComponent<MeshCollider>();
        meshRenderer.material = world.Material;
        chunkObject.transform.SetParent(world.transform);
        chunkObject.transform.position = new Vector3(chunkCoordinates.x * VoxelData.ChunkWidth, 0f, chunkCoordinates.y * VoxelData.ChunkWidth);
        chunkObject.name = $"Chunk {chunkCoordinates.x}, {chunkCoordinates.y}";
        chunkObject.layer = LayerMask.NameToLayer("Ground");

        PopulateVoxelMap();
        UpdateChunk();
        

        meshCollider.sharedMesh = meshFilter.mesh;
    }

    void PopulateVoxelMap()
    {
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    voxelMap[x, y, z] = world.GetVoxel(new Vector3(x, y, z) + WorldPosition);
                }
            }
        }
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
                    if (world.BlockTypes[voxelMap[x, y, z]].IsSolid)
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

    bool VoxelExists(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        if (IsVoxelOutsideCurrentChunk(x, y, z))
            return world.BlockTypes[world.GetVoxel(pos + WorldPosition)].IsSolid;

        return world.BlockTypes[voxelMap[x, y, z]].IsSolid;
    }

    bool IsVoxelOutsideCurrentChunk(int x, int y, int z)
    {
        return x < 0 || x > VoxelData.ChunkWidth - 1 || y < 0 || y > VoxelData.ChunkHeight - 1 || z < 0 || z > VoxelData.ChunkWidth - 1;
    }

    /// <summary>
    /// The method goes over triangles and gets their vertexes,
    /// adds them to the list so that the mesh can map them.
    /// </summary>
    void UpdateMeshData(Vector3 pos)
    {
        for (int p = 0; p < 6; p++)
        {
            if (VoxelExists(pos + VoxelData.SideChecks[p]))
                continue;

            for (int i = 0; i < 6; i++)
            {
                byte BlockID = voxelMap[(int)pos.x, (int)pos.y, (int)pos.z];

                vertices.Add(pos + VoxelData.VoxelVertices[VoxelData.VoxelSides[p, 0]]);
                vertices.Add(pos + VoxelData.VoxelVertices[VoxelData.VoxelSides[p, 1]]);
                vertices.Add(pos + VoxelData.VoxelVertices[VoxelData.VoxelSides[p, 2]]);
                vertices.Add(pos + VoxelData.VoxelVertices[VoxelData.VoxelSides[p, 3]]);

                AddTexture(world.BlockTypes[BlockID].GetTextureID(p));

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

    public void EditVoxel(Vector3 pos, byte newID)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        Debug.Log($"Deleting voxel at {x}, {y}, {z}");

        x -= Mathf.FloorToInt(chunkObject.transform.position.x);
        z -= Mathf.FloorToInt(chunkObject.transform.position.z);

        voxelMap[x, y, z] = newID;

        UpdateSurroundingVoxels(x,y,z);
        UpdateChunk();
    }

    void UpdateSurroundingVoxels(int x, int y, int z)
    {
        Vector3 voxel = new Vector3(x, y, z);

        for (int i = 0; i < 6; i++)
        {
            Vector3 currentVoxel = voxel + VoxelData.SideChecks[i];

            if (!IsVoxelOutsideCurrentChunk((int)currentVoxel.x, (int) currentVoxel.y, (int) currentVoxel.z))
                world.GetChunkFromVector3(currentVoxel + WorldPosition).UpdateChunk();
        }
    }
}
