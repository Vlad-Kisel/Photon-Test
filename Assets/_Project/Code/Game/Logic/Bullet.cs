using Fusion;
using UnityEngine;
using UnityEngine.Serialization;

public class Bullet : NetworkBehaviour
{
    [SerializeField] private float _flyTime = 0.3f;
    
    [Networked] private Vector3 StartPos { get; set; }
    [Networked] private Vector3 TargetPos { get; set; }
    [Networked] private TickTimer LifeTimer { get; set; }

    public void Init(Vector3 target)
    {
        if (!HasStateAuthority) return;

        StartPos = transform.position;
        TargetPos = target;
        LifeTimer = TickTimer.CreateFromSeconds(Runner, _flyTime);
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
            return;

        if (LifeTimer.Expired(Runner))
        {
            Runner.Despawn(Object);
            return;
        }

        var t = 1f - ((LifeTimer.RemainingTime(Runner) ?? 0f) / _flyTime);
        transform.position = Vector3.Lerp(StartPos, TargetPos, t);
    }
}