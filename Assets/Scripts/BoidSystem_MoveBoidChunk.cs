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
		private struct MoveBoidJobChunk : IJobChunk
		{
			[ReadOnly]
			public BoidSharedData boidSharedData;

			[ReadOnly]
			public float deltaTime;
			
			[ReadOnly]
			public NativeArray<float3> alignmentVectors;
			
			[ReadOnly]
			public NativeArray<float3> separationVectors;
			
			public ComponentTypeHandle<LocalToWorld> localToWorldType;

			public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
			{
				NativeArray<LocalToWorld> localToWorldChunk = chunk.GetNativeArray(localToWorldType);
				
				for (int i = 0; i < chunk.Count; i++)
				{
					float3 forward = localToWorldChunk[i].Forward;
				    float3 alignment = boidSharedData.alignment * math.normalizesafe(alignmentVectors[firstEntityIndex + i] - forward);
					float3 separation = boidSharedData.separation * math.normalizesafe(separationVectors[firstEntityIndex + i] - forward);

					float3 combinedBehaviour = math.normalizesafe(alignment + separation);
					
					//as we don't store velocity related datas we have to speculate next move direction.
				    float3 speculativePosition = math.normalizesafe(forward + 0.05f * (combinedBehaviour - forward));
					
					float3 position = localToWorldChunk[i].Position + (speculativePosition * boidSharedData.speed * deltaTime);
					quaternion rotation = quaternion.LookRotationSafe(speculativePosition, math.up());
					float3 size = new float3(1.0f, 1.0f, 1.0f);
					localToWorldChunk[i] = new LocalToWorld()
					{
						Value = float4x4.TRS(position,rotation,size)
					};
				}
			}
		}
	}
}