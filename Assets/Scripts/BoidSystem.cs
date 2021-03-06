using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace FlockOfBirds
{
	[UpdateInGroup(typeof(SimulationSystemGroup))]
	[UpdateBefore(typeof(TransformSystemGroup))]
	internal partial class BoidSystem : SystemBase
	{
		private EntityQuery boidQuery;

		private readonly List<BoidSharedData> boidSharedDatas = new List<BoidSharedData>();

		private const int processingNeighbourCount = 25;
		
		protected override void OnCreate()
		{
			base.OnCreate();
			
			boidQuery = GetEntityQuery(new EntityQueryDesc
			{
				All = new[] { 
					ComponentType.ReadOnly<BoidSharedData>(),
					ComponentType.ReadWrite<LocalToWorld>() 
				},
			});
		}

		protected override void OnUpdate()
		{
			float deltaTime = math.min(0.05f,Time.DeltaTime);
			float3 targetPosition = BoidSystemSettings.Instance.TargetPosition;
			float borderRadius = BoidSystemSettings.Instance.BorderRadius;
			float3 borderOrigin = BoidSystemSettings.Instance.BorderOrigin;
			
			EntityManager.GetAllUniqueSharedComponentData(boidSharedDatas);
			
			for (int i = 0; i < boidSharedDatas.Count; i++)
			{
				BoidSharedData sharedData = boidSharedDatas[i];
				boidQuery.ResetFilter();
				boidQuery.AddSharedComponentFilter(sharedData);
				
				int boidCount = boidQuery.CalculateEntityCount();

				if (boidCount == 0)
				{
					continue;
				}
				
				ComponentTypeHandle<LocalToWorld> ltwComponentHandle = GetComponentTypeHandle<LocalToWorld>();

				NativeArray<int> hashes = CollectionHelper.CreateNativeArray<int, RewindableAllocator>(boidCount, ref World.UpdateAllocator);
				NativeMultiHashMap<int,int> cellHashToIndex = new NativeMultiHashMap<int, int>(boidCount, World.UpdateAllocator.ToAllocator);
				
				HashingJobChunk hashingJobChunk = new HashingJobChunk()
				{
					sharedData = sharedData,
					localToWorldType = ltwComponentHandle,
					hashes = hashes,
					parallelhashMap = cellHashToIndex.AsParallelWriter()
				};

				Dependency = hashingJobChunk.ScheduleParallel(boidQuery, Dependency);
				
				NativeArray<LocalToWorld> neighboursLTW = boidQuery.ToComponentDataArrayAsync<LocalToWorld>(Allocator.TempJob, out JobHandle setQueryHandle);
				Dependency = JobHandle.CombineDependencies(Dependency, setQueryHandle);

				NativeArray<float3> alignmentVectors = CollectionHelper.CreateNativeArray<float3, RewindableAllocator>(boidCount, ref World.UpdateAllocator);
				
				var calculateAlignmentJobChunk = new AlignmentJobChunk()
				{
					localToWorldType = ltwComponentHandle,
					alignments = alignmentVectors,
					neighboursLTW = neighboursLTW,
					hashes = hashes,
					parallelHashMap = cellHashToIndex
				};

				JobHandle calculateAlignmentJobHandler = calculateAlignmentJobChunk.ScheduleParallel(boidQuery, Dependency);
				
				NativeArray<float3> separationVectors = CollectionHelper.CreateNativeArray<float3, RewindableAllocator>(boidCount, ref World.UpdateAllocator);

				var calculateSeparationJobChunk = new SeparationJobChunk()
				{
					boidSharedData = sharedData,
					localToWorldType = ltwComponentHandle,
					separations = separationVectors,
					neighboursLTW = neighboursLTW,
					hashes = hashes,
					parallelHashMap = cellHashToIndex
				};

				JobHandle calculateSeparationJobHandle = calculateSeparationJobChunk.ScheduleParallel(boidQuery, Dependency);
				
				NativeArray<float3> followingVectors = CollectionHelper.CreateNativeArray<float3, RewindableAllocator>(boidCount, ref World.UpdateAllocator);

				var followTargetJobChunk = new FollowTargetJobChunk()
				{
					localToWorldType = ltwComponentHandle,
					targetPosition = targetPosition,
					follwingVectors = followingVectors
				};
				
				JobHandle calculateFollowingTargetJobHandle = followTargetJobChunk.ScheduleParallel(boidQuery, Dependency);
				
				var moveBoidJobChunk = new MoveBoidJobChunk()
				{
					alignmentVectors = alignmentVectors,
					separationVectors = separationVectors,
					followingVectors = followingVectors,
					boidSharedData = sharedData,
					deltaTime = deltaTime,
					localToWorldType = ltwComponentHandle
				};

				var behaviourCalculationDependency = JobHandle.CombineDependencies(calculateAlignmentJobHandler, 
																				   calculateSeparationJobHandle,
																				   calculateFollowingTargetJobHandle);
				
				Dependency = moveBoidJobChunk.ScheduleParallel(boidQuery, behaviourCalculationDependency);
				
				var applyBordersJobChunk = new ApplyBordersBoidChunk()
				{
					borderRadius = borderRadius,
					borderPosition = borderOrigin,
					localToWorldType = ltwComponentHandle,
				};
				
				Dependency = applyBordersJobChunk.ScheduleParallel(boidQuery, Dependency);

				neighboursLTW.Dispose(Dependency);
			}
			
			boidSharedDatas.Clear();
		}
	}
}