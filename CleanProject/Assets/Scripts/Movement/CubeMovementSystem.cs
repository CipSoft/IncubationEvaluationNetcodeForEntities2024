using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
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
            moveInput = math.normalizesafe(moveInput) * deltaTime * speed;

            // Create a movement vector in the player's local space
            var localMove = new float3(moveInput.x, 0, moveInput.y);

            // Transform the local movement vector to world space using the player's rotation
            var worldMove = math.rotate(trans.ValueRO.Rotation, localMove);

            // Update the player's position
            trans.ValueRW.Position += worldMove;
        }
    }
}
