using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.UI;

public class RollerAgent : Agent
{

    public Transform Target;

    public GameObject information;
    Text information_text;
    public GameObject energyText;
    Text energy_text;
    public GameObject EnergySlider;
    Slider slider;

    public Material winMaterial;
    public Material loseMaterial;
    public Material nomarlMaterial;
    public MeshRenderer floorMeshRenderer; 
    public Vector3 initLocation;
    public int times = 0;
    public float energy = 1000;

    Rigidbody rBody;
    RayPerceptionSensorComponent3D ray3D;
     void Start(){
        initLocation = this.transform.localPosition;
        rBody = GetComponent<Rigidbody>();
        ray3D = this.GetComponent<RayPerceptionSensorComponent3D>();
        information_text = information.GetComponent<Text>();
        energy_text = energyText.GetComponent<Text>();
        slider = EnergySlider.GetComponent<Slider>();
        information_text.text = "Episode Times: 0";
        energy_text.text = "Energy: 1000";
        this.rBody.angularVelocity = Vector3.zero;
        this.rBody.velocity = Vector3.zero;
        this.transform.localPosition = initLocation;
    }
    public override void OnEpisodeBegin(){
        //Debug.Log("Episode Times:" + times);
        times += 1;
        energy = 1000;
        information_text.text = "Episode Times: " + times.ToString();
        this.rBody.angularVelocity = Vector3.zero;
        this.rBody.velocity = Vector3.zero;
        this.transform.localPosition = new Vector3(Random.Range(-48f, 48f), 0.5f, Random.Range(-48f, 48f));
        //this.transform.rotation = new Quaternion(0f, Random.Range(-180f, 180f), 0f, Random.Range(-180f, 180f));
        this.transform.localPosition = initLocation;
        //Target.localPosition = new Vector3(Random.Range(-48f, 48f), 0.5f, Random.Range(-48f, 48f));
    }
    public override void CollectObservations(VectorSensor sensor){
        base.CollectObservations(sensor);
        sensor.AddObservation(rBody.velocity);
        // var r2 = ray3D.GetRayPerceptionInput();
        // var r3 = RayPerceptionSensor.Perceive(r2);
        // int count = 0;
        // sensor.AddObservation(this.transform.rotation);
        // foreach(RayPerceptionOutput.RayOutput rayOutput in r3.RayOutputs){
        //     sensor.AddObservation(rayOutput.HitTagIndex);
        //     sensor.AddObservation(rayOutput.HitFraction);
        //         //Debug.Log(rayOutput.HasHit+" "+rayOutput.HitTaggedObject+" "+rayOutput.HitTagIndex+" "+rayOutput.HitFraction);

        //  }
        sensor.AddObservation(this.transform.localPosition);
        sensor.AddObservation(energy);

    }
    public float forceMultiplier = 10f;
    public float rotationMultiplier = 1;
   public override void OnActionReceived(ActionBuffers actionBuffers){
    slider.value = energy;
    energy_text.text = "energy: " + energy.ToString();
    Vector3 controlSignal = Vector3.zero;
    Vector3 rotationControlSignal = Vector3.zero;
    controlSignal.x = actionBuffers.ContinuousActions[0];
    controlSignal.z = actionBuffers.ContinuousActions[1];
    rotationControlSignal.x = actionBuffers.ContinuousActions[2];
    rotationControlSignal.z = actionBuffers.ContinuousActions[3];

    float moveSpeed = controlSignal.magnitude;
    float rotationAngle = rotationControlSignal.magnitude;

    float speedMultiplier = Mathf.Clamp(moveSpeed, 0f, 1f);
    float energyCost = speedMultiplier * 1f; 

    float rotationMultiplier = Mathf.Clamp(rotationAngle, 0f, 1f);
    energyCost += rotationMultiplier * 0.1f;

    energy -= 0f;

    slider.value = energy;
    rotationControlSignal.x = Mathf.Clamp(rotationControlSignal.x, -1f, 1f);
    rotationControlSignal.z = Mathf.Clamp(rotationControlSignal.z, -1f, 1f);

    rBody.AddForce(controlSignal * forceMultiplier);
    this.transform.rotation = new Quaternion(0f, this.transform.rotation.x + rotationControlSignal.x, 0f, this.transform.rotation.z + rotationControlSignal.z);

    float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.localPosition);

    float rotationMagnitude = rotationControlSignal.magnitude;

    if (energy < 0)
    {
        AddReward(-100f);
        EndEpisode();
    }
    if (this.transform.localPosition.y < -1)
    {
        EndEpisode();
    }

}

    public override void Heuristic(in ActionBuffers actionOut){
        ActionSegment<float> continuesActions = actionOut.ContinuousActions;
        continuesActions[0] = Input.GetAxisRaw("Horizontal");
        continuesActions[1] = Input.GetAxisRaw("Vertical");
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent<Goal>(out Goal goal)){
            //Debug.Log("Get Goal");
            floorMeshRenderer.material = winMaterial;
            SetReward(10000f);
            EndEpisode();
        }
        if(other.TryGetComponent<Wall>(out Wall wall)){
            //Debug.Log("Get Wall");
            floorMeshRenderer.material = loseMaterial;
            SetReward(-1000f);
            EndEpisode();
        }
    }
/*
    private Vector3 GetRandomLocation((float x1, float x2), (float y1, float y2) = (0, 0), (float z1, float z2)){
        return new Vector3(Random.Range(x1, x2), Random.Range(y1, y2), Random.Range(z1, z2));

    }*/
}

