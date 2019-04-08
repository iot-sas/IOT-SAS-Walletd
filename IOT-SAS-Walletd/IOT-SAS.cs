using System;
using System.IO.Ports;
using System.Text;
using System.Threading;
using FactomSharp;
using FactomSharp.Factomd;

namespace IOTSASWalletd
{
    public class IOT_SAS : IDisposable
    {
        SerialPort mySerial;

        public static ECAddress ECAddressClass { get; private set;}
    
        //Set up serial port.  On a Raspberry Pi, use /dev/serial0
        public IOT_SAS(String device, uint baudRate = 57600)
        {
            mySerial = new SerialPort(device, (int)baudRate, Parity.None, 8, StopBits.One);
            
            mySerial.Handshake = Handshake.None;
        //    mySerial.Open();

            GetECAddress(null);
        }


        //Make an EC address class using the IOT-SAS public key, and override the signing function
        public FactomSharp.ECAddress GetECAddress(FactomdRestClient factomd)
        {
            var ecAddress = new FactomSharp.ECAddress(factomd, GetPublicECAddress());
            ecAddress.SignFunction = (data) =>
            {
                    return SignEd25519(data);
            };

            ECAddressClass = ecAddress;
            
            return ecAddress;
        }
        
        //Get the public key from the IOT-SAS board.
        private string GetPublicECAddress()
        {
            mySerial.Open();
            
            //Send command to IOT-SAS, asking it for the public key
            var GetEC = new byte[] { 0xFA, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00 };
            mySerial.Write(GetEC, 0, GetEC.Length);

            //Read the IOT-SAS reply, which should be a 52 byte key (TODO validation)
            var key = new byte[52];            
            var count = 0;
            var bytesRx = 0;
            while (count < key.Length)
            {
                bytesRx = mySerial.Read(key, count, key.Length-count);
                count += bytesRx;
            }
            mySerial.Close();
            return Encoding.ASCII.GetString(key);
        }

        
        //Sign data on the IOT-SAS board, unsing the hardware secret key.
        public byte[] SignEd25519(byte[] data)
        {
            mySerial.Open();
            
            //Create a data buffer, which includes header, and data to be signed.
            var toSign = new byte[data.Length + 5];
            toSign[0] = 0xFA;
            toSign[1] = 0x02;
            toSign[2] = 0x02;
            toSign[3] = 0x0;
            toSign[4] = (byte)data.Length;
            Array.Copy(data, 0, toSign, 5, data.Length);            
            
            //Write the buffer to the IOT-SAS device.
            mySerial.Write(toSign, 0, toSign.Length);

            //Read the IOT-SAS reply, which should be a 64 byte signature (TODO validation)
            var signature = new byte[64];
            var count = 0;
            var bytesRx = 0;
            while (count < signature.Length)
            {
                bytesRx = mySerial.Read(signature, count, signature.Length-count);
                count += bytesRx;
            }
            mySerial.Close();
            return signature;
        }
        
        public void Dispose()
        {
          //  mySerial.Close();
        }
    }    
}
