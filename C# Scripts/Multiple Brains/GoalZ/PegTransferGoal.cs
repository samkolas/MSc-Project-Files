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
        
        Vector3 goalDist = area.goal.transform.position - agentA.tip.transform.position;
       
            if (goalDist.z < 0.4f && goalDist.z > 0)
        {
            counter++;
            agentA.AddReward(0.2f);
            if (counter > 50)
            {
                agentA.AddReward(2f);
                // Observation ends and reset the Agent
                Debug.Log("done");
                ResetGoal();
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
        agentA.Done();
        Start();
        agentA.AgentReset();
        area.AreaReset();

    }
}
