using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
[BurstCompile]
public partial struct CameraMovementSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;
        foreach (var (input, trans) in SystemAPI.Query<RefRO<CameraInput>, RefRW<LocalTransform>>().WithAll<Simulate>())
        {
            var cameraXRotation = input.ValueRO.MouseX;

            // Rotate the player around the Y axis
            var playerRotation = trans.ValueRW.Rotation;
            var newPlayerRotation = math.mul(playerRotation, quaternion.RotateY(math.radians(cameraXRotation * deltaTime * 10))); // Adjust sensitivity as needed
            trans.ValueRW.Rotation = newPlayerRotation;
        }
    }
}