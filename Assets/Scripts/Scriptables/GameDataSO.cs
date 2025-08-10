using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelData
{
    public int width;
    public int height;
    public List<GrinderSpawn> grinders;
    public List<ObstacleSpawn> obstacles;
    public List<BlockSpawn> blocks;
}



[CreateAssetMenu(fileName = "GameData", menuName = "Game/GameData")]
public class GameDataSO : ScriptableObject
{
    public Material[] colorMaterials;
    public Material iceMaterial;

    //public Material[] blockTextures;

}
