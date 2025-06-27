using System;
using System.Collections;
using LearnXR.Core.Utilities;
using UnityEngine;
using UnityEngine.Serialization;

public class Container : MonoBehaviour
{
    private static readonly float FullnessError = 0.005f;
    
    [Range(0, 1000)]
    [Tooltip("Max volume of liquid in ml. Assume that this is the volume when maxCapacity = 1")]
    [SerializeField] private int volume = 1000;
    
    [Range(0, 1)]
    [Tooltip("Value between 0 and 1. The volume is relative to object's renderer bounds which does not account for special shapes such as thinner mouth of the bottle so the volumes may appear unnatural. ")]
    [SerializeField] private float minCapacity = 0.05f;
    
    [Range(0, 1)]
    [Tooltip("Value between 0 and 1. The volume is relative to object's renderer bounds which does not account for special shapes such as thinner mouth of the bottle so the volumes may appear unnatural, e.g. when bottle stands upward it appear to have more liquid than when tilted. Lowering the max capacity also helps reduce overflowing i.e. when container is 100% full and it stands upwards, the liquid animation will show upward-flow. ")]
    [SerializeField] private float maxCapacity = 0.7f;

    public float MinCapacity => minCapacity;
    
    public float MaxCapacity => maxCapacity;
    
    [Range(0, 1)]
    [Tooltip("Defines the amount of liquid in the container. 0 if empty, 1 if full.")]
    [SerializeField] private float filled = 0.5f;
    
    /// <summary>
    /// Returns the current filled amount of the bottle.
    /// </summary>
    public float Filled => filled;
    
    private float CurrentVolume => volume * filled;
    private float MaxVolume => volume * maxCapacity;
    private float MinVolume => volume * minCapacity;

    private void ChangeVolume(float newVolume)
    {
        filled = newVolume / volume;
    }

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

    [SerializeField] private LiquidBehaviour liquid;

    public LiquidBehaviour Liquid => liquid;
    public Vector3 PourOriginPos => liquid.pourOrigin.transform.position;

    private void Awake()
    {
        // translate filled value from percentage to amount to match min and max capacity
        filled = filled * (maxCapacity - minCapacity) + minCapacity;
    }

    private void Start()
    {
        liquid.Init(this);

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
        if (CurrentVolume > MinVolume)
        {
            float deltaVolume = liquid.flowVelocity * Time.deltaTime;
            float newVolume = CurrentVolume - deltaVolume;
            
            if (newVolume <= MinVolume)
            {
                ChangeVolume(refillOnEmpty ? MaxVolume: MinVolume);
                return false;
            }

            ChangeVolume(newVolume);

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
        if (CurrentVolume < MaxVolume)
        {
            float deltaVolume = flowVelocity * Time.deltaTime;
            float newVolume = CurrentVolume + deltaVolume;
            
            if (newVolume >= MaxVolume)
            {
                ChangeVolume(emptyOnFilled ? MinVolume : MaxVolume);
            }
            else
            {
                ChangeVolume(newVolume);
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
}