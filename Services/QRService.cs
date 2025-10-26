using QRCoder;
using System.Text;
using System.Drawing.Imaging;
using Aspose.BarCode.BarCodeRecognition;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;

namespace Tagger.Services
{
    public class QRService
    {
        [StructLayout(LayoutKind.Sequential)]
        struct Result
        {
            public int code;
            public IntPtr msg;
        }
        
        [DllImport("Extensions/qr.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern Result decode_qr(byte[] bytes,UIntPtr len);

        [DllImport("Extensions/qr.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void free_result(Result result);
        
        
        public byte[] GenerateQr(string _token,int _PxPrMd)
        {
            using (var qrGenerator = new QRCodeGenerator())
            using (var qrCodeData = qrGenerator.CreateQrCode(_token, QRCodeGenerator.ECCLevel.Q))
                using (var qrCode= new QRCode(qrCodeData))
                    using(var qrCodeImage = qrCode.GetGraphic(_PxPrMd))
                    using (var memoryStream = new MemoryStream())
                    {
                        qrCodeImage.Save(memoryStream, ImageFormat.Png);
                        return memoryStream.ToArray();
                    }
        }

        public async Task<byte[]> GenerateQrAsync(string _token, int _PxPrMd)
        {
            return await Task.Run(() => GenerateQr(_token, _PxPrMd));
        }

        public void SaveQr(string _token,int _PxPrMd, string _path)
        {
            var qrBytes = GenerateQr(_token, _PxPrMd);
            File.WriteAllBytes(_path, qrBytes);
        }
        

        public string ReadQr(string _path)
        {
            string Result = string.Empty;
            string Qr = string.Empty;
            string Type = string.Empty;
            try
            {
                using (FileStream fs = File.OpenRead(_path))
                using (BarCodeReader reader = new BarCodeReader(fs, DecodeType.QR))
                {
                    if (!reader.ReadBarCodes().Any())
                    {
                        return "NULL";
                    }
                    foreach (BarCodeResult result in reader.ReadBarCodes())
                    {
                        Type = result.CodeTypeName;
                        Qr = result.CodeText;
                        if (Qr == "QR")
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Result = Qr;
            return Result;
        }

        public string ReadQrAlt(string _path)
        {
            byte[] picture = File.ReadAllBytes(_path);
            string encPicture = string.Empty;
            encPicture = Convert.ToBase64String(picture);

            Result res = decode_qr(picture, (UIntPtr)picture.Length);

            string msg = Marshal.PtrToStringAnsi(res.msg)!;
            int code = res.code;
            free_result(res);

            if (code == 0)
            {
                return msg;
            }
            else
            {
                return null;
            }
        }
    }
}