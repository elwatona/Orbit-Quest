using UnityEngine;
using System.Collections.Generic;
public class LevelManager : MonoBehaviour
{
    [SerializeField] Limits _limits;
    [SerializeField] GameObject[] _levelBoundsGO;
    private List<ILevelBounds> _levelBounds = new List<ILevelBounds>();
    void Awake()
    {
        foreach (GameObject levelBoundGO in _levelBoundsGO)
        {
            levelBoundGO.TryGetComponent(out ILevelBounds levelBound);
            if (levelBound != null)
            {
                _levelBounds.Add(levelBound);
            }
        }
        foreach (ILevelBounds levelBound in _levelBounds)
        {
            levelBound.SetLimits(_limits);
        }
    }
}