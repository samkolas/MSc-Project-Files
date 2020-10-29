using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PegTransferArea : MonoBehaviour
{
    public GameObject bounds;
    public GameObject target;
    public GameObject goal;
    public GameObject agentA;

    public bool inGoal;
    private Vector3 initGoalPos;
    private Vector3[] positions;



    // Start is called before the first frame update
    public void Start()
    {


        GameObject[] allGoals = GameObject.FindGameObjectsWithTag("Goal");
        positions = new Vector3[allGoals.Length];
        int g = 0;
        foreach (GameObject goal in allGoals)
        {
            positions[g] = goal.transform.position;
            g++;
        }

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void AreaReset()
    {
        GameObject[] allGoals = GameObject.FindGameObjectsWithTag("Goal");
        int i = 0;
        foreach (GameObject goal in allGoals)
        {
            goal.transform.position = positions[i];
            i++;
        }
    }
}


