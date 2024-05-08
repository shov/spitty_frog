using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Storage : MonoBehaviour
{
    public static MainManager.SaveData Load()
    {
        if (System.IO.File.Exists(MainManager.SAVE_FILE_PATH))
        {
            string json = System.IO.File.ReadAllText(MainManager.SAVE_FILE_PATH);
            return JsonUtility.FromJson<MainManager.SaveData>(json);
        }
        else
        {
            return new MainManager.SaveData() {
                topScore = 0
            };
        }
    }

    public static void Save(MainManager.SaveData data)
    {
        string json = JsonUtility.ToJson(data);
        System.IO.File.WriteAllText(MainManager.SAVE_FILE_PATH, json);
    }
}
