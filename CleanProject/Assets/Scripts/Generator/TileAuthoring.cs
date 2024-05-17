using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[System.Serializable]
public struct TileOption
{
    public GameObject TilePrefab;
    [Range(0, 1)]
    public float Probability;
}

public class TileAuthoring : MonoBehaviour
{
    public TileOption[] TileOptions;
    public int FieldWidth;
    public int FieldHeight;

    class Baker : Baker<TileAuthoring>
    {
        public override void Bake(TileAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            var tileOptionArray = AddBuffer<TileOptionBuffer>(entity);
            foreach (var tileOption in authoring.TileOptions)
            {
                tileOptionArray.Add(new TileOptionBuffer
                {
                    TilePrefab = GetEntity(tileOption.TilePrefab, TransformUsageFlags.Dynamic),
                    Probability = tileOption.Probability
                });
            }

            AddComponent(entity, new FieldSizeComponent { Width = authoring.FieldWidth, Height = authoring.FieldHeight });
        }
    }
}

public struct TileOptionBuffer : IBufferElementData
{
    public Entity TilePrefab;
    public float Probability;
}

public struct FieldSizeComponent : IComponentData
{
    public int Width;
    public int Height;
}

[BurstCompile]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct TileGeneratorSystem : ISystem
{
    private EntityQuery _TileQuery;

    public void OnCreate(ref SystemState state)
    {
        _TileQuery = state.GetEntityQuery(ComponentType.ReadOnly<FieldSizeComponent>(), ComponentType.ReadOnly<TileOptionBuffer>());
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (_TileQuery.IsEmptyIgnoreFilter)
            return;

        var tileEntity = _TileQuery.GetSingletonEntity();
        var fieldSize = state.EntityManager.GetComponentData<FieldSizeComponent>(tileEntity);
        var tileOptions = state.EntityManager.GetBuffer<TileOptionBuffer>(tileEntity);

        var entityCommandBuffer = new EntityCommandBuffer(Allocator.Temp);

        var random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1, int.MaxValue));
        const float tileSize = 4f;  // Tile size is 5x5 units

        for (int x = 0; x < fieldSize.Width; x++)
        {
            for (int y = 0; y < fieldSize.Height; y++)
            {
                var selectedTile = SelectTile(tileOptions, ref random);
                var instance = entityCommandBuffer.Instantiate(selectedTile);

                entityCommandBuffer.SetComponent(instance, new LocalTransform
                {
                    Position = new float3(x * tileSize, 0, y * tileSize),
                    Rotation = quaternion.identity,
                    Scale = 1f
                });
            }
        }

        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();

        // Remove the components so the system doesn't run again
        state.EntityManager.RemoveComponent<FieldSizeComponent>(tileEntity);
        state.EntityManager.RemoveComponent<TileOptionBuffer>(tileEntity);
    }

    private Entity SelectTile(DynamicBuffer<TileOptionBuffer> tileOptions, ref Unity.Mathematics.Random random)
    {
        float totalProbability = 0;
        foreach (var option in tileOptions)
        {
            totalProbability += option.Probability;
        }

        float randomValue = random.NextFloat(0, totalProbability);
        float cumulativeProbability = 0;

        foreach (var option in tileOptions)
        {
            cumulativeProbability += option.Probability;
            if (randomValue <= cumulativeProbability)
            {
                return option.TilePrefab;
            }
        }

        return tileOptions[0].TilePrefab; // Fallback in case of rounding errors
    }
}