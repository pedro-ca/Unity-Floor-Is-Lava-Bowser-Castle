using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    Rigidbody rb;
    AudioSource audioSource;

    public AudioClip pickupCoinsAudioClip;

    //Movement
    private bool allowPlayerMovement = true;    //for disabling player movement on death
    public float movementForce = 0.1f;
    public float maxSpeed = 10f;

    //Jump
    public AudioClip jumpAudioClip;
    public float jumpForce = 3;

    private bool isGrounded = false;
    private bool jumpedPreviousFrame = false;   //fixes a bug that allows double jump becasue of coyote time and how unity handle OnCollision events

    public float coyoteTime = 0.2f;
    private float coyoteTimeCounter;

    public float jumpBufferTime = 0.2f;
    private float jumpBufferCounter;

    //Death
    public Camera mainCamera;
    public AudioClip deathAudioClip;    
    public Material deathMaterial;
    

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (allowPlayerMovement)
        {
            HandlePlayerJump();
            HandlePlayerMovement();
        }
    }

    void HandlePlayerJump()
    {
        jumpedPreviousFrame = false;

        //Coyote time allows player to jump a brief moment after being on air
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        //Jump buffer allows player to jump for a brief moment before touching the ground
        if (Input.GetButton("Jump"))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        if (coyoteTimeCounter > 0f && jumpBufferCounter > 0f )
        {
            rb.AddForce(0, jumpForce, 0, ForceMode.Impulse);
            
            audioSource.PlayOneShot(jumpAudioClip, 0.1f);
            isGrounded = false;
            jumpedPreviousFrame = true;
            coyoteTimeCounter = 0f;
            jumpBufferCounter = 0f;
        }

    }

    void HandlePlayerMovement()
    {
        //Player movement on the X axis
        if (Input.GetButton("Horizontal"))
        {
            float directionX = Input.GetAxis("Horizontal");
            rb.AddForce(movementForce * directionX, 0, 0, ForceMode.VelocityChange);
        }

        //Player movement on the Z axis
        if (Input.GetButton("Vertical"))
        {
            float directionZ = Input.GetAxis("Vertical");
            rb.AddForce(0, 0, movementForce  * directionZ, ForceMode.VelocityChange);
        }
    }

    private void HandlePlayerDeath()
    {
        Invoke("ResetScene", 2.25f);

        GetComponent<MeshRenderer>().material = deathMaterial;
        audioSource.Stop();
        audioSource.PlayOneShot(deathAudioClip, 1f);

        //makes the player face the camera
        rb.rotation = Quaternion.Euler(0,90,0);     
        rb.angularVelocity = Vector3.zero;

        rb.velocity = Vector3.zero;
        rb.AddForce(0, 25, 0, ForceMode.Impulse);
        allowPlayerMovement = false;
    }

    private void ResetScene() {
        mainCamera.cullingMask = 0;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


    void OnCollisionEnter(Collision collision)
    {
        if (allowPlayerMovement)
        {
            if (collision.gameObject.CompareTag("Death"))
            {
                HandlePlayerDeath();
            }
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (allowPlayerMovement)
        {
            var contactPoint = collision.contacts[0];
            if (collision.gameObject.CompareTag("Floor") && !jumpedPreviousFrame && contactPoint.normal.y >= 0.34)
            {
                isGrounded = true;
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Floor"))
        {
            isGrounded = false;
            jumpBufferCounter = 0f;
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (allowPlayerMovement)
        {
            if (collider.gameObject.CompareTag("Coin"))
            {
                audioSource.PlayOneShot(pickupCoinsAudioClip, 1f);

                //TODO Coin pickup
                Destroy(collider.gameObject);
            }
        }
    }
}
