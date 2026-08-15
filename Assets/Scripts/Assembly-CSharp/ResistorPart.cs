using System;
using System.Collections.Generic;

public class ResistorPart : ElectricalPart
{
	private bool m_variable;

	private Resistor m_resistor;

	public int CurrentResistorType => customPartIndex - 6;

	public override IEnumerable<CircuitElement> ElectricalElements => m_resistor.ToEnumerable();

	public override bool IsTriggerable()
	{
		if (!base.HasGeneratorRef)
		{
			return m_variable;
		}
		return false;
	}

	public override IEnumerable<UIPartTriggerButtonInfo> GetTriggerButtonInfo()
	{
		yield break;
	}

	public override IEnumerable<UIPartSliderButtonInfo> GetSliderButtonInfo()
	{
		if (m_variable)
		{
			yield return new UIPartSliderButtonInfo(UIPartButtonType.Slider, 0, base.Type, 2, base.ConnectedComponent, new UIPartSliderButton.Range((m_resistor == null) ? 1f : ((float)m_resistor.Resistance), 1f, 0.1f, 10f, 0.1f, 0.01f));
		}
	}

	public override void OnSliderButtonTriggered(UIPartSliderButton button)
	{
		if (m_variable)
		{
			m_resistor.Resistance = button.Value;
		}
	}

	public override void Initialize()
	{
		base.Initialize();
		m_variable = CurrentResistorType == 5;
	}

	public override void CreateElectricalElements()
	{
		m_resistor = new Resistor(CurrentResistorType switch
		{
			0 => 0.01, 
			1 => 0.1, 
			2 => 1.0, 
			3 => 10.0, 
			4 => 100.0, 
			5 => 1.0, 
			_ => throw new InvalidOperationException(), 
		});
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

	public override void SetRotation(GridRotation rotation)
	{
		int rotation2 = (int)rotation % 2;
		base.SetRotation((GridRotation)rotation2);
	}
}
