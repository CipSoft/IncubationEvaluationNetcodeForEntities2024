using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
[BurstCompile]
public partial struct CubeMovementSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;
        var speed = 4f;

        foreach (var (input, trans) in SystemAPI.Query<RefRO<CubeInput>, RefRW<LocalTransform>>().WithAll<Simulate>())
        {
            var moveInput = new float2(input.ValueRO.Horizontal, input.ValueRO.Vertical);
            moveInput = deltaTime * speed * math.normalizesafe(moveInput);

            // Create a movement vector in the player's local space
            var localMove = new float3(moveInput.x, 0, moveInput.y);

            // Transform the local movement vector to world space using the player's rotation
            var worldMove = math.rotate(trans.ValueRO.Rotation, localMove);

            // Calculate the potential new position
            float3 newPosition = trans.ValueRW.Position + worldMove;

            // Update the player's position
            trans.ValueRW.Position = newPosition;
        }
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(BeforePhysicsSystemGroup))]
partial struct PhysicsContraintSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (velocity, trans) in SystemAPI.Query<RefRW<PhysicsVelocity>, RefRW<LocalTransform>>().WithAll<GhostOwnerIsLocal>())
        {
            velocity.ValueRW.Linear = float3.zero;
            velocity.ValueRW.Angular = float3.zero;

            trans.ValueRW.Position.y = 0;

            trans.ValueRW.Rotation.value.x = 0;
            trans.ValueRW.Rotation.value.z = 0;
        }
    }
}