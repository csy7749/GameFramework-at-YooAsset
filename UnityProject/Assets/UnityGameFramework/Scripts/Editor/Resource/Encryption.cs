using System;
using System.IO;
using GameFramework.Resource;
using YooAsset;

/// <summary>
/// 文件偏移加密方式
/// </summary>
public class FileOffsetEncryption : IEncryptionServices
{
    public EncryptResult Encrypt(EncryptFileInfo fileInfo)
    {
        if (fileInfo.BundleName.Contains("DLL"))
        {
            
        }
        
        int offset = 32;
        byte[] fileData = File.ReadAllBytes(fileInfo.FileLoadPath);
        var encryptedData = new byte[fileData.Length + offset];
        Buffer.BlockCopy(fileData, 0, encryptedData, offset, fileData.Length);

        EncryptResult result = new EncryptResult();
        result.Encrypted = true;
        result.EncryptedData = encryptedData;
        return result;
    }
}

/// <summary>
/// 文件流加密方式
/// </summary>
public class FileStreamEncryption : IEncryptionServices
{
    public EncryptResult Encrypt(EncryptFileInfo fileInfo)
    {
        if (fileInfo.BundleName.Contains("DLL"))
        {
            
        }
        
        var fileData = File.ReadAllBytes(fileInfo.FileLoadPath);
        for (int i = 0; i < fileData.Length; i++)
        {
            fileData[i] ^= BundleStream.KEY;
        }

        EncryptResult result = new EncryptResult();
        result.Encrypted = true;
        result.EncryptedData = fileData;
        return result;
    }
}