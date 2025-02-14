using UnityEngine;

public class TorchNetwork : MonoBehaviour
{
    [SerializeField] private TorchLightController mainTorch;
    [SerializeField] private TorchLightController[] satelliteTorches;
    [SerializeField] private float networkRadius = 10f;
    
    private void Start()
    {
        if (satelliteTorches.Length != 4)
        {
            Debug.LogError("Tam olarak 4 yan meşale gerekli!");
            return;
        }
        
        PositionSatelliteTorches();
    }

    private void PositionSatelliteTorches()
    {
        for (int i = 0; i < 4; i++)
        {
            float angle = i * 90f * Mathf.Deg2Rad; // 90 derece aralıklarla
            Vector3 position = mainTorch.transform.position + new Vector3(
                Mathf.Cos(angle) * networkRadius,
                Mathf.Sin(angle) * networkRadius,
                0
            );
            
            satelliteTorches[i].transform.position = position;
        }
    }

    public float GetMainTorchPower()
    {
        return mainTorch.GetLightRatio();
    }

    public bool AreAllTorchesAlive()
    {
        foreach (var torch in satelliteTorches)
        {
            if (!torch.enabled) return false;
        }
        return mainTorch.enabled;
    }
} 