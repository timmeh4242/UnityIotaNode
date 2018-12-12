using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;

namespace uIota
{
    //[DisableAutoCreation]
    public class WeightedRandomWalkTipSelectionSystem : JobComponentSystem
    {
        class Barrier : BarrierSystem { }
        [Inject] Barrier barrier;

        ComponentGroup unprocessedTx;
        ComponentGroup processedTx;

        NativeMultiHashMap<Entity, Entity> parentToChildTree;
        //NativeHashMap<Entity, NativeList<Entity>> parentToChildTree;

        protected override void OnCreateManager()
        {
            base.OnCreateManager();

            parentToChildTree = new NativeMultiHashMap<Entity, Entity>(1024, Allocator.Persistent);
            //parentToChildTree = new NativeHashMap<Entity, NativeList<Entity>>(1024, Allocator.Persistent);

            unprocessedTx = GetComponentGroup(typeof(Trunk), typeof(Branch), ComponentType.Subtractive(typeof(HasTips)));
            processedTx = GetComponentGroup(typeof(Trunk), typeof(Branch), typeof(Hash), typeof(HasTips));
        }

        protected override void OnDestroyManager()
        {
            base.OnDestroyManager();

            parentToChildTree.Dispose();
        }

        //[BurstCompile]
        //struct GetTipsJob : IJobParallelFor
        struct GetTipsJob : IJob
        {
            [ReadOnly] [DeallocateOnJobCompletion]
            public NativeArray<ArchetypeChunk> chunks;
            [ReadOnly] public ArchetypeChunkBufferType<Trunk> trunkType;
            [ReadOnly] public ArchetypeChunkBufferType<Branch> branchType;
            [ReadOnly] public ArchetypeChunkEntityType entityType;

            [ReadOnly] public BufferArray<Hash> processedHashes;
            [ReadOnly] public EntityArray processedEntities;

            public NativeMultiHashMap<Entity, Entity> parentToChildTree;
            //public NativeHashMap<Entity, NativeList<Entity>> parentToChildTree;
            [ReadOnly] public Entity treeRoot;

            //public void Execute(int index)
            //{
            //}

            public void Execute()
            {
                for(var index = 0; index < chunks.Length; index++)
                {
                    var chunk = chunks[index];
                    var trunksAccessor = chunk.GetBufferAccessor(trunkType);
                    var branchesAccessor = chunk.GetBufferAccessor(branchType);
                    var entities = chunk.GetNativeArray(entityType);

                    //var rnd = new Unity.Mathematics.Random((uint)(index + 1));
                    var rnd = new Unity.Mathematics.Random(0x6E624EB7u);

                    for (var i = 0; i < chunk.Count; i++)
                    {
                        var trunkBuffer = trunksAccessor[i];
                        var branchBuffer = branchesAccessor[i];
                        var childEntity = entities[i];

                        NativeMultiHashMapIterator<Entity> iterator;
                        Entity trunkParent = treeRoot;
                        Entity branchParent = treeRoot;
                        NativeList<Entity> walkChildren;

                        while (parentToChildTree.TryGetValues(trunkParent, out walkChildren, out iterator))
                        {
                            trunkParent = walkChildren[rnd.NextInt(0, walkChildren.Length)];
                            walkChildren.Dispose();
                        }
                        //trunkParent = walkChildren[rnd.NextInt(0, walkChildren.Length)];
                        walkChildren.Dispose();
                        var nextIndex = 0;
                        for(var j = 0; j < processedEntities.Length; j++)
                        {
                            nextIndex = j;
                            if(trunkParent == processedEntities[j]) { break; }
                        }
                        var hashBuffer = processedHashes[nextIndex];
                        var parentEntity = processedEntities[nextIndex];
                        trunkBuffer.Clear();
                        trunkBuffer.CopyFrom(hashBuffer.Reinterpret<Trunk>().ToNativeArray());
                        parentToChildTree.Add(parentEntity, childEntity);

                        while (parentToChildTree.TryGetValues(branchParent, out walkChildren, out iterator))
                        {
                            branchParent = walkChildren[rnd.NextInt(0, walkChildren.Length)];
                            walkChildren.Dispose();
                        }
                        //branchParent = walkChildren[rnd.NextInt(0, walkChildren.Length)];
                        walkChildren.Dispose();
                        nextIndex = 0;
                        for (var j = 0; j < processedEntities.Length; j++)
                        {
                            nextIndex = j;
                            if (branchParent == processedEntities[j]) { break; }
                        }
                        hashBuffer = processedHashes[nextIndex];
                        parentEntity = processedEntities[nextIndex];
                        branchBuffer.Clear();
                        branchBuffer.CopyFrom(hashBuffer.Reinterpret<Branch>().ToNativeArray());
                        parentToChildTree.Add(parentEntity, childEntity);
                    }
                }
            }
        }

        //[BurstCompile]
        struct InitializeJob : IJob
        {
            [ReadOnly] public EntityArray entities;
            [ReadOnly] public EntityCommandBuffer.Concurrent entityCommandBuffer;

            public void Execute()
            {
                for (var i = 0; i < entities.Length; i++)
                {
                    entityCommandBuffer.AddComponent(i, entities[i], new HasTips());
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (unprocessedTx.GetEntityArray().Length < 1) { return inputDeps; }

            var chunks = unprocessedTx.CreateArchetypeChunkArray(Allocator.TempJob);
            var processedHashes = processedTx.GetBufferArray<Hash>();
            var job = new GetTipsJob()
            {
                chunks = chunks,
                trunkType = GetArchetypeChunkBufferType<Trunk>(),
                branchType = GetArchetypeChunkBufferType<Branch>(),
                entityType = GetArchetypeChunkEntityType(),
                processedHashes = processedHashes,
                processedEntities = processedTx.GetEntityArray(),
                parentToChildTree = parentToChildTree,
                treeRoot = processedTx.GetEntityArray()[0] //HACK + UNSAFE - set the tree root to 'genesis'
            };
            //var getTipsHandle = job.Schedule(chunks.Length, 32);
            var getTipsHandle = job.Schedule(inputDeps);

            var initializeTxHandle = new InitializeJob()
            {
                entities = unprocessedTx.GetEntityArray(),
                entityCommandBuffer = barrier.CreateCommandBuffer().ToConcurrent()
            };
            var initializeHandle = initializeTxHandle.Schedule(getTipsHandle);

            return initializeHandle;
        }
    }
}
