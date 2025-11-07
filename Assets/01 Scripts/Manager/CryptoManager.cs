using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Security.Cryptography;
using System.Text;
using System;

public class CryptoManager
{
    private byte[] key;
    private byte[] iv;

    // 클라이언트(실행 기기)에 종속되는 키 iv 경로
    private readonly string keyPath = string.Concat(Application.persistentDataPath, "/aesKey.dat");
    private readonly string ivPath = string.Concat(Application.persistentDataPath, "/aesIv.dat");

    public CryptoManager()
    {
#if UNITY_WEBGL
        //  WebGL 환경에서는 파일 접근 불가하므로 고정 Key/IV 사용
        key = Encoding.UTF8.GetBytes("1234567890abcdef1234567890abcdef"); // 32 bytes
        iv  = Encoding.UTF8.GetBytes("abcdef1234567890");                 // 16 bytes
#else
        // 데스크톱/모바일 환경에서는 기존 파일 기반 키 관리
        if (File.Exists(keyPath) && File.Exists(ivPath))
        {
            key = File.ReadAllBytes(keyPath);
            iv = File.ReadAllBytes(ivPath);
        }
        else
        {
            key = GenerateRandomBytes(32); // 256-bit key
            iv = GenerateRandomBytes(16);  // 128-bit IV

            File.WriteAllBytes(keyPath, key);
            File.WriteAllBytes(ivPath, iv);
        }
#endif
    }

    private byte[] GenerateRandomBytes(int length)
    {
        byte[] randomBytes = new byte[length];

        // RNGCryptoServiceProvider : 난수 발생기
        using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
        {
            rng.GetBytes(randomBytes);
        }
        return randomBytes;
    }

    public string EncryptString(string plainText)
    {
        try
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] encrypted = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);

                return Convert.ToBase64String(encrypted);
            }
        }
        catch (CryptographicException ex)
        {
            Debug.LogError($"[EncryptString] 암호화 실패: {ex.Message}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[EncryptString] 일반 예외 발생: {ex.Message}");
        }

        return null;
    }

    public string DecryptString(string cipherText)
    {
        try
        {
            byte[] buffer = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                byte[] decrypted = decryptor.TransformFinalBlock(buffer, 0, buffer.Length);

                return Encoding.UTF8.GetString(decrypted);
            }
        }
        catch (CryptographicException ex)
        {
            //Debug.LogError($"[DecryptString] 복호화 실패 (키/IV 불일치 가능성): {ex.Message}");
            DeleteCorruptedSave();
        }
        catch (FormatException ex)
        {
           // Debug.LogError($"[DecryptString] Base64 포맷 오류: {ex.Message}");
            DeleteCorruptedSave();
        }
        catch (Exception ex)
        {
            //Debug.LogError($"[DecryptString] 일반 예외 발생: {ex.Message}");
            DeleteCorruptedSave();
        }

        return null;
    }
    private void DeleteCorruptedSave()
    {
#if UNITY_WEBGL
        if (PlayerPrefs.HasKey("SaveData"))
        {
            PlayerPrefs.DeleteKey("SaveData");
            PlayerPrefs.Save();
            Debug.LogWarning("[CryptoManager] 손상된 SaveData 삭제 완료 (WebGL)");
        }
#else
        string path = string.Concat(Application.persistentDataPath, "/save.dat");
        if (File.Exists(path))
        {
            File.Delete(path);
           // Debug.LogWarning("[CryptoManager] 손상된 save.dat 삭제 완료");
        }
#endif
    }
}

