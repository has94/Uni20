using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TeacherManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnBackClick()
    {
        SceneManager.LoadScene(Constant.SCENE_MAIN_MENU);
    }

    public void OnAddQuestionClick()
    {
        SceneManager.LoadScene(Constant.SCENE_TEACHER_ADD_QUESTION);
    }

    public void OnModifyQuestion()
    {
        SceneManager.LoadScene(Constant.SCENE_TEACHER_MODIFY_QUESTION);
    }
}
