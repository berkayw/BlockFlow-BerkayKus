using System;
using System.Collections;
using UnityEngine;

public class GrinderManager : MonoBehaviour
{
    public Color color;
    Renderer grinderRenderer;
    public Vector3 direction;
    public Vector3 rayStartOffset;
    public GameDataSO gameData;
    private BlockController _blockController;
    public GrinderType grinderType;
    public int size;
    
    
    public void Awake()
    {
        grinderRenderer = GetComponent<Renderer>();    
    }

    private void OnEnable()
    {
        GameEventSystem.instance.BlockPlacedEvent.AddListener(CheckGrinders);
    }

    public void InitializeGrinder(GrinderColor _color, Vector3 _direction, Vector3 _rayStartOffset, GrinderType _grinderType, int _size)
    {
        SetGrinderColor(_color);
        SetDirectionandRayPoint(_direction, _rayStartOffset);
        SetSize(_grinderType, _size);
    }
    
    
    public void SetGrinderColor(GrinderColor _color)
    {
        switch (_color)
        {
            case GrinderColor.Red: 
                color = gameData.colorMaterials[0].color; 
                break;
            case GrinderColor.Green: 
                color = gameData.colorMaterials[1].color; 
                break;
            case GrinderColor.Blue: 
                color = gameData.colorMaterials[2].color;
                break;
            case GrinderColor.Yellow:
                color = gameData.colorMaterials[3].color; 
                break;
        }

        grinderRenderer.material.color = color;
    }

    public void SetDirectionandRayPoint(Vector3 _direction, Vector3 _rayStartOffset)
    {
        direction = _direction;
        rayStartOffset = _rayStartOffset;
    }

    public void SetSize(GrinderType _grinderType, int _size)
    {
        grinderType = _grinderType;
        size = _size;
    }

    public void CheckGrinders()
    {
        RaycastHit hit;
        if (Physics.Raycast( transform.position + rayStartOffset,-direction.normalized, out hit, 1f))
        {
            if (hit.collider.CompareTag("Block"))
            {
                if (hit.collider.GetComponentInParent<BlockInstanceManager>().blockPrefab.GetComponentInChildren<MeshRenderer>().material
                        .color == grinderRenderer.material.color)
                {
                    if (grinderType  == GrinderType.Horizontal)
                    {
                        if (hit.collider.GetComponentInParent<BlockInstanceManager>().blockInstance.blockData.width <= size)
                        {
                            Destroy(hit.collider.transform.parent.gameObject);
                            BoardManager.Instance.remainingBlocks--;
                        }
                    }
                    else if (grinderType == GrinderType.Vertical)
                    {
                        if (hit.collider.GetComponentInParent<BlockInstanceManager>().blockInstance.blockData.height <= size)
                        {
                            Destroy(hit.collider.transform.parent.gameObject);
                            BoardManager.Instance.remainingBlocks--;
                        }
                    }
                    
                }
                
            }
        }

        //debug
        Debug.DrawRay(transform.position + rayStartOffset, -direction.normalized * 1f, Color.red);
    }
}

