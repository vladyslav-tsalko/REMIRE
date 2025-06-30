using System;
using UnityEngine;

namespace LiquidPhysics
{
    /// <summary>
    /// Represents a container holding a liquid with configurable volume limits, 
    /// fill level, and behaviors for automatic refilling or emptying.
    /// </summary>
    public class Container : MonoBehaviour
    {
        private static readonly float FullnessError = 0.005f;
        
        [Range(0, 1000)]
        [Tooltip("Max volume of liquid in ml. Assume that this is the volume when maxCapacity = 1")]
        [SerializeField] private int volume = 1000;
        
        [Range(0, 1)]
        [Tooltip("Relative to min liquid's renderer bounds and min volume capacity.")]
        [SerializeField] private float minCapacity = 0.05f;
        
        [Range(0, 1)]
        [Tooltip("Relative to max liquid's renderer bounds and max volume capacity.")]
        [SerializeField] private float maxCapacity = 0.7f;

        [Range(0, 1)]
        [Tooltip("Defines the amount of liquid in the container.")]
        [SerializeField] private float filled = 0.5f;
        
        [SerializeField]
        [Tooltip("If true, object will automatically refill when minimum capacity is reached.")]
        private bool refillOnEmpty;

        [SerializeField]
        [Tooltip("If true, object will automatically empty when maximum capacity is reached.")]
        private bool emptyOnFilled;
        
        [SerializeField] private LiquidBehaviour liquid;

        #region Getters
        
        public float MinCapacity => minCapacity;
        
        public float MaxCapacity => maxCapacity;
        
        /// <summary>
        /// Returns the current filled amount of the bottle.
        /// </summary>
        public float Filled => filled;
        
        private float CurrentVolume => volume * filled;
        
        private float MaxVolume => volume * maxCapacity;
        
        private float MinVolume => volume * minCapacity;

        /// <summary>
        /// Returns the fullness of the bottle as a normalized value between 0 and 1,
        /// based on the minimum and maximum capacity.
        /// </summary>
        public float Fullness => (filled - minCapacity) / (maxCapacity - minCapacity);
        
        public float LiquidHeight => liquid.LiquidHeight;
        
        public Vector3 PourOriginPos => liquid.PourOriginPos;

        #endregion

        #region Checkers
        
        public bool IsFull => Math.Abs(filled - maxCapacity) < FullnessError;
        
        public bool IsEmpty => Math.Abs(filled - minCapacity) < FullnessError;
        
        public bool IsFilledEnough(float minSuccessfulFullness = 0.9f) => filled >= maxCapacity * minSuccessfulFullness;
        
        public bool IsPouring => liquid.IsPouring;
        
        #endregion

        #region Setters

        public void Refill() => filled = maxCapacity;
        
        public void MakeEmpty() => filled = minCapacity;
        
        private void ChangeVolume(float newVolume) => filled = newVolume / volume;

        #endregion

        /// <summary>
        /// Adds liquid into the container at a rate defined by flowVelocity, respecting maximum capacity.
        /// </summary>
        /// <param name="flowVelocity">Rate of liquid flow in volume units per second.</param>
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
        
        /// <summary>
        /// Attempts to pour liquid out of the container, respecting minimum capacity.
        /// </summary>
        /// <returns>True if liquid was poured out; false if container is empty or refilled.</returns>
        public bool TryPourOut()
        {
            if (CurrentVolume > MinVolume)
            {
                float deltaVolume = liquid.FlowVelocity * Time.deltaTime;
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
        
        private void Update()
        {
            liquid.Update();
        }
    }
}

