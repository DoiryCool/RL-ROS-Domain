using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.UI;

public class JoymanController : Agent
{
    public GJWarAreaController areaController;


    public GameObject EnergySlider;
    public GameObject SatietySlider;

    public Vector3 initLocation;
    public int times = 0;
    public float energy = 100;
    private float MAX_ENERGY;
    public float satiety = 100;
    private float MAX_SATIETY;

    public float EnergyRecoryScaler = 0.1f;
    public float FoodReward = 10f;
    Rigidbody rBody;
    RayPerceptionSensorComponent3D ray3D;

     void Start(){
        MAX_ENERGY = energy;
        MAX_SATIETY = satiety;
        EnergySlider.GetComponent<Slider>().maxValue = MAX_ENERGY;
        SatietySlider.GetComponent<Slider>().maxValue = MAX_SATIETY;
        initLocation = this.transform.localPosition;
        rBody = GetComponent<Rigidbody>();
        this.rBody.angularVelocity = Vector3.zero;
        this.rBody.velocity = Vector3.zero;
        this.transform.localPosition = initLocation;
    }
    public override void OnEpisodeBegin(){
        //Debug.Log("Episode Times:" + times);
        times += 1;
        energy = MAX_ENERGY;
        satiety = MAX_SATIETY;
        this.rBody.velocity = Vector3.zero;
        this.transform.localPosition = new Vector3(Random.Range(-48f, 48f), 0.5f, Random.Range(-48f, 48f));
        //this.transform.rotation = new Quaternion(0f, Random.Range(-180f, 180f), 0f, Random.Range(-180f, 180f));
        this.transform.localPosition = initLocation;
    }
    public override void CollectObservations(VectorSensor sensor){
        base.CollectObservations(sensor);
        sensor.AddObservation(energy);
        sensor.AddObservation(satiety);
        sensor.AddObservation(rBody.velocity);
        sensor.AddObservation(this.transform.localPosition);
        sensor.AddObservation(this.transform.rotation);
    }
    public float forceMultiplier = 10f;
    public float rotationSpeed = 10f;
    public override void OnActionReceived(ActionBuffers actionBuffers)
{
        Vector3 controlSignal = new Vector3(actionBuffers.ContinuousActions[0], 0f, actionBuffers.ContinuousActions[1]);
        Vector3 rotationControlSignal = new Vector3(0f, actionBuffers.ContinuousActions[2], 0f);

        Vector3 force = controlSignal * forceMultiplier;
        Vector3 rotationChange = rotationControlSignal * rotationSpeed * Time.fixedDeltaTime;

        float controlSignalMagnitude = controlSignal.magnitude;
        float rotationControlSignalMagnitude = rotationControlSignal.magnitude;
        energy -= (controlSignalMagnitude * 1f + rotationControlSignalMagnitude * 0.5f) * Time.fixedDeltaTime;

        if (energy > 0)
        {
            rBody.AddForce(force, ForceMode.Force);
            transform.Rotate(rotationChange);
        }
        else
        {
            rBody.velocity = Vector3.zero;
            rBody.angularVelocity = Vector3.zero;
        }

        if (transform.localPosition.y < -1f)
        {
            EndEpisode();
        }
    }


    public override void Heuristic(in ActionBuffers actionOut){
        ActionSegment<float> continuesActions = actionOut.ContinuousActions;
        continuesActions[0] = Input.GetAxisRaw("Horizontal");
        continuesActions[1] = Input.GetAxisRaw("Vertical");
    }

    void Update(){
    satiety -= 0.1f;
    satiety = Mathf.Clamp(satiety, 0f, MAX_SATIETY);
    
    if (satiety <= 0){
        SetReward(-50f);
        EndEpisode();
    }
    
    if (satiety > 0 && energy < MAX_ENERGY){
        energy += Time.deltaTime * EnergyRecoryScaler; // 根据时间增加能量
        energy = Mathf.Clamp(energy, 0f, MAX_ENERGY);
    }
    
    EnergySlider.GetComponent<Slider>().value = energy;
    SatietySlider.GetComponent<Slider>().value = satiety;
}

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent<GluemanController>(out GluemanController joyman)){
            SetReward(-1000f);
            EndEpisode();
        }
        if(other.TryGetComponent<Wall>(out Wall wall)){
            SetReward(-1000f);
            EndEpisode();
        }
    }

    public void KillMe(float reward = 50f){
        SetReward(reward);
        EndEpisode();
    }
}

