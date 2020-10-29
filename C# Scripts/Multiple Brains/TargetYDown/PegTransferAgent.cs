using DefKit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using Barracuda;

public class PegTransferAgent : Agent
{
    public NNModel FinalTargetYDown;

    private int brain;
    private int counter;
    public GameObject fulcrum;
    public GameObject scope;
    public GameObject tip;
    public GameObject tail;

    public GameObject areaObject;      

    public float insMult = 10.0f;

    public float rotMult = 100.0f;

    private float[] rotLimit = { -45, 45, -45, 45 };
    private float[] insLimit = { -3f, 3f };

    private Vector3 minGoalRelPos;
    private Vector3 currentGoalRelPos;
    private Vector3 prevGoalRelPos;

    private Vector3 prevGoalPos;
    private Vector3 minGoalPos;

    private Vector3 currentTargetPos;
    private Vector3 minTargetRelPos;
    private Vector3 currentTargetRelPos;
    private Vector3 prevTargetRelPos;

    private GameObject currentGoal;
    private GameObject currentTarget;
    private bool isGrasping;
    
    float goalDistX;
    float goalDistY;
    float goalDistZ;
    float prevGoalDistX;
    float prevGoalDistY;
    float prevGoalDistZ;
    float minGoalDistX;
    float minGoalDistY;
    float minGoalDistZ;

    float targetDistX;
    float targetDistY;
    float targetDistZ;
    float prevTargetDistX;
    float prevTargetDistY;
    float prevTargetDistZ;
    float minTargetDistX;
    float minTargetDistY;
    float minTargetDistZ;
    // Start is called before the first frame update
    void Start()
    {
        PegTransferArea area = areaObject.GetComponent<PegTransferArea>();
        currentGoal = area.goal;
        currentTarget = area.target;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Awake()
    {
        PegTransferArea area = areaObject.GetComponent<PegTransferArea>();
        brain = 0;

    }

    public override void AgentReset()
    {
        // Read the parameters from the Area Game Object
        PegTransferArea area = areaObject.GetComponent<PegTransferArea>();

        // Set the intial position of the Agent at the beginning of the observtion
        float toolInPos = 2f;
        fulcrum.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        scope.transform.localPosition = new Vector3(0, 0, toolInPos);

        InitialisePositions();
    }


    void InitialisePositions()
    {
        PegTransferArea area = areaObject.GetComponent<PegTransferArea>();
        Vector3 tipPos = tip.transform.position;

        Vector3 currentGoalPos = currentGoal.GetComponent<Transform>().position;
        minGoalPos = currentGoalPos;
        prevGoalPos = currentGoalPos;
        minGoalRelPos = currentGoalPos - tipPos;
        prevGoalRelPos = currentGoalPos - tipPos;

        prevGoalDistX = currentGoalPos.x - tipPos.x;
        prevGoalDistY = currentGoalPos.y - tipPos.y;
        prevGoalDistZ = currentGoalPos.z - tipPos.z;
        minGoalDistX = currentGoalPos.x - tipPos.x;
        minGoalDistY = currentGoalPos.y - tipPos.y;
        minGoalDistZ = currentGoalPos.z - tipPos.z;

        currentTargetPos = currentTarget.GetComponent<Transform>().GetChild(0).position;
        minTargetRelPos = currentTargetPos - tipPos;
        prevTargetRelPos = currentTargetPos - tipPos;

        prevTargetDistX = currentTargetPos.x - tipPos.x;
        prevTargetDistY = currentTargetPos.y - tipPos.y;
        prevTargetDistZ = currentTargetPos.z - tipPos.z;
        minTargetDistX = currentTargetPos.x - tipPos.x;
        minTargetDistY = currentTargetPos.y - tipPos.y;
        minTargetDistZ = currentTargetPos.z - tipPos.z;

    }

    public override void CollectObservations()
    {
        // Read the parameters from the Area Game Object
        PegTransferArea area = areaObject.GetComponent<PegTransferArea>();
        Vector3 currentGoalPos = currentGoal.transform.position;

        currentGoalRelPos = currentGoalPos - tip.transform.position;
        currentTargetRelPos = currentTargetPos - tip.transform.position;

        // Set the Observbations
        // Note: If you want to normilize the values,
        // maybe normalise them here not in ppo.

        // Position
        AddVectorObs(fulcrum.transform.localRotation);
        // Insertion
        AddVectorObs(scope.transform.localPosition.z);

        if (tip.transform.childCount == 0)
        {
            isGrasping = false;
        }

        if (tip.transform.childCount == 1)
        {
            isGrasping = true;
        }

        AddVectorObs(isGrasping);

        AddVectorObs(currentGoalPos);
        AddVectorObs(prevGoalPos);
        AddVectorObs(currentGoalRelPos);
        AddVectorObs(prevGoalRelPos);
        AddVectorObs(minGoalRelPos);

        AddVectorObs(currentTargetPos);
        AddVectorObs(currentTargetRelPos);
        AddVectorObs(prevTargetRelPos);
        AddVectorObs(minTargetRelPos);

    }

    public override void AgentAction(float[] act)
    {
        PegTransferArea area = areaObject.GetComponent<PegTransferArea>();
        Transform bounds = area.bounds.GetComponent<Transform>();
        PegTransferGoal goalScript = currentGoal.GetComponent<PegTransferGoal>();

        Vector3 targetPos = currentTarget.transform.GetChild(0).GetComponent<Transform>().position;
        float height = currentTarget.transform.GetChild(0).lossyScale.y + 0.15f;
        targetDistX = targetPos.x - tip.transform.position.x;
        targetDistY = height - tip.transform.position.y;
        targetDistZ = targetPos.z - tip.transform.position.z;

        if (targetDistY < -0.35)
        {
            //Debug.Log("height is " + height);
            brain = 0;
            ConfigBrain(brain);
            //Debug.Log(targetDistY);
            // Tool control (Movement)
            float rotX = Mathf.Clamp(act[0] * rotMult * Time.fixedDeltaTime, -1, 1);
            float rotY = Mathf.Clamp(act[1] * rotMult * Time.fixedDeltaTime, -1, 1);
            float insertion = Mathf.Clamp(act[2] * insMult * Time.fixedDeltaTime, -1, 1);
            bool graspActionVal = Mathf.Clamp(act[3] * Time.fixedDeltaTime, -1f, 1f) > 0f;

            // Set the position and rotation of the tool
            fulcrum.transform.Rotate(new Vector3(rotX, 0, 0), Space.Self);
            //scope.transform.Translate(new Vector3(0, 0, insertion), Space.Self);

            Vector3 fulcRot = fulcrum.transform.localRotation.eulerAngles;
            Vector3 insVec = scope.transform.localPosition;

            fulcRot.x = MathUtils.ClampAngle(fulcRot.x, -60, 60);
            //fulcRot.y = MathUtils.ClampAngle(fulcRot.y, rotLimit[2], rotLimit[3]);
            fulcRot.z = 0f;
            fulcrum.transform.localRotation = Quaternion.Euler(fulcRot);

            insVec.z = Mathf.Clamp(insVec.z, insLimit[0], insLimit[1]);
            //scope.transform.localPosition = insVec;

            //// Establish a cube as boundaries - simulation end
            Vector3 tipPos = tip.transform.position;
            Vector3 upperBound = bounds.position + bounds.localScale / 2;
            Vector3 lowerBound = bounds.position - bounds.localScale / 2;

            if (tipPos.x <= lowerBound.x || tipPos.x >= upperBound.x || tipPos.y <= lowerBound.y || tipPos.y >= upperBound.y || tipPos.z <= lowerBound.z || tipPos.z >= upperBound.z)
            {

                // Observation ends and reset the Agent
                AddReward(-3f);
                Done();
                goalScript.ResetGoal();
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
                rotLimit[1] = 60;
                insLimit[1] = 9f; // Maximum insertion value Tip
            }

            if (tail.transform.position.y <= 0)
            {
                rotLimit[0] = fulcRot.x;
                insLimit[0] = insVec.z;
            }
            else
            {
                rotLimit[0] = -60;
                insLimit[0] = 1f; // Maximum insertion value Tail
            }


            currentTargetRelPos = targetPos - tip.transform.position;
            targetDistY = height - tip.transform.position.y;


            if (((targetDistY) < -0.35 && ((targetDistY) > (prevTargetDistY))))
            {
                AddReward(0.5f);
                prevTargetRelPos = currentTargetRelPos;
                prevTargetDistX = targetDistX;
                prevTargetDistY = targetDistY;
                prevTargetDistZ = targetDistZ;
            }
            else
            {
                AddReward(-0.2f);
                prevTargetRelPos = currentTargetRelPos;
                prevTargetRelPos = currentTargetRelPos;
                prevTargetDistX = targetDistX;
                prevTargetDistY = targetDistY;
                prevTargetDistZ = targetDistZ;


            }
        }



        AddReward(-0.1f);
    }

    private void ConfigBrain(int brain)
    {
        if (brain == 0)
        {
            GiveModel("FinalTargetYDown", FinalTargetYDown);
        }
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