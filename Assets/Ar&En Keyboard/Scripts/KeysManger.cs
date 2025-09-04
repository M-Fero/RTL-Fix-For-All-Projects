using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class KeysManger : MonoBehaviour {
    #region singlton
    public static KeysManger keysManger;
    private void Awake()
    {
        keysManger = this;
    }
    #endregion

    public TMP_InputField textToAddTo = null;

    public void FoucseInputField(TMP_InputField inputField)
    {
        textToAddTo = inputField;
    }

    public void AppendCharcter(string character)
    {
        if (textToAddTo != null)
            textToAddTo.text += character;
    }
    public void DeleteCharcter()
    {
        if (textToAddTo != null)
            if(textToAddTo.text.Length>0)
                textToAddTo.text = textToAddTo.text.Substring(0, textToAddTo.text.Length-1);
    }

}
