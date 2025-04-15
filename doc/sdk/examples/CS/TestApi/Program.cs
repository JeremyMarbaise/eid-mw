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
using EidSamples.tests;
using TestApi.services;
using auth_server.Classes;





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
                    services.AddSingleton<EidCardService>();

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



                        endpoints.MapGet("/auth/register", async context =>
                        {
                            var eidService = context.RequestServices.GetRequiredService<EidCardService>();
                            string date = await eidService.GetDateOfBirth();
                            var data = new registerrequest()
                           

                            {
                                DateOfBirth = EidDateParser.ConvertEidDateToIso8601(date),
                                gender= await eidService.GetGender(),
                                name =await  eidService.GetName(),
                                nationalNumber=await eidService.GetNationalNumber(), 
                                address=await eidService.getAddress(),
                                    

                            };
                            await context.Response.WriteAsJsonAsync<registerrequest>(data);



                        }); 
                     
                        // Route to get the photo
                        endpoints.MapGet("/photo", async context =>
                        {
                            var eidService = context.RequestServices.GetRequiredService<EidCardService>();
                            byte[] photoBytes =await eidService.GetPhotoFile();

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

              


                        //signs concatenation of challenge timestampwindow and salt
                        endpoints.MapPost("/auth/authentication", async context =>
                        {
                            var eidService = context.RequestServices.GetRequiredService<EidCardService>();
                            var request= await context.Request.ReadFromJsonAsync< AuthRequest>();
                            if (request == null || request.challenge == null || request.salt==null)
                            {
                                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                                await context.Response.WriteAsync("Invalid request.");
                                return;
                            }
                            string Timestampwindow=GetCurrentWindow().ToString();
                            byte[] data = ConcatBytes(request.challenge, Encoding.UTF8.GetBytes(Timestampwindow), request.salt);
                            var signeddata=await eidService.GetSignedData(data);
                            await context.Response.WriteAsJsonAsync<byte[]>(signeddata);

                        });
             


                        // signs a byte[] array 
                        endpoints.MapPost("/auth/sign", async context =>
                        {
                            var eidService = context.RequestServices.GetRequiredService<EidCardService>();
                            var request = await context.Request.ReadFromJsonAsync<signingdata>();
                            if (request == null || request.data == null)
                            {
                                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                                await context.Response.WriteAsync("Invalid request.");
                                return;
                            }
                            byte[] signeddata = await eidService.GetSignedData(request.data);
 

                            await context.Response.WriteAsJsonAsync<byte[]>(signeddata);
                        });

                        endpoints.MapGet("/auth/certificate", async context =>
                        {
                            var eidService = context.RequestServices.GetRequiredService<EidCardService>();
                            X509Certificate2 certificat = await eidService.getAuthenticationCertificate();
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

                            var eidService = context.RequestServices.GetRequiredService<EidCardService>();
                            PublicKey pubkey = await eidService.GetPublicKey();

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

public class registerrequest
{
   public string name {  get; set; }
   public string nationalNumber { get; set; }
   public string DateOfBirth { get; set; }
   public string gender { get; set; }

    public string address { get; set; } 

}