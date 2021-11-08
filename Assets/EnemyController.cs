
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EnemyController : MonoBehaviour
{
    static int _hitParam = Animator.StringToHash("Hit");

    Animator _animator;

    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    public void OnPlayerAttack()
    {
        _animator.SetTrigger(_hitParam);
    }
}
