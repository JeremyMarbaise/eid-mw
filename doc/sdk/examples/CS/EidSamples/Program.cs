/* ****************************************************************************

 * eID Middleware Project.
 * Copyright (C) 2010-2010 FedICT.
 *
 * This is free software; you can redistribute it and/or modify it
 * under the terms of the GNU Lesser General Public License version
 * 3.0 as published by the Free Software Foundation.
 *
 * This software is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this software; if not, see
 * http://www.gnu.org/licenses/.

**************************************************************************** */

using EidSamples.tests;
using System;

using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace EidSamples
{
    class Program
    {
        static void Main(string[] args)
        {
            // Initialize your data reader
            ReadData dataTest = new ReadData("beidpkcs11.dll");
            Sign sign = new Sign("beidpkcs11.dll");
            dataTest.GetPublicKey("Authentication");
            

            //Sign signTest = new Sign("beidpkcs11.dll");
            //byte[] testdata = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
            //byte[] signeddata = signTest.DoSign(testdata, "Authentication");
            //for(int i=0;i<8;i++) 
            //{
            //    Console.WriteLine(signeddata[i]);

            //}

            




            //// Get the photo as a byte array
            //byte[] photoBytes = dataTest.GetPhotoFile();

            //if (photoBytes != null && photoBytes.Length > 0)
            //{
            //    // Save the photo to a file
            //    string filePath = SavePhotoToFile(photoBytes, "photo.jpg");

            //    if (!string.IsNullOrEmpty(filePath))
            //    {
            //        Console.WriteLine($"Photo saved to: {filePath}");

            //        // Open the saved photo with the default image viewer
            //        OpenImage(filePath);
            //    }
            //    else
            //    {
            //        Console.WriteLine("Failed to save the photo.");
            //    }
            //}
            //else
            //{
            //    Console.WriteLine("No photo data found.");
            //}
        }

        /// <summary>
        /// Saves the byte array as an image file.
        /// </summary>
        /// <param name="photoBytes">The byte array containing the image data.</param>
        /// <param name="fileName">The name of the file to save (e.g., "photo.jpg").</param>
        /// <returns>The full path to the saved file, or null if saving failed.</returns>
        static string SavePhotoToFile(byte[] photoBytes, string fileName)
        {
            try
            {
                // Define the path to save the file
                string filePath = Path.Combine(AppContext.BaseDirectory, fileName);

                // Write the byte array to the file
                File.WriteAllBytes(filePath, photoBytes);

                return filePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving photo: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Opens the image file using the default image viewer.
        /// </summary>
        /// <param name="filePath">The full path to the image file.</param>
        static void OpenImage(string filePath)
        {
            try
            {
                // Use Process.Start to open the file with the default application
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true // This ensures the file is opened with the default application
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening image: {ex.Message}");
            }
        }
    }
}