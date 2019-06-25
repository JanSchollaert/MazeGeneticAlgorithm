using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Summon
{
    public Vector2 gridPos;
    public GameObject summonObject;
    public SummonScript sScript;
    public int movedtimes;
    public float penalty;
    public float score;
    public Queue<int> moves;
}