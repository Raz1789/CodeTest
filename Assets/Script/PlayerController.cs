using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {

    private Rigidbody rBody;
    private CapsuleCollider playerCollider;
    private Camera headCamera;

    [Header("Movement Variables")]
    public float walkSpeed = 1f;
    public float SprintModifier = 1.5f;
    private bool isSprinting;

    [Header("Looking Variables")]
    public float turnSpeed = 1f;
    [Tooltip("Look angle cap considering forward 0")]
    public float lookAngleCap = 85f;

    [Header("Crouch Variables")]
    public float crouchHeightModifier = 0.5f;
    public float crouchSpeedModifier = 0.5f;
    private float originalHeight;
    private bool isCrouching;

    [Header("Climb Variables")]
    public float interactionDistance = 2f;
    public float climbSpeed = 2.5f;
    public GameObject interactionText;
    private bool isClimbing = false;


	// Use this for initialization
	void Start () {
        rBody = GetComponent<Rigidbody>();
        playerCollider = GetComponent<CapsuleCollider>();
        headCamera = GetComponentInChildren<Camera>();
        originalHeight = playerCollider.height;
        Cursor.visible = false;
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        Vector3 newPosition = transform.position;
        Vector3 oldPosition = transform.position;

        if (!isClimbing)
        {

            isSprinting = Input.GetKey(KeyCode.LeftShift);
            isCrouching = !isSprinting && Input.GetKey(KeyCode.C);

            //Forward / Backward Movement
            if (Input.GetKey(KeyCode.W))
            {
                if (isSprinting)
                {
                    newPosition += transform.forward * walkSpeed * SprintModifier * Time.deltaTime;
                }
                else if (isCrouching)
                {
                    newPosition += transform.forward * walkSpeed * crouchSpeedModifier * Time.deltaTime;
                }
                else
                {
                    newPosition += transform.forward * walkSpeed * Time.deltaTime;
                }
            }
            else if (Input.GetKey(KeyCode.S))
            {
                if (isSprinting)
                {
                    newPosition -= transform.forward * walkSpeed * SprintModifier * Time.deltaTime;
                }
                else if (isCrouching)
                {
                    newPosition -= transform.forward * walkSpeed * crouchSpeedModifier * Time.deltaTime;
                }
                else
                {
                    newPosition -= transform.forward * walkSpeed * Time.deltaTime;
                }
            }

            //Straft Right /Left Movement
            if (Input.GetKey(KeyCode.D))
            {
                if (isSprinting)
                {
                    newPosition += transform.right * walkSpeed * SprintModifier * Time.deltaTime;
                }
                else if (isCrouching)
                {
                    newPosition += transform.right * walkSpeed * crouchSpeedModifier * Time.deltaTime;
                }
                else
                {
                    newPosition += transform.right * walkSpeed * Time.deltaTime;
                }
            }
            else if (Input.GetKey(KeyCode.A))
            {
                if (isSprinting)
                {
                    newPosition -= transform.right * walkSpeed * SprintModifier * Time.deltaTime;
                }
                else if (isCrouching)
                {
                    newPosition -= transform.right * walkSpeed * crouchSpeedModifier * Time.deltaTime;
                }
                else
                {
                    newPosition -= transform.right * walkSpeed * Time.deltaTime;
                }
            }

            //Crouch
            if (isCrouching && playerCollider.height == originalHeight)
            {
                playerCollider.height *= crouchHeightModifier;

            }
            else if (!isCrouching && playerCollider.height != originalHeight)
            {
                playerCollider.height = originalHeight;
            }

        } else
        {
            if (Input.GetKey(KeyCode.W))
            {
                newPosition += transform.up * climbSpeed * Time.deltaTime;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                newPosition -= transform.up * climbSpeed * Time.deltaTime;
            }
        }

        transform.position = newPosition;

        //Looking Horizontal
        transform.Rotate(new Vector3(0f, Input.GetAxis("Mouse X") * turnSpeed * Time.deltaTime, 0f));

        //Looking Vertical
        Quaternion newRotation = headCamera.transform.rotation;
        newRotation *= Quaternion.Euler(-Input.GetAxis("Mouse Y") * turnSpeed * Time.deltaTime, 0f, 0f);
        float currentEulerX = newRotation.eulerAngles.x < 180 ? newRotation.eulerAngles.x : newRotation.eulerAngles.x - 360;
        float newEulerX = Mathf.Clamp(currentEulerX, -lookAngleCap, lookAngleCap);
        newRotation.eulerAngles = new Vector3(newEulerX, newRotation.eulerAngles.y, newRotation.eulerAngles.z);
        headCamera.transform.rotation = newRotation;

        RaycastHit hit;
        Ray rayTop = new Ray(headCamera.transform.position, headCamera.transform.forward);
        Ray rayBottom = new Ray((headCamera.transform.position - transform.up * 2f), headCamera.transform.forward);
        if (Physics.Raycast(rayTop,out hit, interactionDistance, 1<<10))
        {
            if (!interactionText.activeSelf)
            {
                interactionText.SetActive(true);
            }

            if (!isClimbing)
            {
                interactionText.GetComponent<Text>().text = "Press E to Climb";
            } else
            {
                interactionText.GetComponent<Text>().text = "Press E to Drop";
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                isClimbing = !isClimbing;
                if (isClimbing)
                {
                    Vector3 climbPosition = new Vector3(hit.point.x, transform.position.y + 1f, hit.point.z);
                    rBody.useGravity = false;
                    climbPosition -= (hit.transform.forward) * (playerCollider.radius);
                    transform.position = climbPosition;
                } else
                {
                    rBody.useGravity = true;
                }
            }
        } else
        {
            if (isClimbing && !Physics.Raycast(rayBottom, out hit, interactionDistance, 1 << 10))
            {
                isClimbing = false;
                rBody.useGravity = true;
            }
            if (interactionText.activeSelf && !isClimbing)
            {
                interactionText.SetActive(false);
                interactionText.GetComponent<Text>().text = "";
            }
        }

    }
}
