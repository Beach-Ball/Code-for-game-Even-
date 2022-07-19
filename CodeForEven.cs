using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Ludiq;
using Bolt;
using TMPro;
using System;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class InputSys : MonoBehaviour
{
    [SerializeField] private InputActionReference grab;
    [SerializeField] private InputActionReference releaseGrab;
    [SerializeField] private InputActionReference portal;
    [SerializeField] private InputActionReference closePortal;
    [SerializeField] private InputActionReference dimSwitch;
    [SerializeField] private InputActionReference releaseDimSwitch;
    [SerializeField] private InputActionReference jump;
    [SerializeField] private InputActionReference releaseJump;
    [SerializeField] private InputActionReference crouch;
    [SerializeField] private InputActionReference releaseCrouch;
    [SerializeField] private InputActionReference swimUp;
    [SerializeField] private InputActionReference releaseSwimUp;
    [SerializeField] private InputActionReference dashRun;
    [SerializeField] private InputActionReference sittingEmote;

    [SerializeField] private GameObject jointsToRotate; [SerializeField] private GameObject ChimeManche1; private Material material1; [SerializeField] private GameObject ChimeManche2; private Material material2; [SerializeField] private GameObject ChimeManche3; private Material material3; [SerializeField] private GameObject ChimeManche4;
    private Material material4; [SerializeField] private GameObject ChimeManche5; private Material material5; [SerializeField] private GameObject ChimeManche6; private Material material6; [SerializeField] private float slopeForce; [SerializeField] private float slideSpeed = 6; public bool WASD; private TextMeshProUGUI txtCrystalCount;
    [SerializeField] private GameObject physMatFeet; [SerializeField] private float fallSpeed = 2f; bool crouching = false; bool swimmingUp = false; [SerializeField] private float rotationSpeed = 5f; private LookControls playerActions; [HideInInspector] public InputAction move; [HideInInspector] public InputAction moveWASD;
    [HideInInspector] public InputAction moveZQSD; [SerializeField] private float boostSpeed; [SerializeField] private float movementForce; private float movementForceStored; [SerializeField] private float maxSpeed; private float maxSpeedStored; private Vector3 forceDirection = Vector3.zero; [SerializeField] private Camera playerCam;
    private float x = 1.5f; [SerializeField] private GameObject camBrain; static float t = 0.0f; static float t1 = 0.0f; static float t2 = 0.0f; static float t3 = 0.0f; static float t4 = 0.0f; [HideInInspector] public float t5 = 0.0f; RaycastHit hit; RaycastHit hit1; [SerializeField] private Animator animatorPlayer;
    private Rigidbody rb; private bool usingX = false; private bool usingZ = false; private bool canChangeFric; [SerializeField] public int maxCrystals; [SerializeField] public int crystalCount; [HideInInspector] public bool useCrystal; private bool jumping; [HideInInspector] public bool sit = false; [HideInInspector] public bool sitLerp1 = false;
    [HideInInspector] public bool sitLerp2 = false; private bool noMove = false; private bool sliding = false; private Vector3 velocityYo; private bool jumpFixSpeed = true; [SerializeField] GameObject stepRayUpper; [SerializeField] GameObject stepRayLower; [HideInInspector] public float stepSmooth = 2f; private bool slideSoundPlay = false;

    private void Awake()
    {
        if (ChimeManche1 && ChimeManche2 && ChimeManche3 && ChimeManche4 && ChimeManche5 && ChimeManche6 != null)
        {
            material1 = ChimeManche1.GetComponent<SkinnedMeshRenderer>().material;
            material2 = ChimeManche2.GetComponent<SkinnedMeshRenderer>().material;
            material3 = ChimeManche3.GetComponent<SkinnedMeshRenderer>().material;
            material4 = ChimeManche4.GetComponent<SkinnedMeshRenderer>().material;
            material5 = ChimeManche5.GetComponent<SkinnedMeshRenderer>().material;
            material6 = ChimeManche6.GetComponent<SkinnedMeshRenderer>().material;
        }

        CheckMaxCrystal();

        movementForceStored = movementForce;
        rb = gameObject.GetComponent<Rigidbody>();
        if(GameObject.Find("Canvas") != null)
        {
            txtCrystalCount = GameObject.Find("Canvas").transform.Find("CrystalCountTxt").GetComponent<TextMeshProUGUI>();
            UpdateCrystalCountUI();
        }
        maxSpeedStored = maxSpeed;
        playerActions = new LookControls();

    }

    // Manage inputs and calls some events in bolt

    private void Update()
    {
        if (grab.action.triggered)
        {
            CustomEvent.Trigger(gameObject, "grab_obj");
        }

        if (releaseGrab.action.triggered)
        {
            CustomEvent.Trigger(gameObject, "release_obj");
        }

        if (portal.action.triggered)
        {
            CustomEvent.Trigger(gameObject, "open_portal");
        }

        if (closePortal.action.triggered)
        {
            CustomEvent.Trigger(gameObject, "close_portal");
        }

        if (dimSwitch.action.triggered)
        {
            CustomEvent.Trigger(camBrain, "dim_switch");
        }

        if (releaseDimSwitch.action.triggered)
        {
            CustomEvent.Trigger(camBrain, "dim_switch_up");
        }

        //Manages jumping

        if (jump.action.triggered && sit == false && Time.deltaTime != 0 && Variables.Object(rb.gameObject).Get("IsGrounded").Equals(true)) 
        {
            
            jumping = true;
            if((move.ReadValue<Vector2>().x == 0 && move.ReadValue<Vector2>().y == 0) || rb.velocity.magnitude < 4)
            {
                //Debug.Log("not moving");
                jumpFixSpeed = false;
                StartCoroutine(jumpWaitForFixSpeed(0.1f));  // c'est pour empecher la grande acceleration du saut en avant 
            }
            CustomEvent.Trigger(gameObject, "jump");
        }
        
        if (releaseJump.action.triggered)
        {
            jumping = false;
        }

        if (crouch.action.triggered)
        {
            crouching = true;
        }

        if (releaseCrouch.action.triggered)
        {
            crouching = false;
            t = 0.0f;
        }

        if (swimUp.action.triggered)
        {
            swimmingUp = true;
        }

        if (releaseSwimUp.action.triggered)
        {
            swimmingUp = false;
            t = 0.0f;
        }

        // Manages sitting mechanic + camera movements

        if (sittingEmote.action.triggered)
        {
            if(Variables.Object(rb.gameObject).Get("Swimming").Equals(false))
            {
                if (sit == false && animatorPlayer.GetBool("Sitting") == false)
                {
                    sit = true;
                    sitLerp1 = true;
                    sitLerp2 = false;
                    animatorPlayer.SetBool("Sitting", true);
                }
                else if (sit == true && animatorPlayer.GetBool("Sitting") == true)
                {
                    sit = false;
                    sitLerp1 = false;
                    sitLerp2 = true;
                    animatorPlayer.SetBool("Sitting", false);
                }
            }
        }

        if (move.ReadValue<Vector2>().x != 0 || move.ReadValue<Vector2>().y != 0 && sit)
        {
            sit = false;
            sitLerp1 = false;
            sitLerp2 = true;
            animatorPlayer.SetBool("Sitting", false);
        }
    }

    // Fixed update for movement calculs

    private void FixedUpdate()
    {
        // gives the direction of the force to move the player
        if(Variables.Object(rb.gameObject).Get("IsGrounded").Equals(false) && Variables.Object(rb.gameObject).Get("Swimming").Equals(false) && animatorPlayer.GetBool("Sliding") == false)
        {
            forceDirection += move.ReadValue<Vector2>().x * GetCameraRight(playerCam) * (movementForce/4);
            forceDirection += move.ReadValue<Vector2>().y * GetCameraForward(playerCam) * (movementForce/4);
        }
        else
        {
            forceDirection += (move.ReadValue<Vector2>().x * GetCameraRight(playerCam) * movementForce);
            forceDirection += move.ReadValue<Vector2>().y * GetCameraForward(playerCam) * movementForce;
            if (forceDirection != Vector3.zero)
            {
                velocityYo = rb.velocity;
            }
        }
        rb.AddForce(Vector3.Lerp(new Vector3(0f, 0f, 0f), forceDirection, t1), ForceMode.Impulse);
        t1 += x * Time.deltaTime;
        if (move.ReadValue<Vector2>().x == 0 && move.ReadValue<Vector2>().y == 0)
        {
            t1 = 0.0f;
        }

        // Swimming up / swimming down

        if (crouching == true)
        {
            if (Variables.Object(rb.gameObject).Get("Swimming").Equals(true))
            {
                rb.velocity = new Vector3(rb.velocity.x, Mathf.Lerp(rb.velocity.y, -20f, t), rb.velocity.z);
                t += 0.15f * Time.deltaTime;
            }
        }

        if (swimmingUp == true)
        {
            if (Variables.Object(rb.gameObject).Get("Swimming").Equals(true))
            {
                rb.velocity = new Vector3(rb.velocity.x, Mathf.Lerp(rb.velocity.y, 20f, t), rb.velocity.z);
                t += 0.15f * Time.deltaTime;
            }
        }
                
        // Rotation on the chara

        if(Variables.Object(rb.gameObject).Get("Aiming").Equals(false) && forceDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(forceDirection), Time.deltaTime * rotationSpeed);
        }

        // Manages ground movement

        if (Variables.Object(rb.gameObject).Get("Swimming").Equals(false))
        {
            // Manages the sliding mechanic

            if (OnSlope() && rb.velocity.y < -0.8 && -0.1f < Vector3.Dot(gameObject.transform.Find("_bot").forward, new Vector3(hit.normal.x, 0, hit.normal.z))) 
            {
                if (Mathf.Abs(hit.normal.z) > 0.15 || Mathf.Abs(hit.normal.x) > 0.15)
                {
                    if(!slideSoundPlay)
                    {
                        GetComponent<AudioManagerScript>().Play("Slide");
                        slideSoundPlay = true;
                    }

                    GetComponent<AudioManagerScript>().Volume("Slide", (rb.velocity.magnitude) / 128);

                    rb.AddForce(Vector3.down * slopeForce);
                    if (Mathf.Abs(hit.normal.z) > 0.15)
                    {
                        usingZ = true;
                        usingX = false;
                    }
                    else if(Mathf.Abs(hit.normal.x) > 0.15)
                    {
                        usingZ = false;
                        usingX = true;
                    }
                    sliding = true;
                    rb.velocity += rb.velocity.normalized * (Mathf.Abs(hit.normal.z) + Mathf.Abs(hit.normal.x) + 2);
                    movementForce = Mathf.Lerp(movementForce,slideSpeed * (Mathf.Abs(hit.normal.z) + Mathf.Abs(hit.normal.x) + 1), t2);
                    t2 += 0.008f * Time.deltaTime;
                    jointsToRotate.transform.localEulerAngles = Vector3.Lerp(jointsToRotate.transform.localEulerAngles, new Vector3(Mathf.Abs((hit.normal.z) + Mathf.Abs(hit.normal.x)*45), 0, 0), 0.1f);
                    animatorPlayer.SetBool("Sliding", true);
                    if(forceDirection == Vector3.zero && Variables.Object(rb.gameObject).Get("IsGrounded").Equals(true))
                    {
                        //Debug.Log("slow down slide");
                        rb.velocity = new Vector3 (velocityYo.x, rb.velocity.y, velocityYo.z);
                        velocityYo = velocityYo / 1.02f;
                        if (rb.velocity.magnitude < 10f)
                        {
                            //Debug.Log("sliding stop");
                            animatorPlayer.SetBool("Sliding", false);
                            jointsToRotate.transform.localEulerAngles = Vector3.Lerp(jointsToRotate.transform.localEulerAngles, Vector3.zero, 0.5f);
                        }
                    }
                }
            }

            else // Not sliding anymore = managing speed lerp down
            {
                t2 = 0f;
                if (forceDirection == Vector3.zero || rb.velocity.magnitude <= 7 )
                {
                    if (slideSoundPlay)
                    {
                        GetComponent<AudioManagerScript>().Stop("Slide");
                        slideSoundPlay = false;
                    }
                    sliding = false;
                    t3 = 0f;
                    animatorPlayer.SetBool("Sliding", false);
                    jointsToRotate.transform.localEulerAngles = Vector3.Lerp(jointsToRotate.transform.localEulerAngles, Vector3.zero, 0.5f);
                    //Debug.Log("Seting to movement force stored");
                    movementForce = movementForceStored;
                }

                else if (movementForce > movementForceStored +1 && (Variables.Object(rb.gameObject).Get("IsGrounded").Equals(true) || Mathf.Abs(hit1.normal.z) < 0.15 || Mathf.Abs(hit1.normal.x) < 0.15))
                {
                    GetComponent<AudioManagerScript>().Volume("Slide", 0.1f-(t3/ 0.65f));
                    
                    movementForce = Mathf.Lerp(movementForce,movementForceStored, t3);
                    t3 += 0.0075f * ((Mathf.Abs(hit.normal.z) + Mathf.Abs(hit.normal.x) + 1) * 4) * Time.deltaTime;
                }

                else if (Variables.Object(rb.gameObject).Get("IsGrounded").Equals(false))
                {
                    if (slideSoundPlay)
                    {
                        GetComponent<AudioManagerScript>().Stop("Slide");
                        slideSoundPlay = false;
                    }
                }

                else if (movementForce < movementForceStored + 1)
                {
                    if (slideSoundPlay)
                    {
                        GetComponent<AudioManagerScript>().Stop("Slide");
                        slideSoundPlay = false;
                    }
                    sliding = false;
                    t3 = 0f;
                    animatorPlayer.SetBool("Sliding", false);
                    jointsToRotate.transform.localEulerAngles = Vector3.Lerp(jointsToRotate.transform.localEulerAngles, Vector3.zero, 0.5f);
                    //Debug.Log("Seting to movement force stored");
                    movementForce = movementForceStored;
                }

                else
                {
                    if (slideSoundPlay)
                    {
                        GetComponent<AudioManagerScript>().Stop("Slide");
                        slideSoundPlay = false;
                    }
                    t3 = 0f;
                    sliding = false;
                    animatorPlayer.SetBool("Sliding", false);
                    jointsToRotate.transform.localEulerAngles = Vector3.Lerp(jointsToRotate.transform.localEulerAngles, Vector3.zero, 0.5f);
                }
            }

            // Manages fall speed

            if (rb.velocity.y < -10f)
            {
                rb.velocity += new Vector3(rb.velocity.x, fallSpeed * Physics.gravity.y, rb.velocity.z) * Time.fixedDeltaTime;
            }

            else if (rb.velocity.y < -0.5f)
            {
                rb.velocity += new Vector3 (rb.velocity.x,  0.5f * Physics.gravity.y, rb.velocity.z) * Time.fixedDeltaTime;
            }

            if(jumping && rb.velocity.y > 0f)
            {
                rb.velocity += new Vector3(rb.velocity.x, -0.7f * Physics.gravity.y, rb.velocity.z) * Time.fixedDeltaTime;
            }
        }

        // Manages the swimming movements

        if (Variables.Object(rb.gameObject).Get("Swimming").Equals(true)) 
        {
            if (slideSoundPlay)
            {
                GetComponent<AudioManagerScript>().Stop("Slide");
                slideSoundPlay = false;
            }
            if (movementForce > movementForceStored + 1 && (move.ReadValue<Vector2>().x != 0 || move.ReadValue<Vector2>().y != 0))
            {
                //Debug.Log("lerping down");
                movementForce = Mathf.Lerp(movementForce, movementForceStored, t3);
                t3 += 0.0005f * Time.deltaTime;
            }
            else if ((movementForce < movementForceStored + 1 && movementForce != movementForceStored) || (move.ReadValue<Vector2>().x == 0 && move.ReadValue<Vector2>().y == 0 && movementForce != movementForceStored))
            {
                t3 = 0f;
                //Debug.Log("Seting to movement force stored");
                movementForce = movementForceStored;
            }
            animatorPlayer.SetBool("Sliding", false);
            jointsToRotate.transform.localEulerAngles = Vector3.Lerp(jointsToRotate.transform.localEulerAngles, Vector3.zero, 0.5f);
            if (canChangeFric)
            {
                canChangeFric = false;
                physMatFeet.GetComponent<CapsuleCollider>().material.dynamicFriction = 0f;
                physMatFeet.GetComponent<CapsuleCollider>().material.staticFriction = 0f;
                physMatFeet.GetComponent<CapsuleCollider>().material.frictionCombine = PhysicMaterialCombine.Minimum;
            }
            if (rb.velocity.sqrMagnitude > maxSpeed * maxSpeed)
            {
                    rb.velocity = rb.velocity.normalized * maxSpeed;
            }
        }

        forceDirection = Vector3.zero;
    }

    private Vector3 GetCameraForward(Camera playerCam)
    {
        Vector3 forward = playerCam.transform.forward;
        if (Variables.Object(rb.gameObject).Get("Swimming").Equals(false))
        {
            forward.y = 0;
        }
        return forward.normalized;
    }

    private Vector3 GetCameraRight(Camera playerCam)
    {
        Vector3 right = playerCam.transform.right;
        if (Variables.Object(rb.gameObject).Get("Swimming").Equals(false))
        {
            right.y = 0;
        }
        return right.normalized;
    }

    // Determines if on slope or not

    private bool OnSlope()
    {
        if (Variables.Object(rb.gameObject).Get("Jumping").Equals(true))
        {
            return false;
        }
        RaycastHit hit;
        if(Physics.Raycast(transform.position, Vector3.down, out hit, 1.3f))
        {
            if(hit.normal != Vector3.up)
            {
                return true;
            }
        }
        return false;
    }

    // When there was a dash

    private void DashAction()
    {
        maxSpeed = Mathf.Lerp(maxSpeed, dashForce, t4);
        if (noMove)
        {
            rb.AddForce(Vector3.Lerp(new Vector3(0f, 0f, 0f), playerCam.transform.forward * 4, t4), ForceMode.Impulse);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(playerCam.transform.forward), Time.deltaTime * rotationSpeed);
            CustomEvent.Trigger(gameObject, "LookDashDir");
        }
        if (move.ReadValue<Vector2>().x != 0 && move.ReadValue<Vector2>().y != 0)
        {
            noMove = false;
        }
        t4 += 0.3f * Time.fixedDeltaTime;

    }

    // Manages maximum amount of charges for the player

    public void CheckMaxCrystal()
    {
        if (maxCrystals == 3)
        {
            ChimeManche1.GetComponent<SkinnedMeshRenderer>().enabled = true;
            ChimeManche2.GetComponent<SkinnedMeshRenderer>().enabled = true;
            ChimeManche3.GetComponent<SkinnedMeshRenderer>().enabled = true; 
            ChimeManche4.GetComponent<SkinnedMeshRenderer>().enabled = false;
            ChimeManche5.GetComponent<SkinnedMeshRenderer>().enabled = false;
            ChimeManche6.GetComponent<SkinnedMeshRenderer>().enabled = false;
        }

        else if (maxCrystals == 4)
        {
            ChimeManche1.GetComponent<SkinnedMeshRenderer>().enabled = true;
            ChimeManche2.GetComponent<SkinnedMeshRenderer>().enabled = true;
            ChimeManche3.GetComponent<SkinnedMeshRenderer>().enabled = true;
            ChimeManche4.GetComponent<SkinnedMeshRenderer>().enabled = true;
            ChimeManche5.GetComponent<SkinnedMeshRenderer>().enabled = false;
            ChimeManche6.GetComponent<SkinnedMeshRenderer>().enabled = false;
        }

        else if (maxCrystals == 5)
        {
            ChimeManche1.GetComponent<SkinnedMeshRenderer>().enabled = true;
            ChimeManche2.GetComponent<SkinnedMeshRenderer>().enabled = true;
            ChimeManche3.GetComponent<SkinnedMeshRenderer>().enabled = true;
            ChimeManche4.GetComponent<SkinnedMeshRenderer>().enabled = true;
            ChimeManche5.GetComponent<SkinnedMeshRenderer>().enabled = true;
            ChimeManche6.GetComponent<SkinnedMeshRenderer>().enabled = false;
        }

        else if (maxCrystals >= 6)
        {
            ChimeManche1.GetComponent<SkinnedMeshRenderer>().enabled = true;
            ChimeManche2.GetComponent<SkinnedMeshRenderer>().enabled = true;
            ChimeManche3.GetComponent<SkinnedMeshRenderer>().enabled = true;
            ChimeManche4.GetComponent<SkinnedMeshRenderer>().enabled = true;
            ChimeManche5.GetComponent<SkinnedMeshRenderer>().enabled = true;
            ChimeManche6.GetComponent<SkinnedMeshRenderer>().enabled = true;
        }
    }

    // Lights up chimes based on amount of charges left

    public void UpdateCrystalCountUI()
    {
        if (txtCrystalCount != null)
        {
            txtCrystalCount.SetText("Crystals : " + crystalCount);
        }

        if (material1 && material2 && material3 && material4 && material5 && material6 != null)
        {
            if (crystalCount == 6)
            {
                material1.SetFloat("_Intensity", 1);
                material2.SetFloat("_Intensity", 1);
                material3.SetFloat("_Intensity", 1);
                material4.SetFloat("_Intensity", 1);
                material5.SetFloat("_Intensity", 1);
                material6.SetFloat("_Intensity", 1);
            }

            else if (crystalCount == 5)
            {
                material1.SetFloat("_Intensity", 1);
                material2.SetFloat("_Intensity", 1);
                material3.SetFloat("_Intensity", 1);
                material4.SetFloat("_Intensity", 1);
                material5.SetFloat("_Intensity", 1);
                material6.SetFloat("_Intensity", 0);
            }

            else if (crystalCount == 4)
            {
                material1.SetFloat("_Intensity", 1);
                material2.SetFloat("_Intensity", 1);
                material3.SetFloat("_Intensity", 1);
                material4.SetFloat("_Intensity", 1);
                material5.SetFloat("_Intensity", 0);
                material6.SetFloat("_Intensity", 0);
            }

            else if (crystalCount == 3)
            {
                material1.SetFloat("_Intensity", 1);
                material2.SetFloat("_Intensity", 1);
                material3.SetFloat("_Intensity", 1);
                material4.SetFloat("_Intensity", 0);
                material5.SetFloat("_Intensity", 0);
                material6.SetFloat("_Intensity", 0);
            }

            else if (crystalCount == 2)
            {
                material1.SetFloat("_Intensity", 1);
                material2.SetFloat("_Intensity", 1);
                material3.SetFloat("_Intensity", 0);
                material4.SetFloat("_Intensity", 0);
                material5.SetFloat("_Intensity", 0);
                material6.SetFloat("_Intensity", 0);
            }

            else if (crystalCount == 1)
            {
                material1.SetFloat("_Intensity", 1);
                material2.SetFloat("_Intensity", 0);
                material3.SetFloat("_Intensity", 0);
                material4.SetFloat("_Intensity", 0);
                material5.SetFloat("_Intensity", 0);
                material6.SetFloat("_Intensity", 0);
            }

            else if (crystalCount == 0)
            {
                material1.SetFloat("_Intensity", 0);
                material2.SetFloat("_Intensity", 0);
                material3.SetFloat("_Intensity", 0);
                material4.SetFloat("_Intensity", 0);
                material5.SetFloat("_Intensity", 0);
                material6.SetFloat("_Intensity", 0);
            }
            else
            {
                material1.SetFloat("_Intensity", 1);
                material2.SetFloat("_Intensity", 1);
                material3.SetFloat("_Intensity", 1);
                material4.SetFloat("_Intensity", 1);
                material5.SetFloat("_Intensity", 1);
                material6.SetFloat("_Intensity", 1);
            }
            //Debug.Log(Variables.Object(rb.gameObject).Get("Swimming").Equals(true));
            if (Variables.Object(rb.gameObject).Get("Swimming").Equals(true))
            {
                material1.SetFloat("_Ocean_Desert", 0);
                material2.SetFloat("_Ocean_Desert", 0);
                material3.SetFloat("_Ocean_Desert", 0);
                material4.SetFloat("_Ocean_Desert", 0);
                material5.SetFloat("_Ocean_Desert", 0);
                material6.SetFloat("_Ocean_Desert", 0);
            }
            
            else
            {
                material1.SetFloat("_Ocean_Desert", 1);
                material2.SetFloat("_Ocean_Desert", 1);
                material3.SetFloat("_Ocean_Desert", 1);
                material4.SetFloat("_Ocean_Desert", 1);
                material5.SetFloat("_Ocean_Desert", 1);
                material6.SetFloat("_Ocean_Desert", 1);
            }
        }

    }

    public void useOneCrystal()
    {
        crystalCount = crystalCount - 1;
        UpdateCrystalCountUI();
        useCrystal = false;
    }
    
    public void StepClimb()
    {
        RaycastHit hitLower;
        if (Physics.Raycast(stepRayLower.transform.position, transform.TransformDirection(Vector3.forward), out hitLower, 0.5f) && Mathf.Abs(hitLower.normal.y) < 0.2)
        {
            RaycastHit hitUpper;
            if (!Physics.Raycast(stepRayUpper.transform.position, transform.TransformDirection(Vector3.forward), out hitUpper, 0.65f))
            {
                rb.position -= new Vector3(0f, -stepSmooth * Time.fixedDeltaTime * 2.5f, 0f);
            }
        }

        RaycastHit hitLower45;
        if (Physics.Raycast(stepRayLower.transform.position, transform.TransformDirection(1.5f, 0, 1), out hitLower45, 0.5f) && Mathf.Abs(hitLower45.normal.y) < 0.2)
        {
            RaycastHit hitUpper45;
            if (!Physics.Raycast(stepRayUpper.transform.position, transform.TransformDirection(1.5f, 0, 1), out hitUpper45, 0.65f))
            {
                rb.position -= new Vector3(0f, -stepSmooth * Time.fixedDeltaTime * 2.5f, 0f);
            }
        }

        RaycastHit hitLowerMinus45;
        if (Physics.Raycast(stepRayLower.transform.position, transform.TransformDirection(-1.5f, 0, 1), out hitLowerMinus45, 0.5f) && Mathf.Abs(hitLowerMinus45.normal.y) < 0.2)
        {
            RaycastHit hitUpperMinus45;
            if (!Physics.Raycast(stepRayUpper.transform.position, transform.TransformDirection(-1.5f, 0, 1), out hitUpperMinus45, 0.65f))
            {
                rb.position -= new Vector3(0f, -stepSmooth * Time.fixedDeltaTime * 2.5f, 0f);
            }
        }
    }

    public IEnumerator jumpWaitForFixSpeed(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        jumpFixSpeed = true;
    }

    private void OnEnable()
    {
        grab.action.Enable();
        releaseGrab.action.Enable();
        portal.action.Enable();
        closePortal.action.Enable();
        dimSwitch.action.Enable();
        releaseDimSwitch.action.Enable();
        jump.action.Enable();
        releaseJump.action.Enable();
        crouch.action.Enable();
        releaseCrouch.action.Enable();
        swimUp.action.Enable();
        releaseSwimUp.action.Enable();
        dashRun.action.Enable();
        sittingEmote.action.Enable();
        if(WASD)
        {
            move = playerActions.MouseController.moveWASD;
        }
        else if(!WASD)
        {
            move = playerActions.MouseController.moveZQSD;
        }
        playerActions.MouseController.Enable();
    }

    private void OnDisable()
    {
        grab.action.Disable();
        releaseGrab.action.Disable();
        portal.action.Disable();
        closePortal.action.Disable();
        closePortal.action.Disable();
        releaseDimSwitch.action.Disable();
        releaseDimSwitch.action.Disable();
        jump.action.Disable();
        releaseJump.action.Disable();
        crouch.action.Disable();
        releaseCrouch.action.Disable();
        swimUp.action.Disable();
        releaseSwimUp.action.Disable();
        dashRun.action.Disable();
        sittingEmote.action.Disable();
    }

    public void ChangeToWASD()
    {
        WASD = true;
        move = playerActions.MouseController.moveWASD;
    }

    public void ChangeToZQSD()
    {
        WASD = false;
        move = playerActions.MouseController.moveZQSD;
    }
}
