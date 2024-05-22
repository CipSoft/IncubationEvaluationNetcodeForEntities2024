using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
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

        // Assume field size is defined somewhere accessible
        // Ideally, this should be a component that we can query, but for this example, we use constants
        const float tileSize = 4f;
        const int fieldWidth = 10; // Replace with actual field width
        const int fieldHeight = 10; // Replace with actual field height

        float minX = -.75f;
        float maxX = .75f + (fieldWidth - 1) * tileSize;
        float minZ = -.75f;
        float maxZ = .75f + (fieldHeight - 1) * tileSize;

        foreach (var (input, trans) in SystemAPI.Query<RefRO<CubeInput>, RefRW<LocalTransform>>().WithAll<Simulate>())
        {
            var moveInput = new float2(input.ValueRO.Horizontal, input.ValueRO.Vertical);
            moveInput = math.normalizesafe(moveInput) * deltaTime * speed;

            // Create a movement vector in the player's local space
            var localMove = new float3(moveInput.x, 0, moveInput.y);

            // Transform the local movement vector to world space using the player's rotation
            var worldMove = math.rotate(trans.ValueRO.Rotation, localMove);

            // Calculate the potential new position
            float3 newPosition = trans.ValueRW.Position + worldMove;

            // Clamp the new position to stay within the field boundaries
            newPosition.x = math.clamp(newPosition.x, minX, maxX);
            newPosition.z = math.clamp(newPosition.z, minZ, maxZ);

            // Update the player's position
            trans.ValueRW.Position = newPosition;
        }
    }
}