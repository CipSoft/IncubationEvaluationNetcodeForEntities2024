using System;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using UnityEngine;

// Create a custom bootstrap, which enables auto-connect.
// The bootstrap can also be used to configure other settings as well as to
// manually decide which worlds (client and server) to create based on user input
[UnityEngine.Scripting.Preserve]
public class GameBootstrap : ClientServerBootstrap
{
    private int _RequestedThinClients = 0;
    private bool _LoadTest = false;

    public override bool Initialize(string defaultWorldName)
    {
        NetworkStreamReceiveSystem.DriverConstructor = new CleanDriverConstructor();

        var consoleArgs = Environment.GetCommandLineArgs();
        var ip = "192.168.200.229";
        short port = 5030;

        for (int i = 0; i < consoleArgs.Length; i++)
        {
            if (consoleArgs[i] == "-thinClients")
            {
                _RequestedThinClients = int.Parse(consoleArgs[i + 1]);
            }

            if (consoleArgs[i] == "-loadtest")
            {
                _LoadTest = true;
            }

            if (consoleArgs[i] == "-ip")
            {
                ip = consoleArgs[i + 1];
            }

            if (consoleArgs[i] == "-port")
            {
                port = short.Parse(consoleArgs[i + 1]);
            }
        }
#if !UNITY_EDITOR
        DefaultConnectAddress = NetworkEndpoint.Parse("192.168.200.229", 5030, NetworkFamily.Ipv4);
#endif
        AutoConnectPort = 5030;

        TryCreateThinClientsIfRequested();
        
        if (_LoadTest)
        {
            Application.targetFrameRate = 10;
            return true;
        }
        else
        {
#if !UNITY_SERVER
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 1;
#endif
            return base.Initialize(defaultWorldName);
        }
    }

    private void TryCreateThinClientsIfRequested()
    {
#if UNITY_EDITOR
        MultiplayerPlayModePreferences.RequestedNumThinClients = _RequestedThinClients;
#endif
        for (int i = 0; i < _RequestedThinClients; i++)
        {
            var world = CreateThinClientWorld();

            if (World.DefaultGameObjectInjectionWorld == null || !World.DefaultGameObjectInjectionWorld.IsCreated)
            {
                World.DefaultGameObjectInjectionWorld = world;
            }
        }
    }
}

public class CleanDriverConstructor : INetworkStreamDriverConstructor
{
    public void CreateClientDriver(World world, ref NetworkDriverStore driverStore, NetDebug netDebug)
    {
        var settings = DefaultDriverBuilder.GetNetworkSettings();
        // Left as default: FixSettingsForMegacityMetro(settings, ???);
        DefaultDriverBuilder.RegisterClientUdpDriver(world, ref driverStore, netDebug, settings);
    }

    public void CreateServerDriver(World world, ref NetworkDriverStore driverStore, NetDebug netDebug)
    {
        var settings = DefaultDriverBuilder.GetNetworkServerSettings();
        if (settings.TryGet(out NetworkConfigParameter networkConfig))
        {
            networkConfig.sendQueueCapacity = networkConfig.receiveQueueCapacity = 5000;
            settings.AddRawParameterStruct(ref networkConfig);
        }
        DefaultDriverBuilder.RegisterServerDriver(world, ref driverStore, netDebug, settings);
    }
}