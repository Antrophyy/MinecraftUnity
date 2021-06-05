using UnityEngine;

[CreateAssetMenu(fileName = "BiomeAttributes", menuName = "Minecraft/Biome Attribute")]
public class BiomeAttributes : ScriptableObject
{
    public string BiomeName;
    public int SolidGroundHeight;
    public int TerrainHeight;
    public float TerrainScale;
}
