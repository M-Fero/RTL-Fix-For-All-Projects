using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;




public class FingerTrigger : MonoBehaviour
{
    [Header("Touch Detection Settings")]
    [Tooltip("Number of simultaneous touches required to trigger the event.")]
    [SerializeField] private int touchCount = 10;

    [Header("Detection State")]
    [Tooltip("Set to true to enable new detection. Set to false to prevent repeated triggers.")]
    public bool newDetection = true;

    [Header("Events")]
    [Tooltip("Event invoked when the specified number of fingers are detected touching the screen.")]
    public UnityEvent OnTenFingersTouch;

    [Tooltip("Event invoked after the first scan is complete.")]
    public UnityEvent AfterFirstScan;

    [SerializeField] float delay = 3f; // Delay before resetting detection state

    #region Legacy Input System
    //void Update()
    //{
    //    // Check if exactly 10 fingers are touching the screen
    //    if (Input.touchCount == 10)
    //    {
    //        // Invoke the UnityEvent if assigned
    //        OnTenFingersTouch?.Invoke();
    //        Debug.Log("Ten fingers detected touching the screen!");
    //    }
    //}
    #endregion
    #region Both Input System
    void Update()
    {
        //if (Touchscreen.current.touches.Count >= touchCount) { }
        if (newDetection)
        {
            DetectTouch();
        }
        else
        {
            // Reset detection state when all fingers are lifted
            bool noTouchesLegacy = Input.touchCount == 0;
            bool noTouchesNew = true;
            if (Touchscreen.current != null)
            {
                foreach (var touch in Touchscreen.current.touches)
                {
                    if (touch.press.isPressed)
                    {
                        noTouchesNew = false;
                        break;
                    }
                }
            }
            if (noTouchesLegacy && noTouchesNew)
            {
                newDetection = true;
            }
        }
    }

    private void DetectTouch()
    {
        // Legacy Input System
        bool legacyTenFingers = Input.touchCount == touchCount;

        // New Input System
        bool newSystemTenFingers = false;
        if (Touchscreen.current != null)
        {
            //Touchscreen.current.touches is a ReadOnlyArray < TouchControl >
            int activeTouches = 0;
            foreach (var touch in Touchscreen.current.touches)
            {
                if (touch.press.isPressed)
                    activeTouches++;
            }
            //if (Touchscreen.current.touches.Count >= touchCount) 
            //{
            //    newSystemTenFingers = true;
            //}
            newSystemTenFingers = activeTouches == touchCount;
        }

        if (legacyTenFingers || newSystemTenFingers)
        {
            Debug.Log("Ten fingers detected touching the screen! (Legacy or New Input System)");
            OnTenFingersTouch?.Invoke();
            newDetection = false;
            StartCoroutine(DelayedEvent(delay));
        }
    }
    #endregion

    IEnumerator DelayedEvent(float delay)
    {
        yield return new WaitForSeconds(delay);
        AfterFirstScan?.Invoke();
        //newDetection = true; // Reset detection state after the delay
    }

}
