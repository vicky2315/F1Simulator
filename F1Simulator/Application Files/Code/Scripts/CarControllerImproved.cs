using System.Collections;
using System.Collections.Generic;
using Unity.Barracuda;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;
using UnityEngine;


public class CarControllerImproved : Agent
{
    // Start is called before the first frame update
    [SerializeField] WheelCollider frontLeft;
    [SerializeField] WheelCollider frontRight;
    [SerializeField] WheelCollider rearLeft;
    [SerializeField] WheelCollider rearRight;
    [SerializeField] Transform frontRightTransform;
    [SerializeField] Transform frontLeftTransform;
    [SerializeField] Transform rearRightTransform;
    [SerializeField] Transform rearLeftTransform;


//Handling
    public float maxAcceleration = 500f;

    public float maxTorque = 500f;

    public float brakingforce = 600f;
    public float maxTurnAngle = 30f;
    public float maxSpeed = 120f;

    public Vector3 centreOfMass;
    
    private float currentAcceleration = 0f;
    private float currentBrakeForce = 0f;
    
    //Turning
    private float moveInput, steerInput;
    private float turnSensitivity = 0.5f;
    private float _steerAngle = 0f;
    
    Rigidbody rb;

//MLAgents
    public float multfwd;   //forward reward
    public float multback; //backward reward
    private Vector3 recall_position;            //spawn position
    private Quaternion recall_rotation;
    public bool doEpisodes = true;
    private int initialCarPosition;
    public int currentCarPosition, previousCarPosition;
    public float BetterLapReward = 0.0f;
    public List<GameObject> checkPointList, allCheckpointList;
    GameObject nextCheckpoint,furtherCheckpoint;


    //Metrics
    public int wallCollisions, carCollisions,DRSUsed = 0;
    public bool isDRSEnabled, lapStart;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centreOfMass;
        checkPointList = new List<GameObject>();
        recall_position = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z);
        recall_rotation = new Quaternion(this.transform.rotation.x, this.transform.rotation.y, this.transform.rotation.z, this.transform.rotation.w);
        allCheckpointList = new List<GameObject>();
        lapStart = true;
    }

    public override void OnEpisodeBegin()
    {
        rb.velocity = Vector3.zero;
        this.transform.position = recall_position;
        this.transform.rotation = recall_rotation;

        allCheckpointList = RaceManager.Instance.monzaCheckpoints;

        nextCheckpoint = allCheckpointList[0];
        furtherCheckpoint = allCheckpointList[1];
        checkPointList.Clear();
        lapStart = false;
        lapStart = true;
        previousCarPosition = RaceManager.Instance.GetPosition(gameObject);
    }
    
    public override void OnActionReceived(ActionBuffers actions)
    {
        float mag = rb.velocity.sqrMagnitude;
        float steerDirection = 0f;
        float alignmentDot = Vector3.Dot(transform.forward, nextCheckpoint.gameObject.transform.forward);
        float checkpointDirection = Vector3.Dot(transform.forward, furtherCheckpoint.gameObject.transform.forward);


        switch (actions.DiscreteActions.Array[0])
        {
            case 0:
                rearLeft.motorTorque = 0;
                rearRight.motorTorque = 0;
                frontLeft.motorTorque = 0;
                frontRight.motorTorque = 0;
                break;
            case 1:
                rearLeft.motorTorque = -maxTorque;
                rearRight.motorTorque = -maxTorque;
                frontLeft.motorTorque = -maxTorque;
                frontRight.motorTorque = -maxTorque;
                if (checkpointDirection < 0.6f && rb.velocity.magnitude > 60)
                {
                    Debug.Log("Brakes applied before corner");
                    AddReward(2.0f);
                }
                AddReward(multback);
                break;
            case 2:
                rearLeft.motorTorque = maxTorque;
                rearRight.motorTorque = maxTorque;
                frontLeft.motorTorque = maxTorque;
                frontRight.motorTorque = maxTorque;
                AddReward(multfwd);
                break;
        }

        //float steerInput = actions.ContinuousActions[0];

        switch (actions.DiscreteActions.Array[1])
        {
            case 0:
                steerDirection = 0f;
                break;
            case 1:
                steerDirection = -1f;//left
                if (alignmentDot > 0.7f)
                    AddReward(alignmentDot / 1000f);
                else if (alignmentDot < 0.2f)
                    AddReward(-(alignmentDot / 1000f));
                break;
            case 2:
                steerDirection = 1f; //right
                if (alignmentDot > 0.7f)
                    AddReward(alignmentDot/1000f);
                else if (alignmentDot < 0.2f)
                    AddReward(-(alignmentDot / 1000f));
                break;
        }

        _steerAngle = steerDirection * turnSensitivity * maxTurnAngle;
        frontLeft.steerAngle = Mathf.Lerp(frontLeft.steerAngle, _steerAngle, 0.6f);
        frontRight.steerAngle = Mathf.Lerp(frontRight.steerAngle, _steerAngle, 0.6f); //right

        switch (actions.DiscreteActions.Array[2])
        {
            case 0:
                currentBrakeForce = 0f;
                break;
            case 1:
                currentBrakeForce = brakingforce * 200;
                if (checkpointDirection < 0.6f && rb.velocity.magnitude>10)
                {
                    Debug.Log("Brakes applied before corner spacebar");
                    AddReward(0.01f);
                }
                break;
        }

        frontLeft.brakeTorque = currentBrakeForce;
        frontRight.brakeTorque = currentBrakeForce;
        rearLeft.brakeTorque = currentBrakeForce;
        rearRight.brakeTorque = currentBrakeForce;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        actionsOut.DiscreteActions.Array[0] = 0;
        actionsOut.DiscreteActions.Array[1] = 0;
        actionsOut.DiscreteActions.Array[2] = 0;


        moveInput = -Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");

        if (moveInput > 0)
            actionsOut.DiscreteActions.Array[0] = 1;    //back
        else if (moveInput < 0)
            actionsOut.DiscreteActions.Array[0] = 2;    //forward

        if (steerInput < 0)
        {
            actionsOut.DiscreteActions.Array[1] = 1;
        }
        else if (steerInput > 0)
        {
            actionsOut.DiscreteActions.Array[1] = 2;
        } 

        if (Input.GetKey(KeyCode.Space))
            actionsOut.DiscreteActions.Array[2] = 1;
        else
            actionsOut.DiscreteActions.Array[2] = 0;
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
                    nextCheckpoint = allCheckpointList[checkPointList.Count];

                    if (nextCheckpoint.transform.tag != "Final")
                    {
                        furtherCheckpoint = allCheckpointList[checkPointList.Count + 1];
                    }

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
                AddReward(-3.0f);
                Debug.Log("Wrong checkpoint, start over");
                checkPointList.Remove(other.gameObject);
            }
        }
        if (other.gameObject.tag == "Final")
        {
            Debug.Log("The agent has completed the track " + BetterLapReward);
            //AddReward(1.5f - ((float)currentCarPosition/2.0f));
            //AddReward(8.0f - (float)BetterLapReward - ((float)currentCarPosition / 3.0f));
            AddReward(10.0f + BetterLapReward);
            checkPointList.Clear();
            lapStart = false;
            EndEpisode();
            OnEpisodeBegin();
        }
        //Debug.Log(currentCarPosition);
    }
    //public override void CollectObservations(VectorSensor sensor)
    //{
    //    sensor.AddObservation(this.transform.position);
    //    sensor.AddObservation(nextCheckpoint.transform.position);
    //}

    public void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.tag == "Wall")
        {
            AddReward(-1.5f); //1.5 for full track
            ++wallCollisions;
            Debug.Log("Wall Crash");
        }

        if (collision.gameObject.tag == "Car")
        {
            AddReward(-0.5f); //was 0.1
            ++carCollisions;
            Debug.Log("Car Crash");
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
        if (collision.gameObject.tag == "Wall")
        {
            AddReward(-0.01f);
        }
        if (collision.gameObject.tag == "Car")
        {
            AddReward(-0.01f);
        }
    }

    void Update()
    {
        //GetInputs();
        DRSEnabler drsEnabled = this.GetComponent<DRSEnabler>();
        isDRSEnabled = drsEnabled.DRSStatus();
        currentCarPosition = RaceManager.Instance.GetPosition(gameObject);

        if (currentCarPosition < previousCarPosition)
        {
            AddReward(2.0f);
            Debug.Log($"Overtake detected! New Position: {currentCarPosition}, Reward Given.");

            previousCarPosition = currentCarPosition;
        }
        else if (currentCarPosition > previousCarPosition)
        {
            AddReward(-1.0f);
            Debug.Log($"Position dropped. Current Position: {currentCarPosition}, Penalty Given.");

            previousCarPosition = currentCarPosition;
        }

        if (isDRSEnabled)
        {
            Debug.Log("DRS is used, reward given");
            AddReward(rb.drag * 10);
            ++DRSUsed;
        }
    }   

    void FixedUpdate()
    {
        currentAcceleration = moveInput * maxTorque * maxAcceleration * Time.deltaTime;
        //ClampSpeed();

        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);
        }

        UpdateSingleWheel(rearLeft, rearLeftTransform);
        UpdateSingleWheel(rearRight, rearRightTransform);
        UpdateSingleWheel(frontLeft, frontLeftTransform);
        UpdateSingleWheel(frontRight, frontRightTransform);
    }

    private void Accelerate()
    {
        if (moveInput < 0)
        {
            rearLeft.motorTorque = -maxTorque;
            rearRight.motorTorque = -maxTorque;
            frontLeft.motorTorque = -maxTorque;
            frontRight.motorTorque = -maxTorque;
            //Debug.Log(rb.velocity.magnitude + " and " + maxSpeed);
        }
        else if (moveInput > 0)
        {
            rearLeft.motorTorque = maxTorque / 1.5f;
            rearRight.motorTorque = maxTorque / 1.5f;
            frontLeft.motorTorque = maxTorque / 1.5f;
            frontRight.motorTorque = maxTorque / 1.5f;
        }
        else
        {
            rearLeft.motorTorque = 0;
            rearRight.motorTorque = 0;
            frontLeft.motorTorque = 0;
            frontRight.motorTorque = 0;
        }
    }
    private void Brake()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            Debug.Log("Brake applied");
            currentBrakeForce = brakingforce * 200;
        }
        else
            currentBrakeForce = 0f;

        frontLeft.brakeTorque = currentBrakeForce;
        frontRight.brakeTorque = currentBrakeForce;
        rearLeft.brakeTorque = currentBrakeForce;
        rearRight.brakeTorque = currentBrakeForce;
    }

    private void ClampSpeed()
    {
        
        // Cap the Rigidbody velocity to maxSpeed
        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);
        }
    }

    void GetInputs()
    {
        moveInput = -Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");
    }

    private void Turn() 
    {

        _steerAngle = steerInput * turnSensitivity * maxTurnAngle;

        frontLeft.steerAngle = Mathf.Lerp(frontLeft.steerAngle, _steerAngle, 0.6f);
        frontRight.steerAngle = Mathf.Lerp(frontRight.steerAngle, _steerAngle, 0.6f);
    }


    void UpdateSingleWheel(WheelCollider col, Transform trans)
    {
        Vector3 position;
        Quaternion rotation;

        col.GetWorldPose(out position, out rotation);
        trans.SetPositionAndRotation(position, rotation);
    }
}
