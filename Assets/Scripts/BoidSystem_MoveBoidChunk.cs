using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

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
				    float3 alignment = boidSharedData.alignment * alignmentVectors[firstEntityIndex + i];
					float3 separation = boidSharedData.separation * separationVectors[firstEntityIndex + i];
					
					float3 combinedBehaviour = math.normalizesafe(alignment + separation);
					
				    float3 speculativeVelocity = math.normalizesafe(forward + deltaTime * (combinedBehaviour - forward) * boidSharedData.maneuverSpeed);
					
					float3 position = localToWorldChunk[i].Position + (speculativeVelocity * boidSharedData.speed * deltaTime);
					quaternion rotation = quaternion.LookRotationSafe(speculativeVelocity, math.up());
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