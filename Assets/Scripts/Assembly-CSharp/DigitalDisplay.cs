using System.Collections.Generic;
using UnityEngine;

public class DigitalDisplay : ElectricalPart
{
	private TextMesh m_text;

	private Wire[] m_inputs;

	private LogicLevel[] m_levels;

	public override IEnumerable<CircuitElement> ElectricalElements => m_inputs;

	public override void Awake()
	{
		base.Awake();
		m_text = base.transform.Find("Text").GetComponent<TextMesh>();
		m_text.text = "0";
		m_text.color = new Color32(18, 98, 179, byte.MaxValue);
	}

	public override void SetRotation(GridRotation rotation)
	{
		base.SetRotation(rotation);
		m_text.transform.rotation = Quaternion.identity;
	}

	public override void CreateElectricalElements()
	{
		m_inputs = new Wire[4];
		m_levels = new LogicLevel[4];
		for (int i = 0; i < 4; i++)
		{
			Wire wire = new Wire(1);
			wire.ElementUpdated += OnElementUpdated;
			m_inputs[i] = wire;
		}
	}

	protected override BitDirection GetConnectionDirection()
	{
		return BitDirection.Any;
	}

	protected override Electrode FindElectrode(BitDirection direction)
	{
		direction = direction.Rotate(0 - m_gridRotation);
		int num = direction.ToIndex();
		if (num != -1)
		{
			return m_inputs[num].Electrodes[0];
		}
		return null;
	}

	private void OnElementUpdated(CircuitSimulator simulator, SimulationResult result)
	{
		for (int i = 0; i < 4; i++)
		{
			if (m_inputs[i] == result.Element)
			{
				m_levels[i] = ElectricalPart.GetLogicLevel(result.U);
			}
		}
	}

	public override void PreUpdateElements()
	{
		for (int i = 0; i < 4; i++)
		{
			m_levels[i] = LogicLevel.Invalid;
		}
	}

	public override void PostUpdateElements()
	{
		int num = 0;
		for (int i = 0; i < 4; i++)
		{
			num += ((m_levels[i] == LogicLevel.High) ? (1 << i) : 0);
		}
		if (num < 10)
		{
			m_text.text = ((char)(48 + num)).ToString();
			m_text.color = new Color32(18, 98, 179, byte.MaxValue);
		}
		else
		{
			m_text.text = ((char)(55 + num)).ToString();
			m_text.color = new Color32(71, 71, 178, byte.MaxValue);
		}
	}
}
