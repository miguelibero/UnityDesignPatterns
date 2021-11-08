
using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EnemyController : MonoBehaviour, IAttackTarget, IStateMachineEventListener
{
    static int _hitParam = Animator.StringToHash("Hit");

    static int _dieParam = Animator.StringToHash("Die");

    [SerializeField] int _health;
    [SerializeField] int _level;
    [SerializeReference, InspectorName("Config")] EnemyConfig _unityConfig;
    
    IEnemyConfig _config;

    public IEnemyConfig Config
    {
        get => _config == null ? _unityConfig : _config;
        set => _config = value;
    }

    Animator _animator;
    Collider _collider;

    bool Dead => _health <= 0;

    void Awake()
    {
        if (Config == null)
        {
            throw new ArgumentNullException(nameof(Config));
        }
    }

    void Start()
    {

        _animator = GetComponent<Animator>();
        _collider = GetComponent<Collider>();
        _health = Config.GetHealth(_level);
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

    void IAttackTarget.OnAttackHit(Vector3 position, int damage)
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
            if (_collider != null)
            {
                _collider.enabled = false;
            }
        }
    }
}
