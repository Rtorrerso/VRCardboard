
# Creando VR con Cardboard

## PASO 1 — Instalar y activar Cardboard (Android)
Creamos un nuevo proyecto con 3D.  

Instala el paquete:  
`Window → Package Manager → + → Add package from Git URL…`  
Pega: `https://github.com/googlevr/cardboard-xr-plugin.git` y acepta.

> **NOTA:** debes tener Git instalado. Si no, descárgalo en [https://git-scm.com/downloads](https://git-scm.com/downloads) e instálalo. Después reinicia el equipo.  
> Algunas veces pedirá reiniciar el proyecto por los nuevos inputs.

Versiones menores a 6.0:  
`Window → Package Manager → busca OpenXR Plugin (o “OpenXR”) e instálalo (si no está).`

**Activa XR para Android:**  
`Edit → Project Settings → XR Plug-in Management`  
Pestaña Android → marca **Cardboard XR Plugin**.

**Habilita el runtime simulado (si aparece):**  
`Edit → Project Settings → XE Plug-in (panel de OpenXR)`  
En Features, habilita **Mock HMD**.

**Configura Android como plataforma de build:**  
`File → Build Settings → Android → Switch Platform`

**Ajustes mínimos de Player (Android):**  
- **Graphics APIs:** deja OpenGLES3 (quita Vulkan si aparece).  
- **Minimum API Level:** Android 8.0 (API 26) o superior.  
- **Active Input Handling:** Both (si usas el nuevo Input System).  

Para **Standalone**:  
`Edit → Project Settings → Player → Standalone:`  
- Desmarca Auto Graphics API for Windows.  
- Deja Direct3D11 en la lista y elimina Vulkan/OpenGL.  

> Si usas URP, habilita **Depth Texture** en la cámara.

**Checklist:**
- Google Cardboard XR Plugin sin errores en la consola.
- XR Plug-in Management → Cardboard marcado.
- Build Settings → plataforma Android activa.
- Player → OpenGLES3 activo, API ≥ 26, Input Handling = Both.

---

## PASO 2 — Escena mínima estereoscópica (Cardboard)
**Objetivo:** ver pantalla dividida (Both Eyes) en Game View.  

1. Crea una escena vacía `File → New Scene → 3D`.  
2. Agrega el rig XR: `GameObject → XR → XR Origin (VR)`.  
3. Verifica que la Main Camera esté dentro de XR Origin.  
4. Configura la cámara:
   - **Clear Flags:** Skybox o Solid Color.
   - **Clipping Planes:** Near 0.01–0.03, Far 100.
   - **Target Eye:** Both.  
5. Agrega referencias visuales: un Plane y un Cube al frente.  
6. Pulsa Play → selecciona Both Eyes en Game View.

**Checklist Paso 2:**  
- Ves doble render (una vista por ojo).
- El menú Both/Left/Right aparece.
- XR está activo y cámara correcta.

---

## PASO 3 — “Head-aim + Tap” para disparar
**Objetivo:** disparar mirando y tocando/clic.  

1. Crea un Cube en `(0, 1, 5)` llamado `Target`, con BoxCollider y material rojo.  
2. Crea el script `HeadAimShoot.cs`:

```csharp
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class HeadAimShoot : MonoBehaviour
{
    public Camera vrCam;
    public float range = 50f;
    public LayerMask hitMask = ~0;

    void Update()
    {
        bool fire = false;
        #if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            fire = true;
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            fire = true;
        #else
        if (Input.GetMouseButtonDown(0)) fire = true;
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) fire = true;
        #endif

        if (!fire || vrCam == null) return;

        var ray = new Ray(vrCam.transform.position, vrCam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, range, hitMask))
        {
            var rend = hit.collider.GetComponent<Renderer>();
            if (rend != null)
                rend.material.color = new Color(Random.value, Random.value, Random.value);
        }
    }
}
```

3. Arrastra el script a un GO (Gameplay/XRRig).  
4. Asigna Main Camera en el campo `vrCam`.  
5. Crea Canvas World Space con un punto (retícula).

**Checklist Paso 3:**
- Al dar clic/tap el cubo cambia de color.
- La retícula permanece al centro.

---

## PASO 4 — HUD de vida/munición
1. Crea un Canvas en World Space → hijo de Main Camera.  
2. Agrega `Text (TMP)` → Texto = "Vida: 100".  
3. Script `PlayerHUD.cs`:

```csharp
using UnityEngine;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    public int vida = 100;
    public int municion = 30;
    public TMP_Text hudText;

    void Update()
    {
        if (hudText != null)
            hudText.text = $"Vida: {vida}
Munición: {municion}";
    }

    public void RecibirDaño(int dmg) { vida = Mathf.Max(0, vida - dmg); }
    public void Disparar() { if (municion > 0) municion--; }
}
```

---

## PASO 5 — Movimiento (PC flechas / Android tocar)
1. Agrega `CharacterController` al XRRig (Height 1.7, Radius 0.25).  
2. Script `VRLocomotion.cs` (movimiento dual y gravedad).  
3. Ajusta `Min Move Distance = 0.001` para fluidez.  
4. Usa giro suave con flechas reemplazando snap turn.

**Checklist Paso 5:**  
- Flechas ↑↓ mueven, ←→ giran suavemente.  
- No atraviesas el suelo.  
- En Android avanzas al mantener tocado.

---

## PASO 6 — Confort y controles en móvil
### 6A) Viñeta de confort
Canvas + Image negro transparente → script `ComfortVignette.cs` que ajusta alpha según velocidad.

### 6B) Botones de giro
Crea dos botones en Canvas (⟲ ⟳), llama funciones de `MobileSnapTurn.cs`.

---

## PASO 7 — Pulido final
1. Retícula dinámica (`DynamicReticle.cs`) que cambia color/tamaño al apuntar.  
2. Límite de FPS con `FramerateLimiter.cs`.  
3. Estructura sugerida del proyecto:

```
/fpsCardboard
    /Directional Light
    /XRRig
        /Camera Offset
            /Main Camera
                /canvas
                    /image
    /target
        /canvas
            /vida (text)
    /Plane
```
