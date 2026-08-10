using System;
using UnityEngine;

public class AutoGun : ExplodingGrapplingHook
{
	private bool m_activated;

	private static System.Random s_random;

	private float m_nextFireTime;

	static AutoGun()
	{
		s_random = new System.Random();
	}

	public override bool CanBeEnabled()
	{
		return true;
	}

	public override bool IsEnabled()
	{
		return m_activated;
	}

	protected override void OnTouch()
	{
		m_activated = !m_activated;
	}

	private void FixedUpdate()
	{
		if (!base.contraption || !base.contraption.IsRunning || !IsEnabled() || this.IsSinglePart())
		{
			return;
		}
		float num = 22.5f;
		float num2 = 96f * 96f;
		float num3 = 0.5f;
		if (Time.time < m_nextFireTime)
		{
			return;
		}
		Vector3 position = base.transform.position;
		Vector3 right = base.transform.right;
		foreach (BasePart part in base.contraption.Parts)
		{
			if (!MarkerManager.IsInSameTeamStatic(this, part))
			{
				Vector3 to = part.transform.position - position;
				if (to.sqrMagnitude <= num2 && Vector3.Angle(right, to) <= num)
				{
					Shoot();
					m_nextFireTime = Time.time + num3;
					break;
				}
			}
		}
	}
}
