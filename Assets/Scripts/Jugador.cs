using UnityEngine;



public class PlayerMovement : MonoBehaviour
{
    [Header("Velocidades")]
    [SerializeField] private float velocidadCaminar = 5f;
    [SerializeField] private float velocidadCorrer = 8f;
    [SerializeField] private float suavizadoMovimiento = 10f; // Qué tan suave es el movimiento
    
    [Header("Salto")]
    [SerializeField] private float fuerzaSalto = 5f;
    [SerializeField] private float gravedad = -9.81f;
    
    [Header("Detección de suelo")]
    [SerializeField] private Transform checkSuelo; // Punto para detectar el suelo
    [SerializeField] private float radioCheckSuelo = 0.4f;
    [SerializeField] private LayerMask capaSuelo; // Qué capas son consideradas "suelo"
    
    [Header("Teclas")]
    [SerializeField] private KeyCode teclaCorrer = KeyCode.LeftShift;
    [SerializeField] private KeyCode teclaSaltar = KeyCode.Space;
    
    // Referencias
    private CharacterController controller;
    
    // Variables de movimiento
    private Vector3 velocidadActual;
    private Vector3 velocidadCaida;
    private bool estaEnSuelo;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        
        // Verificación de seguridad
        if (controller == null)
        {
            Debug.LogError("¡El PlayerMovement necesita un CharacterController!");
        }
    }

    void Update()
    {
        DetectarSuelo();
        ManejarMovimiento();
        ManejarSalto();
    }

    void DetectarSuelo()
    {
        // Detecta si hay suelo debajo del jugador
        if (checkSuelo != null)
        {
            estaEnSuelo = Physics.CheckSphere(checkSuelo.position, radioCheckSuelo, capaSuelo);
        }
        else
        {
            // Si no hay checkSuelo configurado, usa el CharacterController
            estaEnSuelo = controller.isGrounded;
        }
        
        // Resetea la velocidad de caída al tocar el suelo
        if (estaEnSuelo && velocidadCaida.y < 0)
        {
            velocidadCaida.y = -2f; // Un pequeño valor negativo para mantener pegado al suelo
        }
    }

    void ManejarMovimiento()
    {
        // Capturar input del jugador
        float inputX = Input.GetAxis("Horizontal"); // A/D o Flechas
        float inputZ = Input.GetAxis("Vertical");   // W/S o Flechas
        
        // Crear vector de dirección en espacio local
        Vector3 direccion = new Vector3(inputX, 0f, inputZ).normalized;
        
        // Convertir la dirección a espacio mundial (basado en la rotación del jugador)
        Vector3 movimiento = transform.right * inputX + transform.forward * inputZ;
        movimiento = movimiento.normalized;
        
        // Determinar velocidad (caminar o correr)
        float velocidadObjetivo = Input.GetKey(teclaCorrer) ? velocidadCorrer : velocidadCaminar;
        
        // Aplicar velocidad solo si hay input
        if (movimiento.magnitude >= 0.1f)
        {
            velocidadActual = Vector3.Lerp(velocidadActual, movimiento * velocidadObjetivo, suavizadoMovimiento * Time.deltaTime);
        }
        else
        {
            // Frenar suavemente
            velocidadActual = Vector3.Lerp(velocidadActual, Vector3.zero, suavizadoMovimiento * Time.deltaTime);
        }
        
        // Mover el jugador
        controller.Move(velocidadActual * Time.deltaTime);
    }

    void ManejarSalto()
    {
        // Saltar
        if (Input.GetKeyDown(teclaSaltar) && estaEnSuelo)
        {
            velocidadCaida.y = Mathf.Sqrt(fuerzaSalto * -2f * gravedad);
        }
        
        // Aplicar gravedad
        velocidadCaida.y += gravedad * Time.deltaTime;
        
        // Aplicar velocidad de caída
        controller.Move(velocidadCaida * Time.deltaTime);
    }

    // Visualización en el editor
    void OnDrawGizmosSelected()
    {
        if (checkSuelo != null)
        {
            Gizmos.color = estaEnSuelo ? Color.green : Color.red;
            Gizmos.DrawWireSphere(checkSuelo.position, radioCheckSuelo);
        }
    }
}