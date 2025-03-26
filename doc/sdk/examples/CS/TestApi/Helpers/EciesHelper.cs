using Net.Sf.Pkcs11.Objects;
using System;
using System.Security.Cryptography;
using System.Text;

public static class EciesHelper
{



    //*********************************************************//
    // Use public ECDsa key and export the parameters to use Diffiehellamann and a shared secret
    //***********************************************************//

    public static byte[] EncryptWithPublicKey(ECDsa publicKey, byte[] data)
    {
        // Create a new ephemeral key pair for key exchange
        using var ephemeral = ECDiffieHellman.Create(publicKey.ExportParameters(false).Curve);

        // Derive the shared secret
        byte[] sharedSecret;
        try
        {
            sharedSecret = ephemeral.DeriveKeyFromHash(
                otherPartyPublicKey: ephemeral.PublicKey,
                hashAlgorithm: HashAlgorithmName.SHA256,
                secretPrepend: null,
                secretAppend: null);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to derive shared secret", ex);
        }

        // Use the shared secret for symmetric encryption (AES)
        using var aes = Aes.Create();
        aes.Key = sharedSecret[..32]; // Take first 32 bytes for AES-256
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        // Encrypt the data
        using var encryptor = aes.CreateEncryptor();
        byte[] iv = aes.IV;
        byte[] encryptedData = encryptor.TransformFinalBlock(data, 0, data.Length);

        // Prepend the ephemeral public key and IV to the encrypted data
        byte[] ephemeralPublicKey = ephemeral.ExportSubjectPublicKeyInfo();
        byte[] result = new byte[ephemeralPublicKey.Length + iv.Length + encryptedData.Length];
        Buffer.BlockCopy(ephemeralPublicKey, 0, result, 0, ephemeralPublicKey.Length);
        Buffer.BlockCopy(iv, 0, result, ephemeralPublicKey.Length, iv.Length);
        Buffer.BlockCopy(encryptedData, 0, result, ephemeralPublicKey.Length + iv.Length, encryptedData.Length);

        return result;
    }

    //Use private key to decrypt shared secret 
    public static byte[] DecryptWithPrivateKey(ECDsa privateKey, byte[] encryptedData)
    {

       
        // Extract components from the encrypted data
        // Structure: [ephemeral public key][IV][encrypted data]

        // Determine the length of the ephemeral public key (ASN.1 encoded)
        // 91 bytes public key + 23 bytes overhead
        int ephemeralKeyLength = 120; // Adjust based on your curve

        // Extract the ephemeral public key
        byte[] ephemeralPublicKey = new byte[ephemeralKeyLength];
        Buffer.BlockCopy(encryptedData, 0, ephemeralPublicKey, 0, ephemeralKeyLength);

        // Extract the IV (16 bytes for AES)
        byte[] iv = new byte[16];
        Buffer.BlockCopy(encryptedData, ephemeralKeyLength, iv, 0, 16);

        // Extract the actual encrypted data
        byte[] cipherText = new byte[encryptedData.Length - ephemeralKeyLength - 16];
        Buffer.BlockCopy(encryptedData, ephemeralKeyLength + 16, cipherText, 0, cipherText.Length);

        // Import the ephemeral public key
        using var ephemeral = ECDiffieHellman.Create();
        ephemeral.ImportSubjectPublicKeyInfo(ephemeralPublicKey, out _);

        // Derive the shared secret using our private key and their public key
        byte[] sharedSecret;
        try
        {
            sharedSecret = ephemeral.DeriveKeyFromHash(
                ephemeral.PublicKey,
                HashAlgorithmName.SHA256,
                null,
                null);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to derive shared secret", ex);
        }

        // Use the shared secret for symmetric decryption (AES)
        using var aes = Aes.Create();
        aes.Key = sharedSecret[..32]; // Take first 32 bytes for AES-256
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        // Decrypt the data
        using var decryptor = aes.CreateDecryptor();
        byte[] decryptedData = decryptor.TransformFinalBlock(cipherText, 0, cipherText.Length);

        return decryptedData;
    }


   


}