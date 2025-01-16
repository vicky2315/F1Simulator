using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RaceManager : MonoBehaviour
{
    public static RaceManager Instance;

    private Dictionary<GameObject, LapTimer> carLapTimers = new Dictionary<GameObject, LapTimer>();
    public List<GameObject> carsInRace = new List<GameObject>();

    public List<GameObject> monzaCheckpoints = new List<GameObject>();
    public List<GameObject> redbullCheckpoints = new List<GameObject>();
    public List<GameObject> trainingCheckpoints = new List<GameObject>();
    GameObject[] cars = null;

    public GameObject difficultCars;
    public GameObject easyCars;

    public TMP_Text lapTimes;
    private string timeOnBoard, leaderBoardText;

    private void Awake()
    {
        // Initialize the singleton instance
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    // Start is called before the first frame update
    private void Start()
    {
        foreach (GameObject car in carsInRace)
        {
            LapTimer lapTime = car.GetComponent<LapTimer>();
            if (lapTime != null)
            {
                carLapTimers[car] = lapTime;
                Debug.Log($"Car added and lap time is {lapTime}");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
            UpdateStandings();
    }

    private void displayLeaderBoard()
    {
        foreach (GameObject car in carsInRace)
        {
            System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(carLapTimers[car].lapTime);
            timeOnBoard = string.Format("{0:D2}:{1:D2}:{2:D3}", timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
            leaderBoardText += car.name + " : " + timeOnBoard + "\n";
        }
        //lapTimes.SetText(leaderBoardText);
        //leaderBoardText = string.Empty;
    }

    public void NotifyLapCompleted(GameObject car)
    {
        LapTimer completedLapTime = car.GetComponent<LapTimer>();
        carLapTimers[car] = completedLapTime;
        displayLeaderBoard();
    }

    private void UpdateLeaderboard()
    {
        
    }

    private void UpdateStandings()
    {
        carsInRace.Sort((a, b) =>
        {
            int checkpointCountA = a.GetComponent<CarController>().checkPointList.Count;
            int checkpointCountB = b.GetComponent<CarController>().checkPointList.Count;

        if (a.GetComponent<LapTimer>().currentLapNo != b.GetComponent<LapTimer>().currentLapNo)
        {
            return b.GetComponent<LapTimer>().currentLapNo.CompareTo(a.GetComponent<LapTimer>().currentLapNo);
        }
        else if (checkpointCountA != checkpointCountB)
        {
            return b.GetComponent<CarController>().checkPointList.Count.CompareTo(a.GetComponent<CarController>().checkPointList.Count);
        }
        else
        {
            float distanceA = Vector3.Distance(a.transform.position, redbullCheckpoints[checkpointCountA % redbullCheckpoints.Count].transform.position);
                float distanceB = Vector3.Distance(b.transform.position, redbullCheckpoints[checkpointCountB % redbullCheckpoints.Count].transform.position);
                return distanceA.CompareTo(distanceB);
        } 
        });
    }
    private void UpdateStandingsEasy()
    {
        carsInRace.Sort((a, b) =>
        {
            int checkpointCountA = a.GetComponent<CarController>().checkPointList.Count;
            int checkpointCountB = b.GetComponent<CarController>().checkPointList.Count;

            if (a.GetComponent<LapTimer>().currentLapNo != b.GetComponent<LapTimer>().currentLapNo)
            {
                return b.GetComponent<LapTimer>().currentLapNo.CompareTo(a.GetComponent<LapTimer>().currentLapNo);
            }
            else if (checkpointCountA != checkpointCountB)
            {
                return b.GetComponent<CarController>().checkPointList.Count.CompareTo(a.GetComponent<CarController>().checkPointList.Count);
            }
            else
            {
                float distanceA = Vector3.Distance(a.transform.position, trainingCheckpoints[checkpointCountA % trainingCheckpoints.Count].transform.position);
                float distanceB = Vector3.Distance(b.transform.position, trainingCheckpoints[checkpointCountB % trainingCheckpoints.Count].transform.position);
                return distanceA.CompareTo(distanceB);
            }
        });
    }

    public int GetPosition(GameObject car)
    {
        return carsInRace.IndexOf(car) + 1;
    }
}
