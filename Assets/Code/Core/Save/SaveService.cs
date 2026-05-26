using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Core.Editor;
using Core.Extensions;
using Core.GameLoop;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using Essential;
using UnityEngine;

namespace Core.Save
{
    public sealed class SaveService : IService, IInitializeListener
    {
        private static readonly string SavePath = Path.Combine(Application.persistentDataPath, "save_slots.dat");
        private static readonly string Password = _generateKey();
        //private static readonly string HmacKey = "HMAC_SECRET_KEY_123";

        public SaveContainer ModelsContainer => _container ??= _loadContainer();

        private SaveContainer _container;

        private const bool _useEncryption = false;
        public bool IsInitialized { get; set; }

        private SaveSettings _saveSettings;

        private string _lastSlot = "first_slot";


        public UniTask Initialize()
        {
            _saveSettings = Container.Instance.GetConfig<SaveSettings>();

            return UniTask.CompletedTask;
        }

        public void Save()
        {
            try
            {
                string gameModelJson = Container.Instance.GetService<GameModel>().AsJson();

                ModelsContainer.Slots[LastUsedSlot] = gameModelJson;
                
                string json = ModelsContainer.AsJson();

                Log.Info(this, $"Save: {json} ");

                File.WriteAllText(SavePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Save failed: {e.Message}");
            }
        }

        public GameModel LoadLastGameModel()
        {
            if (string.IsNullOrEmpty(ModelsContainer.LastSlot))
            {
                ModelsContainer.LastSlot = _lastSlot;
            }
            
            Log.Info(this, $"Load last game model. slot - {ModelsContainer.LastSlot}");
            
            return LoadGameModel(ModelsContainer.LastSlot);
        }

        public string LastUsedSlot
        {
            get => ParrelSyncUtility.IsClone() ? "clone" : _lastSlot;
            private set
            {
                if (ParrelSyncUtility.IsClone())
                {
                    return;
                }

                _lastSlot = value;
            }
        }

        public void DeleteGameModel(string slotId)
        {
            if (ModelsContainer.Slots.Remove(slotId))
            {
                if (ModelsContainer.LastSlot == slotId)
                {
                    ModelsContainer.LastSlot = null;
                    LastUsedSlot = null;
                }

                File.WriteAllText(SavePath, ModelsContainer.AsJson());
            }
        }

        private GameModel LoadGameModel(string slotId) 
        {
            try
            {
                if (!ModelsContainer.Slots.TryGetValue(slotId, out string slotData))
                {
                    GameModel newGameModel = new();
                    
                    newGameModel.CopyFrom(_saveSettings.DefaultModel);
                    
                    Log.Info(this, $"create new slot {slotId} {newGameModel.AsJson()}");
                  
                    ModelsContainer.Slots.Add(slotId, newGameModel.AsJson());
                    
                    return newGameModel;
                }

                Log.Info(this, $"Load: {slotData}");

                return slotData.AsData<GameModel>();
            }
            catch (Exception e)
            {
                Debug.LogError($"Load failed: {e.Source} {e.Message}");
                return null;
            }
        }

        private SaveContainer _loadContainer()
        {
            Log.Info(this, $"_loadContainer: {SavePath}");

            if (!File.Exists(SavePath))
            {
                Log.Info(this, $"_loadContainer: not exists path. return new save container");
                return new SaveContainer();
            }

            try
            {
                string json = File.ReadAllText(SavePath);
                Log.Info(this, $"_loadContainer: try deserialize {json}");
                return json.AsData<SaveContainer>();
            }
            catch
            {
                Log.Info(this, $"_loadContainer: deserialization error. return new save container");
                return new SaveContainer();
            }
        }

        public List<string> GetSlotIds()
        {
            return new List<string>(ModelsContainer.Slots.Keys);
        }

        private static string _generateKey()
        {
            string deviceId = SystemInfo.deviceUniqueIdentifier;
            using SHA256 sha = SHA256.Create();
            byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(deviceId + "_extra_salt"));
            return Convert.ToBase64String(hash);
        }

        private byte[] _encrypt(string plainText, string password)
        {
            using Aes aes = Aes.Create();
            byte[] key = _deriveKey(password, aes.KeySize / 8);
            aes.Key = key;
            aes.GenerateIV();

            using ICryptoTransform encryptor = aes.CreateEncryptor();
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            byte[] result = new byte[aes.IV.Length + cipherBytes.Length];
            Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
            Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);
            return result;
        }

        private string _decrypt(byte[] encryptedData, string password)
        {
            using Aes aes = Aes.Create();
            byte[] key = _deriveKey(password, aes.KeySize / 8);
            aes.Key = key;

            byte[] iv = new byte[aes.BlockSize / 8];
            byte[] cipherBytes = new byte[encryptedData.Length - iv.Length];

            Buffer.BlockCopy(encryptedData, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(encryptedData, iv.Length, cipherBytes, 0, cipherBytes.Length);
            aes.IV = iv;

            using ICryptoTransform decryptor = aes.CreateDecryptor();
            byte[] plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
            return Encoding.UTF8.GetString(plainBytes);
        }

        private byte[] _deriveKey(string password, int keySize)
        {
            using Rfc2898DeriveBytes pbkdf2 =
                new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes("_fixed_salt_"), 10000);
            return pbkdf2.GetBytes(keySize);
        }

        private byte[] _computeHmac(byte[] data, string key)
        {
            using HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            return hmac.ComputeHash(data);
        }

        private bool _compareBytes(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i]) return false;
            }

            return true;
        }
    }
}