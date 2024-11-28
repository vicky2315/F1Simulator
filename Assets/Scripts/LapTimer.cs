using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LapTimer : MonoBehaviour
{
    public static LapTimer Instance;
    private float lapStartTime;

    public int currentLapNo = 0;
    public float lapTime = 0f;
    public float bestLapTime= 99f;

    public bool isLapActive = false;
    public float isBetter = 0.0f;
    public float reward = 0.0f;

    private void StartNewLap()
    {
        lapStartTime = Time.time;
        isLapActive=true;
        currentLapNo++;
        isBetter = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if(isLapActive)
        {
            lapTime = Time.time - lapStartTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("StartCheckpoint")) 
        {
                StartNewLap();
        }
        if(other.gameObject.CompareTag("Final"))
        {
            if (isLapActive)
                CompleteLap();
        }
    }

    private void CompleteLap()
    {
        if (lapTime <= bestLapTime)
        {
            bestLapTime = lapTime;
            isBetter = 1.0f;
        }
        else if(lapTime - bestLapTime < 0.5f)
        {
            isBetter = 0.0f;
        }
        else
        {
            isBetter = -1.0f;
        }
        reward = isBetter;
        //Debug.Log($"Lap {currentLapNo} completed in {lapTime:F2} seconds");

        RaceManager.Instance.NotifyLapCompleted(gameObject);

        isLapActive=!isLapActive;
    }

    public float BetterTime()
    {
        return reward;
    }

}
