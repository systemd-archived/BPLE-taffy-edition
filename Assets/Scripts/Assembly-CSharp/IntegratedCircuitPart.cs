using System;
using System.Collections.Generic;
using UnityEngine;

public class IntegratedCircuitPart : ElectricalPart
{
	public enum ICType
	{
		BUF,
		NOT,
		OR1,
		OR2,
		NOR1,
		NOR2,
		AND1,
		AND2,
		NAND1,
		NAND2,
		NMOS,
		PMOS,
		DIODE,
		OPAMP,
		DELAY1,
		DELAY2
	}

	public class Diode
	{
		private VoltageSource m_source;

		private double m_I;

		private double m_U1;

		private double m_U2;

		public VoltageSource Source => m_source;

		public Diode()
		{
			m_source = new VoltageSource(0.0, 0.0);
			m_source.ElementUpdated += OnElementUpdated;
			m_I = 0.0;
			m_U1 = 0.0;
			m_U2 = 0.0;
			Update();
		}

		private void OnElementUpdated(CircuitSimulator simulator, SimulationResult result)
		{
			if (result.Electrode == m_source.Anode)
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

		public void Update()
		{
			double num = m_I;
			double num2 = m_U1 - m_U2;
			if (num >= 10.0)
			{
				num2 = Math.Log(num / 1E-06 + 1.0) * 0.05;
			}
			else
			{
				double num3 = Math.Min(num2, 1.0);
				for (int i = 0; i < 16; i++)
				{
					double num4 = Math.Exp(num3 / 0.05);
					double num5 = (1E-06 * (num4 - 1.0) - num) * (1.9999999999999998E-05 * num4) + num3 - num2;
					double num6 = (1E-06 * (2.0 * num4 - 1.0) - num) * (0.0003999999999999999 * num4) + 1.0;
					double num7 = num5 / num6;
					num3 -= num7;
					if (Math.Abs(num7) < 1E-05)
					{
						break;
					}
				}
				num2 = Math.Max(num3, -0.15);
				num = 1E-06 * (Math.Exp(num2 / 0.05) - 1.0);
			}
			double num8 = 0.05 / (num + 1E-06);
			m_source.Resistance = num8;
			m_source.Voltage = num2 - num * num8;
		}
	}

	private Wire m_input1;

	private Wire m_input2;

	private Vcc m_output;

	private Switch m_switch;

	private Diode m_diode;

	private double m_U1;

	private double m_U2;

	private int m_delay;

	private LogicLevel m_level1;

	private LogicLevel m_level2;

	private Queue<LogicLevel> m_delayQueue;

	private bool m_canBeFlipped;

	public override IEnumerable<CircuitElement> ElectricalElements
	{
		get
		{
			if (m_input1 != null)
			{
				yield return m_input1;
			}
			if (m_input2 != null)
			{
				yield return m_input2;
			}
			if (m_output != null)
			{
				yield return m_output;
			}
			if (m_switch != null)
			{
				yield return m_switch;
			}
			if (m_diode != null)
			{
				yield return m_diode.Source;
			}
		}
	}

	public ICType GetICType()
	{
		return (ICType)(customPartIndex - 22);
	}

	public override void Awake()
	{
		base.Awake();
		int num = (int)(GetICType() + 1);
		INSerializedSprite component = GetComponent<INSerializedSprite>();
		component.SpriteName = "IntegratedCircuit" + num + "_Sprite";
		component.UpdateMesh();
		switch (GetICType())
		{
		case ICType.OR2:
		case ICType.NOR2:
		case ICType.AND2:
		case ICType.NAND2:
		case ICType.OPAMP:
			m_canBeFlipped = true;
			m_autoAlign = (AutoAlignType)(-1);
			break;
		case ICType.DELAY1:
			m_delay = 5;
			m_delayQueue = new Queue<LogicLevel>(m_delay - 1);
			break;
		case ICType.DELAY2:
			m_delay = 25;
			m_delayQueue = new Queue<LogicLevel>(m_delay - 1);
			break;
		case ICType.NOR1:
		case ICType.AND1:
		case ICType.NAND1:
		case ICType.NMOS:
		case ICType.PMOS:
		case ICType.DIODE:
			break;
		}
	}

	public override void SetRotation(GridRotation rotation)
	{
		if (m_canBeFlipped)
		{
			SetRotation(GetRotation(rotation, m_flipped));
		}
		else
		{
			base.SetRotation(rotation);
		}
	}

	public override void SetFlipped(bool flipped)
	{
		if (m_canBeFlipped)
		{
			SetRotation(GetRotation(m_gridRotation, flipped));
		}
		else
		{
			base.SetFlipped(flipped);
		}
	}

	public override int GetRotation()
	{
		return GetRotation(m_gridRotation, m_flipped);
	}

	private int GetRotation(GridRotation rotation, bool flipped)
	{
		return (int)rotation * 2 + (flipped ? 1 : 0);
	}

	public override void SetRotation(int rotation)
	{
		int num = rotation % 8;
		int num2 = num / 2;
		bool flag = (m_flipped = num % 2 == 1);
		m_gridRotation = (GridRotation)num2;
		int num3 = ((flag && (num2 == 0 || num2 == 2)) ? 180 : 0);
		int num4 = ((flag && (num2 == 1 || num2 == 3)) ? 180 : 0);
		int num5 = 90 * num2;
		base.transform.localRotation = Quaternion.Euler(num3, num4, num5);
	}

	public override void CreateElectricalElements()
	{
		switch (GetICType())
		{
		case ICType.NMOS:
		case ICType.PMOS:
			m_input1 = new Wire(1);
			m_input1.ElementUpdated += OnInput1Updated;
			m_switch = new Switch();
			return;
		case ICType.DIODE:
			m_diode = new Diode();
			return;
		}
		if (GetInput1Direction() != BitDirection.None)
		{
			m_input1 = new Wire(1);
			m_input1.ElementUpdated += OnInput1Updated;
		}
		if (GetInput2Direction() != BitDirection.None)
		{
			m_input2 = new Wire(1);
			m_input2.ElementUpdated += OnInput2Updated;
		}
		if (GetOutputDirection() != BitDirection.None)
		{
			m_output = new Vcc(0.0, 0.0);
		}
	}

	protected override BitDirection GetConnectionDirection()
	{
		BitDirection direction;
		switch (GetICType())
		{
		case ICType.NMOS:
		case ICType.PMOS:
			direction = (BitDirection)14;
			break;
		case ICType.DIODE:
			direction = BitDirection.LeftAndRight;
			break;
		default:
			direction = GetInput1Direction() | GetInput2Direction() | GetOutputDirection();
			break;
		}
		return direction.Rotate((int)m_gridRotation);
	}

	protected override Electrode FindElectrode(BitDirection direction)
	{
		direction = direction.Rotate(0 - m_gridRotation);
		switch (GetICType())
		{
		case ICType.NMOS:
		case ICType.PMOS:
			return direction switch
			{
				BitDirection.Left => m_input1.Electrodes[0], 
				BitDirection.Up => m_switch.Pole, 
				BitDirection.Down => m_switch.Throw, 
				_ => null, 
			};
		case ICType.DIODE:
			return direction switch
			{
				BitDirection.Left => m_diode.Source.Anode, 
				BitDirection.Right => m_diode.Source.Cathode, 
				_ => null, 
			};
		default:
			if (direction == GetInput1Direction())
			{
				return m_input1.Electrodes[0];
			}
			if (direction == GetInput2Direction())
			{
				return m_input2.Electrodes[0];
			}
			if (direction == GetOutputDirection())
			{
				return m_output.Electrodes[0];
			}
			return null;
		}
	}

	private BitDirection GetInput1Direction()
	{
		switch (GetICType())
		{
		case ICType.BUF:
		case ICType.NOT:
			return BitDirection.Left;
		case ICType.OR1:
		case ICType.NOR1:
		case ICType.AND1:
		case ICType.NAND1:
			return BitDirection.Up;
		case ICType.OR2:
		case ICType.NOR2:
		case ICType.AND2:
		case ICType.NAND2:
			return BitDirection.Left;
		case ICType.OPAMP:
			if (m_flipped)
			{
				return BitDirection.Down;
			}
			return BitDirection.Up;
		case ICType.DELAY1:
		case ICType.DELAY2:
			return BitDirection.Left;
		default:
			return BitDirection.None;
		}
	}

	private BitDirection GetInput2Direction()
	{
		switch (GetICType())
		{
		case ICType.OR1:
		case ICType.NOR1:
		case ICType.AND1:
		case ICType.NAND1:
			return BitDirection.Down;
		case ICType.OR2:
		case ICType.NOR2:
		case ICType.AND2:
		case ICType.NAND2:
			if (m_flipped)
			{
				return BitDirection.Down;
			}
			return BitDirection.Up;
		case ICType.OPAMP:
			if (m_flipped)
			{
				return BitDirection.Up;
			}
			return BitDirection.Down;
		default:
			return BitDirection.None;
		}
	}

	private BitDirection GetOutputDirection()
	{
		return BitDirection.Right;
	}

	private void OnInput1Updated(CircuitSimulator simulator, SimulationResult result)
	{
		if (result.IsGrounded)
		{
			m_level1 = ElectricalPart.GetLogicLevel(result.U);
			m_U1 = result.U;
		}
	}

	private void OnInput2Updated(CircuitSimulator simulator, SimulationResult result)
	{
		if (result.IsGrounded)
		{
			m_level2 = ElectricalPart.GetLogicLevel(result.U);
			m_U2 = result.U;
		}
	}

	public override void PreUpdateElements()
	{
		m_U1 = double.NaN;
		m_U2 = double.NaN;
		m_level1 = LogicLevel.Invalid;
		m_level2 = LogicLevel.Invalid;
	}

	public override void PostUpdateElements()
	{
		switch (GetICType())
		{
		case ICType.NMOS:
			SetInvalid(m_level1 == LogicLevel.Invalid);
			m_switch.Toggle(m_level1 == LogicLevel.High);
			return;
		case ICType.PMOS:
			SetInvalid(m_level1 == LogicLevel.Invalid);
			m_switch.Toggle(m_level1 == LogicLevel.Low);
			return;
		case ICType.DIODE:
			m_diode.Update();
			return;
		case ICType.OPAMP:
			SetInvalid(double.IsNaN(m_U1) || double.IsNaN(m_U2));
			if (!m_invalid)
			{
				double potential = Math.Clamp((m_U1 - m_U2) * 1000.0, -10.0, 10.0);
				SetOutput(potential, 0.05);
			}
			return;
		case ICType.DELAY1:
		case ICType.DELAY2:
		{
			SetInvalid(m_level1 == LogicLevel.Invalid);
			bool output = false;
			if (m_delayQueue.Count == m_delay - 1)
			{
				output = m_delayQueue.Dequeue() == LogicLevel.High;
			}
			m_delayQueue.Enqueue(m_level1);
			SetOutput(output);
			return;
		}
		}
		bool flag = false;
		if (GetInput1Direction() != BitDirection.None)
		{
			flag |= m_level1 == LogicLevel.Invalid;
		}
		if (GetInput2Direction() != BitDirection.None)
		{
			flag |= m_level2 == LogicLevel.Invalid;
		}
		SetInvalid(flag);
		if (!flag)
		{
			bool output2 = false;
			bool flag2 = m_level1 == LogicLevel.High;
			bool flag3 = m_level2 == LogicLevel.High;
			switch (GetICType())
			{
			case ICType.BUF:
				output2 = flag2;
				break;
			case ICType.NOT:
				output2 = !flag2;
				break;
			case ICType.OR1:
			case ICType.OR2:
				output2 = flag2 | flag3;
				break;
			case ICType.NOR1:
			case ICType.NOR2:
				output2 = !(flag2 | flag3);
				break;
			case ICType.AND1:
			case ICType.AND2:
				output2 = flag2 & flag3;
				break;
			case ICType.NAND1:
			case ICType.NAND2:
				output2 = !(flag2 & flag3);
				break;
			}
			SetOutput(output2);
		}
		else
		{
			SetOutput(value: false);
		}
	}

	private void SetOutput(bool value)
	{
		if (value)
		{
			SetOutput(5.0, 0.05);
		}
		else
		{
			SetOutput(0.0, 0.0);
		}
	}

	private void SetOutput(double potential, double resistance)
	{
		m_output.Potential = potential;
		m_output.Resistance = resistance;
	}
}
