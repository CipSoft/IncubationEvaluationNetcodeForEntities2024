using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public struct PlayerStats : IComponentData
{
    public int NetworkId;
    public FixedString64Bytes Name;
    public float FPS;
    public uint NumberOfSystems;

    public int EstimatedRTT;
    public int DeviationRTT;
    public bool ShouldUpdate;
}

public class PlayerStatsAuthoring : MonoBehaviour
{
    [SerializeField] private string _Name;

    class Baker : Baker<PlayerStatsAuthoring>
    {
        public override void Bake(PlayerStatsAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new PlayerStats { Name = authoring._Name });
        }
    }
}