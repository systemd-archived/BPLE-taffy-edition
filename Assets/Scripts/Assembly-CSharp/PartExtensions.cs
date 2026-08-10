using System;

public static class PartExtensions
{
	public static int ToAngle(this BasePart.GridRotation rotation)
	{
		return rotation switch
		{
			BasePart.GridRotation.Deg_0 => 0, 
			BasePart.GridRotation.Deg_45 => 45, 
			BasePart.GridRotation.Deg_90 => 90, 
			BasePart.GridRotation.Deg_135 => 135, 
			BasePart.GridRotation.Deg_180 => 180, 
			BasePart.GridRotation.Deg_225 => 225, 
			BasePart.GridRotation.Deg_270 => 270, 
			BasePart.GridRotation.Deg_315 => 315, 
			BasePart.GridRotation.Deg_Max => 360, 
			_ => throw new InvalidCastException(), 
		};
	}

	public static (int, int) ToDirection(this BasePart.GridRotation rotation)
	{
		return rotation switch
		{
			BasePart.GridRotation.Deg_0 => (1, 0), 
			BasePart.GridRotation.Deg_45 => (1, 1), 
			BasePart.GridRotation.Deg_90 => (0, 1), 
			BasePart.GridRotation.Deg_135 => (-1, 1), 
			BasePart.GridRotation.Deg_180 => (-1, 0), 
			BasePart.GridRotation.Deg_225 => (-1, -1), 
			BasePart.GridRotation.Deg_270 => (0, -1), 
			BasePart.GridRotation.Deg_315 => (1, -1), 
			BasePart.GridRotation.Deg_Max => (1, 0), 
			_ => throw new InvalidCastException(), 
		};
	}

	public static bool IsSinglePart(this BasePart part)
	{
		return part.contraption.ComponentPartCount(part.ConnectedComponent) == 1;
	}

	public static bool HasMultipleRigidbodies(this BasePart part)
	{
		return part.m_partType == BasePart.PartType.Rope;
	}

	public static bool IsSeparatedFrame(this BasePart part)
	{
		return part.TypeInfo.IsSeparatedFrame();
	}

	public static bool IsSeparatedFrame(this PartTypeInfo info)
	{
		if (info.PartType == BasePart.PartType.MetalFrame)
		{
			return info.PartIndex == 8;
		}
		return false;
	}

	public static bool IsLightFrame(this BasePart part)
	{
		return part.TypeInfo.IsLightFrame();
	}

	public static bool IsLightFrame(this PartTypeInfo info)
	{
		if (info.PartType == BasePart.PartType.MetalFrame)
		{
			return info.PartIndex == 10;
		}
		return false;
	}

	public static bool IsAlienMetalFrame(this BasePart part)
	{
		return part.TypeInfo.IsAlienMetalFrame();
	}

	public static bool IsAlienMetalFrame(this PartTypeInfo info)
	{
		if (info.PartType == BasePart.PartType.MetalFrame)
		{
			return info.PartIndex == 11;
		}
		return false;
	}

	public static bool IsColoredrame(this BasePart part)
	{
		return part.TypeInfo.IsColoredrame();
	}

	public static bool IsColoredrame(this PartTypeInfo info)
	{
		if (info.PartType != BasePart.PartType.MetalFrame || info.PartIndex < 12 || info.PartIndex > 129)
		{
			return info.IsTransparentFrame();
		}
		return true;
	}

	public static bool IsTransparentFrame(this BasePart part)
	{
		return part.TypeInfo.IsTransparentFrame();
	}

	public static bool IsTransparentFrame(this PartTypeInfo info)
	{
		if (info.PartType == BasePart.PartType.MetalFrame)
		{
			if (info.PartIndex != 132)
			{
				return info.PartIndex == 133;
			}
			return true;
		}
		return false;
	}

	public static bool IsBracketFrame(this BasePart part)
	{
		return part.TypeInfo.IsBracketFrame();
	}

	public static bool IsBracketFrame(this PartTypeInfo info)
	{
		if (info.PartType == BasePart.PartType.MetalFrame)
		{
			return info.PartIndex == 131;
		}
		return false;
	}

	public static bool IsWoodenBox(this BasePart part)
	{
		return part.TypeInfo.IsWoodenBox();
	}

	public static bool IsWoodenBox(this PartTypeInfo info)
	{
		if (info.PartType == BasePart.PartType.WoodenFrame)
		{
			return info.PartIndex == 10;
		}
		return false;
	}

	public static bool IsMetalBox(this BasePart part)
	{
		return part.TypeInfo.IsMetalBox();
	}

	public static bool IsMetalBox(this PartTypeInfo info)
	{
		if (info.PartType == BasePart.PartType.MetalFrame)
		{
			return info.PartIndex == 130;
		}
		return false;
	}

	public static bool IsAvoidanceRocket(this BasePart part)
	{
		return part.TypeInfo.IsAvoidanceRocket();
	}

	public static bool IsAvoidanceRocket(this PartTypeInfo info)
	{
		if (info.PartType == BasePart.PartType.Rocket)
		{
			if (info.PartIndex != 1)
			{
				return info.PartIndex == 3;
			}
			return true;
		}
		return false;
	}

	public static bool IsTrackingRocket(this BasePart part)
	{
		return part.TypeInfo.IsTrackingRocket();
	}

	public static bool IsTrackingRocket(this PartTypeInfo info)
	{
		if (info.PartType == BasePart.PartType.RedRocket)
		{
			if (info.PartIndex != 1)
			{
				return info.PartIndex == 3;
			}
			return true;
		}
		return false;
	}

	public static bool IsHingePlate(this BasePart part)
	{
		return part.TypeInfo.IsHingePlate();
	}

	public static bool IsHingePlate(this PartTypeInfo info)
	{
		if (info.PartType == BasePart.PartType.Rope && 4 <= info.PartIndex)
		{
			return info.PartIndex <= 7;
		}
		return false;
	}

	public static bool IsMultipartGenerator(this BasePart part)
	{
		return part.TypeInfo.IsMultipartGenerator();
	}

	public static bool IsMultipartGenerator(this PartTypeInfo info)
	{
		if (info.PartType == BasePart.PartType.GrapplingHook && 8 <= info.PartIndex)
		{
			return info.PartIndex <= 10;
		}
		return false;
	}

	public static bool IsAutoGun(this BasePart part)
	{
		return part.TypeInfo.IsAutoGun();
	}

	public static bool IsAutoGun(this PartTypeInfo info)
	{
		if (info.PartType == BasePart.PartType.GrapplingHook)
		{
			return info.PartIndex == 6;
		}
		return false;
	}

	public static bool IsElasticConnector(this BasePart part)
	{
		return part.TypeInfo.IsElasticConnector();
	}

	public static bool IsElasticConnector(this PartTypeInfo info)
	{
		if (info.PartType == BasePart.PartType.Kicker)
		{
			if (info.PartIndex != 2)
			{
				return info.PartIndex == 4;
			}
			return true;
		}
		return false;
	}

	public static bool IsAutoConnector(this BasePart part)
	{
		return part.TypeInfo.IsAutoConnector();
	}

	public static bool IsAutoConnector(this PartTypeInfo info)
	{
		if (info.PartType == BasePart.PartType.Kicker)
		{
			return info.PartIndex == 1;
		}
		return false;
	}

	public static bool IsMarker(this BasePart part)
	{
		return part.TypeInfo.IsMarker();
	}

	public static bool IsMarker(this PartTypeInfo info)
	{
		if (info.PartType == BasePart.PartType.Kicker)
		{
			return info.PartIndex == 3;
		}
		return false;
	}

	public static bool IsEntityLight(this BasePart part)
	{
		return part.TypeInfo.IsEntityLight();
	}

	public static bool IsEntityLight(this PartTypeInfo info)
	{
		if (info.PartType != BasePart.PartType.PointLight || 0 > info.PartIndex || info.PartIndex > 4)
		{
			if (info.PartType == BasePart.PartType.SpotLight && 0 <= info.PartIndex)
			{
				return info.PartIndex <= 3;
			}
			return false;
		}
		return true;
	}

	public static bool IsDecelerationLight(this BasePart part)
	{
		return part.TypeInfo.IsDecelerationLight();
	}

	public static bool IsDecelerationLight(this PartTypeInfo info)
	{
		if (info.PartType == BasePart.PartType.PointLight)
		{
			return info.PartIndex == 5;
		}
		return false;
	}

	public static bool IsAutoControlLight(this BasePart part)
	{
		return part.TypeInfo.IsAutoControlLight();
	}

	public static bool IsAutoControlLight(this PartTypeInfo info)
	{
		if (info.PartType == BasePart.PartType.PointLight)
		{
			return info.PartIndex == 6;
		}
		return false;
	}
}
