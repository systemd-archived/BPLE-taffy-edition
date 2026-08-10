using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TNT : BasePart
{
	public float m_explosionImpulse;

	public float m_explosionRadius;

	public float m_triggerSpeed;

	[SerializeField]
	protected GameObject extraEffect;

	protected bool m_triggered;

	private GameObject m_leftAttachment;

	private GameObject m_rightAttachment;

	private GameObject m_topAttachment;

	private GameObject m_bottomAttachment;

	public GameObject smokeCloud;

	public override bool CanBeEnclosed()
	{
		return true;
	}

	public override void Awake()
	{
		base.Awake();
		Transform transform = base.transform.Find("LeftAttachment");
		Transform transform2 = base.transform.Find("RightAttachment");
		Transform transform3 = base.transform.Find("TopAttachment");
		Transform transform4 = base.transform.Find("BottomAttachment");
		if ((bool)transform)
		{
			m_leftAttachment = transform.gameObject;
			m_leftAttachment.SetActive(value: false);
		}
		if ((bool)transform2)
		{
			m_rightAttachment = transform2.gameObject;
			m_rightAttachment.SetActive(value: false);
		}
		if ((bool)transform3)
		{
			m_topAttachment = transform3.gameObject;
			m_topAttachment.SetActive(value: false);
		}
		if ((bool)transform4)
		{
			m_bottomAttachment = transform4.gameObject;
			m_bottomAttachment.SetActive(value: false);
		}
		m_triggerSpeed = 500f;
	}

	public override Direction EffectDirection()
	{
		if (INSettings.GetBool(INFeature.RotatableTNT))
		{
			return BasePart.Rotate(Direction.Right, m_gridRotation);
		}
		return Direction.Right;
	}

	public override void Initialize()
	{
		base.contraption.ChangeOneShotPartAmount(m_partType, EffectDirection(), 1);
	}

	public override void OnCollisionEnter(Collision c)
	{
		base.OnCollisionEnter(c);
		if (c.relativeVelocity.magnitude > m_triggerSpeed && !base.HasGeneratorRef)
		{
			Explode();
		}
	}

	public override void ChangeVisualConnections()
	{
		bool active = base.contraption.CanConnectTo(this, BasePart.Rotate(Direction.Up, m_gridRotation));
		bool active2 = base.contraption.CanConnectTo(this, BasePart.Rotate(Direction.Down, m_gridRotation));
		bool active3 = base.contraption.CanConnectTo(this, BasePart.Rotate(Direction.Left, m_gridRotation));
		bool active4 = base.contraption.CanConnectTo(this, BasePart.Rotate(Direction.Right, m_gridRotation));
		if ((bool)m_leftAttachment)
		{
			m_leftAttachment.SetActive(active3);
		}
		if ((bool)m_rightAttachment)
		{
			m_rightAttachment.SetActive(active4);
		}
		if ((bool)m_topAttachment)
		{
			m_topAttachment.SetActive(active);
		}
		if ((bool)m_bottomAttachment)
		{
			m_bottomAttachment.SetActive(active2);
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(base.transform.position, m_explosionRadius);
	}

	public override void PrePlaced()
	{
		base.PrePlaced();
		if (INSettings.GetBool(INFeature.RotatableTNT))
		{
			m_autoAlign = AutoAlignType.Rotate;
		}
	}

	public virtual void Explode()
	{
		if (m_triggered)
		{
			return;
		}
		m_triggered = true;
		base.contraption.ChangeOneShotPartAmount(m_partType, EffectDirection(), -1);
		Vector3 center = base.transform.position + base.transform.right * 12f;
		Vector3 halfExtents = new Vector3(12f, 0.3f, 0.3f);
		Quaternion rotation = base.transform.rotation;
		Collider[] array = Physics.OverlapBox(center, halfExtents, rotation);
		foreach (Collider collider in array)
		{
			GameObject gameObject = FindParentWithRigidBody(collider.gameObject);
			if (gameObject != null)
			{
				CountChildColliders(gameObject, 0);
				AddExplosionForce(gameObject, 40f);
			}
			BasePart component = collider.GetComponent<BasePart>();
			TNT tNT = component as TNT;
			if (tNT != null && !(component is AlienTNT) && !component.HasGeneratorRef)
			{
				tNT.Explode();
			}
			if (INSettings.GetBool(INFeature.BlasterTNT))
			{
				BlasterTNT blasterTNT = component as BlasterTNT;
				if (blasterTNT != null && !component.HasGeneratorRef && Vector.DistanceSquared2(base.transform.position, component.transform.position) < 4f)
				{
					blasterTNT.ExplodeSpecial();
				}
			}
		}
		Singleton<AudioManager>.Instance.SpawnOneShotEffect(WPFMonoBehaviour.gameData.commonAudioCollection.tntExplosion, base.transform.position);
		WPFMonoBehaviour.effectManager.CreateParticles(smokeCloud, base.transform.position - Vector3.forward * 5f, force: true);
		if ((bool)extraEffect)
		{
			WPFMonoBehaviour.effectManager.CreateParticles(extraEffect, base.transform.position - Vector3.forward * 4f, force: true);
		}
		CheckForTNTAchievement();
		base.contraption.RemovePart(this);
		List<Joint> list = base.contraption.FindPartJoints(this);
		if (list.Count > 0)
		{
			foreach (Joint item in list)
			{
				bool flag = item.gameObject == this || item.connectedBody == this;
				if (!float.IsInfinity(item.breakForce) | flag)
				{
					UnityEngine.Object.Destroy(item);
				}
			}
			HandleJointBreak();
		}
		else
		{
			HandleJointBreak(playEffects: false);
		}
		StartCoroutine(ShineLight());
	}

	protected int CountChildColliders(GameObject obj, int count)
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

	protected GameObject FindParentWithRigidBody(GameObject obj)
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

	protected void AddExplosionForce(GameObject target, float forceFactor)
	{
		Rigidbody component = target.GetComponent<Rigidbody>();
		if (!(component == null))
		{
			float num = Mathf.Max((target.transform.position - base.transform.position).magnitude, 1f);
			float num2 = ((num <= 20f) ? (1f - 0.5f * (num / 20f)) : ((!(num <= 24f)) ? 0f : (0.5f * (1f - (num - 20f) / 4f))));
			float num3 = forceFactor * m_explosionImpulse * num2 - 500f;
			component.AddForce(num3 * base.transform.right, ForceMode.Impulse);
		}
	}

	public void CheckForTNTAchievement()
	{
		if (!Singleton<SocialGameManager>.IsInstantiated() || !Singleton<GameManager>.Instance.IsInGame())
		{
			return;
		}
		int brokenTNTs = GameProgress.GetInt("Broken_TNTs") + 1;
		GameProgress.SetInt("Broken_TNTs", brokenTNTs);
		((Action<List<string>>)delegate(List<string> achievements)
		{
			foreach (string achievement in achievements)
			{
				if (Singleton<SocialGameManager>.Instance.TryReportAchievementProgress(achievement, 100.0, (int limit) => brokenTNTs > limit))
				{
					break;
				}
			}
		})(new List<string> { "grp.BOOM_BOOM_III", "grp.BOOM_BOOM_II", "grp.BOOM_BOOM_I" });
	}

	protected virtual IEnumerator ShineLight()
	{
		PointLightSource pls = GetComponentInChildren<PointLightSource>();
		if ((bool)pls)
		{
			MeshRenderer componentInChildren = base.transform.GetComponentInChildren<MeshRenderer>();
			if ((bool)componentInChildren)
			{
				componentInChildren.enabled = false;
			}
			pls.onLightTurnOff = (Action)Delegate.Combine(pls.onLightTurnOff, (Action)delegate
			{
				UnityEngine.Object.Destroy(base.gameObject);
			});
			pls.isEnabled = true;
			yield return new WaitForSeconds(pls.turnOnCurve[pls.turnOnCurve.length - 1].time);
			pls.isEnabled = false;
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
