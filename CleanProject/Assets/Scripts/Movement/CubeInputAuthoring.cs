using System;
using System.Diagnostics;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.InputSystem;

public struct CubeInput : IInputComponentData
{
    public int Horizontal;
    public int Vertical;
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
public partial struct SampleCubeInput : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        bool left = Keyboard.current.leftArrowKey.isPressed;
        bool right = Keyboard.current.rightArrowKey.isPressed;
        bool up = Keyboard.current.upArrowKey.isPressed;
        bool down = Keyboard.current.downArrowKey.isPressed;

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
public partial struct SampleCubeInputThinClient : ISystem
{
    int _FrameCount;
    uint _WorldIndex;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkId>();

        // Give every thin client some randomness
        var rand = Unity.Mathematics.Random.CreateFromIndex((uint)Stopwatch.GetTimestamp());
        _FrameCount = rand.NextInt(100);
        _WorldIndex = UInt32.Parse(state.World.Name.Substring(state.World.Name.Length - 1));
    }


    public void OnUpdate(ref SystemState state)
    {
        if (SystemAPI.TryGetSingleton<CommandTarget>(out var commandTarget) && commandTarget.targetEntity == Entity.Null)
            CreateThinClientPlayer(ref state);

        byte left, right, up, down;
        left = right = up = down = 0;

        // Move in a random direction
        var currentState = (int)(SystemAPI.Time.ElapsedTime + _WorldIndex) % 4;
        switch (currentState)
        {
            case 0: left = 1; break;
            case 1: right = 1; break;
            case 2: up = 1; break;
            case 3: down = 1; break;
        }

        // Jump every 100th frame
        if (++_FrameCount % 100 == 0)
        {
            _FrameCount = 0;
        }
        
        //check if the value 
        foreach (var playerInput in SystemAPI.Query<RefRW<CubeInput>>().WithAll<GhostOwnerIsLocal>())
        {
            playerInput.ValueRW = default;
            if (left != 0)
                playerInput.ValueRW.Horizontal = -1;
            if (right != 0)
                playerInput.ValueRW.Horizontal = 1;
            if (up != 0)
                playerInput.ValueRW.Vertical = 1;
            if (down != 0)
                playerInput.ValueRW.Vertical = -1;
        }
    }

    void CreateThinClientPlayer(ref SystemState state)
    {
        var ent = state.EntityManager.CreateEntity();
        state.EntityManager.AddComponent<CubeInput>(ent);
        state.EntityManager.AddBuffer<InputBufferData<CubeInput>>(ent);

        var connectionId = SystemAPI.GetSingleton<NetworkId>().Value;
        var connection = SystemAPI.GetSingletonEntity<NetworkId>();

        state.EntityManager.AddComponentData(ent, new GhostOwner { NetworkId = connectionId });
        state.EntityManager.SetComponentData(connection, new CommandTarget { targetEntity = ent });
        state.EntityManager.AddComponent<GhostOwnerIsLocal>(ent);
    }
}