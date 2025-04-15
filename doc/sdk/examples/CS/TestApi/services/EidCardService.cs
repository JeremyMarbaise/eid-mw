using EidSamples;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;


namespace TestApi.services
{
    public class EidCardService
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public async Task<byte[]> GetPhotoFile()
        {
            await _semaphore.WaitAsync();
            try
            {

                ReadData dataTest = new ReadData("beidpkcs11.dll");
                return dataTest.GetPhotoFile();
            }
            finally { _semaphore.Release(); }   
        }



        public  async Task<string> GetName()
        {

            await _semaphore.WaitAsync();

            try
            {

                ReadData dataTest = new ReadData("beidpkcs11.dll");
                return dataTest.GetFirstname() + " " + dataTest.GetSurname();
            }
            finally { _semaphore.Release(); }   
        }
        public  async Task<string> GetNationalNumber()
        {
            await _semaphore.WaitAsync();
            try
            {

                ReadData dataTest = new ReadData("beidpkcs11.dll");
                return dataTest.GetNationalNumber();
            }
            finally { _semaphore.Release(); }
        }

        public async Task<string> GetDateOfBirth()
        {


            await _semaphore.WaitAsync();
            try
            {
                ReadData dataTest = new ReadData("beidpkcs11.dll");
                return dataTest.GetDateOfBirth();
            }
            finally { _semaphore.Release(); }
        }

        public async Task<string> GetGender()
        {
            await _semaphore.WaitAsync();
            try
            {
                ReadData readData = new ReadData("beidpkcs11.dll");
                return readData.GetData("gender");
            }
            finally { _semaphore.Release(); }
        }
        public async Task<byte[]> GetSignedData(byte[] data)
        {
            await _semaphore.WaitAsync();
            try
            {
                Sign signTest = new Sign("beidpkcs11.dll");
                return signTest.DoSign(data, "Authentication");
            }
            finally { _semaphore.Release(); }
        }

        public  async  Task<PublicKey> GetPublicKey()
        {
      
                X509Certificate2 certificate = await getAuthenticationCertificate();
                // use public key from certificate during verification
                return certificate.PublicKey;
          
        }



        public async Task<X509Certificate2> getAuthenticationCertificate()
        {
            await _semaphore.WaitAsync();
            try
            {
                ReadData dataTest = new ReadData("beidpkcs11.dll");
                byte[] AuthenticationCertificate = dataTest.GetCertificateAuthenticationFile();
                return new X509Certificate2(AuthenticationCertificate);
            }
            finally { _semaphore.Release(); }  


        }


        public async Task<string> getAddress() 
        {
            await _semaphore.WaitAsync();
            try
            {
                ReadData data = new ReadData("beidpkcs11.dll");

                return data.GetData("address_street_and_number") + " "
                       + data.GetData("address_zip") + " "
                       + data.GetData("address_municipality");
            }
            finally {_semaphore.Release(); }
        
        }
    }
}
