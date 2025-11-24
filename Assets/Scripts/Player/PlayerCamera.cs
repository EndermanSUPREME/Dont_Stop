using System;
using System.Threading.Tasks;

using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] float verticalOffset = 4f, shiftSpeed = 10f;
    [SerializeField] Transform playerCenter;
    int strength = 1;
    Vector3 target, bossViewPoint;

    void Start()
    {
        _ = AlignCamera(); // run and forget about it
    }

    void Update()
    {
        target = playerCenter.position;
        target.z = Camera.main.transform.position.z;
        target.y += verticalOffset * strength;
    }

    public void Attach() { transform.SetParent(PlayerManager.Instance.transform); }
    public void Detach() { transform.SetParent(null); }

    Vector2 lastCameraPos = Vector2.zero;
    async Task AlignCamera()
    {
        while (!PlayerManager.Instance.IsDead())
        {
            if (PlayerManager.Instance.inBossFight)
            {
                ShowBossView();
            } else
            {
                // strength = (PlayerManager.Instance.grounded) ? 1 : 0;
                if (PlayerManager.Instance.grounded)
                {
                    lastCameraPos = new Vector2(0, Camera.main.transform.position.y + 1f);
                    strength = 1;
                } else
                    {
                        lastCameraPos.x = playerCenter.position.x;
                        float remainingDistance = Vector2.Distance(lastCameraPos, playerCenter.position);
                        if (remainingDistance < 0.1f && strength == 1)
                        {
                            strength = 0;
                        }
                    }
                FollowPlayer();
            }

            await Task.Yield();
        }
    }
    void FollowPlayer()
    {
        float remainingDistance = Vector3.Distance(Camera.main.transform.position, target);

        if (remainingDistance > 0.1f)
        {
            // allow for smooth transition
            Camera.main.transform.position = Vector3.Lerp(
                Camera.main.transform.position,
                target,
                shiftSpeed * Time.deltaTime
            );
        } else
            {
                // snap when distance is small
                Camera.main.transform.position = target;
            }
    }

    public void SetBossViewPoint(Vector3 pos)
    {
        bossViewPoint = new Vector3(pos.x, pos.y, Camera.main.transform.position.z);
    }

    void ShowBossView()
    {
        float remainingDistance = Vector3.Distance(Camera.main.transform.position, bossViewPoint);

        if (remainingDistance > 0.1f)
        {
            // allow for smooth transition
            Camera.main.transform.position = Vector3.Lerp(
                Camera.main.transform.position,
                bossViewPoint,
                shiftSpeed * 1.5f * Time.deltaTime
            );
        } else
            {
                // snap when distance is small
                Camera.main.transform.position = bossViewPoint;
            }
    }
}//EndScript