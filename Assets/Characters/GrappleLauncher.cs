using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleLauncher : MonoBehaviour
{
    [Header("Scripts Ref:")]
    public GrappleRope grappleRope;

    [Header("Layer Settings:")]
    [SerializeField] private bool grappleToAll = false;
    [SerializeField] private int grappableLayerNumber = 9;

    [Header("Transform Ref:")]
    public Transform gunHolder;
    public Transform firePoint;

    [Header("Physics Ref:")]
    public SpringJoint2D m_springJoint2D;
    public Rigidbody2D m_rigidbody;

    [Header("Distance:")]
    [SerializeField] private bool hasMaxDistance = false;
    [SerializeField] private float maxDistnace = 20;

    private enum LaunchType {
        Transform_Launch,
        Physics_Launch
    }

    [Header("Launching:")]
    [SerializeField] private bool launchToPoint = true;
    [SerializeField] private LaunchType launchType = LaunchType.Physics_Launch;
    [SerializeField] private float launchSpeed = 1;

    [Header("No Launch To Point")]
    [SerializeField] private bool autoConfigureDistance = false;
    [SerializeField] private float targetDistance = 3;
    [SerializeField] private float targetFrequncy = 1;

    [HideInInspector] public Vector2 grapplePoint;
    [HideInInspector] public Vector2 grappleDistanceVector;
    private bool isGrappling = false;

    private void Start() {
        grappleRope.enabled = false;
        m_springJoint2D.enabled = false;

    }

    private void holdGrapple() {
        if (launchToPoint && grappleRope.isGrappling) {
            if (launchType == LaunchType.Transform_Launch) {
                Vector2 firePointDistnace = firePoint.position - gunHolder.localPosition;
                Vector2 targetPos = grapplePoint - firePointDistnace;
                gunHolder.position = Vector2.Lerp(gunHolder.position, targetPos, Time.deltaTime * launchSpeed);
            }
        }
    }

    public void CancelGrapple() {
        grappleRope.enabled = false;
        m_springJoint2D.enabled = false;
        m_rigidbody.gravityScale = 1;
    }

    public void SetGrapplePoint() {
        grapplePoint = new Vector2(m_rigidbody.position.x + 3, m_rigidbody.position.y + 3);
        Vector2 launchPoint = firePoint.position;
        Debug.Log("Launch " + launchPoint);
        Debug.Log("GrapplePoint " + grapplePoint);
        //Vector2 distanceVector = grapplePoint - launchPoint;

        //if (Physics2D.Raycast(firePoint.position, distanceVector.normalized)) {
        //    RaycastHit2D _hit = Physics2D.Raycast(firePoint.position, distanceVector.normalized);
        //    if (_hit.transform.gameObject.layer == grappableLayerNumber || grappleToAll) {
        //        if (Vector2.Distance(_hit.point, firePoint.position) <= maxDistnace || !hasMaxDistance) {
        //grapplePoint = _hit.point;
        grappleDistanceVector = grapplePoint - launchPoint;
        grappleRope.enabled = true;
        //        }
        //    }
        //}
    }

    public void Launch() {
        m_springJoint2D.autoConfigureDistance = false;
        if (!launchToPoint && !autoConfigureDistance) {
            m_springJoint2D.distance = targetDistance;
            m_springJoint2D.frequency = targetFrequncy;
        }
        if (!launchToPoint) {
            if (autoConfigureDistance) {
                m_springJoint2D.autoConfigureDistance = true;
                m_springJoint2D.frequency = 0;
            }

            m_springJoint2D.connectedAnchor = grapplePoint;
            m_springJoint2D.enabled = true;
        } else {
            m_springJoint2D.connectedAnchor = grapplePoint;

            Vector2 distanceVector = firePoint.position - gunHolder.position;

            m_springJoint2D.distance = distanceVector.magnitude;
            m_springJoint2D.frequency = launchSpeed;
            m_springJoint2D.enabled = true;

        }
    }

    private void OnDrawGizmosSelected() {
        if (firePoint != null && hasMaxDistance) {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(firePoint.position, maxDistnace);
        }
    }
}
