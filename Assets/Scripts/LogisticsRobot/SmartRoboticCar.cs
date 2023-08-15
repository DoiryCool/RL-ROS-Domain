using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.UI;



[System.Serializable]
public class AxleInfo {
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
    public bool motor;
    public bool steering;
}

public class SmartRoboticCar : Agent {
        public List<AxleInfo> axleInfos; 
        public float maxMotorTorque;
        public float maxSteeringAngle;
        public float maxBrakeTorque;
        public float wheelBase = 1f; // Distance between front and rear axle
        public float rearTrack = 1f; // Distance between two rear wheels
        public float steeringReturnRate = 5f; // How fast the wheels return to the center position

        Rigidbody rBody;
        private Vector3 initLocation;


        public GameObject InfoTextOB;
        private Text info_text;
        public float fuel = 3000;

        private int times = 0;
        void Start () {
        info_text = InfoTextOB.GetComponent<Text>();
            rBody = GetComponent<Rigidbody>();
            initLocation = this.transform.localPosition;
        }
        public BaseTrainAreaController areaController;
        public override void OnEpisodeBegin(){
        fuel = 3000;
        times++;
        info_text.text = "Episode Times: " + times;
        this.rBody.angularVelocity = Vector3.zero;
        this.rBody.velocity = Vector3.zero;
        //this.transform.localPosition = new Vector3(Random.Range(-48f, 48f), 0.5f, Random.Range(-48f, 48f));
        //this.transform.rotation = new Quaternion(0f, Random.Range(-180f, 180f), 0f, Random.Range(-180f, 180f));
        this.transform.rotation = Quaternion.Euler(new Vector3(0f, Random.Range(0f, 360f), 0f));
        this.transform.localPosition = initLocation;
        //Target.localPosition = new Vector3(Random.Range(12f, 148f), 0.5f, Random.Range(-13f, 13f));
    }

    public override void CollectObservations(VectorSensor sensor){
        base.CollectObservations(sensor);
        sensor.AddObservation(this.transform.rotation);
        sensor.AddObservation(this.transform.localPosition);
        sensor.AddObservation(fuel);

    }

    private float motor;
    private float steeringInput;
    private float brake;

    public float fuelExhaustionPenalty = -100f;
    public override void OnActionReceived(ActionBuffers actionBuffers){
        fuel -= 0.1f;
        motor = actionBuffers.ContinuousActions[0];
        steeringInput = actionBuffers.ContinuousActions[1];
        int brakeAction = actionBuffers.DiscreteActions[0];
        switch(brakeAction)
        {
            case 0: // No brake
                brake = 0f;
                break;
            case 1: // Partial brake
                brake = maxBrakeTorque * 0.5f;
                break;
            case 2: // Full brake
                brake = maxBrakeTorque;
                break;
        }

        motor = maxMotorTorque * motor; // Assuming motor is obtained from actionBuffers
        brake = maxBrakeTorque * brake; // Assuming brake is obtained from actionBuffers
       // If there's no input, return the steering angle towards 0
            if (Mathf.Approximately(steeringInput, 0))
            {
                steeringInput = Mathf.MoveTowards(steeringInput, 0, steeringReturnRate * Time.fixedDeltaTime);
            }
            
            float steering = maxSteeringAngle * steeringInput;

            foreach (AxleInfo axleInfo in axleInfos) {
            if (axleInfo.steering) {
                axleInfo.leftWheel.steerAngle = CalculateAckermannAngle(steering, true);
                axleInfo.rightWheel.steerAngle = CalculateAckermannAngle(steering, false);
                    
                ApplyLocalPositionToVisuals(axleInfo.leftWheel);
                ApplyLocalPositionToVisuals(axleInfo.rightWheel);
            }
            if (axleInfo.motor) {
                axleInfo.leftWheel.motorTorque = motor;
                axleInfo.rightWheel.motorTorque = motor;
                
                // Apply brake torque
                axleInfo.leftWheel.brakeTorque = brake;
                axleInfo.rightWheel.brakeTorque = brake;
            }
        }
        // 燃料耗尽惩罚
        if (fuel < 0)
        {
            AddReward(fuelExhaustionPenalty);
            EndEpisode();
        }
        
        if (this.transform.localPosition.y < -1)
        {
            SetReward(-1000f);
            EndEpisode();
        }
}

        public void ApplyLocalPositionToVisuals(WheelCollider collider)
        {
                
            if (collider.transform.childCount == 0) {
                return;
            }

            Transform visualWheel = collider.transform.GetChild(0);
            Vector3 position;
            Quaternion rotation;
            collider.GetWorldPose(out position, out rotation);
            rotation *= Quaternion.Euler(0, 0, 90);
            visualWheel.transform.rotation = rotation;
            visualWheel.transform.position = position;
        }

        public float CalculateAckermannAngle(float steeringAngle, bool isLeftWheel)
        {
            float tanSteeringAngle = Mathf.Tan(Mathf.Deg2Rad * steeringAngle);
            float denominator = wheelBase / tanSteeringAngle + (isLeftWheel ? -1 : 1) * rearTrack / 2;

            return Mathf.Atan(wheelBase / denominator) * Mathf.Rad2Deg;
        }
        
        public override void Heuristic(in ActionBuffers actionsOut)
{
    var continuousActionsOut = actionsOut.ContinuousActions;
    continuousActionsOut[0] = Input.GetAxis("Vertical");
    continuousActionsOut[1] = Input.GetAxis("Horizontal");

    var discreteActionsOut = actionsOut.DiscreteActions;
    
    if (Input.GetKey(KeyCode.Space))
    {
        // Let's assume:
        // 0 - No brake
        // 1 - Partial brake
        // 2 - Full brake
        if (Input.GetKey(KeyCode.LeftShift)) // Let's say holding Shift while pressing Space applies full brake
        {
            discreteActionsOut[0] = 2;
        }
        else
        {
            discreteActionsOut[0] = 1;
        }
    }
    else
    {
        discreteActionsOut[0] = 0;
    }
}

private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent<Goal>(out Goal goal)){
            fuel += 2000;
            SetReward(100f);
            areaController.ResetTargets();
        }
        if(other.TryGetComponent<Wall>(out Wall wall)){
            SetReward(-500f);
            EndEpisode();
        }
    }


    }