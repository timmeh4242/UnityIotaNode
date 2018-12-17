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
        NativeList<Entity> walkChildren;

        protected override void OnCreateManager()
        {
            base.OnCreateManager();

            parentToChildTree = new NativeMultiHashMap<Entity, Entity>(1024, Allocator.Persistent);
            walkChildren = new NativeList<Entity>(Allocator.Persistent);

            unprocessedTx = GetComponentGroup(typeof(Trunk), typeof(Branch), ComponentType.Subtractive(typeof(HasTips)));
            processedTx = GetComponentGroup(typeof(Trunk), typeof(Branch), typeof(Hash), typeof(HasTips));
        }

        protected override void OnDestroyManager()
        {
            base.OnDestroyManager();

            parentToChildTree.Dispose();
            walkChildren.Dispose();
        }

        //[BurstCompile]
        //struct GetTipsJob : IJobParallelFor
        struct GetTipsJob : IJob
        {
            [ReadOnly] [DeallocateOnJobCompletion]
            public NativeArray<ArchetypeChunk> chunks;
            [ReadOnly] public ArchetypeChunkComponentType<Trunk> trunkType;
            [ReadOnly] public ArchetypeChunkComponentType<Branch> branchType;
            [ReadOnly] public ArchetypeChunkEntityType entityType;

            [ReadOnly] public BufferArray<Hash> processedHashes;
            [ReadOnly] public EntityArray processedEntities;

            public NativeMultiHashMap<Entity, Entity> parentToChildTree;
            [ReadOnly] public Entity treeRoot;
            //[DeallocateOnJobCompletion]
            public NativeList<Entity> walkChildren;

            public EntityCommandBuffer.Concurrent buffer;

            //public void Execute(int index)
            //{
            //}

            public void Execute()
            {
                for(var index = 0; index < chunks.Length; index++)
                {
                    var chunk = chunks[index];
                    var trunksAccessor = chunk.GetNativeArray(trunkType);
                    var branchesAccessor = chunk.GetNativeArray(branchType);
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
                        while (parentToChildTree.TryGetValues(trunkParent, walkChildren, out iterator))
                        {
                            trunkParent = walkChildren[rnd.NextInt(0, walkChildren.Length)];
                            walkChildren.Clear();
                        }
                        //UnityEngine.Debug.Log(childEntity.Index + " " + trunkParent.Index);
                        walkChildren.Clear();
                        buffer.SetComponent(i, childEntity, new Trunk { Value = trunkParent });

                        Entity branchParent = treeRoot;
                        while (parentToChildTree.TryGetValues(branchParent, walkChildren, out iterator))
                        {
                            branchParent = walkChildren[rnd.NextInt(0, walkChildren.Length)];
                            walkChildren.Clear();
                        }
                        //UnityEngine.Debug.Log(childEntity.Index + " " + branchParent.Index);
                        walkChildren.Clear();
                        buffer.SetComponent(i, childEntity, new Branch { Value = branchParent });

                        parentToChildTree.Add(trunkParent, childEntity);
                        parentToChildTree.Add(branchParent, childEntity);
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
                trunkType = GetArchetypeChunkComponentType<Trunk>(),
                branchType = GetArchetypeChunkComponentType<Branch>(),
                entityType = GetArchetypeChunkEntityType(),
                processedHashes = processedHashes,
                processedEntities = processedTx.GetEntityArray(),
                parentToChildTree = parentToChildTree,
                treeRoot = processedTx.GetEntityArray()[0], //HACK + UNSAFE - set the tree root to 'genesis'
                walkChildren = walkChildren,
                buffer = barrier.CreateCommandBuffer().ToConcurrent()
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
