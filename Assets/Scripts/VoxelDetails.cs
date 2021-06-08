using UnityEngine;

[System.Serializable]
public class VoxelDetails
{
    public BlockType BlockType;
    public bool IsSolid;
    public float TimeToDestroy;

    [Header("2D Side Textures")]
    public int SideTexture;
    public int TopSideTexture;
    public int BottomSideTexture;

    public int GetTextureID(int sideIndex)
    {
        return sideIndex switch
        {
            0 => SideTexture,
            1 => SideTexture,
            2 => TopSideTexture,
            3 => BottomSideTexture,
            4 => SideTexture,
            5 => SideTexture,
            _ => SideTexture,
        };
    }
}

