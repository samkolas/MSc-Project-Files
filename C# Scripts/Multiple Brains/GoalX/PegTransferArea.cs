using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PegTransferArea : MonoBehaviour
{
    public GameObject bounds;
    public GameObject goal;
    public GameObject target;
    public GameObject agentA;

    private Vector3 startPos;
    // Start is called before the first frame update
    void Start()
    {
        startPos = goal.transform.position;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void AreaReset()
    {
        goal.transform.position = startPos;
    }
}
