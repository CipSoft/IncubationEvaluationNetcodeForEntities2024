using Unity.Entities;
using UnityEngine;

public class StatsUpdater : MonoBehaviour
{
#if !UNITY_SERVER
    private TMPro.TextMeshProUGUI _FpsText;

    private void OnEnable()
    {
        _FpsText = GetComponent<TMPro.TextMeshProUGUI>();
        //wait until update player stats system is created
        if (World.DefaultGameObjectInjectionWorld == null)
            return;

        foreach (var world in World.All)
        {
            var updatePlayerStatsSystem = world.GetExistingSystemManaged<UpdatePlayerStatsSystem>();
            if (updatePlayerStatsSystem != null)
            {
                updatePlayerStatsSystem.OnPlayerStatsUpdate += OnPlayerStatsUpdate;
                break;
            }
        }
    }

    private void OnDisable()
    {
        if (World.DefaultGameObjectInjectionWorld == null)
            return;
        var updatePlayerStatsSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<UpdatePlayerStatsSystem>();
        if (updatePlayerStatsSystem == null)
            return;
        updatePlayerStatsSystem.OnPlayerStatsUpdate -= OnPlayerStatsUpdate;
    }

    private void OnPlayerStatsUpdate(PlayerStats playerStats)
    {
        _FpsText.text = $"Number of systems: {playerStats.NumberOfSystems}\n" +
                       $"Number of players: {playerStats.NumberOfPlayers}\n" +
                       $"Estimated RTT: {playerStats.EstimatedRTT} ms\n" +
                       $"Deviation RTT: {playerStats.DeviationRTT} ms";
    }
#endif
}
