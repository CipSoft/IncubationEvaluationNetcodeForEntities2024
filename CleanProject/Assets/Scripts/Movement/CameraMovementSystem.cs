using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
[BurstCompile]
public partial struct CameraMovementSystem : ISystem
{
#if !UNITY_SERVER
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
#endif

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;
        foreach (var (input, trans) in SystemAPI.Query<RefRO<CameraInput>, RefRW<LocalTransform>>().WithAll<GhostOwnerIsLocal>())
        {
            var cameraXRotation = input.ValueRO.MouseX;

            // Rotate the player around the Y axis
            var playerRotation = trans.ValueRW.Rotation;
            var newPlayerRotation = math.mul(playerRotation, quaternion.RotateY(math.radians(cameraXRotation * deltaTime * 10))); // Adjust sensitivity as needed
            trans.ValueRW.Rotation = newPlayerRotation;
        }
    }
}