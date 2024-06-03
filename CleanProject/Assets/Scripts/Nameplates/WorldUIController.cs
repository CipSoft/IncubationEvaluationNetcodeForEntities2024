using System.Collections.Generic;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

public class WorldUIController : MonoBehaviour
{
    [SerializeField] private GameObject _NameplatePrefab;

    private Dictionary<int, GameObject> _Nameplates = new ();

    private Transform _MainCameraTransform;

    private void Start()
    {
        _MainCameraTransform = Camera.main.transform;
    }

    public void ShowNameplate(string name, LocalTransform localTransform, int networkId)
    {
        GameObject nameplate;
        if (_Nameplates.ContainsKey(networkId))
        {
            nameplate = _Nameplates[networkId];
            nameplate.transform.position = new Vector3(localTransform.Position.x, localTransform.Position.y + 2.5f, localTransform.Position.z);
            nameplate.transform.LookAt(_MainCameraTransform);
            nameplate.transform.Rotate(0, 180, 0);

            return;
        }

        nameplate = Instantiate(_NameplatePrefab, transform);
        nameplate.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = name;
        _Nameplates.Add(networkId, nameplate);
    }

    public void HideNameplate(int networkId)
    {
        if (_Nameplates.ContainsKey(networkId))
        {
            Destroy(_Nameplates[networkId]);
            _Nameplates.Remove(networkId);
        }
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial class WorldUIControllerSystem : SystemBase
{
    private WorldUIController _WorldUIController;

    protected override void OnCreate()
    {
        _WorldUIController = GameObject.FindFirstObjectByType<WorldUIController>();
    }

    protected override void OnUpdate()
    {
        if (_WorldUIController == null)
        {
            _WorldUIController = GameObject.FindFirstObjectByType<WorldUIController>();
            if (_WorldUIController == null)
            {
                return;
            }
        }
        foreach (var (localTransform, nickname, ghostOwner) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<Nickname>, RefRO<GhostOwner>>())
        {
            _WorldUIController.ShowNameplate(nickname.ValueRO.Value.ToString(), localTransform.ValueRO, ghostOwner.ValueRO.NetworkId);
        }
    }
}