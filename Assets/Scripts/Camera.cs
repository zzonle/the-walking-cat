using UnityEngine;

public class CamaraController : MonoBehaviour
{
    [Header("Sensibilidad del Mouse")]
    [SerializeField] private float sensibilidad = 100f;
    
    [Header("Límites de rotación vertical")]
    [SerializeField] private float limiteVerticalInferior = -10f;
    [SerializeField] private float limiteVerticalSuperior = 20f;
    
    [Header("Configuración Tercera Persona")]
    [SerializeField] private float distanciaDelJugador = 3f;
    [SerializeField] private float alturaDelJugador = 1.5f;
    [SerializeField] private float desplazamientoLateral = 1f; // Cuánto se desplaza en pantalla

    [Header("Cambio de hombro")]
    [SerializeField] private KeyCode teclaCambiarLado = KeyCode.C;
    [SerializeField] private float velocidadTransicion = 8f;

    [Header("Referencias")]
    [SerializeField] private Transform Player;

    private float rotacionHorizontal = 0f;
    private float rotacionVertical = 0f;
    private float ladoObjetivo = 1f; // 1 = derecha, -1 = izquierda
    private float ladoActual = 1f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        rotacionHorizontal = Player.eulerAngles.y;
    }

    void LateUpdate()
    {
        // Detectar cambio de lado
        if (Input.GetKeyDown(teclaCambiarLado))
        {
            ladoObjetivo *= -1;
        }

        // Transición 
        ladoActual = Mathf.Lerp(ladoActual, ladoObjetivo, velocidadTransicion * Time.deltaTime);

        // Capturar movimiento del mouse
        float valorX = Input.GetAxis("Mouse X") * sensibilidad * Time.deltaTime; 
        float valorY = Input.GetAxis("Mouse Y") * sensibilidad * Time.deltaTime;
        
        // Acumular y limitar rotaciones
        rotacionHorizontal += valorX;
        rotacionVertical = Mathf.Clamp(rotacionVertical - valorY, limiteVerticalInferior, limiteVerticalSuperior);
        
        // Rotar el jugador horizontalmente
        Player.rotation = Quaternion.Euler(0f, rotacionHorizontal, 0f);
        
        // Calcular rotación de la cámara (SOLO basada en la rotación, NO en el lado)
        Quaternion rotacionCamara = Quaternion.Euler(rotacionVertical, rotacionHorizontal, 0f);
        
        // CLAVE: Calcular offset lateral usando Player.right (eje local del jugador)
        Vector3 offsetLateral = Player.right * (desplazamientoLateral * ladoActual);
        Vector3 offsetAtras = Player.forward * -distanciaDelJugador;
        Vector3 offsetArriba = Vector3.up * alturaDelJugador;
        
        // Posición final de la cámara
        transform.position = Player.position + offsetLateral + offsetAtras + offsetArriba;
        
        // La cámara mira SIEMPRE hacia adelante del jugador (misma rotación)
        transform.rotation = rotacionCamara;
    }
}