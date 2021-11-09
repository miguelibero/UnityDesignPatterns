using System.Collections.Generic;
using UnityEditor;

[CustomPropertyDrawer(typeof(EnemyConfig))]
public sealed class EnemyConfigPropertyDrawer : UnityObjectDropdownPropertyDrawer<EnemyConfig>
{
    protected override string GetLabel(EnemyConfig obj)
    {
        return obj.Name;
    }

    protected override IEnumerable<string> FindAssets()
    {
        return AssetDatabase.FindAssets("t:scriptableobject");
    }
}
