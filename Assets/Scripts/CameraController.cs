using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    public Transform target; 
    public float closeUpDistance = 2f; // Yakınlaşma mesafesi
    public float closeUpHeight = 1f; // Yakınlaşma yüksekliği
    public float transitionSpeed = 2f; // Geçiş hızı

    private Vector3 velocity = Vector3.zero; 

    private void Start()
    {
       
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }

    public void CloseUp(Vector3 position)
    {
        StartCoroutine(CloseUpCoroutine(position));
    }

    private IEnumerator CloseUpCoroutine(Vector3 position)
    {
        Vector3 closeUpPosition = position + Vector3.up * closeUpHeight - transform.forward * closeUpDistance;
        float elapsedTime = 0f;

        while (elapsedTime < transitionSpeed)
        {
            transform.position = Vector3.SmoothDamp(transform.position, closeUpPosition, ref velocity, transitionSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(position - transform.position), elapsedTime / transitionSpeed);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = closeUpPosition;
        transform.LookAt(position);
    }

    public void ResetCamera()
    {
        StartCoroutine(ResetCameraCoroutine());
    }

    private IEnumerator ResetCameraCoroutine()
    {
        float elapsedTime = 0f;
        Vector3 velocity = Vector3.zero;

        while (elapsedTime < transitionSpeed)
        {
            transform.position = Vector3.SmoothDamp(transform.position, originalPosition, ref velocity, transitionSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, originalRotation, elapsedTime / transitionSpeed);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPosition;
        transform.rotation = originalRotation;
    }
}
