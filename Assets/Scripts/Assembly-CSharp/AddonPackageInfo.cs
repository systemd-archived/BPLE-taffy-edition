using System;
using Innovation;
using Newtonsoft.Json;

[Serializable]
public class AddonPackageInfo
{
	public string ID { get; set; }

	public string Path { get; set; }

	public AddonPackageKind Kind { get; set; }

	public AddonPackageState State { get; set; }

	[JsonProperty("md5")]
	public string MD5 { get; set; }

	[JsonIgnore]
	public AddonPackage Package { get; set; }

	public AddonPackageInfo(string id, string path, AddonPackageKind kind, AddonPackageState state, string md5)
	{
		ID = id;
		Path = path;
		Kind = kind;
		State = state;
		MD5 = md5;
	}
}
