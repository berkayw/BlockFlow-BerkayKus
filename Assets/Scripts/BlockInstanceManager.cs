using System;
using TMPro;
using UnityEngine;

public class BlockInstanceManager : MonoBehaviour
{
    public GameDataSO gameData;
    public BlockInstanceData blockInstance;
    [HideInInspector]public GameObject blockPrefab;
    public GameObject arrow1Prefab;
    public GameObject arrow2refab;
    public GameObject arrow3Prefab;
    public TextMeshProUGUI iceCountText;

    
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
                SetColorMaterial(blockInstance.blockColor);
                break;
            case BlockType.OnlyHorizontal:
                SetColorMaterial(blockInstance.blockColor);
                GameObject arrPrefabH = GetArrowPrefabForSize();
                Instantiate(arrPrefabH, transform.position + new Vector3(GetArrowOffset(),1.1f, 0f), Quaternion.Euler(0f,90f,0f), transform);
                break;
            case BlockType.OnlyVertical:
                SetColorMaterial(blockInstance.blockColor);
                GameObject arrPrefabV = GetArrowPrefabForSize();
                Instantiate(arrPrefabV, transform.position +  new Vector3(0f,1.1f, GetArrowOffset()), Quaternion.Euler(0f,0f,0f), transform);
                break;
            case BlockType.Iced:
                SetIceMaterial();
                RefreshIceText();
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

    public GameObject GetArrowPrefabForSize()
    {
        if (blockInstance.blockType == BlockType.OnlyHorizontal)
        {
            switch (blockInstance.blockData.width)
            {
                case 1: 
                    return arrow1Prefab;
                case 2: 
                    return arrow2refab;
                case 3: 
                    return arrow3Prefab;
            }
        }

        if (blockInstance.blockType == BlockType.OnlyVertical)
        {
            switch (blockInstance.blockData.height)
            {
                case 1: 
                    return arrow1Prefab;
                case 2: 
                    return arrow2refab;
                case 3: 
                    return arrow3Prefab;
            }
        }
        return null;
    }

    public float GetArrowOffset()
    {
        if (blockInstance.blockType == BlockType.OnlyHorizontal)
        {
            switch (blockInstance.blockData.width)
            {
                case 1: 
                    return 0f;
                case 2: 
                    return 0.5f;
                case 3: 
                    return 1f;
            }
        }
        if (blockInstance.blockType == BlockType.OnlyVertical)
        {
            switch (blockInstance.blockData.height)
            {
                case 1: 
                    return 0f;
                case 2: 
                    return 0.5f;
                case 3: 
                    return 1f;
            }
        }
        
        return 0;
    }

    public void RefreshIceText()
    {
        if (blockInstance.iceCount <= 0)
        {
            iceCountText.text = "";
        }
        else
        {
            iceCountText.text = blockInstance.iceCount.ToString();
        }
    }
    
}
