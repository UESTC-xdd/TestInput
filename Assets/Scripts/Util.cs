using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Util : MonoBehaviour
{
    public List<Timer> Timers = new List<Timer>();

    void Update()
    {
        for (int i = 0; i < Timers.Count; i++)
        {
            if(Timers[i].IsCounting)
            {
                if(Timers[i].curTime>0)
                {
                    Timers[i].curTime -= Time.deltaTime;
                }
                else if(Timers[i].curTime <=0 && Timers[i].IsCounting)
                {
                    Timers[i].OnCountStop();
                    Timers[i].IsCounting = false;
                }
            }
        }
    }

    public Timer BeginTimer(float countTime)
    {
        foreach (var timer in Timers)
        {
            if(!timer.IsCounting)
            {
                timer.curTime = countTime;
                timer.IsCounting = true;
                return timer;
            }
        }

        Timer newTimer = new Timer(countTime);
        Timers.Add(newTimer);
        return newTimer;
    }
}

public class Timer
{
    public float curTime;
    public bool IsCounting;
    public UnityAction OnCountStop;

    public Timer(float countTIme)
    {
        curTime = countTIme;
        IsCounting = true;
    }
}
