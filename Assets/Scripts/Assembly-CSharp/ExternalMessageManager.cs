public class ExternalMessageManager : Singleton<ExternalMessageManager>
{
	public delegate void ExternalAppMessageReceived(string message);

	public static event ExternalAppMessageReceived onExternalAppMessageReceived;

	public void OnMessageReceived(string message)
	{
		if (onExternalAppMessageReceived != null)
		{
			onExternalAppMessageReceived(message);
		}
	}

	private void Awake()
	{
		SetAsPersistant();
	}
}
