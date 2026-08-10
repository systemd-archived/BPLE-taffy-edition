public abstract class INBehaviour
{
	public enum StatusCode
	{
		None,
		Building,
		Running
	}

	public virtual StatusCode Status { get; }

	public virtual void Awake()
	{
	}

	public virtual void Start()
	{
	}

	public virtual void FixedUpdate()
	{
	}

	public virtual void Update()
	{
	}

	public virtual void LateUpdate()
	{
	}

	public virtual void OnEnable()
	{
	}

	public virtual void OnDisable()
	{
	}

	public virtual void OnDestroy()
	{
	}
}
