using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] float verticalOffset = 4f, shiftSpeed = 10f;
    [SerializeField] Transform playerCenter;
    [SerializeField] GameObject DeathScreen;
    [SerializeField] Animator screenCover;

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

        if (PlayerManager.Instance.IsDead())
        {
            MenuInteraction();
        }
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

    [SerializeField] List<Button> menuBtns = new List<Button>();
    public void ShowDeathScreen()
    {
        DeathScreen.SetActive(true);
        Invoke("ShowBtns", 1);
    }
    void ShowBtns()
    {
        foreach (var btn in menuBtns)
        {
            btn.gameObject.SetActive(true);
        }
    }

    int btnIndex = 0;
    void MenuInteraction()
    {
        if (DeathScreen.activeInHierarchy)
        {
            // new unity input system usage
            var gamepad = Gamepad.current;
            if (gamepad != null)
            {
                if (gamepad.dpad.up.wasPressedThisFrame)
                {
                    btnIndex = (btnIndex - 1 + menuBtns.Count) % menuBtns.Count;
                } else if (gamepad.dpad.down.wasPressedThisFrame)
                    {
                        btnIndex = ++btnIndex % menuBtns.Count;
                    }
            }

            // select a button
            EventSystem.current.SetSelectedGameObject(menuBtns[btnIndex].gameObject);

            if (PlayerInput.PressedJump())
            {
                // interact with button
                menuBtns[btnIndex].onClick.Invoke();
            }
        }
    }

    // UI Target
    public void ReturnToMenu()
    {
        DeathScreen.SetActive(false);
        screenCover.Play("toBlack");
        Invoke("LoadMainMenu", 4);
    }
    void LoadMainMenu() { SceneManager.LoadScene(0); }

    // UI Target
    public void ReloadGame()
    {
        DeathScreen.SetActive(false);
        screenCover.Play("toBlack");
        Invoke("LoadGame", 4);
    }
    void LoadGame() { SceneManager.LoadScene(1); }
}//EndScript