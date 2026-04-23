using UnityEngine;
[CreateAssetMenu(fileName = "Limits", menuName = "Proto-Pablo/Limits", order = 0)]
public class Limits : ScriptableObject
{
    [SerializeField] Vector2 _min;
    [SerializeField] Vector2 _max;
    public Vector2 Min => _min;
    public Vector2 Max => _max;
    public bool IsInside(Vector2 position)
    {
        return position.x >= Min.x && position.x <= Max.x && position.y >= Min.y && position.y <= Max.y;
    }
}