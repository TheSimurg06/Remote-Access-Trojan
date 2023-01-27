﻿using System;
using System.IO;
using System.Security.Cryptography;

class Program
{
    static void Main(string[] args)
    {
        // File to be encrypted
        string originalFile = @"mylog_archive.txt";

        // Encrypted file
        string encryptedFile = @"enc_data.txt";

        // Password used to encrypt the file
        string password = "mypassword";

        // Salt used to encrypt the file
        byte[] salt = new byte[] { 0x26, 0xdc, 0xff, 0x00, 0xad, 0xed, 0x7a, 0xee, 0xc5, 0xfe, 0x07, 0xaf, 0x4d, 0x08, 0x22, 0x3c };

        // Decrypt the file
        DecryptFile(encryptedFile, originalFile, password, salt);
    }
    static void DecryptFile(string inputFile, string outputFile, string password, byte[] salt)
    {
        // Create a new Aes object to perform string symmetric encryption
        using (Aes aes = Aes.Create())
        {
            // Generate the key from the shared password and salt
            Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(password, salt);

            // Set the key and IV
            aes.Key = key.GetBytes(aes.KeySize / 8);
            aes.IV = key.GetBytes(aes.BlockSize / 8);

            // Create a decrytor to perform the stream transform
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            // Create the streams used for decryption
            using (FileStream input = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
            using (FileStream output = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
            using (CryptoStream cryptoStream = new CryptoStream(input, decryptor, CryptoStreamMode.Read))
            {
                // Decrypt the file
                cryptoStream.CopyTo(output);
            }
        }
    }
}
