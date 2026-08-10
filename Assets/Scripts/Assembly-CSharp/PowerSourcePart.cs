using System;
using System.Collections.Generic;

public class PowerSourcePart : ElectricalPart
{
	private VoltageSource m_powerSource;

	private double m_maxCurrent;

	protected const double CurrentThreshold = 10000.0;

	public override IEnumerable<CircuitElement> ElectricalElements => m_powerSource.ToEnumerable();

	public override void CreateElectricalElements()
	{
		int num = customPartIndex - 12;
		double voltage = 0.0;
		switch (num)
		{
		case 0:
			voltage = 1.0;
			break;
		case 1:
			voltage = 5.0;
			break;
		case 2:
			voltage = 50.0;
			break;
		}
		m_powerSource = new VoltageSource(voltage, 0.05);
		m_powerSource.ElementUpdated += OnElementUpdate;
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
			BitDirection.Right => m_powerSource.Anode, 
			BitDirection.Left => m_powerSource.Cathode, 
			_ => null, 
		};
	}

	public override void PreUpdateElements()
	{
		m_maxCurrent = 0.0;
	}

	private void OnElementUpdate(CircuitSimulator simulator, SimulationResult result)
	{
		if (result.Electrode != null)
		{
			m_maxCurrent = Math.Max(Math.Abs(result.I), m_maxCurrent);
		}
	}

	public override void PostUpdateElements()
	{
		if (m_maxCurrent > 10000.0)
		{
			SetInvalid(invalid: true);
			RemoveAllConnections();
		}
	}
}
