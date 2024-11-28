using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DRSEnabler : MonoBehaviour
{
    public float drsBoostSpeed = 500f;
    public float alignmentForce = 0.2f;
    private float drsRange = 70f;
    public float maxSpeed = 50f;
    public LayerMask carLayer; // Layer for cars only

    public float drsBoostIncrement = 0.05f; // Incremental speed increase per frame
    public float drsBoostDuration = 1.0f;  // Duration of DRS boost in seconds

    private Transform carAhead;
    private Rigidbody rb;
    private CarController carController;
    private bool isDRSActive = false;
    private bool isInDRSZone = false;

    private Vector3 rayStart;
    GameObject lookAt;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        carController = GetComponent<CarController>();
        lookAt = GameObject.FindWithTag("Respawn");
    }

    private void Update()
    {
        //Debug.DrawRay(transform.position, transform.forward * drsRange, Color.red);
        CheckDRSConditions();
        //ClampSpeed();
    }

    private void CheckDRSConditions()
    {
        DetectCarAheadWithRay();

        if (carAhead != null && isInDRSZone)
        {
            float distanceToCarAhead = Vector3.Distance(transform.position, carAhead.position);

            if (distanceToCarAhead <= drsRange)
            {
                ActivateDRS();
                //AlignWithCarAhead();
            }
            else
            {
                DeactivateDRS();
            }
        }
        else
        {
            DeactivateDRS();
        }
    }


    private void DetectCarAheadWithRay()
    {
            carAhead = null;

            Vector3 rayStartCenter = lookAt.transform.position;
            Vector3 rayStartLeft = lookAt.transform.position - lookAt.transform.right * 3.5f;
            Vector3 rayStartRight = lookAt.transform.position + lookAt.transform.right * 3.5f;
        
            RaycastHit hit;

            // Cast center ray
            if (Physics.Raycast(rayStartCenter, -transform.forward, out hit, drsRange) && hit.transform.CompareTag("Car"))
            {
                carAhead = hit.transform;
            }
            // Cast left ray if no car detected by center ray
            else if (Physics.Raycast(rayStartLeft, -transform.forward, out hit, drsRange) && hit.transform.CompareTag("Car"))
            {
                carAhead = hit.transform;
            }
            // Cast right ray if no car detected by center or left rays
            else if (Physics.Raycast(rayStartRight, -transform.forward, out hit, drsRange) && hit.transform.CompareTag("Car"))
            {
                carAhead = hit.transform;
            }
        // Draw the rays in Scene view for debugging
            Debug.DrawRay(rayStartCenter, -transform.forward * drsRange, Color.red);
            Debug.DrawRay(rayStartLeft, -transform.forward * drsRange, Color.green);
            Debug.DrawRay(rayStartRight, -transform.forward * drsRange, Color.blue);
        }


    private void ActivateDRS()
    {
        if (!isDRSActive)
        {
            Debug.Log("DRS is activated");
            StartCoroutine(IApplyDRSBoost());
        }
    }

    private void DeactivateDRS()
    {
        if (isDRSActive)
        {
            isDRSActive = false;
            // Speed clamping handled by ClampSpeed()
        }
    }

    private void AlignWithCarAhead()
    {
        Vector3 directionToCarAhead = (carAhead.position - transform.position).normalized;
        directionToCarAhead.y = 0;

        Vector3 alignmentForceVector = directionToCarAhead * alignmentForce;
        rb.AddForce(alignmentForceVector, ForceMode.Acceleration);
    }

    private void ClampSpeed()
    {
        if (!isDRSActive && rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DRSZone"))
        {
            isInDRSZone = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("DRSZone"))
        {
            isInDRSZone = false;
            DeactivateDRS();
        }
    }


    private IEnumerator IApplyDRSBoost()
    {
        isDRSActive = true;
        float elapsedTime = 0f;

        // Gradually apply the boost over the set duration
        while (elapsedTime < drsBoostDuration)
        {
            rb.drag = 0.1f;
            Debug.Log("Boost applied + "+rb.drag);
            //rb.AddRelativeForce(Vector3.forward * drsBoostIncrement, ForceMode.VelocityChange);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        rb.drag = 0.16f;
        Debug.Log(rb.drag);
        isDRSActive = false;
    }
}
