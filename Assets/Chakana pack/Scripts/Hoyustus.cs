using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement; 
using Cinemachine;

public class Hoyustus : MonoBehaviour
{

   
    [Header("CineMachine")]
    [SerializeField] CinemachineVirtualCamera myVirtualCamera;
    CinemachineComponentBase myComponentBase;
    float myCameraDistance;
    [SerializeField] float myCameraSensivity = 0.006f; 

    public PlayerStateList pState;

    [Header("X Axis Movement")]
    [SerializeField] float walkSpeed = 25f;

    [Space(5)]

    [Header("Y Axis Movement")]
    [SerializeField] float jumpSpeed = 45;
    [SerializeField] float fallSpeed = 45;
    [SerializeField] int jumpSteps = 20;
    [SerializeField] int jumpThreshold = 7;
    [Space(5)]

    [Header("Attacking")]
    [SerializeField] float timeBetweenAttack = 0.4f;
    [SerializeField] Transform attackTransform; 
    [SerializeField] float attackRadius = 1;
    [SerializeField] Transform downAttackTransform;
    [SerializeField] float downAttackRadius = 1;
    [SerializeField] Transform upAttackTransform;
    [SerializeField] float upAttackRadius = 1;
    [SerializeField] LayerMask attackableLayer;
    [Space(5)]

    [Header("Recoil")]
    [SerializeField] int recoilXSteps = 4;
    [SerializeField] int recoilYSteps = 10;
    [SerializeField] float recoilXSpeed = 45;
    [SerializeField] float recoilYSpeed = 45;
    [Space(5)]

    [Header("Ground Checking")]
    [SerializeField] Transform groundTransform; 
    [SerializeField] float groundCheckY = 0.2f; 
    [SerializeField] float groundCheckX = 1;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] LayerMask enemyLayer;
    [SerializeField] LayerMask transitionLayer;
    [SerializeField] LayerMask transitionLayer1;
    [SerializeField] LayerMask transitionLayer2;
    [SerializeField] LayerMask transitionLayer3;
    [Space(5)]

    [Header("Roof Checking")]
    [SerializeField] Transform roofTransform; 
    [SerializeField] float roofCheckY = 0.2f;
    [SerializeField] float roofCheckX = 1; 
    [Space(5)]


    float timeSinceAttack;
    float xAxis;
    float yAxis;
    float grabity;
    int stepsXRecoiled;
    int stepsYRecoiled;
    int stepsJumped = 0;

    public float groundCheckRadius;

    Rigidbody2D rb;
    [SerializeField] Animator anim;

   

    [Header("Audio")]
    [SerializeField] AudioSource AudioWalking;
    [SerializeField] AudioSource AudioJump;
    [SerializeField] AudioSource AudioFall;
    [SerializeField] AudioSource AmbientFlute;

    [SerializeField] AudioSource AudioStep1;
    [SerializeField] AudioSource AudioStep2;
    [SerializeField] AudioSource AudioStep3;
    [SerializeField] AudioSource AudioStep4;
    [SerializeField] AudioSource AudioStep5;
    [SerializeField] AudioSource AudioStep6;
    [SerializeField] AudioSource AudioStep7;
    [SerializeField] AudioSource AudioStep8;

    [SerializeField] CharacterController Controller;

    [Header("Particles")]
    [SerializeField] ParticleSystem ParticleTestParticleTest =null;

    [Header("Movement")]
    public bool doubleJump = false;

    

    string nextPositionXPrefsName = "nextPositionX";
    string nextPositionYPrefsName = "nextPositionY";
    string firstRunPrefsName = "firstRun";
    string flipFlagPrefsName = "flipFlag";

    string escena;

   
    void Start()
    {
        //AudioWalking.Play();
        if (pState == null)
        {
            pState = GetComponent<PlayerStateList>();
        }

        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        grabity = rb.gravityScale;
        PlayAmbientFluteAudio();

        escena = SceneManager.GetActiveScene().name;
        //Debug.Log("Escena: " + escena);

        //Lee datos de memoria

        pState.firstRunFlag = PlayerPrefs.GetInt(firstRunPrefsName, 1);
        pState.flipFlag = PlayerPrefs.GetInt(flipFlagPrefsName, 0);

        if (pState.firstRunFlag == 0)
        {
            pState.x = PlayerPrefs.GetFloat(nextPositionXPrefsName, 0);
            pState.y = PlayerPrefs.GetFloat(nextPositionYPrefsName, 0);
            PlayerPrefs.SetInt(firstRunPrefsName, 1);
            rb.transform.position = new Vector2(pState.x, pState.y);
            Debug.Log("pState.flipFlag : " + pState.flipFlag);
            if (pState.flipFlag == 1) transform.localScale = new Vector2(-1, transform.localScale.y);
        }
    }

    
    void Update()
    {
        
        
        
        //AudioWalking.Play();
        GetInputs();
        //WalkingControl();
        Flip();
        Walk(xAxis);
        Recoil();
        //Attack();
    }

    void FixedUpdate()
    {
        
        if (pState.recoilingX == true && stepsXRecoiled < recoilXSteps)
        {
            stepsXRecoiled++;
        }
        else
        {
            StopRecoilX();
        }
        if (pState.recoilingY == true && stepsYRecoiled < recoilYSteps)
        {
            stepsYRecoiled++;
        }
        else
        {
            StopRecoilY();
        }
        if (Grounded())
        {
            StopRecoilY();
        }
        if (EventTransition())
        {
            //Debug.Log("Disparar Transición");
            LoadNextLevel();
        }
        ImproveJump();
        Jump();
        //WalkingControl();


    }

    private void ImproveJump()
    {
        if (rb.velocity.y < 0)
        {
            //rb.velocity += Vector2.up * Physics2D.gravity.y * (3f - 1) * Time.deltaTime;
            rb.velocity += Vector2.up * Physics2D.gravity.y * (8f - 1) * Time.deltaTime;
            //Debug.Log("_rigidbody.velocity.y<0");
        }
        else if (rb.velocity.y > 0 && !Input.GetButtonDown("Jump"))
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (11.5f - 1) * Time.deltaTime;
            //rb.velocity += Vector2.up * Physics2D.gravity.y * (100.5f - 1) * Time.deltaTime;
            //Debug.Log("_rigidbody.velocity.y >0 && !Input.GetButtonDown()");
        }

    }


    void Jump()
    {

        if (pState.jumping)
        {
            //Debug.Log("Entra a Jump");
            //Debug.Log("stepsJumped: " + stepsJumped);
            //Debug.Log("jumpSteps " + stepsJumped);

            
            //if (stepsJumped < jumpSteps && !Roofed())
            if (stepsJumped < jumpSteps)
                {
                rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);

                //TEST 10/01/2022
                //rb.velocity += Vector2.up * Physics2D.gravity.y * (20f - 1) * Time.deltaTime;

                stepsJumped++;
            }
            else
            {
                StopJumpSlow();
            }
        }

        
        if (rb.velocity.y < -Mathf.Abs(fallSpeed))
        {
            //Debug.Log("rb.velocity.y < -Mathf.Abs(fallSpeed)");
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -Mathf.Abs(fallSpeed), Mathf.Infinity));
        }
    }

    void Walk(float MoveDirection)
    {
        anim.SetBool("Walking", pState.walking); 

        //Rigidbody2D rigidbody2D = rb;
        //float x = MoveDirection * walkSpeed;
        //Vector2 velocity = rb.velocity;
        //rigidbody2D.velocity = new Vector2(x, velocity.y);
        if (!pState.recoilingX)
        {
            //Debug.Log("Entra a Walk");
           

            rb.velocity = new Vector2(MoveDirection * walkSpeed, rb.velocity.y);

            //Cinemachine Zoom
            if (escena == "14-Boss Room")
            {
                //Cinemachine Zoom
                if (myComponentBase == null)
                {
                    myComponentBase = myVirtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Body);
                }

                Debug.Log("CameraDistance: " + (myComponentBase as CinemachineFramingTransposer).m_CameraDistance);

                if ((myComponentBase as CinemachineFramingTransposer).m_CameraDistance <= 20f 
                    && (myComponentBase as CinemachineFramingTransposer).m_CameraDistance >= 8f)
                {
                    myCameraDistance = MoveDirection * 0.007f;

                    if (myComponentBase is CinemachineFramingTransposer)
                    {
                        (myComponentBase as CinemachineFramingTransposer).m_CameraDistance -= myCameraDistance;
                        //(myComponentBase as CinemachineFramingTransposer).m_ScreenY -= myCameraDistance;
                        
                    }
                }
                else
                    (myComponentBase as CinemachineFramingTransposer).m_CameraDistance = 8;
            }



            //AudioWalking.Pause();
            //AudioWalking.Play();
            //AudioWalking.Play();

            if (Mathf.Abs(rb.velocity.x) > 0)
            {
                pState.walking = true;
                
            }
            else
            {
                pState.walking = false;
                
            }
            if (xAxis > 0)
            {
                pState.lookingRight = true;
            }
            else if (xAxis < 0)
            {
                pState.lookingRight = false;
            }
           
            anim.SetBool("Walking", pState.walking);
        }

    }

    void Attack()
    {
        timeSinceAttack += Time.deltaTime;
        if (Input.GetButtonDown("Jump") && timeSinceAttack >= timeBetweenAttack)
        {
            timeSinceAttack = 0;
            //Attack Side
            if (yAxis == 0 || yAxis < 0 && Grounded())
            {
                //anim.SetTrigger("1");
                Collider2D[] objectsToHit = Physics2D.OverlapCircleAll(attackTransform.position, attackRadius, attackableLayer);
                if (objectsToHit.Length > 0)
                {
                    pState.recoilingX = true;
                }
                for (int i = 0; i < objectsToHit.Length; i++)
                {
                   
                    
                }
            }
           
            else if (yAxis > 0)
            {
                //anim.SetTrigger("2");
                Collider2D[] objectsToHit = Physics2D.OverlapCircleAll(upAttackTransform.position, upAttackRadius, attackableLayer);
                if (objectsToHit.Length > 0)
                {
                    pState.recoilingY = true;
                }
                for (int i = 0; i < objectsToHit.Length; i++)
                {
                    //Here is where you would do whatever attacking does in your script.
                    //In my case its passing the Hit method to an Enemy script attached to the other object(s).
                }
            }
           
            else if (yAxis < 0 && !Grounded())
            {
                //anim.SetTrigger("3");
                Collider2D[] objectsToHit = Physics2D.OverlapCircleAll(downAttackTransform.position, downAttackRadius, attackableLayer);
                if (objectsToHit.Length > 0)
                {
                    pState.recoilingY = true;
                }
                for (int i = 0; i < objectsToHit.Length; i++)
                {

                    


                    /*Instantiate(slashEffect1, objectsToHit[i].transform);
                    if (!(objectsToHit[i].GetComponent<EnemyV1>() == null))
                    {
                        objectsToHit[i].GetComponent<EnemyV1>().Hit(damage, 0, -YForce);
                    }

                    if (objectsToHit[i].tag == "Enemy")
                    {
                        Mana += ManaGain;
                    }*/
                }
            }

        }
    }

    void Recoil()
    {
        //since this is run after Walk, it takes priority, and effects momentum properly.
        if (pState.recoilingX)
        {
            if (pState.lookingRight)
            {
                rb.velocity = new Vector2(-recoilXSpeed, 0);
            }
            else
            {
                rb.velocity = new Vector2(recoilXSpeed, 0);
            }
        }
        if (pState.recoilingY)
        {
            if (yAxis < 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, recoilYSpeed);
                rb.gravityScale = 0;
            }
            else
            {
                rb.velocity = new Vector2(rb.velocity.x, -recoilYSpeed);
                rb.gravityScale = 0;
            }

        }
        else
        {
            rb.gravityScale = grabity;
        }
    }

    void Flip()
    {
        if (xAxis > 0)
        {
            transform.localScale = new Vector2(-1, transform.localScale.y);
        }
        else if (xAxis < 0)
        {
            transform.localScale = new Vector2(1, transform.localScale.y);
        }
    }

    void StopJumpQuick()
    {
        
        stepsJumped = 0;
        pState.jumping = false;
        //TEST 01/10/2022
        //rb.velocity += Vector2.up * Physics2D.gravity.y * (70f - 1) * Time.deltaTime;

        //rb.velocity = new Vector2(rb.velocity.x, -1);
        rb.velocity = new Vector2(rb.velocity.x, 1);
        anim.SetFloat("YVelocity", rb.velocity.y);
        anim.SetBool("Walking", false);
        anim.SetTrigger("Jumping");
    }

    void StopJumpSlow()
    {
        
        //TEST 01/10/2022
        //rb.velocity += Vector2.up * Physics2D.gravity.y * (70f - 1) * Time.deltaTime;
        stepsJumped = 0;
        pState.jumping = false;
        anim.SetFloat("YVelocity", rb.velocity.y);
        anim.SetTrigger("Jumping");
    }

    void StopRecoilX()
    {
        stepsXRecoiled = 0;
        pState.recoilingX = false;
    }

    void StopRecoilY()
    {
        stepsYRecoiled = 0;
        pState.recoilingY = false;
    }

    public bool Grounded()
    {
        
        //if (Physics2D.Raycast(groundTransform.position, Vector2.down, groundCheckY, groundLayer) || Physics2D.Raycast(groundTransform.position + new Vector3(-groundCheckX, 0), Vector2.down, groundCheckY, groundLayer) || Physics2D.Raycast(groundTransform.position + new Vector3(groundCheckX, 0), Vector2.down, groundCheckY, groundLayer))
        if(Physics2D.OverlapCircle(groundTransform.position, groundCheckRadius, groundLayer) || Physics2D.OverlapCircle(groundTransform.position, groundCheckRadius, enemyLayer))
        {
            anim.SetBool("Grounded", true);
            return true;
        }
        else
        {
            anim.SetBool("Grounded", false);
            return false;
            
        }

    }

    public bool EventTransition()
    {
        //isGrounded = Physics2D.OverlapCircle(groundTransform.position, groundCheckRadius, groundLayer);
        
        //if (Physics2D.Raycast(groundTransform.position, Vector2.down, groundCheckY, groundLayer) || Physics2D.Raycast(groundTransform.position + new Vector3(-groundCheckX, 0), Vector2.down, groundCheckY, groundLayer) || Physics2D.Raycast(groundTransform.position + new Vector3(groundCheckX, 0), Vector2.down, groundCheckY, groundLayer))
        if (Physics2D.OverlapCircle(groundTransform.position, groundCheckRadius, transitionLayer))
        {
            //Debug.Log("transitionLayer " + transitionLayer.ToString());
            return true;
            
        }
        else if (Physics2D.OverlapCircle(groundTransform.position, groundCheckRadius, transitionLayer1))
        {
            //Debug.Log("transitionLayer1 " + transitionLayer1.ToString());
            return true;
            
        }
        else if (Physics2D.OverlapCircle(groundTransform.position, groundCheckRadius, transitionLayer2))
        {
            Debug.Log("transitionLayer2 " + transitionLayer2.ToString());
            return true;

        }else if (Physics2D.OverlapCircle(groundTransform.position, groundCheckRadius, transitionLayer3))
        {
            //Debug.Log("transitionLayer3 " + transitionLayer3.ToString());
            return true;

        }
        else return false;

    }
    public void LoadNextLevel()
    {
        
        string transitionLayerExit;

        if (Physics2D.OverlapCircle(groundTransform.position, groundCheckRadius, transitionLayer))
        {
            //Debug.Log("transitionLayer " + transitionLayer.ToString());
            transitionLayerExit = "Transition";

        }
        else if (Physics2D.OverlapCircle(groundTransform.position, groundCheckRadius, transitionLayer1))
        {
            //Debug.Log("transitionLayer1 " + transitionLayer1.ToString());
            transitionLayerExit = "Transition1";

        }
        else if (Physics2D.OverlapCircle(groundTransform.position, groundCheckRadius, transitionLayer2))
        {
            //Debug.Log("transitionLayer2 " + transitionLayer2.ToString());
            transitionLayerExit = "Transition2";

        }
        else if (Physics2D.OverlapCircle(groundTransform.position, groundCheckRadius, transitionLayer3))
        {
            //Debug.Log("transitionLayer3 " + transitionLayer3.ToString());
            transitionLayerExit = "Transition3";

        }
        else transitionLayerExit = "";

        //Debug.Log("transitionLayerExit " + transitionLayerExit);

        escena = SceneManager.GetActiveScene().name;
        //Debug.Log("Escena: " + escena);
        //Debug.Log("transitionLayerExit: " + transitionLayerExit);

        if (escena == "00- StartRoom 1")
        {
            //Debug.Log("transitionLayerExit " + transitionLayerExit);

            if (transitionLayerExit == "Transition")
            {
                PlayerPrefs.SetFloat(nextPositionXPrefsName, -29.344f);
                PlayerPrefs.SetFloat(nextPositionYPrefsName, -9.077f);
                PlayerPrefs.SetInt(firstRunPrefsName, 0);
                PlayerPrefs.SetInt(flipFlagPrefsName, 0);
                SceneManager.LoadScene(1+1);
            }
                
            else if (transitionLayerExit == "Transition1")
            {
                PlayerPrefs.SetFloat(nextPositionXPrefsName, 29.576f);
                PlayerPrefs.SetFloat(nextPositionYPrefsName, -9.077f);
                PlayerPrefs.SetInt(firstRunPrefsName, 0);
                PlayerPrefs.SetInt(flipFlagPrefsName, 1);
                SceneManager.LoadScene(5+1);
            }
                
            
        }
        if (escena == "01-Level 1")
        {
            //Debug.Log("transitionLayerExit " + transitionLayerExit);

            if (transitionLayerExit == "Transition")
            {
                PlayerPrefs.SetFloat(nextPositionXPrefsName, -21.880f);
                PlayerPrefs.SetFloat(nextPositionYPrefsName, -9.0776f);
                PlayerPrefs.SetInt(firstRunPrefsName, 0);
                PlayerPrefs.SetInt(flipFlagPrefsName, 1);
                SceneManager.LoadScene(0+1);
            }
            else if (transitionLayerExit == "Transition1")
            {
                PlayerPrefs.SetFloat(nextPositionXPrefsName, -55.934f);
                PlayerPrefs.SetFloat(nextPositionYPrefsName, 14.672f);
                PlayerPrefs.SetInt(firstRunPrefsName, 0);
                PlayerPrefs.SetInt(flipFlagPrefsName, 1);
                SceneManager.LoadScene(2+1);
            }
            else if (transitionLayerExit == "Transition2")
            {
                PlayerPrefs.SetFloat(nextPositionXPrefsName, -137.45f);
                PlayerPrefs.SetFloat(nextPositionYPrefsName, 41.468f);
                PlayerPrefs.SetInt(firstRunPrefsName, 0);
                PlayerPrefs.SetInt(flipFlagPrefsName, 0);
                SceneManager.LoadScene(3+1);
            }
            else if (transitionLayerExit == "Transition3")
            {
                PlayerPrefs.SetFloat(nextPositionXPrefsName, -137.45f);
                PlayerPrefs.SetFloat(nextPositionYPrefsName, -9.0776f);
                PlayerPrefs.SetInt(firstRunPrefsName, 0);
                PlayerPrefs.SetInt(flipFlagPrefsName, 0);
                SceneManager.LoadScene(3+1);
            }
        }
        if (escena == "03-Room 3")
        {
            PlayerPrefs.SetFloat(nextPositionXPrefsName, -56.341f);
            PlayerPrefs.SetFloat(nextPositionYPrefsName, 14.672f);
            PlayerPrefs.SetInt(firstRunPrefsName, 0);
            PlayerPrefs.SetInt(flipFlagPrefsName, 0);
            SceneManager.LoadScene(1+1);
        }
        if (escena == "04-Level 2")
        {
            if (transitionLayerExit == "Transition")
            {
                PlayerPrefs.SetFloat(nextPositionXPrefsName, -133.46f);
                PlayerPrefs.SetFloat(nextPositionYPrefsName, -9.0776f);
                PlayerPrefs.SetInt(firstRunPrefsName, 0);
                PlayerPrefs.SetInt(flipFlagPrefsName, 1);
                SceneManager.LoadScene(1+1);
            }
            else if (transitionLayerExit == "Transition1")
            {
                PlayerPrefs.SetFloat(nextPositionXPrefsName, -132.69f);
                PlayerPrefs.SetFloat(nextPositionYPrefsName, 41.422f);
                PlayerPrefs.SetInt(firstRunPrefsName, 0);
                PlayerPrefs.SetInt(flipFlagPrefsName, 1);
                SceneManager.LoadScene(1+1);
            }
            else if (transitionLayerExit == "Transition2")
            {
                PlayerPrefs.SetFloat(nextPositionXPrefsName, 19.845f);
                PlayerPrefs.SetFloat(nextPositionYPrefsName, -8.851f);
                PlayerPrefs.SetInt(firstRunPrefsName, 0);
                PlayerPrefs.SetInt(flipFlagPrefsName, 0);
                SceneManager.LoadScene(4+1);
            }
        }
        if (escena == "05-Room GA1")
        {
            PlayerPrefs.SetFloat(nextPositionXPrefsName, -188.567f);
            PlayerPrefs.SetFloat(nextPositionYPrefsName, -9.077f);
            PlayerPrefs.SetInt(firstRunPrefsName, 0);
            PlayerPrefs.SetInt(flipFlagPrefsName, 1);
            SceneManager.LoadScene(3+1);
        }
        if (escena == "06- Room 6")
        {
            if (transitionLayerExit == "Transition")
            {
                PlayerPrefs.SetFloat(nextPositionXPrefsName, 19.160f);
                PlayerPrefs.SetFloat(nextPositionYPrefsName, -9.077f);
                PlayerPrefs.SetInt(firstRunPrefsName, 0);
                PlayerPrefs.SetInt(flipFlagPrefsName, 0);
                SceneManager.LoadScene(0+1);
            }
            else if (transitionLayerExit == "Transition1")
            {
                PlayerPrefs.SetFloat(nextPositionXPrefsName, -27.914f);
                PlayerPrefs.SetFloat(nextPositionYPrefsName, 38.672f);
                PlayerPrefs.SetInt(firstRunPrefsName, 0);
                PlayerPrefs.SetInt(flipFlagPrefsName, 1);
                SceneManager.LoadScene(6+1);
            }
                
            
        }
        if (escena == "07-Room 7")
        {
            if (transitionLayerExit == "Transition")
            {
                PlayerPrefs.SetFloat(nextPositionXPrefsName, 100.690f);
                PlayerPrefs.SetFloat(nextPositionYPrefsName, -9.077f);
                PlayerPrefs.SetInt(firstRunPrefsName, 0);
                PlayerPrefs.SetInt(flipFlagPrefsName, 0);
                SceneManager.LoadScene(5+1);
            }
            else if (transitionLayerExit == "Transition1")
            {
                PlayerPrefs.SetFloat(nextPositionXPrefsName, 63.045f);
                PlayerPrefs.SetFloat(nextPositionYPrefsName, -4.417f);
                PlayerPrefs.SetInt(firstRunPrefsName, 0);
                PlayerPrefs.SetInt(flipFlagPrefsName, 0);
                SceneManager.LoadScene(7+1);

            }

        }
        if (escena == "08-Room 8")
        {
            if (transitionLayerExit == "Transition")
            {
                PlayerPrefs.SetFloat(nextPositionXPrefsName, 45.333f);
                PlayerPrefs.SetFloat(nextPositionYPrefsName, -4.418f);
                PlayerPrefs.SetInt(firstRunPrefsName, 0);
                PlayerPrefs.SetInt(flipFlagPrefsName, 1);
                SceneManager.LoadScene(6+1);
            }
            else if (transitionLayerExit == "Transition1")
            {
                PlayerPrefs.SetFloat(nextPositionXPrefsName, 19.160f);
                PlayerPrefs.SetFloat(nextPositionYPrefsName, -9.077f);
                PlayerPrefs.SetInt(firstRunPrefsName, 0);
                PlayerPrefs.SetInt(flipFlagPrefsName, 1);
                SceneManager.LoadScene(8+1);
            }
            else if (transitionLayerExit == "Transition2")
            {
                PlayerPrefs.SetFloat(nextPositionXPrefsName, 119.625f);
                PlayerPrefs.SetFloat(nextPositionYPrefsName, -26.827f);
                PlayerPrefs.SetInt(firstRunPrefsName, 0);
                PlayerPrefs.SetInt(flipFlagPrefsName, 1);
                SceneManager.LoadScene(12+1);
            }
        }
        if (escena == "09-Room 9")
        {
            if (transitionLayerExit == "Transition")
            {
                PlayerPrefs.SetFloat(nextPositionXPrefsName, 70.635f);
                PlayerPrefs.SetFloat(nextPositionYPrefsName, -51.827f);
                PlayerPrefs.SetInt(firstRunPrefsName, 0);
                PlayerPrefs.SetInt(flipFlagPrefsName, 0);
                SceneManager.LoadScene(9+1);

                
            }
            else if (transitionLayerExit == "Transition1")
            {
                PlayerPrefs.SetFloat(nextPositionXPrefsName, 114.528f);
                PlayerPrefs.SetFloat(nextPositionYPrefsName, -51.827f);
                PlayerPrefs.SetInt(firstRunPrefsName, 0);
                PlayerPrefs.SetInt(flipFlagPrefsName, 0);
                SceneManager.LoadScene(7+1);

            }

        }
        if (escena == "10-Room 10 - 11")
        {
            if (transitionLayerExit == "Transition")
            {
                PlayerPrefs.SetFloat(nextPositionXPrefsName, -21.880f);
                PlayerPrefs.SetFloat(nextPositionYPrefsName, -9.0776f);
                PlayerPrefs.SetInt(firstRunPrefsName, 0);
                PlayerPrefs.SetInt(flipFlagPrefsName, 1);
                SceneManager.LoadScene(8+1);
            }
            else if (transitionLayerExit == "Transition1")
            {
               PlayerPrefs.SetFloat(nextPositionXPrefsName, -55.934f);
                PlayerPrefs.SetFloat(nextPositionYPrefsName, 14.672f);
                PlayerPrefs.SetInt(firstRunPrefsName, 0);
                PlayerPrefs.SetInt(flipFlagPrefsName, 1);
                SceneManager.LoadScene(10+1);
            }
            else if (transitionLayerExit == "Transition2")
            {
                PlayerPrefs.SetFloat(nextPositionXPrefsName, -21.880f);
                PlayerPrefs.SetFloat(nextPositionYPrefsName, -9.0776f);
                PlayerPrefs.SetInt(firstRunPrefsName, 0);
                PlayerPrefs.SetInt(flipFlagPrefsName, 1);
                SceneManager.LoadScene(11+1);
            }
        }
        if (escena == "12-Room 12")
        {
            PlayerPrefs.SetFloat(nextPositionXPrefsName, -0.653f);
            PlayerPrefs.SetFloat(nextPositionYPrefsName, -82.824f);
            PlayerPrefs.SetInt(firstRunPrefsName, 0);
            PlayerPrefs.SetInt(flipFlagPrefsName, 0);
            SceneManager.LoadScene(9+1);
        }
        if (escena == "13- SaveRoom")
        {
            //Debug.Log("transitionLayerExit " + transitionLayerExit);

            if (transitionLayerExit == "Transition")
            {
                PlayerPrefs.SetFloat(nextPositionXPrefsName, 70.211f);
                PlayerPrefs.SetFloat(nextPositionYPrefsName, -102.327f);
                PlayerPrefs.SetInt(firstRunPrefsName, 0);
                PlayerPrefs.SetInt(flipFlagPrefsName, 0);
                SceneManager.LoadScene(9+1);
            }

            else if (transitionLayerExit == "Transition1")
            {
                PlayerPrefs.SetFloat(nextPositionXPrefsName, -64.304f);
                PlayerPrefs.SetFloat(nextPositionYPrefsName, -103.677f);
                PlayerPrefs.SetInt(firstRunPrefsName, 0);
                PlayerPrefs.SetInt(flipFlagPrefsName, 1);
                SceneManager.LoadScene(13+1);
            }
         }
        if (escena == "13-Room 13")
        {
            PlayerPrefs.SetFloat(nextPositionXPrefsName, 151.409f);
            PlayerPrefs.SetFloat(nextPositionYPrefsName, -26.827f);
            PlayerPrefs.SetInt(firstRunPrefsName, 0);
            PlayerPrefs.SetInt(flipFlagPrefsName, 1);
            SceneManager.LoadScene(7+1);
        }
        if (escena == "14-Boss Room")
        {
            PlayerPrefs.SetFloat(nextPositionXPrefsName, 19.340f);
            PlayerPrefs.SetFloat(nextPositionYPrefsName, -9.077f);
            PlayerPrefs.SetInt(firstRunPrefsName, 0);
            PlayerPrefs.SetInt(flipFlagPrefsName, 1);
            SceneManager.LoadScene(11+1);
        }
    }

    public bool Roofed()
    {
        
        if (Physics2D.Raycast(roofTransform.position, Vector2.up, roofCheckY, groundLayer) || Physics2D.Raycast(roofTransform.position + new Vector3(roofCheckX, 0), Vector2.up, roofCheckY, groundLayer) || Physics2D.Raycast(roofTransform.position + new Vector3(roofCheckX, 0), Vector2.up, roofCheckY, groundLayer))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void GetInputs()
    {
        
        yAxis = Input.GetAxis("Vertical");
        xAxis = Input.GetAxis("Horizontal");

        
        if (yAxis > 0.25)
        {
            yAxis = 1;
        }
        else if (yAxis < -0.25)
        {
            yAxis = -1;
        }
        else
        {
            yAxis = 0;
        }

        if (xAxis > 0.25)
        {
            xAxis = 1;
        }
        else if (xAxis < -0.25)
        {
            xAxis = -1;
        }
        else
        {
            xAxis = 0;
        }

        anim.SetBool("Grounded", Grounded());
        anim.SetFloat("YVelocity", rb.velocity.y);

         
        //if (Input.GetButtonDown("Jump") && Grounded()) DNA 11/01/2022 SE AUMENTA VARIABLE DOUBLE JUMP 
            if (Input.GetButtonDown("Jump") && Grounded() || doubleJump==true)
        {
            //Debug.Log("Entra a Jumping :" + pState.jumping);
            pState.jumping = true;
            anim.SetTrigger("Jumping");
            anim.SetBool("Jump", Grounded());
            //AudioJump.Play();
        }

        if (!Input.GetButton("Jump") && stepsJumped < jumpSteps && stepsJumped > jumpThreshold && pState.jumping)
        {
            //Debug.Log("Entra a StopJumpQuick :" + pState.jumping);
            StopJumpQuick();
        }
        else if (!Input.GetButton("Jump") && stepsJumped < jumpThreshold && pState.jumping)
        {
            //Debug.Log("Entra a StopJumpSlow :" + pState.jumping);
            StopJumpSlow();
        }

    }

    void WalkingControl()
    {
        //Debug.Log("Entra a la sección de get button");

        if (Input.GetButtonDown("Horizontal"))// && Grounded() )//&& pState.walking == true)
        {
            //Debug.Log("Entra a la sección de get button Horizontal Input");
            if (Grounded())
            {
                //Debug.Log("Entra a la sección de get button Horizontal Input Grounded");

                AudioWalking.Play();
            }
            else
            {

                //Debug.Log("Entra a la sección de get button Horizontal Input Not Grounded");
                AudioWalking.Stop();
            }

        }
        else if (!Grounded())
        {
            if (Grounded())
            {
                //Debug.Log("Entra a la sección de get button Horizontal Input !Grounded Grounded");

                AudioWalking.Play();
            }
            else
            {

                //Debug.Log("Entra a la sección de get button Horizontal Input !Grounded !Grounded");
                AudioWalking.Stop();
            }
        }

        if (Input.GetButtonDown("Horizontal") && Grounded())
        {
            AudioWalking.Play();
        }

        if (Input.GetButtonUp("Horizontal"))
        {
            AudioWalking.Stop();
        }
        if (Input.GetButtonDown("Jump") && Grounded())
        {
            AudioWalking.Stop();
        }

    }

    void PlayJumpAudio()
    {
        AudioJump.Play();

    }
    void PlayFallAudio()
    {
        AudioFall.Play();

    }
    void PlayAmbientFluteAudio()
    {
        
        

    }
    void PlayAudioStep1()
    {
        AudioStep1.Play();
    }
    void PlayAudioStep2()
    {
        AudioStep2.Play();
    }
    void PlayAudioStep3()
    {
        AudioStep3.Play();
    }
    void PlayAudioStep4()
    {
        AudioStep4.Play();
    }
    void PlayAudioStep5()
    {
        AudioStep5.Play();
    }
    void PlayAudioStep6()
    {
        AudioStep6.Play();
    }
    void PlayAudioStep7()
    {
        AudioStep7.Play();
    }
    void PlayAudioStep8()
    {
        AudioStep8.Play();
    }
    void PlayParticles()
    {
        ParticleTestParticleTest.Play();
    }

void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackTransform.position, attackRadius);
        Gizmos.DrawWireSphere(downAttackTransform.position, downAttackRadius);
        Gizmos.DrawWireSphere(upAttackTransform.position, upAttackRadius);
        //Gizmos.DrawWireCube(groundTransform.position, new Vector2(groundCheckX, groundCheckY));

        Gizmos.DrawLine(groundTransform.position, groundTransform.position + new Vector3(0, -groundCheckY));
        Gizmos.DrawLine(groundTransform.position + new Vector3(-groundCheckX, 0), groundTransform.position + new Vector3(-groundCheckX, -groundCheckY));
        Gizmos.DrawLine(groundTransform.position + new Vector3(groundCheckX, 0), groundTransform.position + new Vector3(groundCheckX, -groundCheckY));

        Gizmos.DrawLine(roofTransform.position, roofTransform.position + new Vector3(0, roofCheckY));
        Gizmos.DrawLine(roofTransform.position + new Vector3(-roofCheckX, 0), roofTransform.position + new Vector3(-roofCheckX, roofCheckY));
        Gizmos.DrawLine(roofTransform.position + new Vector3(roofCheckX, 0), roofTransform.position + new Vector3(roofCheckX, roofCheckY));
    }
}