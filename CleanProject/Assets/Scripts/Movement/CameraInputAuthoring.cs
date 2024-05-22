using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;

public struct CameraInput : IInputComponentData
{
    public float MouseX;
    public float MouseY;
}

public class CameraInputAuthoring : MonoBehaviour
{
    class Baking : Baker<CameraInputAuthoring>
    {
        public override void Bake(CameraInputAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<CameraInput>(entity);
        }
    }
}


[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial struct CameraUpdateSystem : ISystem
{
    private float _VerticalAngle;

    public void OnUpdate(ref SystemState state)
    {
        var cam = Camera.main;
        if (cam == null)
            return;

        foreach (var (trans, input) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<CameraInput>>().WithAll<GhostOwnerIsLocal>())
        {
            var playerPosition = trans.ValueRO.Position;
            var playerRotation = trans.ValueRO.Rotation;

            // Update vertical angle
            _VerticalAngle -= input.ValueRO.MouseY * 0.1f; // Adjust sensitivity as needed
            _VerticalAngle = math.clamp(_VerticalAngle, -8f, 45f); // Clamp the vertical angle

            // Camera offset behind the player
            var distanceFromPlayer = 4f;
            var heightAbovePlayer = 3f;

            var cameraOffset = new float3(0, heightAbovePlayer, -distanceFromPlayer);
            var cameraRotation = quaternion.Euler(math.radians(_VerticalAngle), 0, 0);
            var offsetRotated = math.mul(playerRotation, math.mul(cameraRotation, cameraOffset));

            var cameraPosition = playerPosition + offsetRotated;

            // Set camera position
            cam.transform.position = cameraPosition;

            // Camera rotation to look at the player
            cam.transform.LookAt(playerPosition + new float3(0, 1, 0)); // Adjust to look at player's head
        }
    }
}


[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial struct ClientCameraInput : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var mouse = Mouse.current;
        if (mouse == null)
            return;
        
        var mouseDelta = mouse.delta.ReadValue();
        var mouseX = mouseDelta.x;
        var mouseY = mouseDelta.y;

        foreach (var cameraInput in SystemAPI.Query<RefRW<CameraInput>>().WithAll<GhostOwnerIsLocal>())
        {
            cameraInput.ValueRW = new CameraInput
            {
                MouseX = mouseX,
                MouseY = mouseY
            };
        }
    }
}