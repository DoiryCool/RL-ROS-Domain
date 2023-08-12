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

    public GameObject EnergySlider;
    public GameObject SatietySlider;

    public Material winMaterial;
    public Material loseMaterial;
    public Material nomarlMaterial;
    public MeshRenderer floorMeshRenderer; 
    public Vector3 initLocation;
    public int times = 0;
    public float energy = 1000;
    public float satiety = 200;

    public float EnergyRecoryScaler = 4f;

    Rigidbody rBody;
    RayPerceptionSensorComponent3D ray3D;

    public BaseTrainAreaController areaController;
     void Start(){
        initLocation = this.transform.localPosition;
        rBody = GetComponent<Rigidbody>();
        information_text = information.GetComponent<Text>();
        information_text.text = "Episode Times: 0";
        this.rBody.angularVelocity = Vector3.zero;
        this.rBody.velocity = Vector3.zero;
        this.transform.localPosition = initLocation;
    }
    public override void OnEpisodeBegin(){
        //Debug.Log("Episode Times:" + times);
        times += 1;
        energy = 1000;
        satiety = 200;
        information_text.text = "Episode Times: " + times.ToString();
        this.rBody.angularVelocity = Vector3.zero;
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
    public override void OnActionReceived(ActionBuffers actionBuffers){
        Vector3 controlSignal = Vector3.zero;
        Vector3 rotationControlSignal = Vector3.zero;
        controlSignal.x = actionBuffers.ContinuousActions[0];
        controlSignal.z = actionBuffers.ContinuousActions[1];
        rotationControlSignal.y = actionBuffers.ContinuousActions[2];

        if (energy > 0){
            energy -= controlSignal.magnitude + rotationControlSignal.magnitude * 0.5f;
            rBody.AddForce(controlSignal * forceMultiplier);
            Vector3 rotationChange = rotationControlSignal * rotationSpeed * Time.deltaTime;
            transform.Rotate(rotationChange);
        }else{
            this.rBody.velocity = Vector3.zero;
            this.rBody.angularVelocity = Vector3.zero;
        }

        if (this.transform.localPosition.y < -1)
        {
            floorMeshRenderer.material = loseMaterial;
            EndEpisode();
        }

}

    public override void Heuristic(in ActionBuffers actionOut){
        ActionSegment<float> continuesActions = actionOut.ContinuousActions;
        continuesActions[0] = Input.GetAxisRaw("Horizontal");
        continuesActions[1] = Input.GetAxisRaw("Vertical");
    }

    void Update(){
        satiety -= 1f;        
        if (satiety < 0){
            SetReward(-1000f);
            floorMeshRenderer.material = loseMaterial;
            EndEpisode();
        }
        if (satiety > 0 && energy < 1000f){
            energy += 1f * EnergyRecoryScaler ; 
        }
        EnergySlider.GetComponent<Slider>().value = energy;
        SatietySlider.GetComponent<Slider>().value = satiety;
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent<Goal>(out Goal goal)){
            areaController.OnTargetCollided();
            floorMeshRenderer.material = winMaterial;
            satiety = 50f + satiety < 200f ?  50f + satiety : 200f;
            // SetReward(10000f);
            // EndEpisode();
        }
        if(other.TryGetComponent<Wall>(out Wall wall)){
            //Debug.Log("Get Wall");
            floorMeshRenderer.material = loseMaterial;
            SetReward(-1000f);
            EndEpisode();
        }
    }
}

