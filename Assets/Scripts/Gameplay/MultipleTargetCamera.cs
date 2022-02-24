using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZCameraShake;

public class MultipleTargetCamera : MonoBehaviour {
    private List<Transform> targets;
    private List<Transform> defaultTargets;
    private Vector3 offset;
    private Vector3 velocityPos;
    private float velocityZoom;
    private readonly float smoothTime = 0.6f; //1f
    private float minViewWidth = 10.4f;
    private float minViewHeight = 5.8f;
    private Vector3 defaultPos;
    private Vector3 currPos;
    private Camera cam;


    // Camera Shake Params
    private bool isShaking = false;

    // The default position influcence of all shakes created by this shaker.
    public Vector3 DefaultPosInfluence = new Vector3(0.15f, 0.15f, 0.15f);

    // The default rotation influcence of all shakes created by this shaker.
    public Vector3 DefaultRotInfluence = new Vector3(1, 1, 1);

    Vector3 posAddShake, rotAddShake;
    List<CameraShakeInstance> cameraShakeInstances = new List<CameraShakeInstance>();



    private void Awake() {

        defaultPos = transform.position;
        targets = new List<Transform>();
        defaultTargets = new List<Transform>();
        offset = new Vector3(0f, 0f, -10f);
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    private void Update() {
        //checks if there are any shake instances, ending previous shaking
        if (cameraShakeInstances.Count <= 0 && isShaking) {
            isShaking = false;
        }

        if (isShaking) {

            posAddShake = Vector3.zero;
            rotAddShake = Vector3.zero;

            for (int i = 0; i < cameraShakeInstances.Count; i++) {
                if (i >= cameraShakeInstances.Count)
                    break;

                CameraShakeInstance c = cameraShakeInstances[i];

                if (c.CurrentState == CameraShakeState.Inactive && c.DeleteOnInactive) {
                    cameraShakeInstances.RemoveAt(i);
                    i--;
                } else if (c.CurrentState != CameraShakeState.Inactive) {
                    posAddShake += CameraUtilities.MultiplyVectors(c.UpdateShake(), c.PositionInfluence);
                    rotAddShake += CameraUtilities.MultiplyVectors(c.UpdateShake(), c.RotationInfluence);
                }
            }

            transform.position = posAddShake + currPos;
            transform.localEulerAngles = rotAddShake;
        }
    }

    private void LateUpdate() {
        if (targets.Count == 0)
            return;
        
        if (!isShaking)
            CalculateBox();
    }


    //calculates rectangular area of where camera frame should pan/zoom towards
    void CalculateBox() {
        minViewWidth = 5.2f + 5.2f * (-29.43f / Physics2D.gravity.y);
        minViewHeight = 2.9f + 2.9f * (-29.43f / Physics2D.gravity.y);
        //Debug.Log("MinW: " + minViewWidth + "  MinH: " + minViewHeight);

        Bounds box = GetBounds();
        Vector3 centerPoint = box.center;
        float width = box.size.x;
        float height = box.size.y * 2f;

        //Base the 2nd side of the rectangle on whichever side is larger. 
        if (width >= height) {
            height = width / cam.aspect;
        } else {
            width = height * cam.aspect;
        }

        float multiplier = 3;
        width *= multiplier;
        height *= multiplier;

        //Constrain the rectangle to never be larger than the size of the map, but never smaller than the minView
        width = Mathf.Clamp(width, minViewWidth, 19.2f);
        height = Mathf.Clamp(height, minViewHeight, 10.8f);

        //Set the centerpoint of the rectangle so that the camera view never exceeds the extent of the background image
        if (centerPoint.x < (width / 2f))
            centerPoint.x = width / 2f;
        if (centerPoint.x > (19.2f - width / 2f))
            centerPoint.x = 19.2f - width / 2f;

        Vector3 actualCenterPoint = new Vector3(centerPoint.x, height / 2, 0f) + offset;
        float zoom = 2f * Mathf.Atan2(height / 2f, Mathf.Abs(cam.transform.position.z)) * Mathf.Rad2Deg;

        transform.position = Vector3.SmoothDamp(transform.position, actualCenterPoint, ref velocityPos, smoothTime);
        cam.fieldOfView = Mathf.SmoothDamp(cam.fieldOfView, zoom, ref velocityZoom, smoothTime);

        currPos = transform.position;
    }

    private Bounds GetBounds() {
        var bounds = new Bounds(targets[0].position, Vector3.zero);
        for (int i = 0; i < targets.Count; i++) {
            bounds.Encapsulate(targets[i].position);
        }

        Vector3 baseOfScreen = new Vector3(targets[0].position.x, 1.5f, targets[0].position.z);
        bounds.Encapsulate(baseOfScreen);

        return bounds;
    }

    public void AddTarget(Transform transform) {
        targets.Add(transform);
    }

    public void RemoveTarget(Transform transform) {
        targets.Remove(transform);
    }

    public void SetDefaultTargets() {
        defaultTargets.Clear();
        defaultTargets.AddRange(targets);
    }

    public void ResetTargetsToDefault() {
        targets.Clear();
        targets.AddRange(defaultTargets);
    }

    public CameraShakeInstance ShakeOnce(float magnitude, float roughness, float fadeInTime, float fadeOutTime) {
        CameraShakeInstance shake = new CameraShakeInstance(magnitude, roughness, fadeInTime, fadeOutTime);
        shake.PositionInfluence = DefaultPosInfluence;
        shake.RotationInfluence = DefaultRotInfluence;
        cameraShakeInstances.Add(shake);
        isShaking = true;

        return shake;
    }

}

