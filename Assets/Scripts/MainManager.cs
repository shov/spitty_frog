using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class MainManager : MonoBehaviour
{
    [System.Serializable]
    public class SaveData
    {
        public int topScore;
    }

    public static MainManager Instance { get; private set; }
    public static string SAVE_FILE_PATH;

    protected int m_score = 0;
    public int Score
    {
        get
        {
            return m_score;
        }
        set
        {
            m_score = value;
            SaveScore(m_score);
        }
    }

    public void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Start()
    {
        SAVE_FILE_PATH = Application.persistentDataPath + @"/savefile.json";
    }

    protected void SaveScore(int score)
    {
        var loadedData = Storage.Load();
        if (loadedData.topScore < 0 || score > loadedData.topScore)
        {
            loadedData.topScore = score;
            Storage.Save(loadedData);
        }
    }
}
