using System;
using System.Collections.Generic;
using UnityEngine;

public enum BlockType
{
    Normal,
    Iced,
    OnlyHorizontal,
    OnlyVertical
}

public enum BlockColor
{
    Red,
    Green,
    Blue,
    Yellow
}

[System.Serializable]
public class BlockInstanceData
{
    public BlockDataSO blockData;
    public BlockType blockType; //Normal, singleAxis, iced
    public BlockColor blockColor; //Red, Green, Blue, Yellow
    public int iceCount;
}

[CreateAssetMenu(fileName = "BlockData", menuName = "Block/BlockData")]
public class BlockDataSO : ScriptableObject
{
    public int shapeID; //shapes
    public int width;
    public int height;
    public GameObject meshPrefab;
    
    [Header("Shape Offsets (Grid Coordinates)")]
    public List<Vector2Int> shapeOffsets = new List<Vector2Int>();
    
}