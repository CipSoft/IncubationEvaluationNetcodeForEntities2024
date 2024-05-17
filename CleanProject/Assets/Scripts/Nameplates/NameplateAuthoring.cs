using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public struct Nickname : IComponentData
{
    [GhostField]
    public FixedString32Bytes Value;
}

public class NameplateAuthoring : MonoBehaviour
{
    [SerializeField] private string _Nickname;

    class Baker : Baker<NameplateAuthoring>
    {
        public override void Bake(NameplateAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Nickname { Value = authoring._Nickname });
        }
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct SetNicknameSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (nickname, ghostOwner) in SystemAPI.Query<RefRW<Nickname>, RefRO<GhostOwner>>())
        {
            nickname.ValueRW.Value = new FixedString32Bytes($"Player {ghostOwner.ValueRO.NetworkId}");
        }
    }
}

//ISystem to log the nickname of the player
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial struct NameplateSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (nickname, ghostOwner) in SystemAPI.Query<RefRO<Nickname>, RefRO<GhostOwner>>())
        {
            Debug.Log($"Player {nickname.ValueRO.Value} has ghost owner {ghostOwner.ValueRO.NetworkId}");
        }
    }
}