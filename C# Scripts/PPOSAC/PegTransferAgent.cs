using DefKit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using Barracuda;

public class PegTransferAgent : Agent
{
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
    public void Start()
    {
        
    }

    public void Awake()
    {
        PegTransferArea area = areaObject.GetComponent<PegTransferArea>();

    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void AgentReset()
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
        counter = 0;
    }

    void InitialiseGoal()
    {
        PegTransferArea area = areaObject.GetComponent<PegTransferArea>();
        currentGoal = area.goal;
        currentTarget = area.target;
        goalScript = currentGoal.GetComponent<PegTransferGoal>();
        goalScript.enabled = true;
        goalScript.SetGoal();
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
        // Read the parameters from the Area Game Object
        PegTransferArea area = areaObject.GetComponent<PegTransferArea>();
        Transform bounds = area.bounds.GetComponent<Transform>();
        goalScript = area.goal.GetComponent<PegTransferGoal>();

        if (tip.transform.childCount == 0)
        {
            Vector3 currentGoalPos = currentGoal.transform.position;

            goalDistX = currentGoalPos.x - tip.transform.position.x;
            goalDistY = currentGoalPos.y - tip.transform.position.y;
            goalDistZ = currentGoalPos.z - tip.transform.position.z;

            if (goalDistZ > 0.2f || goalDistZ < 0)
            {
                // Tool control (Movement)
                float rotX = Mathf.Clamp(act[0] * rotMult * Time.fixedDeltaTime, -1, 1);
                float rotY = Mathf.Clamp(act[1] * rotMult * Time.fixedDeltaTime, -1, 1);
                float insertion = Mathf.Clamp(act[2] * insMult * Time.fixedDeltaTime, -1, 1);
                bool graspActionVal = Mathf.Clamp(act[3] * Time.fixedDeltaTime, -1f, 1f) > 0f;

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

                if (((goalDistZ) < 0 && ((goalDistZ) > (prevGoalDistZ))) || ((goalDistZ) > 0.2 && ((goalDistZ) < (prevGoalDistZ))))
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


            if (Mathf.Abs(goalDistY) > 0.2f && (goalDistZ < 0.2f && goalDistZ > 0))
            {
                //Debug.Log(goalDistY);
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


            if (Mathf.Abs(goalDistX) > 0.1f && Mathf.Abs(goalDistY) < 0.2f && (goalDistZ < 0.2f && goalDistZ > 0))
            {
                // Tool control (Movement)
                float rotX = Mathf.Clamp(act[0] * rotMult * Time.fixedDeltaTime, -1, 1);
                float rotY = Mathf.Clamp(act[1] * rotMult * Time.fixedDeltaTime, -1, 1);
                float insertion = Mathf.Clamp(act[2] * insMult * Time.fixedDeltaTime, -1, 1);
                bool graspActionVal = Mathf.Clamp(act[3] * Time.fixedDeltaTime, -1f, 1f) > 0f;

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
                    Done();
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
            targetDistX = targetPos.x - currentGoal.transform.position.x;
            targetDistY = height - currentGoal.transform.position.y;
            targetDistZ = targetPos.z - currentGoal.transform.position.z;

            if (targetDistY > -0.25 || targetDistY < -0.35)
            {
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


                currentTargetRelPos = targetPos - currentGoal.transform.position;
                targetDistY = height - currentGoal.transform.position.y;


                if (((targetDistY) < -0.35 && ((targetDistY) > (prevTargetDistY))) || ((targetDistY) > -0.25 && ((targetDistY) < (prevTargetDistY))))
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

            
            if (Mathf.Abs(targetDistX) > 0.1f && (targetDistY < -0.25 && targetDistY > -0.35))
            {
                //Debug.Log(targetDistX);
                // Tool control (Movement)
                float rotX = Mathf.Clamp(act[0] * rotMult * Time.fixedDeltaTime, -1, 1);
                float rotY = Mathf.Clamp(act[1] * rotMult * Time.fixedDeltaTime, -1, 1);
                float insertion = Mathf.Clamp(act[2] * insMult * Time.fixedDeltaTime, -1, 1);
                bool graspActionVal = Mathf.Clamp(act[3] * Time.fixedDeltaTime, -1f, 1f) > 0f;

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


                currentTargetRelPos = targetPos - currentGoal.transform.position;
                targetDistX = targetPos.x - currentGoal.transform.position.x;


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

            if (Mathf.Abs(targetDistZ) > 0.1f && (targetDistY < -0.25 && targetDistY > -0.35) && Mathf.Abs(targetDistX) < 0.1f)
            {
                //Debug.Log(targetDistZ);
                // Tool control (Movement)
                float rotX = Mathf.Clamp(act[0] * rotMult * Time.fixedDeltaTime, -1, 1);
                float rotY = Mathf.Clamp(act[1] * rotMult * Time.fixedDeltaTime, -1, 1);
                float insertion = Mathf.Clamp(act[2] * insMult * Time.fixedDeltaTime, -1, 1);
                bool graspActionVal = Mathf.Clamp(act[3] * Time.fixedDeltaTime, -1f, 1f) > 0f;

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


                currentTargetRelPos = targetPos - currentGoal.transform.position;
                targetDistZ = targetPos.z - currentGoal.transform.position.z;

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

            
        }

        AddReward(-0.1f);
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