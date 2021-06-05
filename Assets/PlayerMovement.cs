using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    float movementSpeed = 6f;
    [SerializeField]
    float jumpHeight = 3f;
    [SerializeField]
    Transform groundCheck;
    [SerializeField]
    LayerMask groundMask;
    [SerializeField]
    float groundDistance = 0.2f;
    [SerializeField]
    float gravity = -18f;
    [SerializeField]
    float sprintSpeed = 1.5f;

    CharacterController controller;
    Vector3 velocity;
    bool isGrounded;

    float speed;


    void Awake() => controller = GetComponent<CharacterController>();

    void Start() => speed = movementSpeed;

    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        if (isGrounded && Input.GetKeyDown(KeyCode.LeftShift))
            speed = sprintSpeed * movementSpeed;

        if (Input.GetKeyUp(KeyCode.LeftShift))
            speed = movementSpeed;

        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        Vector3 moveVector = transform.right * x + transform.forward * y;

        controller.Move(speed * Time.deltaTime * moveVector);

        if (Input.GetButton("Jump") && isGrounded && velocity.y < 0)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);


    }
}
