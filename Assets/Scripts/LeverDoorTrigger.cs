using UnityEngine;

public class LeverDoorTrigger : MonoBehaviour
{
    [Header("Lever")]
    public HingeJoint leverHinge;

    [Header("Door")]
    public Transform doorTransform;
    public float doorOpenAngle = 120f;

    [Header("Trigger Logic")]
    public float openTriggerAngle = 30f;
    public float resetAngle = 5f;

    [Header("Audio")]
    public AudioClip doorOpenClip;
    public AudioClip doorCloseClip;
    public AudioSource leverAudioSource;

    private Quaternion doorClosedRot;
    private Quaternion doorOpenRot;

    private bool doorOpened = false;

    void Start()
    {
        doorClosedRot = doorTransform.localRotation;
        doorOpenRot = Quaternion.Euler(
            doorClosedRot.eulerAngles + new Vector3(0, doorOpenAngle, 0)
        );
    }

    void Update()
    {
        float angle = leverHinge.angle;

        if (leverAudioSource != null)
        {
            if (Mathf.Abs(angle) > 2f && !leverAudioSource.isPlaying)
                leverAudioSource.Play();
            else if (Mathf.Abs(angle) <= 2f)
                leverAudioSource.Stop();
        }

        AudioSource doorAudio = doorTransform.GetComponent<AudioSource>();

        // 拉下超過 Open Trigger Angle → 開門
        if (!doorOpened && angle > openTriggerAngle)
        {
            doorTransform.localRotation = doorOpenRot;
            if (doorAudio && doorOpenClip)
            {
                doorAudio.clip = doorOpenClip;
                doorAudio.Play();
            }
            doorOpened = true;
        }
        // 拉桿回到 Reset Angle 以內（快回原位）→ 關門
        else if (doorOpened && angle < resetAngle)
        {
            doorTransform.localRotation = doorClosedRot;
            if (doorAudio && doorCloseClip)
            {
                doorAudio.clip = doorCloseClip;
                doorAudio.Play();
            }
            doorOpened = false;
        }
    }
}
