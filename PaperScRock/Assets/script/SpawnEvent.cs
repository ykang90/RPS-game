using UnityEngine;

[System.Serializable]
public class SpawnEvent
{
    public float delay;
    public GameObject prefab;
    public int count = 1;
}