using UnityEngine;

public class ChunkCoord
{
    public int X { get; private set; }
    public int Z { get; private set; }

    public ChunkCoord()
    {
        X = 0;
        Z = 0;
    }

    public ChunkCoord(int x, int z)
    {
        X = x;
        Z = z;
    }

    public ChunkCoord(Vector3 pos)
    {
        X = Mathf.FloorToInt(pos.x) / VoxelData.ChunkWidth;
        Z = Mathf.FloorToInt(pos.z) / VoxelData.ChunkWidth;
    }

    public bool Equals(ChunkCoord other)
    {
        if (other == null)
            return false;
        else if (other.X == X && other.Z == Z)
            return true;
        else
            return false;
    }
}
