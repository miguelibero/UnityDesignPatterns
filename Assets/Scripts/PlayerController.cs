using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController), typeof(Animator))]
public class PlayerController : MonoBehaviour, InputControls.IPlayerActions, IStateMachineEventListener
{
    [SerializeField] float _speed = 5.0f;

    [SerializeField] int _attackDamage = 1;

    Vector3 _moveDirection;

    InputControls _controls;
    CharacterController _character;
    Animator _animator;
    readonly HashSet<IAttackTarget> _possibleAttackTargets = new HashSet<IAttackTarget>();
    readonly Queue<IAttackTarget> _attackedTargets = new Queue<IAttackTarget>();


    static int _verticalParam = Animator.StringToHash("Vertical Movement");
    static int _horizontalParam = Animator.StringToHash("Horizontal Movement");
    static int _attackParam = Animator.StringToHash("Attack");

    void Start()
    {
        _character = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();

        _controls = new InputControls();
        _controls.Player.SetCallbacks(this);
        _controls.Player.Enable();

        _entitiesLayer = LayerMask.NameToLayer("Entities");
    }


    void InputControls.IPlayerActions.OnAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            foreach(var target in _possibleAttackTargets)
            {
                if(target != null && target.IsValid)
                {
                    _attackedTargets.Enqueue(target);
                    break;
                }
            }
            _animator.SetTrigger(_attackParam);
        }
    }

    void InputControls.IPlayerActions.OnLook(InputAction.CallbackContext context)
    {
    }

    void InputControls.IPlayerActions.OnMove(InputAction.CallbackContext context)
    {
        var v = context.ReadValue<Vector2>();
        _moveDirection = new Vector3(v.x, 0.0f, v.y);
        _animator.SetFloat(_verticalParam, _moveDirection.z);
        _animator.SetFloat(_horizontalParam, _moveDirection.x);

        if (_moveDirection.sqrMagnitude > 0.0f)
        {
            _possibleAttackTargets.Clear();
        }
    }

    void FixedUpdate()
    {
        _character.Move(_moveDirection * _speed * Time.fixedDeltaTime);
    }

    void Update()
    {
    }

    int _entitiesLayer;

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        foreach (var target in hit.gameObject.GetComponents<IAttackTarget>())
        {
            _possibleAttackTargets.Add(target);
        }
    }

    void IStateMachineEventListener.OnStateMachineEvent(string name, Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (name == "Hit")
        {
            if (_attackedTargets.Count > 0)
            {
                _attackedTargets.Dequeue().OnAttackHit(_attackDamage);
            }
        }
    }
}
