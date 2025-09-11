using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class headAimShoot : MonoBehaviour
{
    public Camera vrCam;              // arrastra aquí la Main Camera (hija de Camera Offset)
    public float range = 50f;
    public LayerMask hitMask = ~0;    // qué capas puede golpear

    // Disparar: clic izq. en Editor, tap en móvil (Input System o Input clásico)
    void Update()
    {
        bool fire = false;

        #if ENABLE_INPUT_SYSTEM
        // Nuevo Input System
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            fire = true;
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            fire = true;
        #else
        // Input clásico
        if (Input.GetMouseButtonDown(0)) fire = true;
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) fire = true;
        #endif

        if (!fire || vrCam == null) return;

        var ray = new Ray(vrCam.transform.position, vrCam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, range, hitMask))
        {
            // Feedback simple: si golpea algo, cámbiale el color
            var rend = hit.collider.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material.color = new Color(Random.value, Random.value, Random.value);
            }
            // Aquí podrías aplicar daño a tu sistema de enemigos:
            // hit.collider.GetComponent<Health>()?.ApplyDamage(10);
        }
    }
}

