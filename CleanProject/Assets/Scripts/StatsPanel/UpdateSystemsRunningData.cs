using Unity.Burst;
using Unity.Entities;

public partial struct UpdateSystemsRunningData : ISystem
{
    private uint _PreviousFrameVersion;
    private uint _CurrentFrameVersion;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerStats>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var playerStats = SystemAPI.GetSingletonRW<PlayerStats>().ValueRW;
        _CurrentFrameVersion = state.EntityManager.GlobalSystemVersion - _PreviousFrameVersion;
        _PreviousFrameVersion = state.EntityManager.GlobalSystemVersion;
        playerStats.NumberOfSystems = _CurrentFrameVersion;
        SystemAPI.SetSingleton(playerStats);
    }
}