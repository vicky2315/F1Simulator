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

    CarController carController;

    private void Start()
    {
        carController = GetComponent<CarController>();
    }

    public void StartNewLap()
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
        if (carController.lapStart && !isLapActive)
        {
            StartNewLap();
        }
        if (other.gameObject.CompareTag("Final"))
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
            isBetter = 3.0f;
        }
        else if(lapTime - bestLapTime < 1.5f)
        {
            isBetter = 0.0f;
        }
        else
        {
            isBetter = -3.0f;
        }
        reward = isBetter;

        RaceManager.Instance.NotifyLapCompleted(gameObject);

        isLapActive=!isLapActive;
    }

    public float BetterTime()
    {
        return reward;
    }

}
