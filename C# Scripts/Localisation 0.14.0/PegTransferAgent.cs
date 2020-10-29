using DefKit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using Barracuda;

public class PegTransferAgent : Agent
{
    public GameObject fulcrum;
    public GameObject scope;
    public GameObject tip;
    public GameObject tail;

    public GameObject areaObject;
    private GameObject currentGoal;

    public float insMult = 10.0f;

    public float rotMult = 100.0f;

    private float[] rotLimit = { -45, 45, -45, 45 };
    private float[] insLimit = { -3f, 3f };
    private Vector3 prevDist;

    public TimerScript timerScript;
    public float timerTot;

    // Start is called before the first frame update
    void Start()
    {
        timerTot = 0;
        timerScript = timerScript.GetComponent<TimerScript>();

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Awake()
    {
        PegTransferArea area = areaObject.GetComponent<PegTransferArea>();
        currentGoal = area.goal;
        prevDist = currentGoal.transform.position - tip.transform.position;

    }

    public override void AgentReset()
    {
        // Read the parameters from the Area Game Object
        PegTransferArea area = areaObject.GetComponent<PegTransferArea>();

        // Set the intial position of the Agent at the beginning of the observtion
        float toolInPos = 2f;
        fulcrum.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        scope.transform.localPosition = new Vector3(0, 0, toolInPos);
        timerTot = 0;
        timerScript = timerScript.GetComponent<TimerScript>();
    }

    public override void CollectObservations()
    {
        // Read the parameters from the Area Game Object
        PegTransferArea area = areaObject.GetComponent<PegTransferArea>();
        // Set the Observbations
        // Note: If you want to normilize the values,
        // maybe normalise them here not in ppo.

        // Position
        AddVectorObs(fulcrum.transform.localRotation);
        // Insertion
        AddVectorObs(scope.transform.localPosition.z);

        Vector3 goalDist = currentGoal.transform.position - tip.transform.position;
        AddVectorObs(goalDist);

        AddVectorObs(currentGoal);

    }

    public override void AgentAction(float[] act)
    {
        timerTot += Time.deltaTime;
        timerScript.SetTimer(timerTot);
        timerScript.GetTimer();
        PegTransferArea area = areaObject.GetComponent<PegTransferArea>();
        Transform bounds = area.bounds.GetComponent<Transform>();


        float rotX = Mathf.Clamp(act[0] * rotMult * Time.fixedDeltaTime, -1, 1);
        float rotY = Mathf.Clamp(act[1] * rotMult * Time.fixedDeltaTime, -1, 1);

        float insertion = Mathf.Clamp(act[2] * insMult * Time.fixedDeltaTime, -1, 1);

        fulcrum.transform.Rotate(new Vector3(rotX, rotY, 0), Space.Self);
        scope.transform.Translate(new Vector3(0, 0, insertion), Space.Self);

        Vector3 fulcRot = fulcrum.transform.localRotation.eulerAngles;
        fulcRot.x = MathUtils.ClampAngle(fulcRot.x, -45, 45);
        fulcRot.y = MathUtils.ClampAngle(fulcRot.y, -45, 45);
        fulcrum.transform.localRotation = Quaternion.Euler(fulcRot);

        Vector3 insVec = scope.transform.localPosition;
        insVec.z = Mathf.Clamp(insVec.z, 1f, 11f);
        scope.transform.localPosition = insVec;

        //// Establish a cube as boundaries - simulation end
        Vector3 tipPos = tip.transform.position;
        Vector3 upperBound = bounds.position + bounds.localScale / 2;
        Vector3 lowerBound = bounds.position - bounds.localScale / 2;

        if (tipPos.x <= lowerBound.x || tipPos.x >= upperBound.x || tipPos.y <= lowerBound.y || tipPos.y >= upperBound.y || tipPos.z <= lowerBound.z || tipPos.z >= upperBound.z)
        {

            // Observation ends and reset the Agent
            AddReward(-3f);
            Done();
            AgentReset();
        }

        // Update Agent limits in x and
        // ground as bound
        if (tip.transform.position.y <= 0)
        {
            rotLimit[1] = fulcRot.x;
            insLimit[1] = insVec.z;
        }
        else
        {
            rotLimit[1] = 45;
            insLimit[1] = 9f; // Maximum insertion value Tip
        }

        if (tail.transform.position.y <= 0)
        {
            rotLimit[0] = fulcRot.x;
            insLimit[0] = insVec.z;
        }
        else
        {
            rotLimit[0] = -45;
            insLimit[0] = 1f; // Maximum insertion value Tail
        }

        Vector3 goalDist = currentGoal.transform.position - tip.transform.position;
        if (Mathf.Abs(goalDist.x) < Mathf.Abs(prevDist.x) && Mathf.Abs(goalDist.z) < Mathf.Abs(prevDist.z))
        {
            AddReward(0.4f);
        }
        else
        {
            AddReward(-0.5f);
        }

        AddReward(-0.01f);
        prevDist = goalDist;
    }

    public override float[] Heuristic()
    {
        var act = new float[4];
        act[0] = Input.GetAxis("Vertical");
        act[1] = Input.GetAxis("Horizontal");
        act[2] = Input.GetAxis("Depth");
        act[3] = Input.GetAxis("Grasp"); // TODO: Calibrate the grasp, not working
        return act;
    }

}