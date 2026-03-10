// Generated using Gemini 3 Fast on 3/5/26
// Prompt: Write me a billboarding script to make 2d items always face the player that does not tilt items up or down.

using UnityEngine;

public class CylindricalBillboard : MonoBehaviour
{
    private Transform mainCameraTransform;

    void Start()
    {
        // Cache the camera once for better performance
        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
    }

    void LateUpdate()
    {
        if (mainCameraTransform == null) return;

        // 1. Get the camera's position
        Vector3 targetPosition = mainCameraTransform.position;

        // 2. Force the target's Y to be the same as the item's Y
        // This stops the "tilting" effect
        targetPosition.y = transform.position.y;

        // 3. Point the item's Forward vector at the adjusted target
        transform.LookAt(targetPosition);
    }
}