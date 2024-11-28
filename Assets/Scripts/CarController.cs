using RoadArchitect;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;


public class CarController : Agent
{

    public List<GameObject> checkPointList;


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
    private int currentCarPosition, previousCarPosition;
    public float BetterLapReward = 0.0f;

    public float acceleration = 50f;
    public float brakingForce = 80f;
    public float dragFactor = 0.0f;
    private float currentSpeed = 0f;
    private float currentSteerAngle, steerInput;

    public float maxSteerAngle = 30f;

    private Transform frontLeftWheel, frontRightWheel, backLeftWheel, backRightWheel;
    private WheelCollider frontLeftCollider, frontRightCollider, backLeftCollider, backRightCollider;

    private Transform objectAhead;

    public override void Initialize()
    {

        GameObject frontLeftWheelObj = GameObject.Find("wheelBackRight");
        GameObject frontRightWheelObj = GameObject.Find("wheelBackLeft");
        GameObject backLeftWheelObj = GameObject.Find("wheelFrontRight");
        GameObject backRightWheelObj = GameObject.Find("wheelFrontLeft");

        rb = this.GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Extrapolate;

        frontLeftWheel = frontLeftWheelObj.transform;
        frontRightWheel = frontRightWheelObj.transform;
        backLeftWheel = backLeftWheelObj.transform;
        backRightWheel = backRightWheelObj.transform;
        frontLeftCollider = frontLeftWheel.GetComponent<WheelCollider>();
        frontRightCollider = frontRightWheel.GetComponent<WheelCollider>();  
        backLeftCollider = backLeftWheel.GetComponent<WheelCollider>();
        backRightCollider = backRightWheel.GetComponent<WheelCollider>();


        rb.velocity = Vector3.zero;
        rb.centerOfMass += Vector3.down * centreOfGravityOffset;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        checkPointList = new List<GameObject>();
        //AlignCarToGround();

        recall_position = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z);
        recall_rotation = new Quaternion(this.transform.rotation.x, this.transform.rotation.y, this.transform.rotation.z, this.transform.rotation.w);

    }

    public override void OnEpisodeBegin()
    {
        rb.velocity = Vector3.zero;
        this.transform.position = recall_position;
        this.transform.rotation = recall_rotation;
        currentSpeed = 0f;
        previousCarPosition = RaceManager.Instance.GetPosition(gameObject);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float mag = rb.velocity.sqrMagnitude;
        float throttle = 0f;


        switch (actions.DiscreteActions.Array[0])
        {
            case 0:
                //currentSpeed = 0f;
                break;
            case 1:
                //throttle = brakingForce;
                //currentSpeed += throttle;
                //currentSpeed = Mathf.Clamp(currentSpeed, -15f, Movespeed);
                rb.AddRelativeForce(Vector3.back * Time.deltaTime, ForceMode.VelocityChange);
                AddReward(multback);
                break;
            case 2:
                //throttle = 10f;
                //currentSpeed = Mathf.Clamp(currentSpeed, -15f, Movespeed);
                rb.AddRelativeForce(Vector3.forward * Time.deltaTime, ForceMode.VelocityChange);
                AddReward(multfwd);
                break;
        }

        switch (actions.DiscreteActions.Array[1]) 
        {
            case 0:
                break;
            case 1:
                this.transform.Rotate(Vector3.up, -Turnspeed * Time.deltaTime); //left
                //currentSteerAngle = -(steerInput * maxSteerAngle);
                break;
            case 2:
                this.transform.Rotate(Vector3.up, Turnspeed * Time.deltaTime); //right
                //currentSteerAngle = steerInput * maxSteerAngle;
                break;
        }

        //leftCollider.steerAngle = currentSteerAngle;
        //rightCollider.steerAngle = currentSteerAngle;

        // Update visual wheels
        //UpdateSingleWheel(leftCollider, frontLeftWheel);
        //UpdateSingleWheel(rightCollider, frontRightWheel);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        actionsOut.DiscreteActions.Array[0] = 0;
        actionsOut.DiscreteActions.Array[1] = 0;

        float move = Input.GetAxis("Vertical");
        float turn = Input.GetAxis("Horizontal");
        //steerInput = Input.GetAxis("Horizontal");

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

        if (other.gameObject.tag == "Checkpoint")
        {
            directionDot = Vector3.Dot(transform.forward, other.gameObject.transform.forward);

            if (directionDot > 0)
            {
                if (!checkPointList.Contains(other.gameObject))
                {
                    checkPointList.Add(other.gameObject);
                    AddReward(0.2f);
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
                AddReward(-1.0f);
                Debug.Log("Wrong checkpoint, start over");
                checkPointList.Remove(other.gameObject);
            }
        }
            if (other.gameObject.tag == "Final")
            {
                Debug.Log("The agent has completed the track " + BetterLapReward);
                //AddReward(1.5f - ((float)currentCarPosition/2.0f));
                AddReward(2.0f - (float)BetterLapReward - ((float)currentCarPosition / 3.0f));
                checkPointList.Clear();
                EndEpisode();
                OnEpisodeBegin();
            }
        Debug.Log(currentCarPosition);
    }

    public void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.tag == "Wall")
        {
            AddReward(-0.2f);
        }

        if (collision.gameObject.tag == "Car")
        {
            AddReward(-0.05f);
        }

        if (collision.gameObject.tag == "StartLine")
        {
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
        currentCarPosition = RaceManager.Instance.GetPosition(gameObject);

        if (currentCarPosition < previousCarPosition)
        {
            AddReward(0.5f);
            Debug.Log($"Overtake detected! New Position: {currentCarPosition}, Reward Given.");

            previousCarPosition = currentCarPosition;
        }
        else if (currentCarPosition > previousCarPosition)
        {
            AddReward(-0.1f);
            Debug.Log($"Position dropped. Current Position: {currentCarPosition}, Penalty Given.");

            previousCarPosition = currentCarPosition;
        }
    }

    private void UpdateSingleWheel(WheelCollider collider, Transform wheelTransform)
    {
        // Get the world position and rotation from the collider
        Vector3 position;
        Quaternion rotation;
        collider.GetWorldPose(out position, out rotation);

        // Apply position and rotation to the visual wheel
        wheelTransform.position = position;
        wheelTransform.rotation = rotation;
    }

    private void AlignCarToGround()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, Mathf.Infinity))
        {
            Vector3 correctedPosition = new Vector3(transform.position.x, hit.point.y + 0.5f, transform.position.z);
            transform.position = correctedPosition;
        }
        else
        {
            Debug.LogWarning("Car is not above the ground!");
        }
    }
}
