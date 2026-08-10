using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodingGrapplingHookProjectile : WPFMonoBehaviour
{
	public Action OnExplosion;

	[SerializeField]
	private float m_explosionImpulse;

	[SerializeField]
	private float m_explosionRadius;

	[SerializeField]
	private float m_force;

	[SerializeField]
	private float m_ttl;

	[SerializeField]
	private GameObject m_smokeCloud;

	private bool m_triggered;

	private Renderer m_renderer;

	private Vector3 m_forceDirection;

	private int m_explosionCount;

	private void Start()
	{
		m_triggered = false;
		m_explosionCount = INSettings.GetInt(INFeature.GunProjectileExplosionCount);
		m_renderer = GetComponentInChildren<Renderer>();
		EventManager.Connect<UIEvent>(OnUIEvent);
		m_forceDirection = base.transform.parent.TransformDirection(Vector3.right);
		base.rigidbody.AddForceAtPosition(m_forceDirection * m_force * INSettings.GetFloat(INFeature.GunProjectileSpeed), Vector3.zero, ForceMode.Impulse);
		StartCoroutine(TTL(m_ttl * INSettings.GetFloat(INFeature.GunProjectileExplosionTime)));
		Explode();
		Explode();
	}

	private void OnDestroy()
	{
		EventManager.Disconnect<UIEvent>(OnUIEvent);
	}

	private void OnCollisionEnter(Collision collision)
	{
		Explode();
	}

	public void Explode()
	{
		if (m_triggered)
		{
			return;
		}
		m_explosionCount = 0;
		if (m_explosionCount <= 0)
		{
			m_triggered = true;
		}
		float num = 96f;
		float num2 = 22.5f;
		Vector3 normalized = m_forceDirection.normalized;
		Collider[] array = Physics.OverlapSphere(base.transform.position, num);
		HashSet<Rigidbody> hashSet = new HashSet<Rigidbody>();
		Collider[] array2 = array;
		foreach (Collider collider in array2)
		{
			Vector3 to = collider.transform.position - base.transform.position;
			if (!(to.magnitude <= num) || !(Vector3.Angle(normalized, to) <= num2))
			{
				continue;
			}
			GameObject gameObject = FindParentWithRigidBody(collider.gameObject);
			if (gameObject != null)
			{
				Rigidbody component = gameObject.GetComponent<Rigidbody>();
				if (component != null && !hashSet.Contains(component))
				{
					hashSet.Add(component);
					component.velocity = Vector3.zero;
					component.angularVelocity = Vector3.zero;
					component.velocity = new Vector3(0f, -200f, 0f);
				}
			}
			BasePart component2 = collider.GetComponent<BasePart>();
			TNT tNT = component2 as TNT;
			if (tNT != null && !component2.HasGeneratorRef)
			{
				tNT.Explode();
			}
			if (INSettings.GetBool(INFeature.BlasterTNT))
			{
				BlasterTNT blasterTNT = component2 as BlasterTNT;
				if (blasterTNT != null && !component2.HasGeneratorRef && Vector.DistanceSquared2(base.transform.position, component2.transform.position) < 4f)
				{
					blasterTNT.ExplodeSpecial();
				}
			}
		}
		WPFMonoBehaviour.effectManager.CreateParticles(m_smokeCloud, base.transform.position - Vector3.forward * 12f, force: true);
		Singleton<AudioManager>.Instance.SpawnOneShotEffect(WPFMonoBehaviour.gameData.commonAudioCollection.tntExplosion, base.transform.position);
		StartCoroutine(ShineLight());
		if (OnExplosion != null)
		{
			OnExplosion();
			OnExplosion = null;
		}
	}

	private int CountChildColliders(GameObject obj, int count)
	{
		if ((bool)obj.GetComponent<Collider>())
		{
			count++;
		}
		for (int i = 0; i < obj.transform.childCount; i++)
		{
			count = CountChildColliders(obj.transform.GetChild(i).gameObject, count);
		}
		return count;
	}

	private GameObject FindParentWithRigidBody(GameObject obj)
	{
		if ((bool)obj.GetComponent<Rigidbody>())
		{
			return obj;
		}
		if ((bool)obj.transform.parent)
		{
			return FindParentWithRigidBody(obj.transform.parent.gameObject);
		}
		return null;
	}

	private void AddExplosionForce(GameObject target, float forceFactor)
	{
		Vector3 vector = target.transform.position - base.transform.position;
		float f = Mathf.Max(vector.magnitude, 1f);
		float num = forceFactor * m_explosionImpulse / Mathf.Pow(f, 1.5f);
		Rigidbody component = target.GetComponent<Rigidbody>();
		if (component.mass < 0.1f)
		{
			num *= component.mass;
		}
		else if (component.mass < 0.4f)
		{
			num *= component.mass / 0.4f;
		}
		component.AddForce(num * vector.normalized, ForceMode.Impulse);
	}

	private IEnumerator ShineLight()
	{
		PointLightSource pls = GetComponentInChildren<PointLightSource>();
		if ((bool)pls)
		{
			if (m_renderer != null)
			{
				m_renderer.enabled = false;
			}
			pls.onLightTurnOff = (Action)Delegate.Combine(pls.onLightTurnOff, (Action)delegate
			{
				base.gameObject.SetActive(value: false);
			});
			pls.isEnabled = true;
			yield return new WaitForSeconds(pls.turnOnCurve[pls.turnOnCurve.length - 1].time);
			pls.isEnabled = false;
		}
		if (m_explosionCount <= 0)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private IEnumerator TTL(float ttl)
	{
		float current = 0f;
		float coolingTime = INSettings.GetFloat(INFeature.GunProjectileCoolingTime);
		while (current < ttl)
		{
			if (OnExplosion != null && current >= coolingTime)
			{
				OnExplosion();
				OnExplosion = null;
			}
			current += Time.deltaTime;
			yield return null;
		}
		while (!m_triggered)
		{
			Explode();
		}
	}

	private void OnUIEvent(UIEvent data)
	{
		if (data.type == UIEvent.Type.Building)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
