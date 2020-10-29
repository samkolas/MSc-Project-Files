using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PegTransferGoal : MonoBehaviour
{
    public GameObject areaObject;
    private bool AInGoal;
    private int counter;

    private GameObject currentGoal;
    private GameObject currentTarget;

    private float startTime;

    // Start is called before the first frame update
    void Start()
    {
        AInGoal = false;
        counter = 0;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetGoal()
    {
        PegTransferArea area = areaObject.GetComponent<PegTransferArea>();
        PegTransferAgent agentA = area.agentA.GetComponent<PegTransferAgent>();

        string goalName = "Ring (" + agentA.goalNum.ToString() + ")";
        currentGoal = GameObject.Find(goalName);

        string targetName = "Stack (" + agentA.targetNum.ToString() + ")";
        currentTarget = GameObject.Find(targetName);



    }

    private void FixedUpdate()
    {
        PegTransferArea area = areaObject.GetComponent<PegTransferArea>();
        PegTransferAgent agentA = area.agentA.GetComponent<PegTransferAgent>();

        if (AInGoal)
        {
            Rigidbody goalBody = currentGoal.GetComponent<Rigidbody>();
            Vector3 currentGoalPos = currentGoal.transform.position;
            float targetDistX = currentGoalPos.x - currentTarget.transform.GetChild(0).position.x;
            float targetDistZ = currentGoalPos.z - currentTarget.transform.GetChild(0).position.z;
            currentGoal.transform.parent = agentA.tip.transform;
            currentGoal.transform.localPosition = new Vector3(0, 0, 0.35f);
            goalBody.isKinematic = true;
            counter++;
            agentA.AddReward(0.2f);
            if (Mathf.Abs(targetDistX) < 0.075f && Mathf.Abs(targetDistZ) < 0.075f)
            {
                currentGoal.transform.parent = null;
                goalBody.isKinematic = false;
                agentA.AddReward(2f);
                // Observation ends and reset the Agent
                agentA.Done();
                ResetGoal();
                Debug.Log("Done");
                Start();
                area.inGoal = true;

            }

        }

    }

    private void OnTriggerEnter(Collider other)
    {
        // Read the parameters from the Area Game Object
        PegTransferArea area = areaObject.GetComponent<PegTransferArea>();
        PegTransferAgent agentA = area.agentA.GetComponent<PegTransferAgent>();

        // If the Goal object hits the target:
        if (other.gameObject == agentA.tip)
        {
            AInGoal = true;
        }

    }

    public void ResetGoal()
    {
        PegTransferArea area = areaObject.GetComponent<PegTransferArea>();
        PegTransferAgent agentA = area.agentA.GetComponent<PegTransferAgent>();
        Rigidbody goalBody = currentGoal.GetComponent<Rigidbody>();
        AInGoal = false;
        goalBody.isKinematic = false;
        currentGoal.transform.parent = null;
        if (agentA.isLast == false)
        {
            agentA.k++;
        }
        if (agentA.isLast == true)
        {
            area.AreaReset();
            agentA.Start();
        }
        agentA.AgentReset();
    }
}
