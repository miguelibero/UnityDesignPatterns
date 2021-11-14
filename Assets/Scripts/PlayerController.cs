using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController), typeof(Animator))]
public class PlayerController : MonoBehaviour, InputControls.IPlayerActions, IStateMachineEventListener
{
    [SerializeField] float _speed = 5.0f;

    [SerializeField] int _attackDamage = 1;

    [SerializeField] GameObject[] _weaponSensors;

    Vector3 _moveDirection;

    InputControls _controls;
    CharacterController _character;
    Animator _animator;
    readonly HashSet<IAttackTarget> _attackedTargets = new HashSet<IAttackTarget>();
    readonly List<Vector3> _lastWeaponSensorPositions = new List<Vector3>();
    RaycastHit[] _attackRaycastHits = new RaycastHit[1];
    bool _attacking;

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
    void InputControls.IPlayerActions.OnAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            _attacking = true;
            _attackedTargets.Clear();
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

    void Update()
    {
        if(_attacking)
        {
            UpdatePossibleAttackTargets();
        }
    }

    void UpdatePossibleAttackTargets()
    {
        for (var i = 0; i < _weaponSensors.Length; i++)
        {
            if (_lastWeaponSensorPositions.Count <= i)
            {
                continue;
            }
            var pos = _lastWeaponSensorPositions[i];
            var fwd = pos - _weaponSensors[i].transform.position;
            Physics.RaycastNonAlloc(pos, fwd, _attackRaycastHits, fwd.magnitude);
            foreach (var hit in _attackRaycastHits)
            {
                if(hit.collider == null)
                {
                    continue;
                }
                foreach (var target in hit.collider.gameObject.GetComponents<IAttackTarget>())
                {
                    if (target.IsValid)
                    {
                        _attackedTargets.Add(target);
                    }
                }
            }
        }
        _lastWeaponSensorPositions.Clear();
        foreach (var sensor in _weaponSensors)
        {
            _lastWeaponSensorPositions.Add(sensor.transform.position);
        }
    }

    async void IStateMachineEventListener.OnStateMachineEvent(string name, Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (name == "Hit")
        {
            _attacking = false;
            var tasks = new List<Task>();
            foreach(var target in _attackedTargets)
            {
                tasks.Add(target.OnAttackHit(transform.position, _attackDamage));
            }
            _attackedTargets.Clear();
            await Task.WhenAll(tasks);
            Debug.Log("attack finished");
        }
    }
}
