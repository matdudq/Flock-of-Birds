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
		private struct AlignmentJobChunk : IJobChunk
		{
			[ReadOnly]
			public BoidSharedData boidSharedData;
			[ReadOnly]
			public NativeArray<LocalToWorld> neighboursLTW;
			[ReadOnly]
			public ComponentTypeHandle<LocalToWorld> localToWorldType;
			
			public NativeArray<float3> alignments;
			
			public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
			{
				NativeArray<LocalToWorld> chunkLTW = chunk.GetNativeArray(localToWorldType);
				
				for (int i = 0; i < chunk.Count; i++)
				{
					int entityGlobalIndex = firstEntityIndex + i;
					float3 position = chunkLTW[i].Position;

					float3 alignment = new float3(0,0,0);
					float neihboursCount = 0;
					
					for (int j = 0; j < neighboursLTW.Length; j++)
					{
						if (entityGlobalIndex == j)
						{
							continue;
						}
					
						float3 neighbourPosition = neighboursLTW[j].Position;
						
						if (math.length(position - neighbourPosition) <= boidSharedData.alignmentRadius)
						{
							alignment += neighboursLTW[j].Forward * boidSharedData.speed;
							neihboursCount++;
						}
					}

					alignments[entityGlobalIndex] = neihboursCount > 0 ? alignment / neihboursCount : alignment;
				}
			}
		}
	}
}