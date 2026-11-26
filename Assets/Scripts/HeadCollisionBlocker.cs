using UnityEngine;
using Unity.XR.CoreUtils;

// 放在 XR Origin (XR Rig) 上。
// 頭（Camera）碰到門、牆這類固定的碰撞體時，把整個 XR Origin 推回去，讓玩家不能穿過去。
// 會動的物件（有非 Kinematic Rigidbody 的，例如鑰匙、拉桿）和 Trigger 不會擋人。
public class HeadCollisionBlocker : MonoBehaviour
{
    [Tooltip("頭的位置，留空會自動抓 XR Origin 的 Camera")]
    public Transform head;

    [Tooltip("頭的碰撞半徑（公尺），越大離牆越遠就會被擋")]
    public float headRadius = 0.25f;

    [Tooltip("哪些 Layer 會擋人")]
    public LayerMask blockingLayers = ~0;

    private readonly Collider[] hits = new Collider[16];

    void Start()
    {
        if (head == null)
        {
            XROrigin origin = GetComponent<XROrigin>();
            if (origin != null && origin.Camera != null) head = origin.Camera.transform;
            else if (Camera.main != null) head = Camera.main.transform;
        }
    }

    void LateUpdate()
    {
        if (head == null) return;

        Vector3 p = head.position;
        int count = Physics.OverlapSphereNonAlloc(p, headRadius, hits, blockingLayers, QueryTriggerInteraction.Ignore);
        Vector3 push = Vector3.zero;

        for (int i = 0; i < count; i++)
        {
            Collider c = hits[i];
            if (c.transform.IsChildOf(transform)) continue;                                  // 自己身上的
            if (c.attachedRigidbody != null && !c.attachedRigidbody.isKinematic) continue;   // 鑰匙、拉桿
            if (c is MeshCollider mc && !mc.convex) continue;                                // 地板（Plane）

            Vector3 closest = c.ClosestPoint(p);
            Vector3 diff = p - closest;
            diff.y = 0f;
            float dist = diff.magnitude;
            if (dist < 0.0001f) continue;
            if (dist < headRadius) push += diff / dist * (headRadius - dist);
        }

        if (push != Vector3.zero) transform.position += push;
    }
}
