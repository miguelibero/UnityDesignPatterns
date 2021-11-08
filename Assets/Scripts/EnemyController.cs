
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EnemyController : MonoBehaviour, IAttackTarget, IStateMachineEventListener
{
    static int _hitParam = Animator.StringToHash("Hit");

    static int _dieParam = Animator.StringToHash("Die");

    [SerializeField] int _health = 5;

    Animator _animator;

    bool Dead => _health <= 0;

    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    bool IAttackTarget.IsValid => this != null && enabled && !Dead;

    void Update()
    {
        CheckDead();
    }

    bool CheckDead()
    {
        if (Dead)
        {
            _animator.SetTrigger(_dieParam);
            return true;
        }
        return false;
    }

    void IAttackTarget.OnAttackHit(int damage)
    {
        if (Dead)
        {
            return;
        }
        _health -= damage;
        if(CheckDead())
        {
            return;
        }
        _animator.SetTrigger(_hitParam);
    }

    void IStateMachineEventListener.OnStateMachineEvent(string name, Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (name == "Dead")
        {
            _animator.enabled = false;
        }
    }
}
