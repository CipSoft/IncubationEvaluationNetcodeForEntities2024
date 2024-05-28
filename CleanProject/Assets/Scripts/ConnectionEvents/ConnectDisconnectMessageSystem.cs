using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[UpdateInGroup(typeof(Unity.Entities.SimulationSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[BurstCompile]
public partial struct ConnectDisconnectMessageSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var connectionEventsForServer = SystemAPI.GetSingleton<NetworkStreamDriver>().ConnectionEventsForTick;

        foreach(var connectionEvent in connectionEventsForServer)
        {
            switch (connectionEvent.State)
            {
                case ConnectionState.State.Connecting:
                    Debug.Log($"Server: Client {connectionEvent.ConnectionId} is connecting");
                    break;
                case ConnectionState.State.Connected:
                    Debug.Log($"Server: Client {connectionEvent.ConnectionId} is connected");
                    break;
                case ConnectionState.State.Disconnected:
                    Debug.Log($"Server: Client {connectionEvent.ConnectionId} is disconnected");
                    break;
            }
        }
    }
}