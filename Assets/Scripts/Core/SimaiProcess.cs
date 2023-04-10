// source code from https://github.com/LingFeng-bbben/MajdataEdit/blob/master/SimaiProcess.cs
// Licensed under GPL-3.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UnityEngine;

public class SimaiProcess: MonoBehaviour
{
    static public string title;
    static public string artist;
    static public string designer;
    static public string other_commands;
    static public float first = 0;
    static public string[] fumens = new string[7];
    static public string[] levels = new string[7];
    /// <summary>
    /// the timing points that contains notedata
    /// </summary>
    static public List<SimaiTimingPoint> notelist = new List<SimaiTimingPoint>(); 
    /// <summary>
    /// the timing points made by "," in maidata
    /// </summary>
    static public List<SimaiTimingPoint> timinglist = new List<SimaiTimingPoint>();
    /// <summary>
    /// Reset all the data in the static class.
    /// </summary>
    static public void ClearData()
    {
        title = "";
        artist = "";
        designer = "";
        first = 0;
        fumens = new string[7];
        levels = new string[7];
        notelist = new List<SimaiTimingPoint>(); 
        timinglist = new List<SimaiTimingPoint>();
    }
    /// <summary>
    /// Read the maidata.txt into the static class, including the variables. Show up a messageBox when enconter any exception.
    /// </summary>
    /// <param name="filename">file path of maidata.txt</param>
    /// <returns>if the read process faced any error</returns>
    static public bool ReadData(string filename)
    {
        try
        {
            string[] maidataTxt = File.ReadAllLines(filename, Encoding.UTF8);
            return ReadDataRaw(maidataTxt);
        }
        catch (Exception e){
            Debug.LogError("error while loading maidata: " + e.Message);
            return false;
        }
    }
    static public bool ReadDataRaw(string[] maidataTxt)
    {
        int i = 0;
        other_commands = "";
        try
        {
            for (i = 0; i < maidataTxt.Length; i++)
            {
                if (maidataTxt[i].StartsWith("&title="))
                    title = GetValue(maidataTxt[i]);
                else if (maidataTxt[i].StartsWith("&artist="))
                    artist = GetValue(maidataTxt[i]);
                else if (maidataTxt[i].StartsWith("&des="))
                    designer = GetValue(maidataTxt[i]);
                else if (maidataTxt[i].StartsWith("&first="))
                    first = float.Parse(GetValue(maidataTxt[i]));
                else if (maidataTxt[i].StartsWith("&lv_") || maidataTxt[i].StartsWith("&inote_"))
                {
                    for (int j = 1; j < 8 && i < maidataTxt.Length; j++)
                    {
                        if (maidataTxt[i].StartsWith("&lv_" + j + "="))
                            levels[j - 1] = GetValue(maidataTxt[i]);
                        if (maidataTxt[i].StartsWith("&inote_" + j + "="))
                        {
                            string TheNote = "";
                            TheNote += GetValue(maidataTxt[i]) + "\n";
                            i++;
                            for (; i < maidataTxt.Length; i++)
                            {
                                if ((i) < maidataTxt.Length)
                                {
                                    if (maidataTxt[i].StartsWith("&"))
                                        break;
                                }
                                TheNote += maidataTxt[i] + "\n";
                            }
                            fumens[j - 1] = TheNote;
                        }
                    }
                }
                else
                {
                    other_commands += maidataTxt[i].Trim() + "\n";
                }

            }
            other_commands = other_commands.Trim();
            return true;
        }
        catch (Exception e){
            Debug.LogError("Error reading maidata.txt on line "+(i+1)+":\n"+e.Message);
            return false;
        }

    }
    /// <summary>
    /// Save the static data to maidata.txt
    /// </summary>
    /// <param name="filename">file path of maidata.txt</param>
    static public void SaveData(string filename)
    {
        List<string> maidata = new List<string>();
        maidata.Add("&title=" + title);
        maidata.Add("&artist=" + artist);
        maidata.Add("&first=" + first);
        maidata.Add("&des=" + designer);
        maidata.Add(other_commands);
        for (int i = 0; i < levels.Length; i++)
        {
            if (levels[i] != null && levels[i] != "")
            {
                maidata.Add("&lv_" + (i + 1) + "=" + levels[i].Trim());
            }
        }
        for (int i = 0; i < fumens.Length; i++)
        {
            if (fumens[i] != null && fumens[i] != "")
            {
                maidata.Add("&inote_" + (i+1) + "=" + fumens[i].Trim());
            }
        }
        File.WriteAllLines(filename, maidata.ToArray(),Encoding.UTF8);
    }
    static private string GetValue(string varline)
    {
        return varline.Split('=')[1];
    }
    /// <summary>
    /// This method serialize the fumen data and load it into the static class.
    /// </summary>
    /// <param name="text">fumen text</param>
    /// <param name="position">the position of the cusor, to get the return time</param>
    /// <returns>the song time at the position</returns>
    static public double Serialize(string text, long position=0)
    {
        List<SimaiTimingPoint> _notelist = new List<SimaiTimingPoint>();
        List<SimaiTimingPoint> _timinglist = new List<SimaiTimingPoint>();
        try
        {
            float bpm = 0;
            float curHSpeed = 1f;
            double time = first; //in seconds
            double requestedTime = 0;
            int beats = 4;
            bool haveNote = false;
            string noteTemp = "";
            int Ycount=0, Xcount = 0;

            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '|' && i+1 < text.Length && text[i+1] == '|')
                {
                    // 跳过注释
                    Xcount++;
                    while(i < text.Length && text[i] != '\n')
                    {
                        i++;
                        Xcount++;
                    }
                    Ycount++;
                    Xcount = 0;
                    continue;
                }
                if (text[i] == '\n')
                {
                    Ycount++;
                    Xcount = 0;
                }
                else
                {
                    Xcount++;
                }
                if (i-1 < position)
                {
                    requestedTime = time;
                }
                if (text[i] == '(')
                //Get bpm
                {
                    haveNote = false;
                    noteTemp = "";
                    string bpm_s = "";
                    i++;
                    Xcount++;
                    while (text[i] != ')')
                    {
                        bpm_s += text[i];
                        i++;
                        Xcount++;
                    }
                    bpm = float.Parse(bpm_s);
                    //Console.WriteLine("BPM" + bpm);
                    continue;
                }
                if (text[i] == '{')
                //Get beats
                {
                    haveNote = false;
                    noteTemp = "";
                    string beats_s = "";
                    i++;
                    Xcount++;
                    while (text[i] != '}')
                    {
                        beats_s += text[i];
                        i++;
                        Xcount++;
                    }
                    beats = int.Parse(beats_s);
                    //Console.WriteLine("BEAT" + beats);
                    continue;
                }
                if (text[i] == 'H')
                //Get HS
                {
                    haveNote = false;
                    noteTemp = "";
                    string hs_s = "";
                    if (text[i+1] == 'S' && text[i+2] == '*')
                    {
                        i += 3;
                        Xcount += 3;
                    }
                    while (text[i] != '>')
                    {
                        hs_s += text[i];
                        i++;
                        Xcount++;
                    }
                    curHSpeed = float.Parse(hs_s);
                    //Console.WriteLine("HS" + curHSpeed);
                    continue;
                }
                if (isNote(text[i]))
                {
                    haveNote = true;
                }
                if (haveNote && text[i] != ',' )
                {
                    noteTemp += text[i];
                }
                if (text[i] == ',')
                {
                    if (haveNote)
                    {
                        if (noteTemp.Contains('`'))
                        {
                            // 伪双
                            string[] fakeEachList = noteTemp.Split('`');
                            double fakeTime = time;
                            double timeInterval = 1.875 / bpm; // 128分音
                            foreach(string fakeEachGroup in fakeEachList)
                            {
                                Console.WriteLine(fakeEachGroup);
                                _notelist.Add(new SimaiTimingPoint(fakeTime, Xcount, Ycount, fakeEachGroup, bpm, curHSpeed));
                                fakeTime += timeInterval;
                            }
                        }
                        else
                        {
                            _notelist.Add(new SimaiTimingPoint(time, Xcount, Ycount, noteTemp, bpm, curHSpeed));
                        }
                        //Console.WriteLine("Note:" + noteTemp);
                        
                        noteTemp = "";
                    }
                    _timinglist.Add(new SimaiTimingPoint(time,Xcount,Ycount,"",bpm));


                    time += (1d / (bpm / 60d)) * 4d / (double)beats;
                    //Console.WriteLine(time);
                    haveNote = false;
                    continue;
                }
            }
            notelist = _notelist;
            timinglist = _timinglist;
            return requestedTime;
        }
        catch(Exception e)
        {
            Debug.LogError("Error loading Level: "+e.Message);
            return -1;
        }
    }
    static public void ClearNoteListPlayedState()
    {
        notelist.Sort((x, y) => x.time.CompareTo(y.time));
        for (int i = 0; i < notelist.Count; i++)
        {
            notelist[i].havePlayed = false;
        }
    }
    static private bool isNote(char noteText)
    {
        string SlideMarks = "1234567890ABCDE"; ///ABCDE for touch
        foreach (var mark in SlideMarks)
        {
            if (noteText==mark) return true;
        }
        return false;
    }
    static public string GetDifficultyText(int index)
    {
        if (index == 0) return "EASY";
        if (index == 1) return "BASIC";
        if (index == 2) return "ADVANCED";
        if (index == 3) return "EXPERT";
        if (index == 4) return "MASTER";
        if (index == 5) return "Re:MASTER";
        if (index == 6) return "ORIGINAL";
        return "DEFAULT";
    }
}
