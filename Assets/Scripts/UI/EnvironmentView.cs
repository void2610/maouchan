using UnityEngine;

public class EnvironmentView : MonoBehaviour
{
    [SerializeField] private GameObject wall;

    public Vector3 WallPosition => wall.transform.position;

    public void BreakWall() => wall.SetActive(false);
}
