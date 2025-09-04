using System.Collections.Generic;
[System.Serializable]
public class Question
{
    public string questionText;
    public List<string> answers = new List<string>(4);
    public int correctAnswerIndex;
}
