using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController), typeof(Animator))]
public class CharacterMovement : MonoBehaviour, InputControls.IPlayerActions, IStateMachineEventListener
{
    [SerializeField] float _speed = 5.0f;

    Vector3 _moveDirection;

    InputControls _controls;
    CharacterController _character;
    Animator _animator;
    EnemyController _enemy;


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
    }


    void InputControls.IPlayerActions.OnFire(InputAction.CallbackContext context)
    {
        if (context.started)
        {
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
    }

    void FixedUpdate()
    {
        _character.Move(_moveDirection * _speed * Time.fixedDeltaTime);
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        var enemy = hit.gameObject.GetComponent<EnemyController>();
        _enemy ??= enemy;
    }

    void IStateMachineEventListener.OnStateMachineEvent(string name, Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (name == "Hit")
        {
            if (_enemy != null)
            {
                _enemy.OnPlayerAttack();
            }
        }
    }
}
