using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.InputSystem;

public struct BulletInput : IInputComponentData
{
    public bool Fire;
}

public class BulletInputAuthoring : MonoBehaviour
{
    class Baking : Baker<BulletInputAuthoring>
    {
        public override void Bake(BulletInputAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<BulletInput>(entity);
        }
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[UpdateInGroup(typeof(GhostInputSystemGroup))]
public partial struct BulletSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var fire = Keyboard.current.spaceKey.isPressed;
        
        foreach (var bulletInput in SystemAPI.Query<RefRW<BulletInput>>().WithAll<GhostOwnerIsLocal>())
        {
            bulletInput.ValueRW = new BulletInput { Fire = fire };
        }
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.ThinClientSimulation)]
[UpdateInGroup(typeof(GhostInputSystemGroup))]
public partial struct ThinClientBulletSystem : ISystem
{
    private float _LastFireTime;

    public void OnUpdate(ref SystemState state)
    {
        foreach (var bulletInput in SystemAPI.Query<RefRW<BulletInput>>().WithAll<GhostOwnerIsLocal>())
        {
            //only fire every 2 seconds
            if (Time.time - _LastFireTime > 2f)
            {
                bulletInput.ValueRW = new BulletInput { Fire = true };
                _LastFireTime = Time.time;
            }
            else
            {
                bulletInput.ValueRW = new BulletInput { Fire = false };
            }
        }
    }
}