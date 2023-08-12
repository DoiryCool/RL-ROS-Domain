using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.UI;

public class AnimalAgent : Agent
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

    Vector3 previousPosition;

    public GameObject[] legs = new GameObject[4];

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

        for (int i = 0; i < legs.Length; i++){
            legs[i].transform.rotation = Quaternion.Euler(Vector3.zero);
        }

    }
    public override void CollectObservations(VectorSensor sensor){
        base.CollectObservations(sensor);
        sensor.AddObservation(rBody.velocity);
        for (int i = 0; i < legs.Length; i++)
        {
            Quaternion rotation = legs[i].transform.rotation;
            sensor.AddObservation(rotation.eulerAngles);
        }
        sensor.AddObservation(this.transform.localPosition);
        sensor.AddObservation(energy);

    }
    public float forceMultiplier = 10f;
    public float rotationMultiplier = 1f;
   public override void OnActionReceived(ActionBuffers actionBuffers){
    slider.value = energy;
    energy-=0.1f;
    energy_text.text = "Energy: " + energy.ToString();
    previousPosition = this.transform.localPosition; // 记录动作前的位置

    for (int i = 0; i < 2; i++)
    {
        Vector3 rotation = legs[i].transform.rotation.eulerAngles;
        rotation.x = Mathf.Clamp(actionBuffers.ContinuousActions[3 * i + 0 ]*1 + rotation.x, -30f, 30f);
        rotation.y += actionBuffers.ContinuousActions[3 * i + 0 ]*0;
        rotation.z = Mathf.Clamp(actionBuffers.ContinuousActions[3 * i + 0 ]*1 + rotation.z, -10f, 10f);
        legs[i].transform.rotation = Quaternion.Euler(rotation);
        legs[i + 2].transform.rotation = Quaternion.Euler(rotation);
    }

    if (energy < 0)
    {
        AddReward(-100f);
        EndEpisode();
    }
    if (this.transform.localPosition.y < -1)
    {
        EndEpisode();
    }

    Vector3 movement = new Vector3(0, 0, 12f) - previousPosition; // 计算动作后的移动距离
    float reward = Vector3.Dot((Target.localPosition - this.transform.localPosition).normalized, movement.normalized); // 计算动物移动方向与目标方向的相似度
    AddReward(-reward * 10); // 如果动物朝着目标移动，奖励将会是正的
}

    public override void Heuristic(in ActionBuffers actionOut){
        ActionSegment<float> continuesActions = actionOut.ContinuousActions;
        continuesActions[0] = Input.GetAxisRaw("Horizontal");
        continuesActions[2] = Input.GetAxisRaw("Vertical");

        continuesActions[1] = 0f;
        continuesActions[3] = 0f;
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

