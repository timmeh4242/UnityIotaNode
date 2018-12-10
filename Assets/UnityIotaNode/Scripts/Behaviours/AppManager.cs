using UnityEngine;
using Unity.Entities;
using uIota;
using System.Collections;
using System.Security.Cryptography;
using System.Text;

public class AppManager : MonoBehaviour
{
    public static World World;
    public static EntityManager EntityManager;
    //private IotaSystem iotaSystem;

    private SHA256 hasher = SHA256.Create();
    private static char[] chars = new char[] { 'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z','9', };

    //public static EntityArchetype TransactionArchetype;
    public static EntityArchetype BaseTransactionArchetype;
    public static EntityArchetype ValueTransactionArchetype;
    public static EntityArchetype ZeroValueTransactionArchetype;

    public static GameObject TransactionPrefab;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize()
    {
        World = World.Active;
        EntityManager = World.GetOrCreateManager<EntityManager>();

        //TransactionArchetype = World.Active.GetOrCreateManager<EntityManager>().CreateArchetype
        //(
        //    typeof(Hash),
        //    typeof(SignatureMessageFragment),
        //    typeof(Address),
        //    //typeof(TransactionValue),
        //    typeof(Bundle),
        //    typeof(Trunk),
        //    typeof(Branch),
        //    typeof(Nonce)
        //);

        BaseTransactionArchetype = World.Active.GetOrCreateManager<EntityManager>().CreateArchetype
        (
            //typeof(Transaction),
            typeof(Hash),
            typeof(SignatureMessageFragment),
            typeof(Address),
            //typeof(TransactionValue),
            typeof(Bundle),
            typeof(Trunk),
            typeof(Branch),
            typeof(Nonce)
        );

        ValueTransactionArchetype = World.Active.GetOrCreateManager<EntityManager>().CreateArchetype
        (
            //typeof(Transaction),
            typeof(Hash),
            typeof(SignatureMessageFragment),
            typeof(Address),
            typeof(IotaValue),
            typeof(Bundle),
            typeof(Trunk),
            typeof(Branch),
            typeof(Nonce)
        );

        ZeroValueTransactionArchetype = World.Active.GetOrCreateManager<EntityManager>().CreateArchetype
        (
            //typeof(Transaction),
            typeof(Hash),
            typeof(SignatureMessageFragment),
            typeof(Address),
            typeof(Bundle),
            typeof(Trunk),
            typeof(Branch),
            typeof(Nonce)
        );

        TransactionPrefab = Resources.Load<GameObject>("TransactionPrefab");
    }

    private void Awake()
    {
        //World = World.Active;
        //EntityManager = World.GetOrCreateManager<EntityManager>();
        //iotaSystem = world.GetExistingManager<IotaSystem>();
    }

    private void OnEnable()
    {
        StartCoroutine(TransactionLoop());
    }

    private void OnDisable()
    {
        StopCoroutine(TransactionLoop());
    }

    private IEnumerator TransactionLoop()
    {
        while(true)
        {
            var numberOfTx = Random.Range(1, 5);
            for (var i = 0; i < numberOfTx; i++)
            {
                CreateTransaction();
            }
            yield return new WaitForSeconds(3f);
        }
    }

    public static Entity CreateTransaction()
    {
        //var entity = EntityManager.CreateEntity(AppManager.BaseTransactionArchetype);
        var entity = EntityManager.CreateEntity();

        EntityManager.AddBuffer<Hash>(entity);
        byte[] hashBytes = new byte[81];
        //NativeArray<Hash> hashBytes;
        using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
        {
            crypto.GetBytes(hashBytes);
        }
        for (var i = 0; i < hashBytes.Length; i++)
        {
            hashBytes[i] = (byte)(hashBytes[i] % chars.Length);
        }

        var hashArray = new Hash[hashBytes.Length];
        for (var i = 0; i < hashBytes.Length; i++)
        {
            hashArray[i].Value = hashBytes[i];
        }
        var hashBuffer = EntityManager.GetBuffer<Hash>(entity);
        hashBuffer.CopyFrom(hashArray);

        //buffer.Clear();
        //buffer.Reinterpret<Hash>().AddRange(hashBytes);

        var timeStamps = new TimeStamps();
        timeStamps.TimeStamp = (long)Time.realtimeSinceStartup;
        EntityManager.AddComponent(entity, typeof(TimeStamps));
        EntityManager.SetComponentData(entity, timeStamps);
        EntityManager.AddBuffer<Trunk>(entity);
        EntityManager.AddBuffer<Branch>(entity);
        return entity;
    }

    //private byte[] GetHash(string message)
    //{
    //    return hasher.ComputeHash(Encoding.UTF8.GetBytes(message));
    //}

    //private string GetString(byte[] data)
    //{
    //    var sBuilder = new StringBuilder();

    //    for (int i = 0; i < data.Length; i++)
    //    {
    //        sBuilder.Append(data[i].ToString("x2"));
    //    }

    //    return sBuilder.ToString();
    //}
}
