using UnityEngine;

public class HandMover : MonoBehaviour
{
    public bool IsHandMoving { get; set; }
    bool goingUp;

    void Update()
    {
        if (IsHandMoving)
        {
            if (FastApproximately(transform.localEulerAngles.x, 85f, 10f))
                goingUp = false;

            if (FastApproximately(transform.localEulerAngles.x, 22f, 10f))
                goingUp = true;


            if (!goingUp)
                transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(22f, -1.89f, -13.74f), 20f * Time.deltaTime);

            if (goingUp)
                transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(85f, -1.89f, -13.74f), 20f * Time.deltaTime);
        }
        else
        {
            transform.localRotation = Quaternion.Euler(75f, -1.89f, -13.74f);
            goingUp = false;
        }
    }

    bool FastApproximately(float a, float b, float threshold) => ((a - b) < 0 ? ((a - b) * -1) : (a - b)) <= threshold;
}
