using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.AudioSettings;


public class ScanningFileHandler : MonoBehaviour
{
    public static ScanningFileHandler Instance { get; private set; }

    [SerializeField] TMP_InputField name, mobile;
    public string result; // Make public for access
    FileInfo f;
    string path, data;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        path = Application.dataPath + "/" + "BigFive.csv";
        Debug.Log(Application.dataPath);
        f = new FileInfo(path);
    }
    public void FileSave()
    {
        StreamWriter w;
        if (!f.Exists)
        {
            w = f.CreateText();
            w.WriteLine("Name,Phone,Result");
        }
        else
        {
            w = new StreamWriter(path, true);
        }
        w.WriteLine(name.text + "," + mobile.text + "," + result); // input field data
        w.Close();
    }

    public void readTextFile()
    {
        StreamWriter w;
        if (!f.Exists)
        {
            w = f.CreateText();
            w.WriteLine("Name,phone,result");
        }
        else
        {
            w = new StreamWriter(path, true);
        }
        w.Close();
        StreamReader inp_stm = new StreamReader(path);
        data = "";
        while (!inp_stm.EndOfStream)
        {
            string inp_ln = inp_stm.ReadLine();
            data += inp_ln + "\r\n";
        }
        inp_stm.Close();
    }
    public bool IsMobileExists(string mobileToCheck)
    {
        if (!File.Exists(path))
            return false;

        using (var reader = new StreamReader(path))
        {
            string line;
            bool isFirstLine = true;
            while ((line = reader.ReadLine()) != null)
            {
                if (isFirstLine) { isFirstLine = false; continue; } // skip header
                var parts = line.Split(',');
                if (parts.Length > 1 && parts[1] == mobileToCheck)
                    return true;
            }
        }
        return false;
    }
}
