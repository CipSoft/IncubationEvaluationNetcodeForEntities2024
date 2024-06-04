using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
[BurstCompile]
public partial struct BulletMovementSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Bullet>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var prefab = SystemAPI.GetSingleton<Bullet>().BulletPrefab;
        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (input, trans) in SystemAPI.Query<RefRW<BulletInput>, RefRO<LocalTransform>>().WithAll<Simulate>())
        {
            if (input.ValueRO.Fire)
            {
                var bullet = commandBuffer.Instantiate(prefab);

                var initialVelocity = math.mul(trans.ValueRO.Rotation, new float3(0, 1, 1)) * 10f; // Adjust initial speed

                var newTransform = new LocalTransform
                {
                    Position = trans.ValueRO.Position,
                    Rotation = trans.ValueRO.Rotation,
                    Scale = trans.ValueRO.Scale
                };

                commandBuffer.AddComponent(bullet, newTransform);

                // Set the initial time and initial velocity for the bullet
                float initialTime = (float)SystemAPI.Time.ElapsedTime;

                commandBuffer.SetComponent(bullet, new BulletBehaviour
                {
                    Speed = 10f, // Adjust speed
                    LifeTime = 2.5f, // Adjust lifetime
                    Entity = bullet,
                    InitialVelocity = initialVelocity,
                    InitialTime = initialTime,
                    Gravity = -9.81f // Adjust gravity as needed
                });
            }
        }
        commandBuffer.Playback(state.EntityManager);
    }
}