using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private QuestionData defaultQuestionData = null;

    [Header("Inspect")]
    public QuestionData questionData = null;

    private string QUESTION_SAVE_PATH;

    private void Awake()
    {
        QUESTION_SAVE_PATH = Application.persistentDataPath + Path.DirectorySeparatorChar + "questionsData.json";
        LoadQuestionData();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }
    
    public void LoadQuestionData()
    {
        if (File.Exists(QUESTION_SAVE_PATH))
        {
            try
            {
                questionData = ScriptableObject.CreateInstance<QuestionData>();
                string json = File.ReadAllText(QUESTION_SAVE_PATH);
                JsonUtility.FromJsonOverwrite(json, questionData);
            }
            catch (IOException ex)
            {
                Debug.LogErrorFormat("Cannot load question data file: {0} - {1}!", QUESTION_SAVE_PATH, ex.ToString());
            }
        }
        else
        {
            if (defaultQuestionData != null)
            {
                questionData = Instantiate(defaultQuestionData);
            }
            else
            {
                Debug.LogError("Default Question is null.");
            }
        }
    }

    public void SaveQuestionData()
    {
        File.WriteAllText(QUESTION_SAVE_PATH, JsonUtility.ToJson(questionData));
    }
}
