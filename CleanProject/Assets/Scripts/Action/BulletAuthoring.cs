using Unity.Entities;
using UnityEngine;

public struct Bullet : IComponentData
{
    public float Speed;
    public float LifeTime;
    public Entity BulletPrefab;
}

public class BulletAuthoring : MonoBehaviour
{
    public float Speed;
    public float LifeTime;
    public GameObject BulletPrefab;

    class Baking : Baker<BulletAuthoring>
    {
        public override void Bake(BulletAuthoring authoring)
        {
            Bullet component = default;
            component.Speed = authoring.Speed;
            component.LifeTime = authoring.LifeTime;
            component.BulletPrefab = GetEntity(authoring.BulletPrefab, TransformUsageFlags.Dynamic);
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, component);
        }
    }
}