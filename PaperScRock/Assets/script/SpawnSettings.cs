using UnityEngine;

[CreateAssetMenu(fileName = "SpawnSettings", menuName = "Game/Spawn Settings")]
public class SpawnSettings : ScriptableObject
{
    public SpawnEvent[] events;
}