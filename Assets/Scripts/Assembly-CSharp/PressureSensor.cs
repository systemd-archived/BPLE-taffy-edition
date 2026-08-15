using System;
using System.Collections.Generic;
using UnityEngine;

public class PressureSensor : ElectricalPart
{
	private Resistor m_resistor;

	private float m_pressure;

	public override IEnumerable<CircuitElement> ElectricalElements => m_resistor.ToEnumerable();

	public override void CreateElectricalElements()
	{
		m_resistor = new Resistor(10000.0);
	}

	protected override BitDirection GetConnectionDirection()
	{
		return BitDirection.LeftAndRight.Rotate((int)m_gridRotation);
	}

	protected override Electrode FindElectrode(BitDirection direction)
	{
		direction = direction.Rotate(0 - m_gridRotation);
		return direction switch
		{
			BitDirection.Left => m_resistor.Electrode1, 
			BitDirection.Right => m_resistor.Electrode2, 
			_ => null, 
		};
	}

	public override void Initialize()
	{
		base.rigidbody.sleepThreshold = 0f;
	}

	public override void OnCollisionEnter(Collision collision)
	{
		base.OnCollisionEnter(collision);
		HandleCollision(collision);
	}

	public override void OnCollisionStay(Collision collision)
	{
		base.OnCollisionStay(collision);
		HandleCollision(collision);
	}

	private void HandleCollision(Collision collision)
	{
		Vector3 position = base.transform.position;
		Vector3 right = base.transform.right;
		bool flag = false;
		ContactPoint[] contacts = collision.contacts;
		foreach (ContactPoint contactPoint in contacts)
		{
			Vector3 point = contactPoint.point;
			if (Vector.Cross2(right, point - position) > 0.4f)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			float num = Math.Abs(Vector.Cross2(right, collision.impulse));
			m_pressure += num / Time.fixedDeltaTime;
		}
	}

	private void FixedUpdate()
	{
		if ((bool)base.contraption && base.contraption.IsRunning)
		{
			m_resistor.Resistance = Math.Min(500f / m_pressure, 10000f);
			m_pressure = 0f;
		}
	}
}
