using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace FlockOfBirds
{
	internal partial class BoidSystem 
	{
		[BurstCompile]
		private struct ApplyBordersBoidChunk : IJobChunk
		{
			[ReadOnly]
			public float3 borderPosition;
			[ReadOnly]
			public float borderRadius;
			
			public ComponentTypeHandle<LocalToWorld> localToWorldType;

			public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
			{
				NativeArray<LocalToWorld> chunkLTW = chunk.GetNativeArray(localToWorldType);
				for (int i = 0; i < chunk.Count; i++)
				{
					float3 position = ClampInSphereBorder(chunkLTW[i].Position, borderPosition, borderRadius);
					
					quaternion rotation = chunkLTW[i].Rotation;
					float3 size = new float3(1.0f, 1.0f, 1.0f);
					
					chunkLTW[i] = new LocalToWorld()
					{
						Value = float4x4.TRS(position,rotation,size)
					};
				}

				float3 ClampInSphereBorder(float3 currentPosition, float3 sphereCenter, float sphereRadius)
				{
					if (math.length(currentPosition - sphereCenter) > sphereRadius){
						return sphereCenter + math.normalize(sphereCenter - currentPosition) * sphereRadius;
					}

					return currentPosition;
				}
			}
		}
	}
}