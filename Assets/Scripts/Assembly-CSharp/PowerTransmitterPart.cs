using System;
using System.Collections.Generic;

public class PowerTransmitterPart : ElectricalPart
{
	public enum PowerTransmitterType
	{
		Sender,
		Receiver
	}

	private CircuitElement m_element;

	private PowerTransmitterPart m_connectedPart;

	private Electrode m_electrode;

	public bool IsSender => TransmitterType == PowerTransmitterType.Sender;

	public bool IsReceiver => TransmitterType == PowerTransmitterType.Receiver;

	public PowerTransmitterType TransmitterType => (PowerTransmitterType)(customPartIndex - 40);

	public override IEnumerable<CircuitElement> ElectricalElements => m_element.ToEnumerable();

	public override void Awake()
	{
		base.Awake();
	}

	public int GetChannel()
	{
		if (m_enclosedInto != null && m_enclosedInto.IsColoredrame())
		{
			return m_enclosedInto.Index;
		}
		return -1;
	}

	public override void CreateElectricalElements()
	{
		switch (TransmitterType)
		{
		case PowerTransmitterType.Sender:
			m_element = new Wire(1);
			break;
		case PowerTransmitterType.Receiver:
			m_element = new Resistor(0.0);
			break;
		}
	}

	protected override BitDirection GetConnectionDirection()
	{
		return TransmitterType switch
		{
			PowerTransmitterType.Sender => BitDirection.Down.Rotate((int)m_gridRotation), 
			PowerTransmitterType.Receiver => BitDirection.Up.Rotate((int)m_gridRotation), 
			_ => BitDirection.None, 
		};
	}

	protected override Electrode FindElectrode(BitDirection direction)
	{
		direction = direction.Rotate(0 - m_gridRotation);
		if (direction == BitDirection.Down && TransmitterType == PowerTransmitterType.Sender)
		{
			return m_element.Electrodes[0];
		}
		if (direction == BitDirection.Up && TransmitterType == PowerTransmitterType.Receiver)
		{
			return m_element.Electrodes[0];
		}
		return null;
	}

	public void Connect(PowerTransmitterPart other, float distance)
	{
		if (m_connectedPart != other)
		{
			Disconnect();
			Electrode electrode = new Electrode(m_element, null);
			Electrode electrode2 = new Electrode(other.m_element, null);
			CircuitFactory.Connect(electrode, electrode2);
			electrode.Element.Electrodes.Add(electrode);
			electrode2.Element.Electrodes.Add(electrode2);
			m_electrode = electrode;
			m_connectedPart = other;
		}
		Resistor obj = (Resistor)m_element;
		distance = Math.Max(distance - 1f, 0f);
		obj.Resistance = 0.1f * distance * distance + 0.01f;
	}

	public void Disconnect()
	{
		if (m_connectedPart != null)
		{
			Electrode electrode = m_electrode;
			Electrode connectedElectrode = m_electrode.ConnectedElectrode;
			CircuitFactory.Disconnect(electrode, connectedElectrode);
			electrode.Element.Electrodes.Remove(electrode);
			connectedElectrode.Element.Electrodes.Remove(connectedElectrode);
			m_electrode = null;
			m_connectedPart = null;
		}
		((Resistor)m_element).Resistance = 0.0;
	}
}
