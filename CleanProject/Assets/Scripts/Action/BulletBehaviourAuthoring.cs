using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

public struct BulletBehaviour : IComponentData
{
    public float Speed;
    public float LifeTime;
    public Entity Entity;
    public float3 InitialVelocity; // Initial velocity of the bullet
    public float InitialTime;      // To keep track of the initial spawn time
    public float Gravity;          // Gravity to apply to the bullet
}

public class BulletBehaviourAuthoring : MonoBehaviour
{
    public float Speed;
    public float LifeTime;
    public float Gravity = -9.81f; // Default gravity value

    class Baker : Baker<BulletBehaviourAuthoring>
    {
        public override void Bake(BulletBehaviourAuthoring authoring)
        {
            BulletBehaviour component = default;
            component.Speed = authoring.Speed;
            component.LifeTime = authoring.LifeTime;
            component.Gravity = authoring.Gravity;
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, component);
        }
    }
}

[UpdateInGroup(typeof(SimulationSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[BurstCompile]
public partial struct BulletBehaviourSystem : ISystem
{
    [BurstCompile]
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

            // Calculate the elapsed time since the bullet was spawned
            float elapsedTime = (float)SystemAPI.Time.ElapsedTime - bulletBehaviour.ValueRO.InitialTime;

            // Calculate the new velocity, considering gravity
            float3 velocity = bulletBehaviour.ValueRO.InitialVelocity;
            velocity.y += bulletBehaviour.ValueRO.Gravity * elapsedTime;

            // Calculate the displacement
            float3 displacement = velocity * deltaTime;

            float3 newPosition = trans.ValueRW.Position + displacement;

            trans.ValueRW.Position = newPosition;
        }

        commandBuffer.Playback(state.EntityManager);
        commandBuffer.Dispose();
    }
}

[UpdateInGroup(typeof(BeforePhysicsSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[BurstCompile]
public partial struct BulletCollisionSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var entities = state.EntityManager.GetAllEntities(Allocator.Temp);

        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);

        foreach (var entity in entities)
        {
            if (state.EntityManager.HasComponent<BulletBehaviour>(entity))
            {
                var hits = new NativeList<ColliderCastHit>(Allocator.Temp);
                var bulletBehaviour = state.EntityManager.GetComponentData<BulletBehaviour>(entity);
                var bulletTransform = state.EntityManager.GetComponentData<LocalTransform>(entity);

                physicsWorld.SphereCastAll(bulletTransform.Position, bulletTransform.Scale, float3.zero, 1, ref hits, CollisionFilter.Default);

                foreach (var hit in hits)
                {
                    if (hit.Entity == Entity.Null || entity == Entity.Null || hit.Entity == bulletBehaviour.Entity || state.EntityManager.HasComponent<BulletBehaviour>(hit.Entity))
                        continue;
                }
            }
        }

        commandBuffer.Playback(state.EntityManager);
        commandBuffer.Dispose();
        entities.Dispose();
    }
}