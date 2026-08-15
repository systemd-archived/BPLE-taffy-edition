using System.Collections.Generic;
using UnityEngine;

public class PointChargePart : ElectricalPart
{
	private bool m_enabled;

	private MeshRenderer m_renderer;

	public bool IsPositive => m_gridRotation == GridRotation.Deg_0;

	public float Charge
	{
		get
		{
			if (!m_enabled)
			{
				return 0f;
			}
			if (!IsPositive)
			{
				return -20f;
			}
			return 20f;
		}
	}

	public override void Awake()
	{
		base.Awake();
		m_renderer = GetComponent<MeshRenderer>();
	}

	public override bool IsEnabled()
	{
		return m_enabled;
	}

	public override bool IsTriggerable()
	{
		return !base.HasGeneratorRef;
	}

	public override IEnumerable<UIPartTriggerButtonInfo> GetTriggerButtonInfo()
	{
		yield return new UIPartTriggerButtonInfo(UIPartButtonType.Trigger, 0, base.Type, 4, base.ConnectedComponent);
	}

	public override void SetRotation(GridRotation rotation)
	{
		bool flag = (m_gridRotation = (GridRotation)((int)rotation % 2)) == GridRotation.Deg_0;
		INSerializedSprite component = GetComponent<INSerializedSprite>();
		component.SpriteName = (flag ? "PointCharge1_Sprite" : "PointCharge2_Sprite");
		component.UpdateMesh();
	}

	public override void PostInitialize()
	{
		m_enabled = true;
	}

	protected override void OnTouch()
	{
		m_enabled = !m_enabled;
	}
}
