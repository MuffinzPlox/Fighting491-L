﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(InputController), typeof(SoundController))]
public class ComputerAI : MonoBehaviour
{
    /// <summary>
    /// Notes:
    /// -Input Manager
    /// -int PlayerNumber
    /// -Transform Enemy
    /// -Health Script
    /// -bool for each animation state
    /// 
    /// start, update, fixed update, ongroundcheck, updateanimator, oncollision enter& exit
    /// 
    /// possible switch to brawler style
    /// move, jump, crouch
    /// Mid Attack, Mid Power Attack (Power is slower but does more damage and is longer range)
    /// Low Attack, Low Power Attack
    /// Air Attack, Air Power Attack
    /// Block (Blocks mid & high), Low Block (blocks low & mid)
    /// Grab
    /// Special Attack
    /// 
    /// Transition from attack <-> power needs fixing
    /// </summary>

    public enum Direction
    {
        left,
        right
    };

    public Transform Computer;
    public Transform Player;
    public float Speed;
    public float JumpForce;
    bool crouch = false;
    bool onGround = false;
    [HideInInspector]
    public bool Damaged = false;
    [HideInInspector]
    public bool Nullify = false;
    private float Distance;
    float attackTimer;
    float attackCooldown = 3.0f;
    private InputController PlayerScript;

    Animator anim;
    Rigidbody2D body;
    //max speed, jump duration?, attack rate?
    //Vector3 movement
    //bool for states

    InputController input;
    SoundController sound;

    private GameObject headBouncer;

    public Direction direct = Direction.left;

    void Start()
    {
        StartCoroutine(WaitFunction());
        Computer = transform;
        Player = GameObject.FindGameObjectWithTag("Player").transform;
        GameObject test = GameObject.FindGameObjectWithTag("Player");
        PlayerScript = test.GetComponent<InputController>();

        body = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        input = GetComponent<InputController>();
        sound = GetComponent<SoundController>();

        //to bounce off enemy head if they land on top
        headBouncer = new GameObject("Head Bouncer");
        headBouncer.tag = "HeadBouncer";
        headBouncer.AddComponent<HeadBouncerScript>();
        headBouncer.AddComponent<BoxCollider2D>();
        headBouncer.GetComponent<BoxCollider2D>().isTrigger = true;
        headBouncer.transform.parent = gameObject.transform;
        headBouncer.transform.position = new Vector3(
                     transform.position.x,
                     transform.position.y + 1f,
                     transform.position.z);
    }

    void AttackSounds()
    {
        if (input.Attack)
        {
            sound.Attack();
        }

        if (input.Power)
        {
            sound.Power();
        }
    }

    void OrientDirection()
    {
        //if enemy pos < pos::localScale = -1,1,1 else 1,1,1(flip using scale)
        if (direct == Direction.left)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (direct == Direction.right)
        {
            transform.localScale = Vector2.one;
        }
    }

    public void Jump()
    {
        Vector2 velocity = body.velocity;
        velocity.y = Mathf.Sqrt(2f * JumpForce * -Physics2D.gravity.y);
        body.velocity = velocity;
        sound.Jump();
    }

    void Movement()
    {
        int JumpChance = Random.Range(1, 100);

        if (Player.position.y > 1.3)
        {
            if (JumpChance < 5)
            {
                Debug.Log("Jumping");
                input.Vertical = 1;
                if (input.Vertical > 0 && onGround)
                {
                    Jump();
                }
                input.Vertical = 0;
            }
        }

        if (!crouch && !anim.GetCurrentAnimatorStateInfo(0).IsName("attack") && !anim.GetCurrentAnimatorStateInfo(0).IsName("block"))
        {
            if (input.Horizontal != 0)
            {
                Computer.Translate(Vector2.right * input.Horizontal * Speed * Time.deltaTime);
            }
        }

        Computer.rotation = Quaternion.Euler(Vector3.zero);
    }

    void Follow()
    {
        Distance = Mathf.Abs(Vector3.Distance(Player.position, Computer.position));

        if (Computer.position.x > Player.position.x && Distance < 10)
        {
            input.Horizontal = -1;
            direct = Direction.left;
        }
        else if (Computer.position.x < Player.position.x && Distance < 10)
        {
            input.Horizontal = 1;
            direct = Direction.right;
        }

        int RandomAttack = Random.Range(0, 100);

        if (Distance < 3.5)
        {
            if (RandomAttack < 50)
                input.Attack = true;
            else
                input.Power = true;
        }
    }

    void Defend()
    {
        int blockChance = Random.Range(1, 100);
        if ((PlayerScript.Attack == true
            || PlayerScript.Power == true) &&
            blockChance < 50)
        {
            Debug.Log("Blocking");
            input.Horizontal = 0;
            input.Attack = false;
            input.Power = false;
            input.Block = true;
            Nullify = true;
        }
        else
        {
            input.Block = false;
            Nullify = false;
        }
    }

    void UpdateAnimator()
    {
        anim.SetBool("Crouch", crouch);
        anim.SetBool("Ground", onGround);
        anim.SetBool("Attack", input.Attack);
        anim.SetBool("Power", input.Power);
        anim.SetBool("Block", input.Block);
        anim.SetBool("Damage", Damaged);
        anim.SetFloat("Speed", Mathf.Abs(input.Horizontal));
    }

    void Update()
    {
        AttackSounds();
        OrientDirection();
        Movement();
        InvokeRepeating("Follow", 0.0f, 3.0f);
        InvokeRepeating("Defend", 0.0f, 3.0f);
        UpdateAnimator();

        Damaged = false;
        if (input.Block == false)
        {
            Nullify = false;
        }
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            onGround = true;
            body.velocity = Vector2.zero;
        }

        if (collision.gameObject.tag == "Player" && input.Attack == false && input.Power == false)
        {
            Debug.Log("Attacking!");
            int attackSelection = Random.Range(1, 100);
            if (attackSelection < 50)
            {
                input.Attack = true;
            }
            else
            {
                input.Power = true;
            }
            attackTimer = attackCooldown;
        }
        //StartCoroutine(AttackCheck());
    }

    public void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            onGround = false;
        }
    }

    IEnumerator AttackCheck()
    {
        while (true)
        {
            if (input.Attack || input.Power)
            {
                if (attackTimer > 1.0f)
                {
                    Debug.Log("Decreasing");
                    attackTimer -= 1.0f;
                }

                if (attackTimer <= 1.0f)
                {
                    input.Attack = false;
                    input.Power = false;
                    Debug.Log("Done attacking!");
                }
            }
            yield return new WaitForSeconds(1);
        }
    }

    IEnumerator WaitFunction()
    {
        yield return new WaitForSeconds(3);
    }
}