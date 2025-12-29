using System;
using System.Collections.Generic;

[Serializable]
public class ChartData
{
    public string songName;
    public float bpm;
    public float offset;

    public List<NoteData> notes; 
}

[Serializable]
public class NoteData
{
    public float time; 
    public int lane;
    public float duration;
}
