using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.InputSystem;

public struct CubeInput : IInputComponentData
{
    public sbyte Horizontal;
    public sbyte Vertical;
}

[DisallowMultipleComponent]
public class CubeInputAuthoring : MonoBehaviour
{
    class Baking : Baker<CubeAuthoring>
    {
        public override void Bake(CubeAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<CubeInput>(entity);
        }
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[UpdateInGroup(typeof(GhostInputSystemGroup))]
public partial struct ClientCubeInput : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        bool left = Keyboard.current.aKey.isPressed;
        bool right = Keyboard.current.dKey.isPressed;
        bool up = Keyboard.current.wKey.isPressed;
        bool down = Keyboard.current.sKey.isPressed;

        foreach (var playerInput in SystemAPI.Query<RefRW<CubeInput>>().WithAll<GhostOwnerIsLocal>())
        {
            playerInput.ValueRW = default;
            if (left)
                playerInput.ValueRW.Horizontal = -1;
            if (right)
                playerInput.ValueRW.Horizontal = 1;
            if (up)
                playerInput.ValueRW.Vertical = 1;
            if (down)
                playerInput.ValueRW.Vertical = -1;
        }
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.ThinClientSimulation)]
[UpdateInGroup(typeof(GhostInputSystemGroup))]
public partial struct ThinCubeInput : ISystem
{
    int _FrameCount;
    uint _WorldIndex;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkId>();
    }


    public void OnUpdate(ref SystemState state)
    {
        if (SystemAPI.TryGetSingleton<CommandTarget>(out var commandTarget) && commandTarget.targetEntity == Entity.Null)
            CreateThinClientPlayer(ref state);

        var randomAngle = math.round(math.radians(UnityEngine.Random.Range(0, 127)));
        
        //check if the value 
        foreach (var (playerInput, cameraInput) in SystemAPI.Query<RefRW<CubeInput>, RefRW<CameraInput>>().WithAll<GhostOwnerIsLocal>())
        {
            playerInput.ValueRW = default;
            playerInput.ValueRW.Vertical = 1;

            cameraInput.ValueRW.MouseX = (sbyte) randomAngle;
        }
    }

    private void CreateThinClientPlayer(ref SystemState state)
    {
        var ent = state.EntityManager.CreateEntity();
        state.EntityManager.AddComponent<CubeInput>(ent);
        state.EntityManager.AddComponent<CameraInput>(ent);
        state.EntityManager.AddComponent<BulletInput>(ent);
        state.EntityManager.AddBuffer<InputBufferData<CubeInput>>(ent);
        state.EntityManager.AddBuffer<InputBufferData<CameraInput>>(ent);
        state.EntityManager.AddBuffer<InputBufferData<BulletInput>>(ent);

        var connectionId = SystemAPI.GetSingleton<NetworkId>().Value;
        var connection = SystemAPI.GetSingletonEntity<NetworkId>();

        state.EntityManager.AddComponentData(ent, new GhostOwner { NetworkId = connectionId });
        state.EntityManager.SetComponentData(connection, new CommandTarget { targetEntity = ent });
        state.EntityManager.AddComponent<GhostOwnerIsLocal>(ent);
    }
}