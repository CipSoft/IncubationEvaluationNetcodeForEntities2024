using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ClientSimulation)]
[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
[BurstCompile]
public partial struct BulletMovementSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;

        var prefab = SystemAPI.GetSingleton<Bullet>().BulletPrefab;

        state.EntityManager.GetName(prefab, out var prefabName);
        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (input, trans) in SystemAPI.Query<RefRW<BulletInput>, RefRO<LocalTransform>>().WithAll<Simulate>())
        {
            if (input.ValueRO.Fire)
            {
                var bullet = commandBuffer.Instantiate(prefab);
                commandBuffer.AddComponent(bullet, trans.ValueRO);
                input.ValueRW = new BulletInput { Fire = false };
            }
        }
        commandBuffer.Playback(state.EntityManager);
    }
}