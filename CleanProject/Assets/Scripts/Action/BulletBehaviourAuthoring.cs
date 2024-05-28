using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public struct BulletBehaviour : IComponentData
{
    public float Speed;
    public float LifeTime;
    public Entity Entity;
}



public class BulletBehaviourAuthoring : MonoBehaviour
{
    public float Speed;
    public float LifeTime;

    class Baking : Baker<BulletBehaviourAuthoring>
    {
        public override void Bake(BulletBehaviourAuthoring authoring)
        {
            BulletBehaviour component = default;
            component.Speed = authoring.Speed;
            component.LifeTime = authoring.LifeTime;
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, component);
        }
    }
}

[UpdateInGroup(typeof(SimulationSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ClientSimulation)]
public partial struct BulletBehaviourSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;

        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (bulletBehaviour, trans) in SystemAPI.Query<RefRW<BulletBehaviour>, RefRW<LocalTransform>>())
        {
            var entity = bulletBehaviour.ValueRO.Entity;
            if (bulletBehaviour.ValueRO.LifeTime <= 0)
            {
                commandBuffer.DestroyEntity(entity);
                continue;
            }
            
            bulletBehaviour.ValueRW.LifeTime -= deltaTime;

            //move entity in the direction it is facing
            var move = new float3(0, bulletBehaviour.ValueRO.Speed * deltaTime, 0);
            var worldMove = math.rotate(trans.ValueRO.Rotation, move);

            float3 newPosition = trans.ValueRO.Position + worldMove;

            trans.ValueRW.Position = newPosition;
        }
        commandBuffer.Playback(state.EntityManager);
    }
}