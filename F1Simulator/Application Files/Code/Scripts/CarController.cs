using RoadArchitect;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;


public class CarController : Agent
{

    public List<GameObject> checkPointList, allCheckpointList;


    public float Movespeed = 300f;
    public float Turnspeed = 300f;
    public float multfwd = 0.09f;   //forward reward
    public float multback = 0.001f; //backward reward
    private Rigidbody rb = null;
    private Vector3 recall_position;            //spawn position
    private Quaternion recall_rotation;
    private Bounds bnd;
    public float centreOfGravityOffset = 10f;
    public bool doEpisodes = true;
    public int currentCarPosition, previousCarPosition;
    public float BetterLapReward = 0.0f;

    public float acceleration = 50f;
    public float brakingForce = 80f;
    public float dragFactor = 0.0f;
    private float currentSpeed = 0f;
    private float currentSteerAngle, steerInput;

    public float maxSteerAngle = 30f;

    private Transform objectAhead;

    public int wallCollisions, carCollisions, DRSUsed = 0;


    public bool isDRSEnabled, lapStart;
    GameObject nextCheckpoint;

 

    public override void Initialize()
    {
        rb = this.GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Extrapolate;

        rb.velocity = Vector3.zero;
        rb.centerOfMass += Vector3.down * centreOfGravityOffset;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        checkPointList = new List<GameObject>();
        allCheckpointList = new List<GameObject>();
        //AlignCarToGround();
        recall_position = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z);
        recall_rotation = new Quaternion(this.transform.rotation.x, this.transform.rotation.y, this.transform.rotation.z, this.transform.rotation.w);
        lapStart = true;
    }

    public override void OnEpisodeBegin()
    {
        rb.velocity = Vector3.zero;
        this.transform.position = recall_position;
        this.transform.rotation = recall_rotation;
        currentSpeed = 0f;
        allCheckpointList = RaceManager.Instance.redbullCheckpoints;
        nextCheckpoint = allCheckpointList[0];
        checkPointList.Clear();
        previousCarPosition = RaceManager.Instance.GetPosition(gameObject);
        lapStart = true;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float mag = rb.velocity.sqrMagnitude;
        //float throttle = 0f;


        switch (actions.DiscreteActions.Array[0])
        {
            case 0:
                break;
            case 1:
                rb.AddRelativeForce(Vector3.back * Movespeed * Time.deltaTime, ForceMode.VelocityChange);
                AddReward(multback);
                break;
            case 2:
                rb.AddRelativeForce(Vector3.forward * (Movespeed / 2) * Time.deltaTime, ForceMode.VelocityChange);
                AddReward(multfwd);
                break;
        }

        switch (actions.DiscreteActions.Array[1])
        {
            case 0:
                break;
            case 1:
                this.transform.Rotate(Vector3.up, -Turnspeed * Time.deltaTime); //left
                break;
            case 2:
                this.transform.Rotate(Vector3.up, Turnspeed * Time.deltaTime); //right
                break;
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        actionsOut.DiscreteActions.Array[0] = 0;
        actionsOut.DiscreteActions.Array[1] = 0;

        float move = Input.GetAxis("Vertical");
        float turn = Input.GetAxis("Horizontal");

        if (move < 0)
            actionsOut.DiscreteActions.Array[0] = 1;    //back
        else if (move > 0)
            actionsOut.DiscreteActions.Array[0] = 2;    //forward

        if (turn < 0)
            actionsOut.DiscreteActions.Array[1] = 1;    //left
        else if (turn > 0)
            actionsOut.DiscreteActions.Array[1] = 2;    //right
    }

    public void OnTriggerEnter(Collider other)
    {
        float directionDot;
        BetterLapReward = gameObject.GetComponent<LapTimer>().BetterTime();

        if (other.gameObject.tag == "Checkpoint" || other.gameObject.tag == "StartCheckpoint")
        {
            directionDot = Vector3.Dot(transform.forward, other.gameObject.transform.forward);

            if (directionDot > 0)
            {
                if (!checkPointList.Contains(other.gameObject))
                {
                    checkPointList.Add(other.gameObject);
                    AddReward(0.5f);
                    nextCheckpoint = allCheckpointList[checkPointList.Count];
                    Debug.Log("Checkpoint reached");
                }
                else
                {
                    AddReward(0.05f);
                    Debug.Log("Passing same checkpoint");
                }
            }
            else
            {
                AddReward(-5.0f);
                Debug.Log("Wrong checkpoint, start over");
                checkPointList.Remove(other.gameObject);
            }
        }
            if (other.gameObject.tag == "Final")
            {
                Debug.Log("The agent has completed the track " + BetterLapReward);
            //AddReward(1.5f - ((float)currentCarPosition/2.0f));
            //AddReward(2.0f - (float)BetterLapReward - ((float)currentCarPosition / 3.0f));
                AddReward(5.0f);
                AddReward(BetterLapReward);
                checkPointList.Clear();
                lapStart = false;
                EndEpisode();
                OnEpisodeBegin();
            }
    }

    public void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.tag == "Wall")
        {
            AddReward(-0.5f);
            //AddReward(-0.2f);
            ++wallCollisions;
            Debug.Log("Wall Crash");
        }

        if (collision.gameObject.tag == "Car")
        {
            AddReward(-0.01f);
            ++carCollisions;
            Debug.Log("Car Crash");
        }

        if (collision.gameObject.tag == "StartLine")
        {
            lapStart = false;
            Debug.Log("Reached the start line again, turn around");
            AddReward(-2.0f);
            checkPointList.Clear();
            EndEpisode();
            OnEpisodeBegin();

        }
    }

    public void OnCollisionStay(Collision collision)
    {
        if(collision.gameObject.tag == "Wall")
        {
            AddReward(-0.01f);
        }
        if (collision.gameObject.tag == "Car")
        {
            AddReward(-0.01f);
        }
    }

    private void Update()
    {
        DRSEnabler drsEnabled = this.GetComponent<DRSEnabler>();
        currentCarPosition = RaceManager.Instance.GetPosition(gameObject);
        isDRSEnabled = drsEnabled.DRSStatus();
        float alignmentDot = Vector3.Dot(transform.forward, nextCheckpoint.gameObject.transform.forward);


        if (currentCarPosition < previousCarPosition)
        {
            AddReward(2.0f);
            Debug.Log($"Overtake detected! New Position: {currentCarPosition}, Reward Given.");

            previousCarPosition = currentCarPosition;
        }
        else if (currentCarPosition > previousCarPosition)
        {
            AddReward(-1.5f);
            Debug.Log($"Position dropped. Current Position: {currentCarPosition}, Penalty Given.");

            previousCarPosition = currentCarPosition;
        }
        if (isDRSEnabled)
        {
            Debug.Log("DRS activated, reward given");
            //AddReward(0.001f);
            ++DRSUsed;
        }

        if (alignmentDot > 0.8f)
            AddReward(alignmentDot / 1000f);
    }
}
