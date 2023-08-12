using System.Collections;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.UI;

public class CarController : Agent
{
    public Transform Target;
    Rigidbody rBody;
    public Vector3 initLocation;
    public GameObject EnergySlider;
    Slider slider;
    Vector3 carDirection;

    public GameObject HeadOB;
    public GameObject BodyOB;
    Vector3 headPosition;
    Vector3 bodyPosition;

    public GameObject InfoTextOB;
    private Text info_text;

    private float h;//横轴的输入
    private float v;//纵轴的输入
 
    private float angle;//角度
 
    public WheelCollider lf, rf, lb, rb;
 
    public Transform lfT, rfT, lbT, rbT;
 
    public float angleSpeed = 30f;//转向速度
    public float speed = 300f;//移动速度

    public float fuel = 3000;
    public float distanceThreshold = 200f;

    public float distanceRewardWeight = 5f;
    public float collisionPenalty = -100f;
    public float speedPenalty = -0.1f;
    public float angularVelocityPenaltyFactor = 0.5f;
    public float fuelExhaustionPenalty = -100f;

    public bool[] cp = new bool[10]; // 假设分成了10个检查点

    private int times = 0;
    void Start(){
        info_text = InfoTextOB.GetComponent<Text>();
        for (int i = 0; i < cp.Length; i++)
        {
            cp[i] = false;
        }

        rBody = GetComponent<Rigidbody>();
        slider = EnergySlider.GetComponent<Slider>();
        slider.value = fuel;
        // 记录初始位置
        initLocation = this.transform.localPosition;
    }

    private void Update()
    {
        Vector3 headPosition = HeadOB.transform.position;
        Vector3 bodyPosition = BodyOB.transform.position;

        carDirection = headPosition - bodyPosition;

        // 可以根据需要进行归一化（长度为1）处理
        carDirection.Normalize();
    }

    public override void OnEpisodeBegin(){
        fuel = 3000;
        for (int i = 0; i < cp.Length; i++)
        {
            cp[i] = false;
        }

        times++;
        info_text.text = "Episode Times: " + times;
        this.rBody.angularVelocity = Vector3.zero;
        this.rBody.velocity = Vector3.zero;
        this.transform.localPosition = new Vector3(Random.Range(-48f, 48f), 0.5f, Random.Range(-48f, 48f));
        //this.transform.rotation = new Quaternion(0f, Random.Range(-180f, 180f), 0f, Random.Range(-180f, 180f));
        Vector3 randomEulerAngles = new Vector3(0f, Random.Range(0f, 360f), 0f);
        this.transform.rotation = Quaternion.Euler(randomEulerAngles);
        this.transform.localPosition = initLocation;
        //Target.localPosition = new Vector3(Random.Range(12f, 148f), 0.5f, Random.Range(-13f, 13f));
    }

    public override void CollectObservations(VectorSensor sensor){
        base.CollectObservations(sensor);
        sensor.AddObservation(this.transform.rotation);
        sensor.AddObservation(this.transform.localPosition);
        sensor.AddObservation(fuel);

    }

    public override void OnActionReceived(ActionBuffers actionBuffers){
        h = actionBuffers.ContinuousActions[0];
        v = actionBuffers.ContinuousActions[1];
        angle = angleSpeed * h;
        lf.steerAngle = angle;//左前
        rf.steerAngle = angle;//右前
 
        //移动
        lf.motorTorque = v * speed;
        rf.motorTorque = v * speed;
 
        UpdateWheel(lfT, lf);
        UpdateWheel(rfT, rf);
        UpdateWheel(lbT, lb);
        UpdateWheel(rbT, rb);

        fuel -= 0.5f;
        slider.value = fuel;
        // 计算距离目标的奖励
        float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.localPosition);
        float distanceReward = (distanceToTarget / (distanceThreshold + distanceToTarget));
        AddReward(-distanceReward * distanceRewardWeight);
        // Debug.Log("distanceReward: " + -distanceReward * distanceRewardWeight);

        // 速度惩罚
        AddReward(-rBody.velocity.magnitude * speedPenalty);
        // Debug.Log("速度惩罚: " + rBody.velocity.magnitude * speedPenalty);

        // 角速度惩罚
        float angularVelocityMagnitude = rBody.angularVelocity.magnitude;
        AddReward(-angularVelocityMagnitude * angularVelocityPenaltyFactor);
        // Debug.Log("角速度惩罚: " + angularVelocityMagnitude * angularVelocityPenaltyFactor);

        // 燃料耗尽惩罚
        if (fuel < 0)
        {
            AddReward(fuelExhaustionPenalty);
            EndEpisode();
        }
        for (int i = 0; i < cp.Length; i++)
        {
            float checkpointX = -139.7f + ((i + 1) * 27f); // 根据分布计算每个检查点的 x 坐标
            if (this.transform.localPosition.x > checkpointX)
            {
                if (!cp[i])
                {
                    cp[i] = true;
                    AddReward(100f); // 给予检查点奖励
                    fuel += 1000;
                }else{
                    AddReward(-0.1f * i);
                }
            }else{
                AddReward(-0.2f * i);
            }
        }
        
        if (this.transform.localPosition.y < -1)
        {
            SetReward(-1000f);
            EndEpisode();
        }
}

    public override void Heuristic(in ActionBuffers actionOut)
    {
        ActionSegment<float> continuesActions = actionOut.ContinuousActions;
        continuesActions[0] = Input.GetAxisRaw("Horizontal");
        continuesActions[1] = Input.GetAxisRaw("Vertical");
    }
 
    //同步车轮
    void UpdateWheel(Transform trans,WheelCollider wheel)
    {
        Vector3 pos = trans.position;
        Quaternion rot = trans.rotation;
        wheel.GetWorldPose(out pos, out rot);//将轮胎的位置以及角度给到上面的值
        trans.position = pos;
        trans.rotation = rot;
    }

    public LongTrainAreaController areaController;
    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent<Goal>(out Goal goal)){
            fuel += 2000;
            SetReward(1000f);
            areaController.OnTargetCollided();
        }
        if(other.TryGetComponent<Wall>(out Wall wall)){
            SetReward(-5000f);
            EndEpisode();
        }
         if (other.CompareTag("Car"))
        {
            // 两车相撞，触发惩罚或其他逻辑
            SetReward(-100f); // 或者根据需求设置其他惩罚
            EndEpisode();   // 结束本回合
        }
        // if(other.TryGetComponent<Wall>(out Wall wall)){
        //     //Debug.Log("Get Wall");
        //     floorMeshRenderer.material = loseMaterial;
        //     SetReward(-1000f);
        //     EndEpisode();
        // }
    }
}
