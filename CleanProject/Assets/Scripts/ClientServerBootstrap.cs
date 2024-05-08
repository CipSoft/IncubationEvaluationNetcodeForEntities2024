using System;
using Unity.Entities;
using Unity.NetCode;
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
        AutoConnectPort = 5030;

        var consoleArgs = Environment.GetCommandLineArgs();

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
        }

        TryCreateThinClientsIfRequested();
        
        if (_LoadTest)
        {
            return true;
        }
        else
        {
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

        Debug.Log($"Created {_RequestedThinClients} thin clients");
    }
}