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
    public GameObject WallPrefab;
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
            AddComponent(entity, new WallPrefabComponent { WallPrefab = GetEntity(authoring.WallPrefab, TransformUsageFlags.Dynamic) });
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

public struct WallPrefabComponent : IComponentData
{
    public Entity WallPrefab;
}

[BurstCompile]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct TileGeneratorSystem : ISystem
{
    private EntityQuery _TileQuery;

    public void OnCreate(ref SystemState state)
    {
        _TileQuery = state.GetEntityQuery(ComponentType.ReadOnly<FieldSizeComponent>(), ComponentType.ReadOnly<TileOptionBuffer>(), ComponentType.ReadOnly<WallPrefabComponent>());
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (_TileQuery.IsEmptyIgnoreFilter)
            return;

        var tileEntity = _TileQuery.GetSingletonEntity();
        var fieldSize = state.EntityManager.GetComponentData<FieldSizeComponent>(tileEntity);
        var tileOptions = state.EntityManager.GetBuffer<TileOptionBuffer>(tileEntity);
        var wallPrefab = state.EntityManager.GetComponentData<WallPrefabComponent>(tileEntity).WallPrefab;

        var entityCommandBuffer = new EntityCommandBuffer(Allocator.Temp);

        var random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1, int.MaxValue));
        const float tileSize = 4f;  // Tile size is 5x5 units

        // Generate the field of tiles
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

        // Generate the walls around the field
        for (int x = -1; x <= fieldSize.Width; x++)
        {
            for (int y = -1; y <= fieldSize.Height; y++)
            {
                if (x == -1 || x == fieldSize.Width || y == -1 || y == fieldSize.Height)
                {
                    var wallInstance = entityCommandBuffer.Instantiate(wallPrefab);

                    float3 position = float3.zero;
                    quaternion rotation = quaternion.identity;

                    if (x == -1) // Left wall
                    {
                        position = new float3(-tileSize / 2, 0, y * tileSize);
                        rotation = quaternion.RotateY(math.radians(90));
                    }
                    else if (x == fieldSize.Width) // Right wall
                    {
                        position = new float3(fieldSize.Width * tileSize - tileSize / 2, 0, y * tileSize);
                        rotation = quaternion.RotateY(math.radians(90));
                    }
                    else if (y == -1) // Bottom wall
                    {
                        position = new float3(x * tileSize, 0, -tileSize / 2);
                    }
                    else if (y == fieldSize.Height) // Top wall
                    {
                        position = new float3(x * tileSize, 0, fieldSize.Height * tileSize - tileSize / 2);
                    }

                    entityCommandBuffer.SetComponent(wallInstance, new LocalTransform
                    {
                        Position = position,
                        Rotation = rotation,
                        Scale = 1f
                    });
                }
            }
        }

        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();

        // Remove the components so the system doesn't run again
        state.EntityManager.RemoveComponent<FieldSizeComponent>(tileEntity);
        state.EntityManager.RemoveComponent<TileOptionBuffer>(tileEntity);
        state.EntityManager.RemoveComponent<WallPrefabComponent>(tileEntity);
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
