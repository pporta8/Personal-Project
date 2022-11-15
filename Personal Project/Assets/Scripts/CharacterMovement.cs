using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class CharacterMovement : NetworkBehaviour
{
    [Header("Movement")]
    private Vector3 playerMovementInput;
    private Vector2 playerMouseInput;
    private float xRot;

    [SerializeField] private Camera playerCamera;
    [SerializeField] private float speed;
    [SerializeField] private float sensitivity;

    [Header("Network Movement")]
    [SerializeField] private NetworkVariable<float> horizontalNMovement = new NetworkVariable<float>();
    [SerializeField] private NetworkVariable<float> verticalNMovement = new NetworkVariable<float>();


    [Header("Sprint")]
    [SerializeField] float walkSpeed;
    [SerializeField] float sprintSpeed;

    [Header("Jump")]
    [SerializeField] private float jumpForce;


    [Header("GroundDetection")]
    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask groundMask;
    bool isGrounded;
    float groundDistance = 0.4f;

    [Header("KeyBinds")]
    [SerializeField] KeyCode jumpKey = KeyCode.Space;
    [SerializeField] KeyCode sprintKey = KeyCode.LeftShift;

    public GameObject bullet, spawner;
    public float force;
    public int ammo, clipSize, score, maxAmmo;
    public Text ammoT, maxAmmoT, scoreT;

    public int health;
    public AudioSource shooting;
    public AmmoDrop ammoDrop;

    Rigidbody rb;

    void Awake()
    {
        rb = this.GetComponent<Rigidbody>();
        playerCamera = GetComponentInChildren<Camera>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        ammo = clipSize;
        ammoT.text = ammo.ToString();
        maxAmmoT.text = maxAmmo.ToString();
        scoreT.text = score.ToString();
    }

    // Update is called once per frame
    void Update()
    {

        if(IsServer)
        {
            UpdateServer();
        }

        if (IsHost)
        {
            UpdateServer();
        }

        if (IsClient && IsOwner)
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

            MoveCamera();
            Movement();
            ControlSpeed();

            if (Input.GetKeyDown(jumpKey) && isGrounded)
            {
                Jump();
            }

            if (Input.GetKeyDown(KeyCode.Mouse0) && ammo > 0)
            {
                InstantiateProjectile(spawner.transform);
                shooting.Play();
                ammo--;
                ammoT.text = ammo.ToString();
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                ammo = clipSize;
                maxAmmo = maxAmmo - ammo;
                maxAmmoT.text = maxAmmo.ToString();
                ammoT.text = ammo.ToString();
            }

        }

        if(health <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void UpdateServer()
    {
       //transform.position = new Vector3(transform.position.x + verticalNMovement.Value, transform.position.y, transform.position.z + horizontalNMovement.Value);
    }

    void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    void Movement()
    {
        playerMovementInput = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        playerMouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        Vector3 MoveVector = transform.TransformDirection(playerMovementInput) * speed;
        rb.velocity = new Vector3(MoveVector.x, rb.velocity.y, MoveVector.z);


        UpdateClientPositionServerRpc(playerMovementInput.z, playerMovementInput.x);
    }

    void ControlSpeed()
    {
        if(Input.GetKeyDown(sprintKey)&& isGrounded)
        {
            speed = sprintSpeed;
        }
        else
        {
            speed = walkSpeed;
        }
    }

    private void MoveCamera()
        {
            xRot -= playerMouseInput.y * sensitivity;

            transform.Rotate(0f, playerMouseInput.x * sensitivity, 0f);
            xRot = Mathf.Clamp(xRot, -90, 90);
            playerCamera.transform.localRotation = Quaternion.Euler(xRot, 0f, 0f);


        }
    void InstantiateProjectile(Transform firepoint)
    {
        var projectileObj = Instantiate(bullet, firepoint.position, Quaternion.identity) as GameObject;
        Vector3 direction = transform.forward * force;
        projectileObj.GetComponent<Rigidbody>().AddForce(direction, ForceMode.Impulse);

    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Enemy")
        {
            health--;
        }

        if(other.tag == "Ammo")
        {
            Debug.Log("touched");
            ammoDrop = other.GetComponentInParent<AmmoDrop>();
            ammoDrop.pickedUp = true;
            maxAmmo += 10;
        }
    }

    [ServerRpc]
    public void UpdateClientPositionServerRpc(float horizontal, float vertical)
    {
        horizontalNMovement.Value = horizontal;
        verticalNMovement.Value = vertical;
    }


}
