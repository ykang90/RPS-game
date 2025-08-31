using UnityEngine;

[CreateAssetMenu(fileName = "CarvesMetaInfo", menuName = "CARVES/MetaInfo")]
public class CarvesMetaInfo : ScriptableObject
{
    public CarvesMetaEntry[] Entries;
}