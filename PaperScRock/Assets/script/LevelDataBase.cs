using UnityEngine;

[CreateAssetMenu(fileName = "LevelDatabase", menuName = "Game/LevelDatabase")]
public class LevelDatabase : ScriptableObject
{
    public LevelData[] levels;
}