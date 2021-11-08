
public interface IAttackTarget
{
    bool IsValid { get; }
    void OnAttackHit(int damage);
}