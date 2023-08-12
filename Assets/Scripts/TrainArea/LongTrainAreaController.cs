using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongTrainAreaController : MonoBehaviour
{
     public GameObject target;
    public bool ifRandom = false;

    public Vector3 targePosition;
    private Vector3 targetInitPosition;
    void Start()
    {
        targetInitPosition = target.transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTargetCollided()
    {
        if (ifRandom){
            target.transform.localPosition = new Vector3(Random.Range(12f, 148f), 0.5f, Random.Range(-13f, 13f));
        }
    }
}
