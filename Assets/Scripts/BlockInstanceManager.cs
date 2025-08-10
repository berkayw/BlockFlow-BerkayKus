using System;
using UnityEngine;

public class BlockInstanceManager : MonoBehaviour
{
    public GameDataSO gameData;
    public BlockInstanceData blockInstance;
    [HideInInspector]public GameObject blockPrefab;
    
    private void Start()
    {
        blockPrefab = Instantiate(blockInstance.blockData.meshPrefab, transform);
        SetBlockVisuals();
    }
    
    public void SetBlockVisuals()
    {
        switch (blockInstance.blockType)
        {
            case BlockType.Normal:
                Debug.Log("normal texture");
                SetColorMaterial(blockInstance.blockColor);
                break;
            case BlockType.SingleAxis:
                Debug.Log("single axis");
                SetColorMaterial(blockInstance.blockColor);
                break;
            case BlockType.Iced:
                SetIceMaterial();
                Debug.Log("iced");
                break;
        }
    }

    public void SetColorMaterial(BlockColor color)
    {
        blockPrefab.GetComponentInChildren<MeshRenderer>().material= gameData.colorMaterials[color.GetHashCode()];
    }

    public void SetIceMaterial()
    {
        blockPrefab.GetComponentInChildren<MeshRenderer>().material= gameData.iceMaterial;
    }

    public void SetAxis()
    {
        
    }
    
}
