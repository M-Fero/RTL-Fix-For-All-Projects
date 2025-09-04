using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class InputFieldSupport : MonoBehaviour {
    public TMP_InputField myRefrance;
    private void Start()
    {
        myRefrance = GetComponent<TMP_InputField>();
    }
    public void SendReferance()
    {
        KeysManger.keysManger.FoucseInputField(myRefrance);
    }

}
