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

    //public static EntityArchetype TransactionArchetype;
    public static EntityArchetype BaseTransactionArchetype;
    public static EntityArchetype ValueTransactionArchetype;
    public static EntityArchetype ZeroValueTransactionArchetype;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize()
    {
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
            typeof(Transaction),
            typeof(Hash),
            typeof(SignatureMessageFragment),
            typeof(Address),
            //typeof(TransactionValue),
            typeof(Bundle),
            //typeof(Trunk),
            //typeof(Branch),
            typeof(Nonce)
        );

        ValueTransactionArchetype = World.Active.GetOrCreateManager<EntityManager>().CreateArchetype
        (
            typeof(Transaction),
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
            typeof(Transaction),
            typeof(Hash),
            typeof(SignatureMessageFragment),
            typeof(Address),
            typeof(Bundle),
            typeof(Trunk),
            typeof(Branch),
            typeof(Nonce)
        );
    }

    private void Awake()
    {
        World = World.Active;
        EntityManager = World.GetOrCreateManager<EntityManager>();
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
            var numberOfTx = Random.Range(0, 100);
            for (var i = 0; i < numberOfTx; i++)
            {
                CreateTransaction();
            }
            yield return new WaitForSeconds(3f);
        }
    }

    private void CreateTransaction()
    {
        //var entity = EntityManager.CreateEntity(AppManager.BaseTransactionArchetype);
        var entity = EntityManager.CreateEntity();
        EntityManager.AddComponent(entity, typeof(Transaction));
        EntityManager.AddBuffer<Hash>(entity);
        EntityManager.AddBuffer<Bundle>(entity);

        var hashBytes = Encoding.UTF8.GetBytes("YT9CVQDZMIFXNAYXAPHIFGEMIEBVGXIZVPXCYFI9YSOJRVWKY9SNYPWNXQVHGLVZTFWMBLSWEIPVA9999");
        var hashArray = new Hash[hashBytes.Length];
        for (var i = 0; i < hashBytes.Length; i++)
        {
            hashArray[i].Value = hashBytes[i];
        }
        var hashBuffer = EntityManager.GetBuffer<Hash>(entity);
        hashBuffer.CopyFrom(hashArray);
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
