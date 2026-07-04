
using System.Collections.Generic;
using UnityEngine;

public class DataRepo : MonoBehaviour
{
    public static DataRepo Instance { get; private set; }

    public LocalizationDataBase localizationDataBase;
    public List<AudioDataBase> audioDataList;
    public AvatarDataBase avatarData;
    public void InitInstance() => Instance = this;

    public void Init()
    {
    }
}
