using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PegTransferGoal : MonoBehaviour
{
    public GameObject areaObject;
    private bool AInGoal;
    private int counter;
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

    private void FixedUpdate()
    {
        PegTransferArea area = areaObject.GetComponent<PegTransferArea>();
        PegTransferAgent agentA = area.agentA.GetComponent<PegTransferAgent>();
        Rigidbody goalBody = area.goal.GetComponent<Rigidbody>();

        if (AInGoal)
        { 
            counter++;
            area.goal.transform.parent = agentA.tip.transform;
            area.goal.transform.localPosition = new Vector3(0, 0, 0.35f);
            goalBody.isKinematic = true;
            agentA.AddReward(0.2f);
            if (counter > 50)
            {
                area.goal.transform.parent = null;
                goalBody.isKinematic = false;
                agentA.AddReward(2f);
                // Observation ends and reset the Agent
                agentA.Done();
                Start();
                agentA.AgentReset();
                Debug.Log("done");
                area.AreaReset();
                
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
}
