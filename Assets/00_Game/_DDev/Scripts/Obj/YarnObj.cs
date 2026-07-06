using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class YarnObj : MonoBehaviour
{
    public static YarnObj Instance { get; private set; }

    [SerializeField] private Transform root;
    [SerializeField] private int minCube = 1;
    [SerializeField] private int maxCube = 3;

    [Header("Grow")]
    [SerializeField] private float growDuration = 0.35f;
    [SerializeField] private Ease growEase = Ease.OutBack;

    private List<InteractableObject> interactableObjects;

    public Transform Root => root != null ? root : transform;
    public float GrowDuration => growDuration;
    public Ease GrowEase => growEase;

    public int TotalLen { get; private set; }
    public int CurrentLen { get; private set; }

    private void Start() => Init();

    private void Init()
    {
        Instance = this;
        interactableObjects = new List<InteractableObject>(GetComponentsInChildren<InteractableObject>());
        GenerateChildRuntime();
    }

    private void GenerateChildRuntime()
    {
        TotalLen = 0;
        foreach (var obj in interactableObjects)
        {
            if (obj == null) continue;
            obj.Init();
            obj.totalCube = Random.Range(minCube, maxCube + 1);
            obj.BuildCore();
            TotalLen += obj.totalCube + 1;
        }
    }

    public void AddLen(InteractableObject obj)
    {
        if (obj != null) interactableObjects.Add(obj);
    }

    public void RemoveLen(InteractableObject obj)
    {
        interactableObjects.Remove(obj);
        CurrentLen++;
    }
}
