[System.Serializable]
public class VoxelSideType
{
    public string BlockName;
    public bool IsSolid;

    public int BackSideTexture;
    public int FrontSideTexture;
    public int TopSideTexture;
    public int BottomSideTexture;
    public int LeftSideTexture;
    public int RightSideTexture;

    public int GetTextureID(int sideIndex)
    {
        return sideIndex switch
        {
            0 => BackSideTexture,
            1 => FrontSideTexture,
            2 => TopSideTexture,
            3 => BottomSideTexture,
            4 => LeftSideTexture,
            5 => RightSideTexture,
            _ => BackSideTexture,
        };
    }
}

