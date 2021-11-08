
using UnityEngine;

public interface IAttackTarget
{
    bool IsValid { get; }
    void OnAttackHit(Vector3 position, int damage);
}