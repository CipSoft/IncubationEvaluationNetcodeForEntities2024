using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ClientSimulation)]
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
                //move trans forward in the direction it is facing
                var move = new float3(0, 0, 2);
                var worldMove = math.rotate(trans.ValueRO.Rotation, move);
                float3 newPosition = trans.ValueRO.Position + worldMove;
                var newTransform = new LocalTransform { Position = newPosition, Rotation = trans.ValueRO.Rotation, Scale = trans.ValueRO.Scale };
                commandBuffer.AddComponent(bullet, newTransform);
                commandBuffer.SetComponent(bullet, new BulletBehaviour { Speed = 5, LifeTime = 2, Entity = bullet });
            }
        }
        commandBuffer.Playback(state.EntityManager);
    }
}