using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[CreateAssetMenu(fileName = "Default Question Data", menuName = "Question Data", order = 52)]
public class QuestionData : ScriptableObject
{
    public List<Question> questions = new List<Question>();
}
