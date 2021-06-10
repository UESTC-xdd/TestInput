using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Util : MonoBehaviour
{
    private List<Timer> Timers=new List<Timer>();

    private static Util instance;
    public static Util Instance
    {
        get { return instance; }
    }

    public static bool IsValid
    {
        get { return instance != null; }
    }

    private void Awake()
    {
        #region µ¥Àý
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            if (instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        DontDestroyOnLoad(gameObject);
        #endregion
    }

    void Update()
    {
        for (int i = 0; i < Timers.Count; i++)
        { 
            if(Timers[i]!=null && Timers[i].IsCounting)
            {
                if(Timers[i].curTime>0)
                {
                    Timers[i].curTime -= Time.deltaTime;
                }
                else if(Timers[i].curTime <=0 && Timers[i].IsCounting)
                {
                    Timers[i].OnCountStop?.Invoke();
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

    public void ResetTimer(float countTime)
    {
        curTime = countTime;
        IsCounting = true;
    }

    public void Stop()
    {
        curTime = 0;
        IsCounting = false;
    }
}
