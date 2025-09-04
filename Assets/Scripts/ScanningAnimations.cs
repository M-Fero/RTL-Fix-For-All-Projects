using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.Events;

public class ScanningAnimations : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("Animator component to control the scanning animation. If not assigned, it will try to get the Animator component from this GameObject.")]
    [SerializeField] private Animator animator;

    [Tooltip("Name of the trigger parameter in the Animator to start the scanning animation. Default is 'Play'.")]
    [SerializeField] private string triggerName = "Play";

    [Header("Events")]
    [Tooltip("Event invoked after the scanning animation finishes.")]
    public UnityEvent AfterScanningFinish;

    [Tooltip("Event invoked after the scanning animation finishes.")]
    public UnityEvent AfterAnimationFinishPlaying;

    [Header("After Animation Delay")]
    [SerializeField] private float delay;
    void Awake()
    {
        if (animator == null)
        {
            // Attempt to get the Animator component if not assigned in the inspector
            animator = GetComponent<Animator>();
        }
    }

    /// <summary>
    /// Triggers the "Play" animation once.
    /// </summary>
    public void PlayAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger(triggerName);
        }
    }
    public void OnAnimationFinish()
    {
        // This method can be called from the animation event to signal that the animation has finished.
        AfterScanningFinish?.Invoke();
    }
    public void OnAnimationFinishDelayed()
    {
        StartCoroutine(AfterAnimation(delay));
    }
    private IEnumerator AfterAnimation(float delay)
    {
        yield return new WaitForSeconds(delay);
        AfterAnimationFinishPlaying?.Invoke();
    }
}
