using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoyustusContoller : MonoBehaviour
{
    //variables
    public float speed = 8.5f;
    public float jumpForce = 10.5f;

    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundCheckRadius;
    public bool isGrounded = true;   
    

    private Rigidbody2D _rigidbody;

    //movement variables
    private Vector2 _movement;
    private bool _isFacingLeft = false;


    //awake
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        //Movement
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        _movement = new Vector2(horizontalInput, 0f);

        //Flip player
        if (horizontalInput<0f && _isFacingLeft==true)
        { Flip(); }
        else if (horizontalInput>0f && _isFacingLeft == false)
            { Flip(); }

        //Debug.Log("Is Grounded: " + isGrounded);
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        //Debug.Log("Is Grounded: " + isGrounded);

        //Jump player
        ImproveJump();
        if (isGrounded) { 
            if (Input.GetButtonDown("Jump") || Input.GetButtonDown("Fire1"))
            {
                _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, 0);
                _rigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse); }

            //if (Input.GetButtonDown("Fire1"))
            //{ _rigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse); }
        }

    }
    private void ImproveJump()
    {
        if (_rigidbody.velocity.y<0)
        {
            _rigidbody.velocity += Vector2.up * Physics2D.gravity.y * (3.5f - 1) * Time.deltaTime;
            Debug.Log("_rigidbody.velocity.y<0");
        } 
        else if (_rigidbody.velocity.y >0 && !Input.GetButtonDown("Jump"))
        {
            _rigidbody.velocity += Vector2.up * Physics2D.gravity.y * (0.9f - 1) * Time.deltaTime;
            Debug.Log("_rigidbody.velocity.y >0 && !Input.GetButtonDown()");
        }
        
    }



   
    private void FixedUpdate()
    {
        float horizontalVelocity = _movement.normalized.x * speed;
        _rigidbody.velocity = new Vector2(horizontalVelocity, _rigidbody.velocity.y);
    }

    private void Flip()
    {
        _isFacingLeft = !_isFacingLeft;
        float localScaleX = transform.localScale.x;
        localScaleX = localScaleX * -1f;
        transform.localScale = new Vector3(localScaleX, transform.localScale.y, transform.localScale.z);
    }
}
