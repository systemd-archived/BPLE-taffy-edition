using System.Collections.Generic;
using UnityEngine;

public class IndicatorPart : ElectricalPart
{
	private enum IndicatorType
	{
		Ammeter,
		Voltmeter
	}

	[SerializeField]
	private IndicatorType m_type;

	private Resistor m_indicator;

	private double m_I;

	private double m_U1;

	private double m_U2;

	private GameObject m_symbol;

	private TextMesh m_text;

	public override IEnumerable<CircuitElement> ElectricalElements => m_indicator.ToEnumerable();

	public override void Awake()
	{
		base.Awake();
		m_symbol = base.transform.Find("Symbol").gameObject;
		m_text = base.transform.Find("Text").GetComponent<TextMesh>();
		m_I = double.NaN;
		m_U1 = double.NaN;
		m_U2 = double.NaN;
	}

	public override void CreateElectricalElements()
	{
		Resistor resistor = null;
		switch (m_type)
		{
		case IndicatorType.Ammeter:
			resistor = new Resistor(0.0);
			break;
		case IndicatorType.Voltmeter:
			resistor = new Resistor(1000000.0);
			break;
		}
		resistor.ElementUpdated += OnElementUpdated;
		m_indicator = resistor;
	}

	private void OnElementUpdated(CircuitSimulator simulator, SimulationResult result)
	{
		if (result.Electrode == m_indicator.Electrode1)
		{
			m_I = result.I;
			m_U1 = result.U;
		}
		else
		{
			m_I = 0.0 - result.I;
			m_U2 = result.U;
		}
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
			BitDirection.Left => m_indicator.Electrode1, 
			BitDirection.Right => m_indicator.Electrode2, 
			_ => null, 
		};
	}

	public override void SetRotation(GridRotation rotation)
	{
		base.SetRotation(rotation);
		m_symbol.transform.rotation = Quaternion.identity;
		m_text.transform.rotation = Quaternion.identity;
	}

	public override void PostUpdateElements()
	{
		double num = 0.0;
		switch (m_type)
		{
		case IndicatorType.Ammeter:
			num = m_I;
			break;
		case IndicatorType.Voltmeter:
			num = m_U1 - m_U2;
			break;
		}
		if (double.IsNaN(num))
		{
			num = 0.0;
		}
		m_I = double.NaN;
		m_U1 = double.NaN;
		m_U2 = double.NaN;
		bool flag = num >= 0.0;
		num = (flag ? num : (0.0 - num));
		string text = ((num >= 1000.0) ? "999" : ((!(num >= 100.0)) ? num.ToString("00.0") : num.ToString("000")));
		m_text.text = (flag ? string.Empty : "-") + text;
	}
}
