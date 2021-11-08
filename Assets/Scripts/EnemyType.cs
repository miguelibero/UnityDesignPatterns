
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyType", menuName = "ScriptableObjects/EnemyType")]
public class EnemyType : ScriptableObject, IEnemyType
{
    public string Name => _name;

    [SerializeField]
    string _name;

    [SerializeField]
    List<int> _healthPerLevel;


    public int GetHealth(int level)
    {
        if(_healthPerLevel == null || level < 0 || level >= _healthPerLevel.Count)
        {
            return 0;
        }
        return  _healthPerLevel[level];
    }
}
