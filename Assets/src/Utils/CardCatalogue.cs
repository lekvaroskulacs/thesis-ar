using System.Collections.Generic;
using System.IO;
using UnityEngine;

using Newtonsoft.Json;
using System;

[Serializable]
public class CardInfo
{
    public string prefabPath;
    public string trackedImagePath;
}

public class CardCatalogue
{
    static private Dictionary<string, CardInfo> _catalogue;
    static public Dictionary<string, CardInfo> GetCatalogue()
    {
        if (_catalogue != null)
        {
            return _catalogue;
        }

        string path = Application.streamingAssetsPath + "/CardCatalogue.json";
        string json = null;

#if UNITY_ANDROID && !UNITY_EDITOR
        // On Android, StreamingAssets is packed in a jar, so use UnityWebRequest
        using (var www = UnityEngine.Networking.UnityWebRequest.Get(path))
        {
            www.SendWebRequest();
            while (!www.isDone) { }
            if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                json = www.downloadHandler.text;
            else
                throw new FileNotFoundException(path);
        }
#else
        if (System.IO.File.Exists(path))
        {
            json = System.IO.File.ReadAllText(path);
        }
        else
        {
            throw new FileNotFoundException(path);
        }
#endif

        var loadedCatalogue = JsonConvert.DeserializeObject<Dictionary<string, CardInfo>>(json);
        if (loadedCatalogue != null)
        {
            _catalogue = loadedCatalogue;
            return loadedCatalogue;
        }
        else
        {
            throw new FileLoadException("Failed to parse CardCatalogue.json");
        }
    }

    static public GameObject GetPrefabForCard(string key)
    {
        return Resources.Load<GameObject>(GetCatalogue()[key].prefabPath);
    }
}
