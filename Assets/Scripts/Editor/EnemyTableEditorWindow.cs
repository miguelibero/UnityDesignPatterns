using UnityEditor;


public class EnemyTableEditorWindow : TableEditorWindow<EnemyConfig>
{
    [MenuItem("Window/Enemy Table")]
    static void Init()
    {
        GetWindow(typeof(EnemyTableEditorWindow)).Show();
    }
}