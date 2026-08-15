using System;
using System.Collections.Generic;
using UnityEngine;

public class MechanicalGate : MechanicalPart
{
	private struct CurveController
	{
		private float m_start;

		private float m_end;

		private float m_delta;

		private float m_progress;

		private float m_value;

		public float Start => m_start;

		public float End => m_end;

		public float Progress => m_progress;

		public float Value => m_value;

		public CurveController(float start, float end, float delta)
		{
			this = default(CurveController);
			Set(start, end, delta);
		}

		public void Set(float start, float end, float delta)
		{
			m_start = start;
			m_end = end;
			m_delta = delta;
			m_progress = 0f;
			m_value = start;
		}

		public void Update()
		{
			float num = Math.Clamp(m_progress + m_delta, 0f, 1f);
			if (m_progress != num)
			{
				float num2 = 0.5f * (1f - MathF.Cos(MathF.PI * num));
				m_progress = num;
				m_value = (1f - num2) * m_start + num2 * m_end;
			}
		}
	}

	private BoxCollider m_gateCollider;

	private MeshRenderer m_gateRenderer;

	private bool m_enabled;

	private float m_targetLength;

	private CurveController m_controller;

	public int CurrentType => customPartIndex;

	public override void Awake()
	{
		base.Awake();
		INSerializedSprite component = GetComponent<INSerializedSprite>();
		component.SpriteName = "MechanicalGate" + (CurrentType + 1) + "_Sprite";
		component.UpdateMesh();
	}

	public override bool CanBeEnabled()
	{
		return true;
	}

	public override bool IsEnabled()
	{
		return m_enabled;
	}

	public override void Initialize()
	{
		base.Initialize();
		m_targetLength = GetTargetLength();
		m_gateCollider = base.transform.Find("GateCollider").GetComponent<BoxCollider>();
		m_gateRenderer = base.transform.Find("GateRenderer").GetComponent<MeshRenderer>();
		Physics.IgnoreCollision(GetComponent<Collider>(), m_gateCollider);
	}

	private float GetTargetLength()
	{
		return CurrentType switch
		{
			0 => 1f, 
			1 => 2f, 
			2 => 4f, 
			3 => 8f, 
			4 => 1f, 
			_ => throw new InvalidOperationException(), 
		};
	}

	public override IEnumerable<UIPartSliderButtonInfo> GetSliderButtonInfo()
	{
		if (CurrentType == 4)
		{
			yield return new UIPartSliderButtonInfo(UIPartButtonType.Slider, 1, base.Type, 0, base.ConnectedComponent, new UIPartSliderButton.Range(m_targetLength, 1f, 0.1f, 8f, 0.1f, 0.01f));
		}
	}

	public override void OnSliderButtonTriggered(UIPartSliderButton button)
	{
		m_targetLength = button.Value;
		if (m_enabled)
		{
			SetController(m_controller.Value, m_targetLength);
		}
	}

	protected override void OnTouch()
	{
		SetEnabled(!m_enabled);
	}

	public override void SetEnabled(bool enabled)
	{
		if (m_enabled != enabled)
		{
			m_enabled = enabled;
			float value = m_controller.Value;
			float end = (enabled ? m_targetLength : 0f);
			SetController(value, end);
		}
	}

	private void SetController(float start, float end)
	{
		float delta = Math.Min(Math.Abs(0.2f / (end - start)), 1f);
		m_controller.Set(start, end, delta);
	}

	private void FixedUpdate()
	{
		if (!(base.contraption == null) && base.contraption.IsRunning)
		{
			m_controller.Update();
			float value = m_controller.Value;
			m_gateCollider.transform.localPosition = new Vector3(0.5f + value * 0.5f, 0f, 0f);
			m_gateCollider.transform.localScale = new Vector3(value, 1f, 1f);
			m_gateRenderer.transform.localPosition = new Vector3(0.5f + value * 0.5f, 0f, 0.01f);
			m_gateRenderer.transform.localScale = new Vector3(value, 1f, 1f);
		}
	}
}
