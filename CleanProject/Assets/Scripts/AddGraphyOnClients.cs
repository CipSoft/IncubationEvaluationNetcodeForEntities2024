using UnityEngine;

public class AddGraphyOnClients : MonoBehaviour
{
    [SerializeField] private GameObject _GraphyPrefab;

    private void Start()
    {
#if !UNITY_SERVER
        Instantiate(_GraphyPrefab);
#endif
    }
}
