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