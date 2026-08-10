using System;
using UnityEngine;

public struct PartTypeInfo : IEquatable<PartTypeInfo>
{
	public BasePart.PartType PartType;

	public int PartIndex;

	public PartTypeInfo(GameObject gameObject)
		: this(gameObject.GetComponent<BasePart>())
	{
	}

	public PartTypeInfo(BasePart part)
		: this(part.Type, part.Index)
	{
	}

	public PartTypeInfo(BasePart.PartType partType, int partIndex)
	{
		PartType = partType;
		PartIndex = partIndex;
	}

	public override bool Equals(object other)
	{
		if (other is PartTypeInfo other2)
		{
			return Equals(other2);
		}
		return false;
	}

	public bool Equals(PartTypeInfo other)
	{
		if (PartType == other.PartType)
		{
			return PartIndex == other.PartIndex;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine<int, int>((int)PartType, PartIndex);
	}

	public void Deconstruct(out BasePart.PartType partType, out int partIndex)
	{
		partType = PartType;
		partIndex = PartIndex;
	}

	public static bool operator ==(PartTypeInfo left, PartTypeInfo right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(PartTypeInfo left, PartTypeInfo right)
	{
		return !(left == right);
	}
}
