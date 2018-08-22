using UnityEngine;
using System.Collections;

public class StageDirector : MonoBehaviour
{
    // Control options.
    public bool ignoreFastForward = true;

    public RuntimeAnimatorController animatorController;

    // Prefabs.
    public GameObject spray;
    public GameObject musicPlayerPrefab;
    public GameObject mainCameraRigPrefab;
    public GameObject[] prefabsNeedsActivation;
    public GameObject[] miscPrefabs;

    // Camera points.
    public Transform[] cameraPoints;

    // Exposed to animator.
    public float overlayIntensity = 1.0f;

    // Objects to be controlled.
    GameObject musicPlayer;
    CameraSwitcher mainCameraSwitcher;
    ScreenOverlay[] screenOverlays;
    GameObject[] objectsNeedsActivation;
    GameObject[] objectsOnTimeline;

    void Awake()
    {
        // Instantiate the prefabs.
        musicPlayer = (GameObject)Instantiate(musicPlayerPrefab);

        var cameraRig = (GameObject)Instantiate(mainCameraRigPrefab);
        mainCameraSwitcher = cameraRig.GetComponentInChildren<CameraSwitcher>();
        screenOverlays = cameraRig.GetComponentsInChildren<ScreenOverlay>();

        objectsNeedsActivation = new GameObject[prefabsNeedsActivation.Length];
        for (var i = 0; i < prefabsNeedsActivation.Length; i++)
            objectsNeedsActivation[i] = (GameObject)Instantiate(prefabsNeedsActivation[i]);

        var path = Application.dataPath + "/vrmModels/model.vrm";
        var go = VRM.VRMImporter.LoadFromPath(path);
        var animator = go.GetComponent<Animator>();
        animator.runtimeAnimatorController = animatorController;
        animator.applyRootMotion = true;

        var cameraTarget = Instantiate(new GameObject("Empty")) as GameObject;
        cameraTarget.name = "CameraTarget";
        cameraTarget.transform.parent = animator.GetBoneTransform(HumanBodyBones.Head);
        cameraTarget.transform.position = cameraTarget.transform.parent.transform.position;

        var leftSpray = Instantiate(spray) as GameObject;
        leftSpray.transform.parent = animator.GetBoneTransform(HumanBodyBones.LeftHand);
        leftSpray.transform.position = leftSpray.transform.parent.transform.position;

        var rightSpray = Instantiate(spray);
        rightSpray.transform.parent = animator.GetBoneTransform(HumanBodyBones.LeftHand);
        rightSpray.transform.position = rightSpray.transform.parent.transform.position;

        foreach (var p in miscPrefabs) Instantiate(p);
    }

    void Update()
    {
        foreach (var so in screenOverlays)
        {
            so.intensity = overlayIntensity;
            so.enabled = overlayIntensity > 0.01f;
        }
    }

    public void StartMusic()
    {
        foreach (var source in musicPlayer.GetComponentsInChildren<AudioSource>())
            source.Play();
    }

    public void ActivateProps()
    {
        foreach (var o in objectsNeedsActivation) o.BroadcastMessage("ActivateProps");
    }

    public void SwitchCamera(int index)
    {
        if (mainCameraSwitcher)
            mainCameraSwitcher.ChangePosition(cameraPoints[index], true);
    }

    public void StartAutoCameraChange()
    {
        if (mainCameraSwitcher)
            mainCameraSwitcher.StartAutoChange();
    }

    public void StopAutoCameraChange()
    {
        if (mainCameraSwitcher)
            mainCameraSwitcher.StopAutoChange();
    }

    public void FastForward(float second)
    {
        if (!ignoreFastForward)
        {
            FastForwardAnimator(GetComponent<Animator>(), second, 0);
            foreach (var go in objectsOnTimeline)
                foreach (var animator in go.GetComponentsInChildren<Animator>())
                    FastForwardAnimator(animator, second, 0.5f);
        }
    }

    void FastForwardAnimator(Animator animator, float second, float crossfade)
    {
        for (var layer = 0; layer < animator.layerCount; layer++)
        {
            var info = animator.GetCurrentAnimatorStateInfo(layer);
            if (crossfade > 0.0f)
                animator.CrossFade(info.nameHash, crossfade / info.length, layer, info.normalizedTime + second / info.length);
            else
                animator.Play(info.nameHash, layer, info.normalizedTime + second / info.length);
        }
    }

    public void EndPerformance()
    {
        Application.LoadLevel(0);
    }
}
