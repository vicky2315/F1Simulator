using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CarControllerImproved : MonoBehaviour
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



    public float maxAcceleration = 500f;

    public float maxTorque = 500f;

    public float brakingforce = 600f;
    public float maxTurnAngle = 30f;
    public float maxSpeed = 120f;

    public Vector3 centreOfMass;

    private float currentAcceleration = 0f;
    
    private float currentBrakeForce = 0f;
    
    private float moveInput, steerInput;

    private float turnSensitivity = 0.5f;
    private float _steerAngle = 0f;
    
    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centreOfMass;
    }

    void Update()
    {
        GetInputs();
    }

    void FixedUpdate()
    {
        currentAcceleration = moveInput * maxTorque * maxAcceleration * Time.deltaTime;
        //ClampSpeed();

        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);
        }
        
        Accelerate();
        Brake();
        Turn();

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
