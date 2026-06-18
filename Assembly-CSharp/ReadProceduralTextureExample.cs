using System;
using JBooth.MicroSplat;
using UnityEngine;

[ExecuteInEditMode]
public class ReadProceduralTextureExample : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (this.proceduralConfig == null)
		{
			return;
		}
		RaycastHit raycastHit;
		if (Physics.Raycast(new Ray(base.transform.position, Vector3.down), ref raycastHit))
		{
			Terrain component = raycastHit.collider.GetComponent<Terrain>();
			if (component == null)
			{
				return;
			}
			Material materialTemplate = component.materialTemplate;
			if (materialTemplate == null)
			{
				return;
			}
			Vector2 textureCoord = raycastHit.textureCoord;
			Vector3 point = raycastHit.point;
			Vector3 normal = raycastHit.normal;
			Vector3 up = Vector3.up;
			MicroSplatProceduralTextureUtil.NoiseUVMode noiseUVMode = MicroSplatProceduralTextureUtil.NoiseUVMode.World;
			if (this.keywords.IsKeywordEnabled("_PCNOISEUV"))
			{
				noiseUVMode = MicroSplatProceduralTextureUtil.NoiseUVMode.UV;
			}
			else if (this.keywords.IsKeywordEnabled("_PCNOISETRIPLANAR"))
			{
				noiseUVMode = MicroSplatProceduralTextureUtil.NoiseUVMode.Triplanar;
			}
			Vector4 vector;
			MicroSplatProceduralTextureUtil.Int4 @int;
			MicroSplatProceduralTextureUtil.Sample(textureCoord, point, normal, up, noiseUVMode, materialTemplate, this.proceduralConfig, out vector, out @int);
			if (@int.x != this.lastHit)
			{
				string[] array = new string[10];
				array[0] = "PC Texture Index : (";
				array[1] = @int.x.ToString();
				array[2] = ", ";
				array[3] = @int.y.ToString();
				array[4] = ", ";
				array[5] = @int.z.ToString();
				array[6] = ", ";
				array[7] = @int.z.ToString();
				array[8] = ")      ";
				int num = 9;
				Vector4 vector2 = vector;
				array[num] = vector2.ToString();
				Debug.Log(string.Concat(array));
				this.lastHit = @int.x;
			}
		}
	}

	public MicroSplatProceduralTextureConfig proceduralConfig;

	public MicroSplatKeywords keywords;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int lastHit = -1;
}
