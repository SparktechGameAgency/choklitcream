using System.Collections;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Instance;

    public Transform player;
    public float smoothSpeed = 5f;

    [Header("Shake Settings")]
    public float defaultShakeDuration = 0.25f;
    public float defaultShakeMagnitude = 0.18f;

    private Vector3 shakeOffset = Vector3.zero;
    private bool isShaking = false;

    void Awake() => Instance = this;

    void LateUpdate()
    {
        if (player == null) return;

        Vector3 target = new Vector3(
            player.position.x,
            player.position.y,
            transform.position.z
        );

        transform.position = Vector3.Lerp(
            transform.position,
            target,
            smoothSpeed * Time.deltaTime
        ) + shakeOffset;
    }

    // Call this from anywhere: CameraFollow.Instance.Shake();
    public void Shake(float duration = -1f, float magnitude = -1f)
    {
        if (duration < 0) duration = defaultShakeDuration;
        if (magnitude < 0) magnitude = defaultShakeMagnitude;

        if (isShaking) StopCoroutine(nameof(ShakeRoutine));
        StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        isShaking = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Strength falls off toward the end of the shake
            float strength = Mathf.Lerp(magnitude, 0f, elapsed / duration);

            shakeOffset = new Vector3(
                Random.Range(-1f, 1f) * strength,
                Random.Range(-1f, 1f) * strength,
                0f
            );

            elapsed += Time.unscaledDeltaTime; // unscaled so shake works during hit-stop
            yield return null;
        }

        shakeOffset = Vector3.zero;
        isShaking = false;
    }
}