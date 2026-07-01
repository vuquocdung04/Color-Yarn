using UnityEngine;

public class CubePhaseController : MonoBehaviour
{
    [Header("Root cho cac loi tach ra")]
    [SerializeField] private Transform root;

    public Transform Root => root != null ? root : transform;
}