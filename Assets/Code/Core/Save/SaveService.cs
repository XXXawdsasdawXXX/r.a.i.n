using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Core.Editor;
using Core.ServiceLocator;
using Essential;
using Newtonsoft.Json;
using UnityEngine;

namespace Core.Save
{
    internal sealed class SaveService : IService
    {
        private static readonly string SavePath = Path.Combine(Application.persistentDataPath, "save_slots.dat");
        private static readonly string Password = _generateKey();
        private static readonly string HmacKey = "HMAC_SECRET_KEY_123";

        private const bool _useEncryption = false;
        
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
        
        private string _lastSlot;

        private class SaveContainer
        {
            public Dictionary<string, string> Slots = new();
            public string LastSlot;
        }

        public void Save<T>(string slotId, T data)
        {
            try
            {
                SaveContainer container = _loadContainer();
                string json = JsonConvert.SerializeObject(data);

                if (_useEncryption)
                {
                    byte[] encryptedData = _encrypt(json, Password);
                    byte[] hmac = _computeHmac(encryptedData, HmacKey);
                    byte[] fullBytes = new byte[hmac.Length + encryptedData.Length];
                    Buffer.BlockCopy(hmac, 0, fullBytes, 0, hmac.Length);
                    Buffer.BlockCopy(encryptedData, 0, fullBytes, hmac.Length, encryptedData.Length);
                    container.Slots[slotId] = Convert.ToBase64String(fullBytes);
                }
                else
                {
                    container.Slots[slotId] = json;
                }

                container.LastSlot = slotId;
                LastUsedSlot = slotId;

                string fullJson = JsonConvert.SerializeObject(container);
                File.WriteAllText(SavePath, fullJson);
            }
            catch (Exception e)
            {
                Debug.LogError($"Save failed: {e.Message}");
            }
        }

        public T Load<T>(string slotId) where T : class
        {
            try
            {
                if (!File.Exists(SavePath)) return null;

                string fileContent = File.ReadAllText(SavePath);
                SaveContainer container = JsonConvert.DeserializeObject<SaveContainer>(fileContent);
                if (!container.Slots.TryGetValue(slotId, out string slotData)) return null;

                LastUsedSlot = slotId;

                if (_useEncryption)
                {
                    byte[] rawData = Convert.FromBase64String(slotData);
                    if (rawData.Length < 32) return null;

                    byte[] storedHmac = new byte[32];
                    byte[] encryptedData = new byte[rawData.Length - 32];
                    Buffer.BlockCopy(rawData, 0, storedHmac, 0, 32);
                    Buffer.BlockCopy(rawData, 32, encryptedData, 0, encryptedData.Length);

                    byte[] computedHmac = _computeHmac(encryptedData, HmacKey);
                    if (!_compareBytes(storedHmac, computedHmac))
                    {
                        Debug.LogError("HMAC validation failed. Save file may have been tampered with.");
                        return null;
                    }

                    string json = _decrypt(encryptedData, Password);
                    return JsonConvert.DeserializeObject<T>(json);
                }
                else
                {
                    return JsonConvert.DeserializeObject<T>(slotData);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Load failed: {e.Message}");
                return null;
            }
        }

        public T LoadLast<T>() where T : class
        {
            SaveContainer container = _loadContainer();
            
            if (!string.IsNullOrEmpty(container.LastSlot))
            {
                return Load<T>(container.LastSlot);
            }

            return null;
        }

        public void DeleteSlot(string slotId)
        {
            SaveContainer container = _loadContainer();
            if (container.Slots.Remove(slotId))
            {
                if (container.LastSlot == slotId)
                {
                    container.LastSlot = null;
                    LastUsedSlot = null;
                }

                string fullJson = JsonConvert.SerializeObject(container);
                File.WriteAllText(SavePath, fullJson);
            }
        }

        public List<string> GetSlotIds()
        {
            SaveContainer container = _loadContainer();
            return new List<string>(container.Slots.Keys);
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

        private SaveContainer _loadContainer()
        {
            Log.Info(this, $"{SavePath}");
            if (!File.Exists(SavePath)) return new SaveContainer();
            try
            {
                string json = File.ReadAllText(SavePath);
                return JsonConvert.DeserializeObject<SaveContainer>(json) ?? new SaveContainer();
            }
            catch
            {
                return new SaveContainer();
            }
        }
    }
}