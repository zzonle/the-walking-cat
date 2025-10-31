using UnityEngine;

public class CamaraController : MonoBehaviour
{
    [Header("Sensibilidad del Mouse")]
    [SerializeField] private float sensibilidad = 100f;
    
    [Header("Límites de rotación vertical")]
    [SerializeField] private float limiteVerticalInferior = -90f;
    [SerializeField] private float limiteVerticalSuperior = 90f;
    
    [Header("Configuración Primera Persona")]
    [SerializeField] private float alturaOjos = 0.5f;

    [Header("Brazos FPS")]
    [SerializeField] private Transform brazosContainer;
    [SerializeField] private Vector3 posicionBrazos = new Vector3(0.07f, -1.291397f, 0.6726489f);

    [Header("Animación Caminar")]
    [SerializeField] private float balanceoCaminarIntensidad = 0.03f;
    [SerializeField] private float balanceoCaminarVelocidad = 10f;

    [Header("Animación Correr")]
    [SerializeField] private float balanceoCorrerIntensidad = 0.06f;
    [SerializeField] private float balanceoCorrerVelocidad = 15f;

    [Header("Animación Saltar")]
    [SerializeField] private float alturaRetroceso = -0.15f;

    [Header("Referencias")]
    [SerializeField] private Transform Player;

    private float rotacionHorizontal = 0f;
    private float rotacionVertical = 0f;
    private float tiempoBalanceo = 0f;
    private Vector3 posicionInicialBrazos;
    private Vector3 posicionObjetivoBrazos;

    // Estado actual de movimiento
    private EstadoMovimiento estadoActual = EstadoMovimiento.Quieto;

    public enum EstadoMovimiento
    {
        Quieto,
        Caminando,
        Corriendo,
        Saltando
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        rotacionHorizontal = Player.eulerAngles.y;

        if (brazosContainer != null)
        {
            brazosContainer.SetParent(transform);
            brazosContainer.localPosition = posicionBrazos;
            brazosContainer.localRotation = Quaternion.identity;
            posicionInicialBrazos = brazosContainer.localPosition;
            posicionObjetivoBrazos = posicionInicialBrazos;
        }
    }

    void LateUpdate()
    {
        float valorX = Input.GetAxis("Mouse X") * sensibilidad * Time.deltaTime; 
        float valorY = Input.GetAxis("Mouse Y") * sensibilidad * Time.deltaTime;
        
        rotacionHorizontal += valorX;
        rotacionVertical = Mathf.Clamp(rotacionVertical - valorY, limiteVerticalInferior, limiteVerticalSuperior);
        
        Player.rotation = Quaternion.Euler(0f, rotacionHorizontal, 0f);
        transform.position = Player.position + Vector3.up * alturaOjos;
        transform.rotation = Quaternion.Euler(rotacionVertical, rotacionHorizontal, 0f);

        AnimarBrazos();
    }

    // Método público para cambiar el estado desde PlayerMovement
    public void CambiarEstado(EstadoMovimiento nuevoEstado)
    {
        estadoActual = nuevoEstado;
        
        if (nuevoEstado == EstadoMovimiento.Quieto)
        {
            tiempoBalanceo = 0f;
        }
    }

    private void AnimarBrazos()
    {
        if (brazosContainer == null) return;

        switch (estadoActual)
        {
            case EstadoMovimiento.Quieto:
                AnimarQuieto();
                break;
            
            case EstadoMovimiento.Caminando:
                AnimarCaminando();
                break;
            
            case EstadoMovimiento.Corriendo:
                AnimarCorriendo();
                break;
            
            case EstadoMovimiento.Saltando:
                AnimarSaltando();
                break;
        }

        // Aplicar la posición suavemente
        brazosContainer.localPosition = Vector3.Lerp(
            brazosContainer.localPosition, 
            posicionObjetivoBrazos, 
            Time.deltaTime * 10f
        );
    }

    private void AnimarQuieto()
    {
        tiempoBalanceo = 0f;
        posicionObjetivoBrazos = posicionInicialBrazos;
    }

    private void AnimarCaminando()
    {
        tiempoBalanceo += Time.deltaTime * balanceoCaminarVelocidad;
        
        float balanceoX = Mathf.Sin(tiempoBalanceo) * balanceoCaminarIntensidad;
        float balanceoY = Mathf.Cos(tiempoBalanceo * 2) * balanceoCaminarIntensidad * 0.5f;
        
        posicionObjetivoBrazos = posicionInicialBrazos + new Vector3(balanceoX, balanceoY, 0f);
    }

    private void AnimarCorriendo()
    {
        tiempoBalanceo += Time.deltaTime * balanceoCorrerVelocidad;
        
        float balanceoX = Mathf.Sin(tiempoBalanceo) * balanceoCorrerIntensidad;
        float balanceoY = Mathf.Cos(tiempoBalanceo * 2) * balanceoCorrerIntensidad * 0.5f;
        float balanceoZ = Mathf.Sin(tiempoBalanceo * 2) * balanceoCorrerIntensidad * 0.3f;
        
        posicionObjetivoBrazos = posicionInicialBrazos + new Vector3(balanceoX, balanceoY, balanceoZ);
    }

    private void AnimarSaltando()
    {
        // Retroceso hacia abajo cuando salta
        posicionObjetivoBrazos = posicionInicialBrazos + new Vector3(0f, alturaRetroceso, 0f);
    }
}