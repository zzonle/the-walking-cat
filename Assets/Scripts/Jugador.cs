using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Velocidades")]
    [SerializeField] private float velocidadCaminar = 5f;
    [SerializeField] private float velocidadCorrer = 8f;
    [SerializeField] private float suavizadoMovimiento = 10f;
    
    [Header("Salto")]
    [SerializeField] private float fuerzaSalto = 5f;
    [SerializeField] private float gravedad = -9.81f;
    
    [Header("Detección de suelo")]
    [SerializeField] private Transform checkSuelo;
    [SerializeField] private float radioCheckSuelo = 0.4f;
    [SerializeField] private LayerMask capaSuelo;
    
    [Header("Teclas")]
    [SerializeField] private KeyCode teclaCorrer = KeyCode.LeftShift;
    [SerializeField] private KeyCode teclaSaltar = KeyCode.Space;
    
    [Header("Referencias")]
    [SerializeField] private CamaraController camaraController; // NUEVA REFERENCIA
    
    // Referencias
    private CharacterController controller;
    
    // Variables de movimiento
    private Vector3 velocidadActual;
    private Vector3 velocidadCaida;
    private bool estaEnSuelo;
    private bool estabaSaltando = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        
        if (controller == null)
        {
            Debug.LogError("¡El PlayerMovement necesita un CharacterController!");
        }

        // Buscar la cámara automáticamente si no está asignada
        if (camaraController == null)
        {
            camaraController = GetComponentInChildren<CamaraController>();
            if (camaraController == null)
            {
                Debug.LogWarning("No se encontró CamaraController. Las animaciones de brazos no funcionarán.");
            }
        }
    }

    void Update()
    {
        DetectarSuelo();
        ManejarMovimiento();
        ManejarSalto();
        ActualizarEstadoCamara(); // NUEVO: Actualizar estado de animación
    }

    void DetectarSuelo()
    {
        if (checkSuelo != null)
        {
            estaEnSuelo = Physics.CheckSphere(checkSuelo.position, radioCheckSuelo, capaSuelo);
        }
        else
        {
            estaEnSuelo = controller.isGrounded;
        }
        
        if (estaEnSuelo && velocidadCaida.y < 0)
        {
            velocidadCaida.y = -2f;
            estabaSaltando = false; // Ya no está saltando
        }
    }

    void ManejarMovimiento()
    {
        float inputX = Input.GetAxis("Horizontal");
        float inputZ = Input.GetAxis("Vertical");
        
        Vector3 direccion = new Vector3(inputX, 0f, inputZ).normalized;
        Vector3 movimiento = transform.right * inputX + transform.forward * inputZ;
        movimiento = movimiento.normalized;
        
        float velocidadObjetivo = Input.GetKey(teclaCorrer) ? velocidadCorrer : velocidadCaminar;
        
        if (movimiento.magnitude >= 0.1f)
        {
            velocidadActual = Vector3.Lerp(velocidadActual, movimiento * velocidadObjetivo, suavizadoMovimiento * Time.deltaTime);
        }
        else
        {
            velocidadActual = Vector3.Lerp(velocidadActual, Vector3.zero, suavizadoMovimiento * Time.deltaTime);
        }
        
        controller.Move(velocidadActual * Time.deltaTime);
    }

    void ManejarSalto()
    {
        if (Input.GetKeyDown(teclaSaltar) && estaEnSuelo)
        {
            velocidadCaida.y = Mathf.Sqrt(fuerzaSalto * -2f * gravedad);
            estabaSaltando = true;
        }
        
        velocidadCaida.y += gravedad * Time.deltaTime;
        controller.Move(velocidadCaida * Time.deltaTime);
    }

    // NUEVO MÉTODO: Actualizar estado de la cámara
    void ActualizarEstadoCamara()
    {
        if (camaraController == null) return;

        // Si está saltando, prioridad máxima
        if (estabaSaltando || !estaEnSuelo)
        {
            camaraController.CambiarEstado(CamaraController.EstadoMovimiento.Saltando);
            return;
        }

        // Detectar si se está moviendo
        float inputX = Input.GetAxis("Horizontal");
        float inputZ = Input.GetAxis("Vertical");
        bool seEstaMooviendo = Mathf.Abs(inputX) > 0.1f || Mathf.Abs(inputZ) > 0.1f;

        // Determinar estado
        if (!seEstaMooviendo)
        {
            camaraController.CambiarEstado(CamaraController.EstadoMovimiento.Quieto);
        }
        else if (Input.GetKey(teclaCorrer))
        {
            camaraController.CambiarEstado(CamaraController.EstadoMovimiento.Corriendo);
        }
        else
        {
            camaraController.CambiarEstado(CamaraController.EstadoMovimiento.Caminando);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (checkSuelo != null)
        {
            Gizmos.color = estaEnSuelo ? Color.green : Color.red;
            Gizmos.DrawWireSphere(checkSuelo.position, radioCheckSuelo);
        }
    }
}