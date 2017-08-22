﻿using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.ThirdPerson;

public class GruntEnemy : Enemy
{
    private AICharacterControl ai;
    private NavMeshAgent agent;
    private Transform player;
    private Animator animator;
    AnimatorStateInfo state;

    private float wanderRadius = 15;
    private float wanderTimer = 5;
    private float timer;

    private int testNumber;
    private float testStopDistance = 3.5f;

    private float attackTimer = 1.5f;
    private float timer2;

    private int attackHash;
    private int blockHash;

    private Vector3 gruntOrigin;

    public Shield shield;

    Rigidbody rb;

    private void Awake()
    {
        ai = GetComponent<AICharacterControl>();
        agent = GetComponent<NavMeshAgent>();
        player = FindObjectOfType<Player>().transform;
        animator = GetComponent<Animator>();
        attackHash = Animator.StringToHash("Base Layer.Attack");
        blockHash = Animator.StringToHash("Base Layer.Block");

        agent.stoppingDistance = 1.5f;
        gruntOrigin = transform.position;
    }

    private void Update()
    {
        if(SlowedTime)
        {
            animator.speed = TimeFreeze.FROZEN_TIME_SCALE;
            agent.speed = TimeFreeze.FROZEN_TIME_SCALE;
        }
        else
        {
            animator.speed = 1f;
            agent.speed = 1f;
        }

        float dist = Mathf.Abs(Vector3.Distance(transform.position, player.position));
        if (ai.target == null && dist < 10f)
            ai.SetTarget(player);

        /*
         * If there is no target,
         * Select a new position using RandomNavSphere and travel there
         * Repeat after every timer until target is found
         */ 
        else if (ai.target == null)
        {
            timer += Time.deltaTime;

            if (timer >= wanderTimer)
            {
                Vector3 newPos = RandomNavSphere(gruntOrigin, wanderRadius, 1);
                agent.SetDestination(newPos);
                timer = 0;
            }
        }
        
        /* 
         * Checks if the target is still within line of sight of the enemy
         * If not, remove the target
         */
        if (ai.target != null)
        {
            Vector3 aiPosition = transform.position;
            RaycastHit raycastHit;
            Vector3 rayDirection = ai.target.transform.position - aiPosition;

            if (Physics.Raycast(aiPosition, rayDirection, out raycastHit))
            {
                if (raycastHit.transform.tag != "Player")
                    ai.target = null;
            }
        }

        state = animator.GetCurrentAnimatorStateInfo(0);

        if (dist < agent.stoppingDistance + 0.5)
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, 2);

            foreach (Collider hit in hitColliders)
            {
                if (hit.transform.tag == "Enemy")
                {
                    if (Vector3.Distance(hit.transform.position, transform.position) < 5)
                    {
                        float step = (float)-0.1;
                        transform.position = Vector3.MoveTowards(transform.position, hit.transform.position, step);
                    }
                }

            }                
            

            state = animator.GetCurrentAnimatorStateInfo(0);
            if (state.fullPathHash != attackHash)
            {
                timer2 += Time.deltaTime;

                if (timer2 >= attackTimer)
                {
                    transform.LookAt(player); //AI tends to attack at air because wrong rotation
                    animator.SetTrigger("Attack");
                    timer2 = 0;
                }
            }
        }

        state = animator.GetCurrentAnimatorStateInfo(0);
        if (shield.IsBlocking == false && state.fullPathHash != blockHash && this.GetComponent<Health>().timeSinceDamageTaken < 1)
        {
            transform.LookAt(player);
            SetBlocking(true);
        }
        else if (shield.IsBlocking == true && this.GetComponent<Health>().timeSinceDamageTaken > 1)
        {
            SetBlocking(false);
        }

        AlertOthers();
    }

    private void AlertOthers()
    {
        if (ai.target != null)
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, 10f);

            foreach (Collider hit in hitColliders)
            {
                if (hit.tag == "Enemy")
                {
                    hit.GetComponent<GruntEnemy>().ai.SetTarget(player);
                }
            }
        }
    }

    private void SetBlocking(bool value)
    {
        shield.IsBlocking = value;
        animator.SetBool("Block", value);
    }

    private static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;

        randDirection += origin;

        NavMeshHit navHit;

        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);

        return navHit.position;
    }

    /// <summary>Checks if the entity can be attacked, and attacks them if so.</summary>
    /// <param name="target">The entity to attack.</param>
    /// <param name="damage">The damage to deal to the entity.</param>
    public override void Attack(Entity target, int damage)
    {
        base.Attack(target, damage);
    }
    /*
 *         if (dist < agent.stoppingDistance)
        {
            if (GetComponent<Player>().attackerList.Length == 2 && Random.Range(0, 100) < 50)
            {
                Collider[] hitColliders = Physics.OverlapSphere(transform.position, 2);

                foreach (Collider hit in hitColliders)
                {
                    for (int i = 0; i < GetComponent<Player>().attackerList.Length; i++)
                    {
                        if (hit.gameObject == GetComponent<Player>().attackerList[i])
                        {
                            if (Vector3.Distance(hit.transform.position, transform.position) < 5)
                            {
                                rb.AddForce(transform.right);
                            }
                        }
                    }
                }                
            }
            else if (GetComponent<Player>().attackerList.Length == 2 && Random.Range(0, 100) > 51)
            {
                Collider[] hitColliders = Physics.OverlapSphere(transform.position, 2);

                foreach (Collider hit in hitColliders)
                {
                    for (int i = 0; i < GetComponent<Player>().attackerList.Length; i++)
                    {
                        if (hit.gameObject == GetComponent<Player>().attackerList[i])
                        {
                            if (Vector3.Distance(hit.transform.position, transform.position) < 5)
                            {
                                rb.AddForce(-(transform.right));
                            }
                        }
                    }
                }  
            }
            else if (GetComponent<Player>().attackerList.Length < 2)
            {
                GetComponent<Player>().attackerList[GetComponent<Player>().attackerList.Length] = this.gameObject;
            }
 */
}



