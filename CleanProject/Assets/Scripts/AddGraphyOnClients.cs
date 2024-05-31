using UnityEngine;

public class AddGraphyOnClients : MonoBehaviour
{
    [SerializeField] private GameObject[] _GraphyPrefab;

    private void Start()
    {
#if !UNITY_SERVER
        foreach (var prefab in _GraphyPrefab)
        {
            Debug.Log("Instantiating " + prefab.name);
            Instantiate(prefab);
        }
#endif
    }
}
