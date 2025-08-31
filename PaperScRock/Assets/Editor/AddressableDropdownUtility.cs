using System.Collections.Generic;
using System.Linq;

public static class AddressableDropdownUtility
{
#if UNITY_EDITOR
    // —— 编辑器：返回真实 Key 列表 ——
    public static IEnumerable<string> GetAllKeys()
    {
        var settings = UnityEditor.AddressableAssets
            .AddressableAssetSettingsDefaultObject.Settings;

        return settings.groups
            .SelectMany(g => g.entries)
            .Select(e => e.address)
            .Distinct();
    }
#else
    // —— 运行时/IL2CPP：返回空，保证符号存在即可 ——
    public static IEnumerable<string> GetAllKeys()
        => System.Array.Empty<string>();
#endif
}