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
		private struct FollowTargetJobChunk : IJobChunk
		{
			[ReadOnly]
			public ComponentTypeHandle<LocalToWorld> localToWorldType;

			[ReadOnly]
			public float3 targetPosition;
			
			[WriteOnly]
			public NativeArray<float3> follwingVectors;
			
			public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
			{
				NativeArray<LocalToWorld> chunkLTW = chunk.GetNativeArray(localToWorldType);
				
				for (int i = 0; i < chunk.Count; i++)
				{
					int entityGlobalIndex = firstEntityIndex + i;
					follwingVectors[entityGlobalIndex] = math.normalize(targetPosition - chunkLTW[i].Position);
				}
			}
		}
	}
}