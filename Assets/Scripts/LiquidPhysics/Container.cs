using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class Container : MonoBehaviour
{
    public static readonly float FullnessError = 0.005f;
    
    [Range(0, 1)]
    [Tooltip("Value between 0 and 1. The volume is relative to object's renderer bounds which does not account for special shapes such as thinner mouth of the bottle so the volumes may appear unnatural. ")]
    [SerializeField] private float minCapacity = 0.05f;

    public float MinCapacity => minCapacity;
    
    public float MaxCapacity => maxCapacity;

    [Range(0, 1)]
    [Tooltip("Value between 0 and 1. The volume is relative to object's renderer bounds which does not account for special shapes such as thinner mouth of the bottle so the volumes may appear unnatural, e.g. when bottle stands upward it appear to have more liquid than when tilted. Lowering the max capacity also helps reduce overflowing i.e. when container is 100% full and it stands upwards, the liquid animation will show upward-flow. ")]
    [SerializeField] private float maxCapacity = 0.7f;

    [FormerlySerializedAs("_filled")]
    [Range(0, 1)]
    [Tooltip("Defines the amount of liquid in the container. 0 if empty, 1 if full.")]
    [SerializeField] private float filled = 0.5f;
    
    /// <summary>
    /// Returns the current filled amount of the bottle.
    /// </summary>
    public float Filled => filled;
    
    /// <summary>
    /// Returns the fullness of the bottle as a normalized value between 0 and 1,
    /// based on the minimum and maximum capacity.
    /// </summary>
    public float Fullness => (filled - minCapacity) / (maxCapacity - minCapacity);

    [SerializeField]
    [Tooltip("If true, object will automatically refill when minimum capacity is reached.")]
    private bool refillOnEmpty = false;

    [SerializeField]
    [Tooltip("If true, object will automatically empty when maximum capacity is reached.")]
    private bool emptyOnFilled = false;

    [SerializeField]
    [Tooltip("If true, object will automatically tilt to pour liquid, for demonstration purposes. This will temporarily set object to kinematic.")]
    private bool tiltOnStart = false;

    [SerializeField] private LiquidBehaviour liquid;
    //private float initialFilled = 0f;

    public LiquidBehaviour Liquid => liquid;
    public Vector3 PourOriginPos => liquid.pourOrigin.transform.position;

    private void Awake()
    {
        // translate filled value from percentage to amount to match min and max capacity
        filled = filled * (maxCapacity - minCapacity) + minCapacity;
        //initialFilled = filled;
    }

    private void Start()
    {
        liquid.Init(this);

        // check if working in play mode - shouldn't execute tilt in edit mode
        /*if (Application.isPlaying)
        {
            StartCoroutine(Tilt());
        }*/

        if (filled < minCapacity)
            filled = minCapacity;

        if (filled > maxCapacity)
            filled = maxCapacity;
    }

    public bool IsFilledEnough(float minSuccessfulFullness = 0.9f)
    {
        return filled >= maxCapacity * minSuccessfulFullness;
    }

    private void Update()
    {
        liquid.Update();
    }

    public bool TryPourOut()
    {
        if (filled > minCapacity)
        {
            filled -= liquid.flowVelocity * Time.deltaTime;
            
            if (filled <= minCapacity)
            {
                filled = refillOnEmpty ? maxCapacity : minCapacity;
                return false;
                //ContainerEmpty?.Invoke();
            }

            return true;
        }

        return false;
    }

    public void Refill()
    {
        filled = maxCapacity;
    }

    public bool IsFull => Math.Abs(filled - maxCapacity) < FullnessError;
    
    public bool IsEmpty => Math.Abs(filled - minCapacity) < FullnessError;

    public void PourIn(float flowVelocity)
    {
        if (filled < maxCapacity)
        {
            filled += flowVelocity * Time.deltaTime;

            // make sure capacity never goes above max.
            if (filled >= maxCapacity)
            {
                filled = emptyOnFilled ? minCapacity : maxCapacity;
            }
        }
    }

    public void MakeEmpty()
    {
        filled = minCapacity;
    }

    public bool IsPouring()
    {
        return liquid.IsPouring();
    }

    /*#region DEBUGGING

    // Plays bottle tilt animation for debugging purposes. Will animate tilting the container only if 'tiltAnimationOnStart' set to true in the inspector.
    private IEnumerator Tilt()
    {
        // reset bottle position
        transform.localRotation = Quaternion.Euler(270f, 0f, 0f);

        // set object to kinematic so the animation is not affected by gravity
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        bool kinematicOnAnimationExit = rb.isKinematic;
        rb.isKinematic = true;

        while (tiltOnStart)
        {
            float rotationSpeed = 30f;
            float maxRotation = 110f;

            // tilt right
            yield return StartCoroutine(Rotate(maxRotation, rotationSpeed, 1));

            // keep in position
            yield return new WaitForSeconds(5f);

            // tilt left
            yield return StartCoroutine(Rotate(maxRotation, rotationSpeed, -1));

            // reset bottle to be full
            if (filled <= minCapacity)
                filled = maxCapacity;
        }

        rb.isKinematic = kinematicOnAnimationExit;
        yield return null;
    }

    // direction 1 for right tilt, -1 for left tilt
    private IEnumerator Rotate(float maxRotation, float rotationSpeed, int direction)
    {
        float currentRotation = 0f;

        while (gameObject.activeSelf && currentRotation <= maxRotation)
        {
            float rotation = rotationSpeed * Time.deltaTime;
            transform.Rotate(direction * rotation, 0, 0);
            currentRotation += rotation;
            yield return null;
        }
    }

    #endregion DEBUGGING*/
}