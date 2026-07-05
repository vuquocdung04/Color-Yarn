using UnityEngine;

public class ColorRepo : MonoBehaviour
{
    public static ColorRepo Instance { get; private set; }

    public void InitInstance() => Instance = this;

    [SerializeField] private ColorDatabase database;

    public void Init()
    {
    }

    public ColorEntry GetSet(string key) => database.GetSet(key);
}
