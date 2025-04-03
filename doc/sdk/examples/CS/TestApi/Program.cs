using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using EidSamples;
using System.Security.Cryptography;
using System.Text;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Asn1.Cmp;





public class Program
{
    private const int WindowSizeMinutes = 5;
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureServices(services =>
                {
                    services.AddCors(options =>
                    {
                        options.AddPolicy("AllowAllOrigins", builder =>
                        {
                            builder.AllowAnyOrigin()
                                   .AllowAnyMethod()
                                   .AllowAnyHeader();
                        });
                    });
                });
           
                webBuilder.Configure(app =>
                {
                    app.UseCors("AllowAllOrigins");

                    // Enable routing
                    app.UseRouting();



                    // Define endpoints
                    app.UseEndpoints(endpoints =>
                    {
                        // Route to get the photo
                        endpoints.MapGet("/photo", async context =>
                        {
                            byte[] photoBytes = GetPhotoFile();

                            if (photoBytes != null)
                            {
                                context.Response.ContentType = "image/jpeg";
                                await context.Response.Body.WriteAsync(photoBytes, 0, photoBytes.Length);
                            }
                            else
                            {
                                context.Response.StatusCode = StatusCodes.Status404NotFound;
                                await context.Response.WriteAsync("Photo not found.");
                            }
                        });

                        // Route to get the name
                        endpoints.MapGet("/name", async context =>
                        {
                            string name = GetName();
                            await context.Response.WriteAsJsonAsync(new { Name = name });
                        });

                        // Route to get the address
                        endpoints.MapGet("/labels", async context =>
                        {
                            string label = GetLabels();
                            await context.Response.WriteAsJsonAsync(new { Label = label });
                        });

                        // Route to get the date of birth
                        endpoints.MapGet("/dob", async context =>
                        {
                            string dob = GetDateOfBirth();
                            await context.Response.WriteAsJsonAsync(new { DateOfBirth = dob });
                        });


                        //signs concatenation of challenge timestampwindow and salt
                        endpoints.MapPost("/auth/authentication", async context =>
                        {
                            var request= await context.Request.ReadFromJsonAsync< AuthRequest>();
                            if (request == null || request.challenge == null || request.salt==null)
                            {
                                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                                await context.Response.WriteAsync("Invalid request.");
                                return;
                            }
                            string Timestampwindow=GetCurrentWindow().ToString();
                            byte[] data = ConcatBytes(request.challenge, Encoding.UTF8.GetBytes(Timestampwindow), request.salt);
                            var signeddata=GetSignedData(data);
                            await context.Response.WriteAsJsonAsync<byte[]>(signeddata);

                        });
             


                        // signs a byte[] array 
                        endpoints.MapPost("/auth/sign", async context =>
                        {
                            var request = await context.Request.ReadFromJsonAsync<signingdata>();
                            if (request == null || request.data == null)
                            {
                                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                                await context.Response.WriteAsync("Invalid request.");
                                return;
                            }
                            byte[] signeddata = GetSignedData(request.data);
 

                            await context.Response.WriteAsJsonAsync<byte[]>(signeddata);
                        });

                        endpoints.MapGet("/auth/certificate", async context =>
                        {
                            X509Certificate2 certificat = getAuthenticationCertificate();
                            if (certificat == null)
                            {
                                context.Response.StatusCode = StatusCodes.Status404NotFound;
                                await context.Response.WriteAsync("Certificate not found.");
                                return;
                            }

                            // Export the certificate as a Base64-encoded string (DER format)
                            byte[] certBytes = certificat.Export(X509ContentType.Cert);
                            string base64Cert = Convert.ToBase64String(certBytes);

                            // Send the Base64-encoded certificate in the response
                            await context.Response.WriteAsJsonAsync(new { certificate = base64Cert });

                        });
                        endpoints.MapGet("/auth/publickey", async context =>
                        {

                           PublicKey pubkey = GetPublicKey();

                            if (pubkey == null) 
                            {

                                context.Response.StatusCode = StatusCodes.Status404NotFound;
                                await context.Response.WriteAsync("Certificate not found.");
                                return;

                            }
                            
                            byte[] keybytes=pubkey.ExportSubjectPublicKeyInfo();
                            string base64key = Convert.ToBase64String(keybytes);
                            await context.Response.WriteAsJsonAsync(new { pubkey = base64key});


                        });


                    });
                });
            });

   


    private static byte[] GetPhotoFile()
    {
        ReadData dataTest = new ReadData("beidpkcs11.dll");
        return dataTest.GetPhotoFile();
    }

    private static string GetName()
    {
        // Replace this with your logic to get the name
        ReadData dataTest = new ReadData("beidpkcs11.dll");
        return dataTest.GetSurname(); // Assuming you have a method like GetName()
    }

    private static string GetLabels()
    {
        // Replace this with your logic to get the address
        ReadData dataTest = new ReadData("beidpkcs11.dll");
        return dataTest.GetCertificateLabels()[0]; // Assuming you have a method like GetAddress()
    }

    private static string GetDateOfBirth()
    {
        // Replace this with your logic to get the date of birth
        ReadData dataTest = new ReadData("beidpkcs11.dll");
        return dataTest.GetDateOfBirth(); // Assuming you have a method like GetDateOfBirth()
    }


    private static byte[] GetSignedData(byte[] data)
    {
        Sign signTest = new Sign("beidpkcs11.dll");
        return signTest.DoSign(data, "Authentication");
    }

    private static PublicKey GetPublicKey()
    {

        var certificate = getAuthenticationCertificate();

        // use public key from certificate during verification
        return certificate.PublicKey;



    }




    private static X509Certificate2 getAuthenticationCertificate()
    {
        ReadData dataTest = new ReadData("beidpkcs11.dll");
        byte[] AuthenticationCertificate = dataTest.GetCertificateAuthenticationFile();
        return new X509Certificate2(AuthenticationCertificate);
        

    }

    public static DateTime GetCurrentWindow()
    {
        DateTime inputTime = DateTime.UtcNow;
        DateTime windowTime;

        // Calculate minutes to subtract to get to the previous 5-minute window
        int minutesToSubtract = inputTime.Minute % WindowSizeMinutes;

        int secondsToAdd = (WindowSizeMinutes % 2) * 60;
        //round to nearest half and subtract 5 seconds to account for the time difference of the client
        if (minutesToSubtract > WindowSizeMinutes / 2 && (secondsToAdd > 30|| secondsToAdd == 0))
        {
            //round to next time window
            windowTime = inputTime.AddMinutes(-minutesToSubtract + WindowSizeMinutes)
                                 .AddSeconds(-inputTime.Second + secondsToAdd)
                                 .AddMilliseconds(-inputTime.Millisecond);

        }
        else
        {
            // round to prevous time window
            windowTime = inputTime.AddMinutes(-minutesToSubtract)
                                        .AddSeconds(-inputTime.Second)
                                        .AddMilliseconds(-inputTime.Millisecond);
        }
        return windowTime;
    }


    // Concatenates three byte arrays into one byte array
    private static byte[] ConcatBytes(byte[] first, byte[] second, byte[] third)
    {
        byte[] result = new byte[first.Length + second.Length + third.Length];
        Buffer.BlockCopy(first, 0, result, 0, first.Length);
        Buffer.BlockCopy(second, 0, result, first.Length, second.Length);
        Buffer.BlockCopy(third, 0, result, second.Length + first.Length, third.Length);
        return result;
    }



}

public class AuthRequest
{
    public byte[] challenge { get; set; }
    public byte[] salt { get; set; }
}

public class signingdata
{
    public byte[] data { get; set; }

}