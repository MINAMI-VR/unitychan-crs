﻿using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class WaitingRoom : MonoBehaviour
{
    public GameObject cameraPrefab;
    public GameObject characterPrefab;
    public float fadeTime = 0.5f;
    public RuntimeAnimatorController animatorContoroller;

    ScreenOverlay[] screenOverlays;
    float overlayIntensity = 1.0f;

    Animator animator;
    bool start;

    void Awake()
    {
        // Instantiate the camera.
        var go = (GameObject)Instantiate(cameraPrefab);
        screenOverlays = go.GetComponentsInChildren<ScreenOverlay>();

        var path = Application.dataPath + "/vrmModels/model.vrm";
        go = VRM.VRMImporter.LoadFromPath(path);
        animator = go.GetComponent<Animator>();
        animator.runtimeAnimatorController = animatorContoroller;
    }

    IEnumerator Start()
    {
        // Reset character animation repeatedly.
        while (true)
        {
            yield return new WaitForSeconds(3.0f);

            for (var layer = 0; layer < animator.layerCount; layer++)
            {
                var info = animator.GetCurrentAnimatorStateInfo(layer);
                animator.CrossFade(info.nameHash, 0.5f / info.length, layer, 0);
            }
        }
    }

    void Update()
    {
        start = Input.anyKeyDown;
        
        if (start)
        {
            // White out.
            overlayIntensity = Mathf.Min(1.0f, overlayIntensity + Time.deltaTime / fadeTime);
            if (overlayIntensity == 1.0f) Application.LoadLevel(1);
            SceneManager.LoadScene("Main");
        }
        else
        {
            // Fade in.
            overlayIntensity = Mathf.Max(0.0f, overlayIntensity - Time.deltaTime / fadeTime);
        }

        foreach (var so in screenOverlays)
        {
            so.intensity = overlayIntensity;
            so.enabled = overlayIntensity > 0.01f;
        }
    }
}
