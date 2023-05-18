using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Bullet 스크립트 상속
public class BossMissile : Bullet
{
    public Transform target;
    NavMeshAgent nav;

    void Awake()
    {
        nav = GetComponent<NavMeshAgent>();
    }
    void Update()
    {
        nav.SetDestination(target.position);  // 플레이어 유도하는 미사일
    }
}
