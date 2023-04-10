using System;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class highlightData
{
    public int start;
    public int length;
    public string value;
    public Color color;
    public highlightData(int s, int l, string v, Color c) {start = s;length = l;value = v;color = c;}
}

public class maihighlight : MonoBehaviour
{
    [Header("Settings")]
    public float updateDuration = 1f;

    // [Header("Pre Info")]
    // public Color titleColor;
    // public Color artistColor;
    // public Color wholebpmColor;
    // public Color lvColor;
    // public Color firstColor;
    // public Color inoteColor;

    [Header("Marks")]
    public Color bpmColor;
    public Color beatMarkColor;
    public Color tapColor;
    
    [Header("Notes")]
    public Color emptyBeatColor;

    // Regex titleReg = new Regex(@"&title=.*");
    // Regex artistReg = new Regex(@"&artist=.*");
    // Regex wholebpmReg = new Regex(@"&wholebpm=[0-9\.]*");
    // Regex lvReg = new Regex(@"&lv_[0-9]=[0-9\.]*");
    // Regex firstReg = new Regex(@"&first=[0-9\.]*");
    // Regex inoteReg = new Regex(@"&inote_[0-9]=");

    Regex bpmReg = new Regex(@"\([0-9\.]*\)");
    Regex beatReg = new Regex(@"\{[0-9.]*\}");
    Regex tapReg = new Regex(@"([1-8]),");
    Regex commaReg = new Regex(@"\,+");
    // Each: 
    Regex eachReg = new Regex(@"([A-Za-z0-9])+(\[\d:\d\])?(\/)+([A-Za-z0-9])+(\[\d:\d\])?");

    private List<highlightData> highlights = new List<highlightData>{};

    public void UpdateHighlight(string fumen)
    {
        print("update!");
        List<string> highlighted = new List<string>();
        string[] lines = fumen.Split("\n");
        foreach(string line in lines)
        {
            highlights.Clear();
            string h = line;
            // h = SetColor(h, titleReg, titleColor);
            // h = SetColor(h, artistReg, artistColor);
            // h = SetColor(h, wholebpmReg, wholebpmColor);
            // h = SetColor(h, lvReg, lvColor);
            // h = SetColor(h, firstReg, firstColor);
            // h = SetColor(h, inoteReg, inoteColor);

            h = SetColor(h, bpmReg, bpmColor);
            h = SetColor(h, beatReg, beatMarkColor);
            h = SetColor(h, tapReg, tapColor);

            h = SetColor(h, commaReg, emptyBeatColor);
            highlighted.Add(h);
        }
        GetComponent<TMP_InputField>().text = string.Join("\n", highlighted);
    }

    string SetColor(string line, Regex reg, Color color)
    {
        MatchCollection matches = reg.Matches(line);
        foreach(Match m in matches) {
            line = line.Replace(m.Value, string.Format("<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGB(color), m.Value));
        }
        return line;
    }

}
