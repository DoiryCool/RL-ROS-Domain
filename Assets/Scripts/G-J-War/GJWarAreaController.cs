using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GJWarAreaController : MonoBehaviour
{
    public GameObject[] targets;
    public bool ifRandom = false;
    public Vector3 randomRange = new Vector3(48f, 0.5f, 48f);

    private Vector3[] targetInitPositions;

    public GameObject targetPrefab; // 预制体，即目标的模板
    public int numberOfTargets = 5; // 要生成的目标数量

    void Start()
    {
        Debug.Log("Generating Target.");
        targets = new GameObject[numberOfTargets]; // 初始化数组
        targetInitPositions = new Vector3[numberOfTargets]; // 初始化数组
        GenerateTargets();
    }

    public void ResetTargets()
    {
        for (int i = 0; i < targets.Length; i++)
        {
            if (ifRandom)
            {
                Vector3 randomPosition = new Vector3(
                    Random.Range(-randomRange.x, randomRange.x),
                    randomRange.y,
                    Random.Range(-randomRange.z, randomRange.z)
                );
                targets[i].transform.localPosition = randomPosition;
            }
            else
            {
                targets[i].transform.localPosition = targetInitPositions[i];
            }
        }
    }

    void GenerateTargets()
    {
        for (int i = 0; i < numberOfTargets; i++)
        {
            Vector3 spawnPosition = new Vector3(Random.Range(-150f, 150f), 0.5f, Random.Range(-150f, 150f)); // 生成位置范围
            GameObject newTarget = Instantiate(targetPrefab, spawnPosition, Quaternion.identity); // 在指定位置生成目标
            newTarget.transform.parent = this.transform;
            targets[i] = newTarget;
            targetInitPositions[i] = spawnPosition;
            targets[i].transform.localPosition = targetInitPositions[i];
        }
    }
}
