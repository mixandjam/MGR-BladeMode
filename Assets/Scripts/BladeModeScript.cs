using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cinemachine;
using EzySlice;
using UnityEngine.Rendering.PostProcessing;

public class BladeModeScript : MonoBehaviour
{
    public bool bladeMode;

    private Animator anim;
    private MovementInput movement;
    private Vector3 normalOffset;
    public Vector3 zoomOffset;
    private float normalFOV;
    public float zoomFOV = 15;

    public Transform cutPlane;

    public CinemachineFreeLook TPCamera;

    public Material crossMaterial;
    private CinemachineComposer[] composers;

    public LayerMask layerMask;
    ParticleSystem[] particles;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        cutPlane.gameObject.SetActive(false);

        anim = GetComponent<Animator>();
        movement = GetComponent<MovementInput>();
        normalFOV = TPCamera.m_Lens.FieldOfView;
        composers = new CinemachineComposer[3];
        for (int i = 0; i < 3; i++)
            composers[i] = TPCamera.GetRig(i).GetCinemachineComponent<CinemachineComposer>();
        normalOffset = composers[0].m_TrackedObjectOffset;

        particles = cutPlane.GetComponentsInChildren<ParticleSystem>();
    }

    void Update()
    {
        anim.SetFloat("x", Mathf.Clamp(Camera.main.transform.GetChild(0).localPosition.x + 0.3f,-1,1));
        anim.SetFloat("y", Mathf.Clamp(Camera.main.transform.GetChild(0).localPosition.y + .18f, -1,1));

        if (Input.GetMouseButtonDown(1))
        {
            Zoom(true);
        }

        if (Input.GetMouseButtonUp(1))
        {
            Zoom(false);
        }

        if (bladeMode)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation,Camera.main.transform.rotation,.2f);
            RotatePlane();

            if (Input.GetMouseButtonDown(0))
            {
                cutPlane.GetChild(0).DOComplete();
                cutPlane.GetChild(0).DOLocalMoveX(cutPlane.GetChild(0).localPosition.x * -1, .05f).SetEase(Ease.OutExpo);
                ShakeCamera();
                Slice();
            }
        }

        Debug();
    }

    public void Slice()
    {
        Collider[] hits = Physics.OverlapBox(cutPlane.position, new Vector3(5, 0.1f, 5), cutPlane.rotation, layerMask);

        if (hits.Length <= 0)
            return;

        for (int i = 0; i < hits.Length; i++)
        {
            SlicedHull hull = SliceObject(hits[i].gameObject, crossMaterial);
            if (hull != null)
            {
                GameObject bottom = hull.CreateLowerHull(hits[i].gameObject, crossMaterial);
                GameObject top = hull.CreateUpperHull(hits[i].gameObject, crossMaterial);
                AddHullComponents(bottom);
                AddHullComponents(top);
                Destroy(hits[i].gameObject);
            }
        }
    }

    public void AddHullComponents(GameObject go)
    {
        go.layer = 9;
        Rigidbody rb = go.AddComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        MeshCollider collider = go.AddComponent<MeshCollider>();
        collider.convex = true;

        rb.AddExplosionForce(100, go.transform.position, 20);
    }

    public SlicedHull SliceObject(GameObject obj, Material crossSectionMaterial = null)
    {
        // slice the provided object using the transforms of this object
        if (obj.GetComponent<MeshFilter>() == null)
            return null;

        return obj.Slice(cutPlane.position, cutPlane.up, crossSectionMaterial);
    }

    public void Zoom(bool state)
    {
        bladeMode = state;
        anim.SetBool("bladeMode", bladeMode);

        cutPlane.localEulerAngles = Vector3.zero;
        cutPlane.gameObject.SetActive(state);

        string x = state ? "Horizontal" : "Mouse X"; string y = state ? "Vertical" : "Mouse Y";
        TPCamera.m_XAxis.m_InputAxisName = x; TPCamera.m_YAxis.m_InputAxisName = y;

        float fov = state ? zoomFOV : normalFOV;
        Vector3 offset = state ? zoomOffset : normalOffset;
        float timeScale = state ? .2f : 1;

        DOVirtual.Float(Time.timeScale, timeScale, .02f, SetTimeScale);
        DOVirtual.Float(TPCamera.m_Lens.FieldOfView, fov, .1f, FieldOfView);
        DOVirtual.Float(composers[0].m_TrackedObjectOffset.x, offset.x, .2f, CameraOffset).SetUpdate(true);

        //Stop character movement
        movement.enabled = !state;

        if (state)
        {
            //GetComponent<Animator>().SetTrigger("draw");
        }
        else
        {
            transform.DORotate(new Vector3(0, transform.eulerAngles.y, 0), .2f);
        }

        //POST PROCESSING
        float vig = state ? .6f : 0;
        float chrom = state ? 1 : 0;
        float depth = state ? 4.8f : 8;
        float vig2 = state ? 0f : .6f;
        float chrom2 = state ? 0 : 1;
        float depth2 = state ? 8 : 4.8f;
        DOVirtual.Float(chrom2, chrom, .1f, Chromatic);
        DOVirtual.Float(vig2, vig, .1f, Vignette);
        DOVirtual.Float(depth2, depth, .1f, DepthOfField);
    }

    public void RotatePlane()
    {
        cutPlane.eulerAngles += new Vector3(0, 0, -Input.GetAxis("Mouse X") * 5);
    }

    void FieldOfView(float fov)
    {
        TPCamera.m_Lens.FieldOfView = fov;
    }

    void CameraOffset(float x)
    {
        foreach (CinemachineComposer c in composers)
        {
            c.m_TrackedObjectOffset.Set(x, c.m_TrackedObjectOffset.y, c.m_TrackedObjectOffset.z);
        }
    }

    void SetTimeScale(float time)
    {
        Time.timeScale = time;
    }


    void Debug()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }

    public void ShakeCamera()
    {
        TPCamera.GetComponent<CinemachineImpulseSource>().GenerateImpulse();
        foreach (ParticleSystem p in particles)
        {
            p.Play();
        }
    }

    void Chromatic(float x)
    {
        Camera.main.GetComponentInChildren<PostProcessVolume>().profile.GetSetting<ChromaticAberration>().intensity.value = x;
    }

    void Vignette(float x)
    {
        Camera.main.GetComponentInChildren<PostProcessVolume>().profile.GetSetting<Vignette>().intensity.value = x;
    }

    void DepthOfField(float x)
    {
        Camera.main.GetComponentInChildren<PostProcessVolume>().profile.GetSetting<DepthOfField>().aperture.value = x;
    }
}
