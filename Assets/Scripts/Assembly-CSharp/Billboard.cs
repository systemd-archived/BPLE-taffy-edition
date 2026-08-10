using UnityEngine;

public class Billboard : MonoBehaviour
{
	public Transform m_upFrom;

	private Camera m_mainCamera;

	private void Start()
	{
		m_mainCamera = Camera.main;
	}

	public void LateUpdate()
	{
		if (m_mainCamera == null)
		{
			m_mainCamera = Camera.main;
			if (m_mainCamera == null) return;
		}
		Vector3 normalized = (base.transform.position - m_mainCamera.transform.position).normalized;
		Vector3 rhs = base.transform.right;
		if ((bool)m_upFrom)
		{
			rhs = Vector3.Cross(m_upFrom.up, normalized);
		}
		Vector3 normalized2 = Vector3.Cross(normalized, rhs).normalized;
		base.transform.rotation = Quaternion.LookRotation(normalized, normalized2);
	}
}
