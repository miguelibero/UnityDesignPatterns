
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public interface IAttackTarget
{
    bool IsValid { get; }
    Task OnAttackHit(Vector3 position, int damage, CancellationToken cancellationToken = default);
}