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
		private struct HashingJobChunk : IJobChunk
		{
			[ReadOnly]
			public BoidSharedData sharedData;
			
			[ReadOnly]
			public ComponentTypeHandle<LocalToWorld> localToWorldType;
			
			public NativeArray<int> hashes;
			public NativeMultiHashMap<int, int>.ParallelWriter parallelhashMap;
			
			public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
			{
				NativeArray<LocalToWorld> chunkLTW = chunk.GetNativeArray(localToWorldType);

				for (int i = 0; i < chunk.Count; i++)
				{
					int globalIndex = firstEntityIndex + i;
					int3 cord = new int3(math.floor(chunkLTW[i].Position / sharedData.cellRadius));
					hashes[globalIndex] = (int)math.hash(cord);
					parallelhashMap.Add(hashes[globalIndex], firstEntityIndex + i);
				}
			}
		}
	}
}