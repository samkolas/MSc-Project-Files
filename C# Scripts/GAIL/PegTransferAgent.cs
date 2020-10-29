using DefKit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class PegTransferAgent : Agent
{
    /*public NNModel NewGoalZ;
    public NNModel NewGoalY;
    public NNModel NewGoalX;
    public NNModel NewTargetYUp;
    public NNModel NewTargetYDown;
    public NNModel NewTargetZ;
    public NNModel NewTargetX;*/

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

    private bool isGrasping;

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
    private PegTransferGoal goalScript;
    private int numGoals;
    public int goalNum;
    public int targetNum;
    private int[] goals;
    public int k;
    public bool isLast;

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

    public TimerScript timerScript;
    public float timerTot;

    System.Random rnd = new System.Random();

    // Start is called before the first frame update
    public void Start()
    {
        GameObject[] allGoals = GameObject.FindGameObjectsWithTag("Goal");
        numGoals = allGoals.Length;
        goals = new int[numGoals];

        for (int i = 0; i < goals.Length; i++)
        {
            goals[i] = i;
        }

        /*for (int i = goals.Length - 1; i > 0; i--)
        {
            int randInd = rnd.Next(0, i + 1);
            int temp = goals[i];
            goals[i] = goals[randInd];
            goals[randInd] = temp;
        }*/

        k = 0;
        isLast = false;
        timerTot = 0;
        timerScript = timerScript.GetComponent<TimerScript>();
    }

    public void Awake()
    {
        PegTransferArea area = areaObject.GetComponent<PegTransferArea>();
        brain = 0;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void OnEpisodeBegin()
    {
        // Read the parameters from the Area Game Object
        PegTransferArea area = areaObject.GetComponent<PegTransferArea>();

        // Set the intial position of the Agent at the beginning of the observtion
        float toolInPos = 2f;
        fulcrum.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        scope.transform.localPosition = new Vector3(0, 0, toolInPos);

        // Clear Flags
        isGrasping = false;

        //goalScript.enabled = false;
        //goalScript = area.goal.GetComponent<PegTransferGoal>();
        //goalScript.enabled = false;

        InitialiseGoal();
        InitialisePositions();
        brain = 0;
        counter = 0;
    }

    void InitialiseGoal()
    {
        goalNum = goals[k];
        targetNum = goals[k];
        PegTransferArea area = areaObject.GetComponent<PegTransferArea>();
        string goalName = "Ring (" + goalNum.ToString() + ")";
        //Debug.Log(goalName);
        currentGoal = GameObject.Find(goalName);
        //area.goal = currentGoal;

        string targetName = "Stack (" + targetNum.ToString() + ")";
        currentTarget = GameObject.Find(targetName);
        goalScript = currentGoal.GetComponent<PegTransferGoal>();
        goalScript.enabled = true;
        goalScript.SetGoal();
        if (k == (numGoals - 1))
        {
            isLast = true;
        }
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

    public override void CollectObservations(VectorSensor sensor)
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
        sensor.AddObservation(fulcrum.transform.localRotation);
        // Insertion
        sensor.AddObservation(scope.transform.localPosition.z);

        if (tip.transform.childCount == 0)
        {
            isGrasping = false;
        }

        if (tip.transform.childCount == 1)
        {
            isGrasping = true;
        }

        sensor.AddObservation(isGrasping);

        sensor.AddObservation(currentGoalPos);
        sensor.AddObservation(prevGoalPos);
        sensor.AddObservation(currentGoalRelPos);
        sensor.AddObservation(prevGoalRelPos);
        sensor.AddObservation(minGoalRelPos);

        sensor.AddObservation(currentTargetPos);
        sensor.AddObservation(currentTargetRelPos);
        sensor.AddObservation(prevTargetRelPos);
        sensor.AddObservation(minTargetRelPos);

    }

    public override void OnActionReceived(float[] actionsOut)
    {
        // Read the parameters from the Area Game Object
        timerTot += Time.deltaTime;
        timerScript.SetTimer(timerTot);
        timerScript.GetTimer();
        PegTransferArea area = areaObject.GetComponent<PegTransferArea>();
        Transform bounds = area.bounds.GetComponent<Transform>();
        goalScript = area.goal.GetComponent<PegTransferGoal>();

        if (tip.transform.childCount == 0)
        {
            Vector3 currentGoalPos = currentGoal.transform.position;

            goalDistX = currentGoalPos.x - tip.transform.position.x;
            goalDistY = currentGoalPos.y - tip.transform.position.y;
            goalDistZ = currentGoalPos.z - tip.transform.position.z;

            if (goalDistZ > 0.4f || goalDistZ < 0)
            {
                brain = 0;
                //ConfigBrain(brain);
                // Tool control (Movement)
                float rotX = Mathf.Clamp(actionsOut[0] * rotMult * Time.fixedDeltaTime, -1, 1);
                float rotY = Mathf.Clamp(actionsOut[1] * rotMult * Time.fixedDeltaTime, -1, 1);
                float insertion = Mathf.Clamp(actionsOut[2] * insMult * Time.fixedDeltaTime, -1, 1);
                bool graspActionVal = Mathf.Clamp(actionsOut[3] * Time.fixedDeltaTime, -1f, 1f) > 0f;

                // Set the position and rotation of the tool
                //fulcrum.transform.Rotate(new Vector3(rotX, rotY, 0), Space.Self);
                scope.transform.Translate(new Vector3(0, 0, insertion), Space.Self);

                Vector3 fulcRot = fulcrum.transform.localRotation.eulerAngles;
                Vector3 insVec = scope.transform.localPosition;

                fulcRot.x = MathUtils.ClampAngle(fulcRot.x, rotLimit[0], rotLimit[1]);
                fulcRot.y = MathUtils.ClampAngle(fulcRot.y, rotLimit[2], rotLimit[3]);
                fulcRot.z = 0f;
                //fulcrum.transform.localRotation = Quaternion.Euler(fulcRot);

                insVec.z = Mathf.Clamp(insVec.z, -3f, 9f);
                scope.transform.localPosition = insVec;

                //// Establish a cube as boundaries - simulation end
                Vector3 tipPos = tip.transform.position;
                Vector3 upperBound = bounds.position + bounds.localScale / 2;
                Vector3 lowerBound = bounds.position - bounds.localScale / 2;

                if (tipPos.x <= lowerBound.x || tipPos.x >= upperBound.x || tipPos.y <= lowerBound.y || tipPos.y >= upperBound.y || tipPos.z <= lowerBound.z || tipPos.z >= upperBound.z)
                {

                    // Observation ends and reset the Agent
                    AddReward(-3f);
                    EndEpisode();
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


                Vector3 updatedCurrentGoalPos = currentGoal.GetComponent<Transform>().position;
                currentGoalRelPos = updatedCurrentGoalPos - tip.transform.position;
                goalDistZ = updatedCurrentGoalPos.z - tip.transform.position.z;

                if (((goalDistZ) < 0 && ((goalDistZ) > (prevGoalDistZ))) || ((goalDistZ) > 0.3 && ((goalDistZ) < (prevGoalDistZ))))
                {
                    AddReward(0.5f);
                    prevGoalPos = updatedCurrentGoalPos;
                    prevGoalRelPos = currentGoalRelPos;
                    prevGoalDistX = goalDistX;
                    prevGoalDistY = goalDistY;
                    prevGoalDistZ = goalDistZ;
                    //Debug.Log(targetDistZ + " closer");
                }
                else
                {
                    AddReward(-0.2f);
                    prevGoalPos = updatedCurrentGoalPos;
                    prevGoalRelPos = currentGoalRelPos;
                    prevGoalDistX = goalDistX;
                    prevGoalDistY = goalDistY;
                    prevGoalDistZ = goalDistZ;
                    //Debug.Log(targetDistZ + " farther");

                }

                if (((goalDistZ) < 0 && ((goalDistZ) > (minGoalDistZ))) || ((goalDistZ) > 0.3 && ((goalDistZ) < (minGoalDistZ))))
                {
                    AddReward(0.5f);
                    prevGoalPos = updatedCurrentGoalPos;
                    prevGoalRelPos = currentGoalRelPos;
                    prevGoalDistX = goalDistX;
                    prevGoalDistY = goalDistY;
                    prevGoalDistZ = goalDistZ;
                    minGoalPos = updatedCurrentGoalPos;
                    minGoalRelPos = currentGoalRelPos;
                    minGoalDistX = goalDistX;
                    minGoalDistY = goalDistY;
                    minGoalDistZ = goalDistZ;
                    //Debug.Log(targetDistZ + " new");
                }
                else
                {
                    AddReward(-0.2f);
                    prevGoalPos = updatedCurrentGoalPos;
                    prevGoalRelPos = currentGoalRelPos;
                    prevGoalDistX = goalDistX;
                    prevGoalDistY = goalDistY;
                    prevGoalDistZ = goalDistZ;
                    //Debug.Log(targetDistZ + " same");

                }
            }


            if (Mathf.Abs(goalDistY) > 0.1f && (goalDistZ < 0.4f && goalDistZ > 0))
            {
                brain = 1;
                //ConfigBrain(brain);
                //Debug.Log(goalDistY);
                // Tool control (Movement)
                float rotX = Mathf.Clamp(actionsOut[0] * rotMult * Time.fixedDeltaTime, -1, 1);
                float rotY = Mathf.Clamp(actionsOut[1] * rotMult * Time.fixedDeltaTime, -1, 1);
                float insertion = Mathf.Clamp(actionsOut[2] * insMult * Time.fixedDeltaTime, -1, 1);
                bool graspActionVal = Mathf.Clamp(actionsOut[3] * Time.fixedDeltaTime, -1f, 1f) > 0f;

                // Set the position and rotation of the tool
                fulcrum.transform.Rotate(new Vector3(rotX, 0, 0), Space.Self);
                //scope.transform.Translate(new Vector3(0, 0, insertion), Space.Self);

                Vector3 fulcRot = fulcrum.transform.localRotation.eulerAngles;
                Vector3 insVec = scope.transform.localPosition;

                fulcRot.x = MathUtils.ClampAngle(fulcRot.x, rotLimit[0], rotLimit[1]);
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
                    AddReward(-3f);
                    // Observation ends and reset the Agent
                    EndEpisode();
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


                Vector3 updatedCurrentGoalPos = currentGoal.GetComponent<Transform>().position;
                currentGoalRelPos = updatedCurrentGoalPos - tip.transform.position;
                goalDistY = updatedCurrentGoalPos.y - tip.transform.position.y;

                if (Mathf.Abs(goalDistY) < Mathf.Abs(prevGoalDistY))
                {
                    AddReward(0.5f);
                    prevGoalPos = updatedCurrentGoalPos;
                    prevGoalRelPos = currentGoalRelPos;
                    prevGoalDistX = goalDistX;
                    prevGoalDistY = goalDistY;
                    prevGoalDistZ = goalDistZ;
                    //Debug.Log(targetDistZ + " closer");
                }
                else
                {
                    AddReward(-0.2f);
                    prevGoalPos = updatedCurrentGoalPos;
                    prevGoalRelPos = currentGoalRelPos;
                    prevGoalDistX = goalDistX;
                    prevGoalDistY = goalDistY;
                    prevGoalDistZ = goalDistZ;
                    //Debug.Log(targetDistZ + " farther");

                }

                if (Mathf.Abs(goalDistY) < Mathf.Abs(minGoalDistY))
                {
                    AddReward(0.5f);
                    prevGoalPos = updatedCurrentGoalPos;
                    prevGoalRelPos = currentGoalRelPos;
                    prevGoalDistX = goalDistX;
                    prevGoalDistY = goalDistY;
                    prevGoalDistZ = goalDistZ;
                    minGoalPos = updatedCurrentGoalPos;
                    minGoalRelPos = currentGoalRelPos;
                    minGoalDistX = goalDistX;
                    minGoalDistY = goalDistY;
                    minGoalDistZ = goalDistZ;
                    //Debug.Log(targetDistZ + " new");
                }
                else
                {
                    AddReward(-0.2f);
                    prevGoalPos = updatedCurrentGoalPos;
                    prevGoalRelPos = currentGoalRelPos;
                    prevGoalDistX = goalDistX;
                    prevGoalDistY = goalDistY;
                    prevGoalDistZ = goalDistZ;
                    //Debug.Log(targetDistZ + " same");

                }
            }


            if (Mathf.Abs(goalDistX) > 0.35f && Mathf.Abs(goalDistY) < 0.1f && (goalDistZ < 0.4f && goalDistZ > 0))
            {
                brain = 2;
                //ConfigBrain(brain);
                // Tool control (Movement)
                float rotX = Mathf.Clamp(actionsOut[0] * rotMult * Time.fixedDeltaTime, -1, 1);
                float rotY = Mathf.Clamp(actionsOut[1] * rotMult * Time.fixedDeltaTime, -1, 1);
                float insertion = Mathf.Clamp(actionsOut[2] * insMult * Time.fixedDeltaTime, -1, 1);
                bool graspActionVal = Mathf.Clamp(actionsOut[3] * Time.fixedDeltaTime, -1f, 1f) > 0f;

                // Set the position and rotation of the tool
                fulcrum.transform.Rotate(new Vector3(0, rotY, 0), Space.Self);
                //scope.transform.Translate(new Vector3(0, 0, insertion), Space.Self);

                Vector3 fulcRot = fulcrum.transform.localRotation.eulerAngles;
                Vector3 insVec = scope.transform.localPosition;

                //fulcRot.x = MathUtils.ClampAngle(fulcRot.x, rotLimit[0], rotLimit[1]);
                fulcRot.y = MathUtils.ClampAngle(fulcRot.y, rotLimit[2], rotLimit[3]);
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
                    AddReward(-3f);
                    // Observation ends and reset the Agent
                    EndEpisode();
                    goalScript.ResetGoal();
                    // Create new observation scenario
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


                Vector3 updatedCurrentGoalPos = currentGoal.GetComponent<Transform>().position;
                currentGoalRelPos = updatedCurrentGoalPos - tip.transform.position;
                goalDistX = updatedCurrentGoalPos.x - tip.transform.position.x;

                if (Mathf.Abs(goalDistX) < Mathf.Abs(prevGoalDistX))
                {
                    AddReward(0.5f);
                    prevGoalPos = updatedCurrentGoalPos;
                    prevGoalRelPos = currentGoalRelPos;
                    prevGoalDistX = goalDistX;
                    prevGoalDistY = goalDistY;
                    prevGoalDistZ = goalDistZ;
                    //Debug.Log(targetDistZ + " closer");
                }
                else
                {
                    AddReward(-0.2f);
                    prevGoalPos = updatedCurrentGoalPos;
                    prevGoalRelPos = currentGoalRelPos;
                    prevGoalDistX = goalDistX;
                    prevGoalDistY = goalDistY;
                    prevGoalDistZ = goalDistZ;
                    //Debug.Log(targetDistZ + " farther");

                }

                if (Mathf.Abs(goalDistX) < Mathf.Abs(minGoalDistX))
                {
                    AddReward(0.5f);
                    prevGoalPos = updatedCurrentGoalPos;
                    prevGoalRelPos = currentGoalRelPos;
                    prevGoalDistX = goalDistX;
                    prevGoalDistY = goalDistY;
                    prevGoalDistZ = goalDistZ;
                    minGoalPos = updatedCurrentGoalPos;
                    minGoalRelPos = currentGoalRelPos;
                    minGoalDistX = goalDistX;
                    minGoalDistY = goalDistY;
                    minGoalDistZ = goalDistZ;
                    //Debug.Log(targetDistZ + " new");
                }
                else
                {
                    AddReward(-0.2f);
                    prevGoalPos = updatedCurrentGoalPos;
                    prevGoalRelPos = currentGoalRelPos;
                    prevGoalDistX = goalDistX;
                    prevGoalDistY = goalDistY;
                    prevGoalDistZ = goalDistZ;
                    //Debug.Log(targetDistZ + " same");

                }
            }

        }

        if (tip.transform.childCount == 1)
        {
            Vector3 targetPos = currentTarget.transform.GetChild(0).GetComponent<Transform>().position;
            float height = currentTarget.transform.GetChild(0).lossyScale.y + 0.15f;
            targetDistX = targetPos.x - tip.transform.position.x;
            targetDistY = height - tip.transform.position.y;
            targetDistZ = targetPos.z - tip.transform.position.z;

            if (targetDistY > -0.25)
            {
                //Debug.Log("height is " + height);
                brain = 3;
                //ConfigBrain(brain);
                //Debug.Log(targetDistY);
                // Tool control (Movement)
                float rotX = Mathf.Clamp(actionsOut[0] * rotMult * Time.fixedDeltaTime, -1, 1);
                float rotY = Mathf.Clamp(actionsOut[1] * rotMult * Time.fixedDeltaTime, -1, 1);
                float insertion = Mathf.Clamp(actionsOut[2] * insMult * Time.fixedDeltaTime, -1, 1);
                bool graspActionVal = Mathf.Clamp(actionsOut[3] * Time.fixedDeltaTime, -1f, 1f) > 0f;

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
                    EndEpisode();
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


                if (((targetDistY) < -0.1 && ((targetDistY) > (prevTargetDistY))) || ((targetDistY) > -0.08 && ((targetDistY) < (prevTargetDistY))))
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

            if (targetDistY < -0.35)
            {
                //Debug.Log("height is " + height);
                brain = 4;
                //ConfigBrain(brain);
                //Debug.Log(targetDistY);
                // Tool control (Movement)
                float rotX = Mathf.Clamp(actionsOut[0] * rotMult * Time.fixedDeltaTime, -1, 1);
                float rotY = Mathf.Clamp(actionsOut[1] * rotMult * Time.fixedDeltaTime, -1, 1);
                float insertion = Mathf.Clamp(actionsOut[2] * insMult * Time.fixedDeltaTime, -1, 1);
                bool graspActionVal = Mathf.Clamp(actionsOut[3] * Time.fixedDeltaTime, -1f, 1f) > 0f;

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
                    EndEpisode();
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


                if (((targetDistY) < -0.1 && ((targetDistY) > (prevTargetDistY))) || ((targetDistY) > -0.08 && ((targetDistY) < (prevTargetDistY))))
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

            if (Mathf.Abs(targetDistZ) > 0.6f && (targetDistY < -0.25 && targetDistY > -0.35))
            {
                brain = 5;
                //ConfigBrain(brain);
                //Debug.Log(targetDistZ);
                // Tool control (Movement)
                float rotX = Mathf.Clamp(actionsOut[0] * rotMult * Time.fixedDeltaTime, -1, 1);
                float rotY = Mathf.Clamp(actionsOut[1] * rotMult * Time.fixedDeltaTime, -1, 1);
                float insertion = Mathf.Clamp(actionsOut[2] * insMult * Time.fixedDeltaTime, -1, 1);
                bool graspActionVal = Mathf.Clamp(actionsOut[3] * Time.fixedDeltaTime, -1f, 1f) > 0f;

                // Set the position and rotation of the tool
                //fulcrum.transform.Rotate(new Vector3(rotX, 0, 0), Space.Self);
                scope.transform.Translate(new Vector3(0, 0, insertion), Space.Self);

                Vector3 fulcRot = fulcrum.transform.localRotation.eulerAngles;
                Vector3 insVec = scope.transform.localPosition;

                fulcRot.x = MathUtils.ClampAngle(fulcRot.x, rotLimit[0], rotLimit[1]);
                fulcRot.y = MathUtils.ClampAngle(fulcRot.y, rotLimit[2], rotLimit[3]);
                fulcRot.z = 0f;
                //fulcrum.transform.localRotation = Quaternion.Euler(fulcRot);

                if (insVec.z > insLimit[1])
                {
                    counter++;
                    Debug.Log(counter);
                    AddReward(-0.2f);

                }


                insVec.z = Mathf.Clamp(insVec.z, insLimit[0], insLimit[1]);
                scope.transform.localPosition = insVec;

                //// Establish a cube as boundaries - simulation end
                Vector3 tipPos = tip.transform.position;
                Vector3 upperBound = bounds.position + bounds.localScale / 2;
                Vector3 lowerBound = bounds.position - bounds.localScale / 2;

                if (tipPos.x <= lowerBound.x || tipPos.x >= upperBound.x || tipPos.y <= lowerBound.y || tipPos.y >= upperBound.y || tipPos.z <= lowerBound.z || tipPos.z >= upperBound.z || counter > 100)
                {
                    AddReward(-3f);
                    // Observation ends and reset the Agent
                    EndEpisode();
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
                    insLimit[0] = -3f; // Maximum insertion value Tail
                }


                currentTargetRelPos = targetPos - tip.transform.position;
                targetDistZ = targetPos.z - tip.transform.position.z;

                if (Mathf.Abs(targetDistZ) < Mathf.Abs(prevTargetDistZ))
                {
                    AddReward(0.5f);
                    prevTargetRelPos = currentTargetRelPos;
                    prevTargetDistX = targetDistX;
                    prevTargetDistY = targetDistY;
                    prevTargetDistZ = targetDistZ;
                    //Debug.Log(targetDistZ + " closer");
                }
                else
                {
                    AddReward(-0.2f);
                    prevTargetRelPos = currentTargetRelPos;
                    prevTargetRelPos = currentTargetRelPos;
                    prevTargetDistX = targetDistX;
                    prevTargetDistY = targetDistY;
                    prevTargetDistZ = targetDistZ;
                    //Debug.Log(targetDistZ + " farther");

                }

                if (Mathf.Abs(targetDistZ) < Mathf.Abs(minTargetDistZ))
                {
                    AddReward(0.5f);
                    minTargetRelPos = currentTargetRelPos;
                    prevTargetRelPos = currentTargetRelPos;
                    minTargetDistX = targetDistX;
                    minTargetDistY = targetDistY;
                    minTargetDistZ = targetDistZ;
                    prevTargetDistX = targetDistX;
                    prevTargetDistY = targetDistY;
                    prevTargetDistZ = targetDistZ;
                    //Debug.Log(targetDistZ + " new");
                }
                else
                {
                    AddReward(-0.2f);
                    prevTargetRelPos = currentTargetRelPos;
                    prevTargetDistX = targetDistX;
                    prevTargetDistY = targetDistY;
                    prevTargetDistZ = targetDistZ;
                    //Debug.Log(targetDistZ + " same");

                }
            }

            if (Mathf.Abs(targetDistX) > 0.2f && Mathf.Abs(targetDistZ) < 0.6f && (targetDistY < -0.25 && targetDistY > -0.35))
            {
                brain = 6;
                //ConfigBrain(brain);
                //Debug.Log(targetDistX);
                // Tool control (Movement)
                float rotX = Mathf.Clamp(actionsOut[0] * rotMult * Time.fixedDeltaTime, -1, 1);
                float rotY = Mathf.Clamp(actionsOut[1] * rotMult * Time.fixedDeltaTime, -1, 1);
                float insertion = Mathf.Clamp(actionsOut[2] * insMult * Time.fixedDeltaTime, -1, 1);
                bool graspActionVal = Mathf.Clamp(actionsOut[3] * Time.fixedDeltaTime, -1f, 1f) > 0f;

                // Set the position and rotation of the tool
                fulcrum.transform.Rotate(new Vector3(0, rotY, 0), Space.Self);
                //scope.transform.Translate(new Vector3(0, 0, insertion), Space.Self);

                Vector3 fulcRot = fulcrum.transform.localRotation.eulerAngles;
                Vector3 insVec = scope.transform.localPosition;

                //fulcRot.x = MathUtils.ClampAngle(fulcRot.x, rotLimit[0], rotLimit[1]);
                fulcRot.y = MathUtils.ClampAngle(fulcRot.y, rotLimit[2], rotLimit[3]);
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
                    AddReward(-3f);
                    // Observation ends and reset the Agent
                    EndEpisode();
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


                currentTargetRelPos = targetPos - tip.transform.position;
                targetDistX = targetPos.x - tip.transform.position.x;


                if (Mathf.Abs(targetDistX) < Mathf.Abs(prevTargetDistX))
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
                    prevTargetDistX = targetDistX;
                    prevTargetDistY = targetDistY;
                    prevTargetDistZ = targetDistZ;
                    //Debug.Log("farther");

                }

                if (Mathf.Abs(targetDistX) < Mathf.Abs(minTargetDistX))
                {
                    AddReward(0.5f);
                    minTargetRelPos = currentTargetRelPos;
                    prevTargetRelPos = currentTargetRelPos;
                    minTargetDistX = targetDistX;
                    minTargetDistY = targetDistY;
                    minTargetDistZ = targetDistZ;
                    prevTargetDistX = targetDistX;
                    prevTargetDistY = targetDistY;
                    prevTargetDistZ = targetDistZ;
                    //Debug.Log("new");
                }
                else
                {
                    AddReward(-0.2f);
                    prevTargetRelPos = currentTargetRelPos;
                    prevTargetDistX = targetDistX;
                    prevTargetDistY = targetDistY;
                    prevTargetDistZ = targetDistZ;
                    //Debug.Log("same");

                }
            }
        }

        AddReward(-0.1f);
    }

    /*private void ConfigBrain(int brain)
    {
        if (brain == 0)
        {
            SetModel("NewGoalZ", NewGoalZ);
        }
        if (brain == 1)
        {
            SetModel("NewGoalY", NewGoalY);
        }

        if (brain == 2)
        {
            SetModel("NewGoalX", NewGoalX);
        }

        if (brain == 3)
        {
            SetModel("NewTargetYUp", NewTargetYUp);
        }

        if (brain == 4)
        {
            SetModel("NewTargetYDown", NewTargetYDown);
        }

        if (brain == 5)
        {
            SetModel("NewTargetZ", NewTargetZ);
        }

        if (brain == 6)
        {
            SetModel("NewTargetX", NewTargetX);
        }
    }*/

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = Input.GetAxis("Vertical");
        actionsOut[1] = Input.GetAxis("Horizontal");
        actionsOut[2] = Input.GetAxis("Depth");
        actionsOut[3] = Input.GetAxis("Grasp"); // TODO: Calibrate the grasp, not working
    }


}