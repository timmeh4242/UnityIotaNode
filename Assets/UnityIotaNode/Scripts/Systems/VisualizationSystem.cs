using Unity.Entities;
using Unity.Collections;

namespace uIota
{
    public class VisualizationSystem : ComponentSystem
    {
        class Barrier : BarrierSystem { }
        [Inject] Barrier barrier;

        ComponentGroup addedTx;
        //ComponentGroup removedTx;

        //ArchetypeChunkEntityType entityChunkType;
        //ArchetypeChunkBufferType<Hash> hashChunkType;
        //ArchetypeChunkBufferType<Trunk> trunkChunkType;
        //ArchetypeChunkBufferType<Branch> branchChunkType;
        //ArchetypeChunkComponentType<TimeStamps> timeStampChunkType;

        protected override void OnCreateManager()
        {
            base.OnCreateManager();

            addedTx = GetComponentGroup(typeof(TimeStamps), typeof(Hash), typeof(Trunk), typeof(Branch),typeof(HasTips), ComponentType.Subtractive(typeof(Initialized)));
            //removedTx = GetComponentGroup(typeof(TimeStamps), typeof(Initialized));
        }

        struct Initialized : ISystemStateComponentData { }

        protected override void OnUpdate()
        {
            var chunks = addedTx.CreateArchetypeChunkArray(Allocator.TempJob);
            if(chunks.Length < 1)
            {
                chunks.Dispose();
                return;
            }

            var entityChunkType = GetArchetypeChunkEntityType();
            var hashChunkType = GetArchetypeChunkBufferType<Hash>();
            var trunkChunkType = GetArchetypeChunkBufferType<Trunk>();
            var branchChunkType = GetArchetypeChunkBufferType<Branch>();
            var timeStampChunkType = GetArchetypeChunkComponentType<TimeStamps>();

            var commandBuffer = barrier.CreateCommandBuffer();

            for (var i = 0; i < chunks.Length; i++)
            {
                var chunk = chunks[i];
                var entities = chunk.GetNativeArray(entityChunkType);
                var hashes = chunk.GetBufferAccessor(hashChunkType);
                var trunks = chunk.GetBufferAccessor(trunkChunkType);
                var branches = chunk.GetBufferAccessor(branchChunkType);
                var timeStamps = chunk.GetNativeArray(timeStampChunkType);
                for (var j = 0; j < chunk.Count; j++)
                {
                    var go = UnityEngine.GameObject.Instantiate(AppManager.TransactionPrefab);
                    var yPosition = UnityEngine.Mathf.Lerp((float)-chunk.Count / 2f, (float)chunk.Count / 2f, (float)j / (float)chunk.Count) * 2f;
                    go.transform.position = new UnityEngine.Vector3(timeStamps[j].TimeStamp, yPosition, 0);
                    var hash = "";
                    var hashArray = hashes[j];
                    for (var k = 0; k < hashArray.Length; k++)
                    { hash += ((int)hashArray[k].Value).ToString(); }
                    go.name = hash;

                    if (hash != "999999999")
                    {
                        var trunk = "";
                        var trunkArray = trunks[j];
                        for (var k = 0; k < trunkArray.Length; k++)
                        { trunk += ((int)trunkArray[k].Value).ToString(); }
                        var trunkLineParent = UnityEngine.GameObject.Instantiate(AppManager.TransactionPrefab, go.transform);
                        var trunkLineRenderer = trunkLineParent.AddComponent<UnityEngine.LineRenderer>();
                        var trunkGO = UnityEngine.GameObject.Find(trunk);
                        trunkLineRenderer.SetPosition(0, go.transform.position);
                        trunkLineRenderer.SetPosition(1, trunkGO.transform.position);
                        trunkLineRenderer.startWidth = 0.15f;
                        trunkLineRenderer.endWidth = 0.15f;

                        var branch = "";
                        var branchArray = branches[j];
                        for (var k = 0; k < branchArray.Length; k++)
                        { branch += ((int)branchArray[k].Value).ToString(); }
                        var branchLineParent = UnityEngine.GameObject.Instantiate(AppManager.TransactionPrefab, go.transform);
                        var branchLineRenderer = branchLineParent.AddComponent<UnityEngine.LineRenderer>();
                        var branchGO = UnityEngine.GameObject.Find(branch);
                        branchLineRenderer.SetPosition(0, go.transform.position);
                        branchLineRenderer.SetPosition(1, branchGO.transform.position);
                        branchLineRenderer.startWidth = 0.15f;
                        branchLineRenderer.endWidth = 0.15f;
                    }

                    commandBuffer.AddComponent(entities[j], new Initialized());
                }
            }
            chunks.Dispose();

            //for (var i = 0; i < removedTx.GetEntityArray().Length; i++)
            //{

            //}
        }
    }
}