﻿//---------------------------------------------------------------------------------
// Copyright (c) August 2018, devMobile Software
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
//---------------------------------------------------------------------------------
namespace devMobile.IoT.Rfm9x.LoRaDeviceClient
{
	using System;
	using System.Diagnostics;
	using System.Text;
	using System.Threading.Tasks;

	using devMobile.IoT.Rfm9x;
	using Windows.ApplicationModel.Background;

	public sealed class StartupTask : IBackgroundTask
	{
		const double Frequency = 915000000.0;
		private byte MessageCount = Byte.MaxValue;

#if DRAGINO
		private const byte ChipSelectLine = 25;
		private const byte ResetLine = 17;
		private const byte InterruptLine = 4;
		private Rfm9XDevice rfm9XDevice = new Rfm9XDevice(ChipSelectPin.CS0, ChipSelectLine, ResetLine, InterruptLine);
#endif
#if M2M
		private const byte ChipSelectLine = 25;
		private const byte ResetLine = 17;
		private const byte InterruptLine = 4;
		private Rfm9XDevice rfm9XDevice = new Rfm9XDevice(ChipSelectPin.CS0, ChipSelectLine, ResetLine, InterruptLine);
#endif
#if ELECROW
		private const byte ResetLine = 22;
		private const byte InterruptLine = 25;
		private Rfm9XDevice rfm9XDevice = new Rfm9XDevice(ChipSelectPin.CS1, ResetLine, InterruptLine);
#endif
#if ELECTRONIC_TRICKS
		private const byte ResetLine = 22;
		private const byte InterruptLine = 25;
		private Rfm9XDevice rfm9XDevice = new Rfm9XDevice(ChipSelectPin.CS0, ResetLine, InterruptLine);
#endif
#if UPUTRONICS_RPIZERO_CS0
		private const byte InterruptLine = 25;
		private Rfm9XDevice rfm9XDevice = new Rfm9XDevice(ChipSelectPin.CS0, InterruptLine);
#endif
#if UPUTRONICS_RPIZERO_CS1
		private const byte InterruptLine = 16;
		private Rfm9XDevice rfm9XDevice = new Rfm9XDevice(ChipSelectPin.CS1, InterruptLine);
#endif

#if UPUTRONICS_RPIPLUS_CS0 && !UPUTRONICS_RPIPLUS_CS1
		private const byte InterruptLine = 25;
		private Rfm9XDevice rfm9XDevice = new Rfm9XDevice(ChipSelectPin.CS0, InterruptLine);
#endif
#if !UPUTRONICS_RPIPLUS_CS0 && UPUTRONICS_RPIPLUS_CS1
		private const byte InterruptLine = 16;
		private Rfm9XDevice rfm9XDevice = new Rfm9XDevice(ChipSelectPin.CS1, InterruptLine);
#endif

#if UPUTRONICS_RPIPLUS_CS0 && UPUTRONICS_RPIPLUS_CS1 // 433MHz and 915MHz in my setup
		private const byte InterruptLineCS0 = 25;
		private Rfm9XDevice rfm9XDeviceCS0 = new Rfm9XDevice(ChipSelectPin.CS0, InterruptLineCS0);
		private const byte InterruptLineCS1 = 16;
		private Rfm9XDevice rfm9XDeviceCS1 = new Rfm9XDevice(ChipSelectPin.CS1, InterruptLineCS1);
#endif

#if ADAFRUIT_RADIO_BONNET
		private const byte ResetLine = 25;
		private const byte InterruptLine = 22;
		private Rfm9XDevice rfm9XDevice = new Rfm9XDevice(ChipSelectPin.CS1, ResetLine, InterruptLine);
#endif

#if UPUTRONICS_RPIPLUS_CS0 && UPUTRONICS_RPIPLUS_CS1
		public void Run(IBackgroundTaskInstance taskInstance)
		{
			rfm9XDeviceCS0.Initialise(915000000.0, paBoost: true, rxPayloadCrcOn: true);
			rfm9XDeviceCS1.Initialise(433000000.0, paBoost: true, rxPayloadCrcOn: true);
#if DEBUG
			rfm9XDeviceCS0.RegisterDump();
			rfm9XDeviceCS1.RegisterDump();
#endif

			rfm9XDeviceCS0.OnReceive += Rfm9XDevice_OnReceive;
			rfm9XDeviceCS1.OnReceive += Rfm9XDevice_OnReceive;
#if ADDRESSED_MESSAGES_PAYLOAD
			rfm9XDeviceCS0.Receive(UTF8Encoding.UTF8.GetBytes(Environment.MachineName));
			rfm9XDeviceCS1.Receive(UTF8Encoding.UTF8.GetBytes(Environment.MachineName));
#else
			rfm9XDeviceCS0.Receive();
			rfm9XDeviceCS1.Receive();
#endif
			rfm9XDeviceCS0.OnTransmit += Rfm9XDevice_OnTransmit;
			rfm9XDeviceCS1.OnTransmit += Rfm9XDevice_OnTransmit;

			Task.Delay(10000).Wait();

			while (true)
			{
				string messageText = string.Format("Hello from {0} ! {1}", Environment.MachineName, MessageCount);
				MessageCount -= 1;

				byte[] messageBytes = UTF8Encoding.UTF8.GetBytes(messageText);
				Debug.WriteLine("{0:HH:mm:ss}-TX {1} byte message {2}", DateTime.Now, messageBytes.Length, messageText);
#if ADDRESSED_MESSAGES_PAYLOAD
				this.rfm9XDeviceCS0.Send(UTF8Encoding.UTF8.GetBytes("Netduino"), messageBytes);
				this.rfm9XDeviceCS1.Send(UTF8Encoding.UTF8.GetBytes("Arduino1"), messageBytes);
#else
				this.rfm9XDeviceCS0.Send(messageBytes);
				this.rfm9XDeviceCS1.Send(messageBytes);
#endif
				Task.Delay(10000).Wait();
			}
		}

#else

		public void Run(IBackgroundTaskInstance taskInstance)
		{
			rfm9XDevice.Initialise(Frequency, paBoost: true, rxPayloadCrcOn : true);
#if DEBUG
			rfm9XDevice.RegisterDump();
#endif

		rfm9XDevice.OnReceive += Rfm9XDevice_OnReceive;
#if ADDRESSED_MESSAGES_PAYLOAD
			rfm9XDevice.Receive(UTF8Encoding.UTF8.GetBytes(Environment.MachineName));
#else
			rfm9XDevice.Receive();
#endif
			rfm9XDevice.OnTransmit += Rfm9XDevice_OnTransmit;

			Task.Delay(10000).Wait();

			while (true)
			{
				string messageText = string.Format("Hello from {0} ! {1}", Environment.MachineName, MessageCount);
				MessageCount -= 1;

				byte[] messageBytes = UTF8Encoding.UTF8.GetBytes(messageText);
				Debug.WriteLine("{0:HH:mm:ss}-TX {1} byte message {2}", DateTime.Now, messageBytes.Length, messageText);
#if ADDRESSED_MESSAGES_PAYLOAD
				this.rfm9XDevice.Send(UTF8Encoding.UTF8.GetBytes("AddressHere"), messageBytes);
#else
				this.rfm9XDevice.Send(messageBytes);
#endif
				Task.Delay(10000).Wait();
			}
		}
#endif

		private void Rfm9XDevice_OnReceive(object sender, Rfm9XDevice.OnDataReceivedEventArgs e)
		{
			try
			{
				string messageText = UTF8Encoding.UTF8.GetString(e.Data);

#if ADDRESSED_MESSAGES_PAYLOAD
				string addressText = UTF8Encoding.UTF8.GetString(e.Address);

				Debug.WriteLine(@"{0:HH:mm:ss}-RX From {1} PacketSnr {2:0.0} Packet RSSI {3}dBm RSSI {4}dBm = {5} byte message ""{6}""", DateTime.Now, addressText, e.PacketSnr, e.PacketRssi, e.Rssi, e.Data.Length, messageText);
#else
				Debug.WriteLine(@"{0:HH:mm:ss}-RX PacketSnr {1:0.0} Packet RSSI {2}dBm RSSI {3}dBm = {4} byte message ""{5}""", DateTime.Now, e.PacketSnr, e.PacketRssi, e.Rssi, e.Data.Length, messageText);
#endif
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}
		}

		private void Rfm9XDevice_OnTransmit(object sender, Rfm9XDevice.OnDataTransmitedEventArgs e)
		{
			Debug.WriteLine("{0:HH:mm:ss}-TX Done", DateTime.Now);
		}
	}
}
