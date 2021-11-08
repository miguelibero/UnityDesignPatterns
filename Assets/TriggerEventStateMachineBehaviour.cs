
using UnityEngine;

interface IStateMachineEventListener
{
    void OnStateMachineEvent(string name, Animator animator, AnimatorStateInfo stateInfo, int layerIndex);
}

public class TriggerEventStateMachineBehaviour : StateMachineBehaviour
{
    [SerializeField] float _time;
    [SerializeField] string _name;

    bool _triggered;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _triggered = false;
        if(_time <= 0.0f)
        {
            Trigger(animator, stateInfo, layerIndex);
        }
        base.OnStateEnter(animator, stateInfo, layerIndex);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);
        if(!_triggered && stateInfo.normalizedTime >= _time)
        {
            Trigger(animator, stateInfo, layerIndex);
        }
    }

    void Trigger(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _triggered = true;
        var listeners = animator.gameObject.GetComponents<IStateMachineEventListener>();
        foreach(var listener in listeners)
        {
            listener.OnStateMachineEvent(_name, animator, stateInfo, layerIndex);
        }
    }
}
