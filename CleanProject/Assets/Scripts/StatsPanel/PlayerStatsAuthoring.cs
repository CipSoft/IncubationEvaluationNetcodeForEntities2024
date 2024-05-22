using System;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public struct PlayerStats : IComponentData
{
    public uint NumberOfSystems;
    public int NumberOfPlayers;

    public int EstimatedRTT;
    public int DeviationRTT;
}

public class PlayerStatsAuthoring : MonoBehaviour
{
    class Baker : Baker<PlayerStatsAuthoring>
    {
        public override void Bake(PlayerStatsAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new PlayerStats());
        }
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial class UpdatePlayerStatsSystem : SystemBase
{
    private uint _PreviousFrameVersion;
    private uint _CurrentFrameVersion;

    public Action<PlayerStats> OnPlayerStatsUpdate;

    protected override void OnCreate()
    {
        RequireForUpdate<PlayerStats>();
    }

    protected override void OnUpdate()
    {
        var playerStats = SystemAPI.GetSingletonRW<PlayerStats>().ValueRW;
        _CurrentFrameVersion = EntityManager.GlobalSystemVersion - _PreviousFrameVersion;
        _PreviousFrameVersion = EntityManager.GlobalSystemVersion;
        playerStats.NumberOfSystems = _CurrentFrameVersion;
        playerStats.NumberOfPlayers = EntityManager.CreateEntityQuery(typeof(GhostOwner)).CalculateEntityCount();
        foreach (var networkSnapshotAck in SystemAPI.Query<RefRO<NetworkSnapshotAck>>())
        {
            playerStats.EstimatedRTT = (int)networkSnapshotAck.ValueRO.EstimatedRTT;
            playerStats.DeviationRTT = (int)networkSnapshotAck.ValueRO.DeviationRTT;
        }
        if (_CurrentFrameVersion % 10 == 0)
            OnPlayerStatsUpdate?.Invoke(playerStats);
        SystemAPI.SetSingleton(playerStats);
    }
}