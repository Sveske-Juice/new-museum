using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Freddy : MonoBehaviour
{
    public NavMeshAgent navAgent;
    public Transform playerTrans;

    private void Update()
    {
        navAgent.SetDestination(playerTrans.position);
    }
}
