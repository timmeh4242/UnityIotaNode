using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

public class EventSystem : JobComponentSystem
{

    public void Publish(EventData eventData)
    {
        eventQueue.Enqueue(eventData);
    }

    NativeQueue<EventData> eventQueue = new NativeQueue<EventData>();

    struct DestroyEventsJob : IJobProcessComponentDataWithEntity<EventData>
    {
        public EntityCommandBuffer.Concurrent ComponentBuffer;
        public void Execute(Entity entity, int index, [ReadOnly] ref EventData data)
        {
            ComponentBuffer.DestroyEntity(index, entity);
        }
    }

    struct CreateEventsJob : IJob
    {
        public EntityCommandBuffer ComponentBuffer;
        public NativeQueue<EventData> EventQueue;
        public void Execute()
        {
            while (EventQueue.TryDequeue(out EventData eventData))
            {
                ComponentBuffer.CreateEntity();
                ComponentBuffer.AddComponent(eventData);
            }
        }
    }

    protected override void OnCreateManager()
    {
        if (!eventQueue.IsCreated)
        {
            eventQueue = new NativeQueue<EventData>(Allocator.Persistent);
        }
    }

    protected override void OnDestroyManager()
    {
        if (eventQueue.IsCreated)
        {
            eventQueue.Dispose();
        }
    }

    [Inject] EndFrameBarrier EndFrameBarrier;
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var destroyEventsJob = new DestroyEventsJob
        {
            ComponentBuffer = EndFrameBarrier.CreateCommandBuffer().ToConcurrent(),
        };
        var createEventsJob = new CreateEventsJob
        {
            ComponentBuffer = EndFrameBarrier.CreateCommandBuffer(),
            EventQueue = eventQueue,
        };
        inputDeps = destroyEventsJob.Schedule(this, inputDeps);
        inputDeps = createEventsJob.Schedule(inputDeps);
        return inputDeps;
    }
}

public struct EventData : IComponentData
{
    public int someValues;
}

//To create an event:
//World.Active.GetExistingManager<FireEventSystem>().FireEvent(new EventData());