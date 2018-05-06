#include "pch.h"
#include <iostream>
#include <winrt/Windows.Foundation.h>
#include <winrt/Windows.Foundation.Collections.h>
#include <winrt/Windows.Devices.Bluetooth.h>
#include <winrt/Windows.Devices.Bluetooth.Advertisement.h>
#include <winrt/Windows.Devices.Enumeration.h>
#include <sstream>
#include <string>
#include <iomanip>
#include <ppltasks.h>
#include <pplawait.h>
#include <experimental/resumable>

using namespace winrt;
using namespace winrt::Windows::Foundation;
using namespace winrt::Windows::Devices;
using namespace winrt::Windows::Devices::Enumeration;

unsigned long TimpointerServiceUUID = 0xdfb0;
unsigned long TimpointerCharacteristicUUID = 0xdfb1;

std::wstring formatBluetoothAddress(unsigned long long BluetoothAddress) {
	std::wostringstream ret;
	ret << std::hex << std::setfill(L'0')
		<< std::setw(2) << ((BluetoothAddress >> (5 * 8)) & 0xff) << ":"
		<< std::setw(2) << ((BluetoothAddress >> (4 * 8)) & 0xff) << ":"
		<< std::setw(2) << ((BluetoothAddress >> (3 * 8)) & 0xff) << ":"
		<< std::setw(2) << ((BluetoothAddress >> (2 * 8)) & 0xff) << ":"
		<< std::setw(2) << ((BluetoothAddress >> (1 * 8)) & 0xff) << ":"
		<< std::setw(2) << ((BluetoothAddress >> (0 * 8)) & 0xff);
	return ret.str();
}

//
//void addHandler(DeviceWatcher watcher, DeviceInformation arg)
//{
//	
//	std::cout << "added : " << arg.Name().c_str() << ", " << arg.IsEnabled() << ", " << arg.Id().c_str() << std::endl;
//}
//
//void updateHandler(DeviceWatcher watcher, DeviceInformationUpdate arg)
//{
//	std::cout << "updated : " << arg.Id().c_str() << std::endl;
//}
//
//void removeHandler(DeviceWatcher watcher, DeviceInformationUpdate arg)
//{
//	std::cout << "removed : " << arg.Id().c_str() << std::endl;
//}
//
//
//void stopHandler(DeviceWatcher watcher, IInspectable inspectable)
//{
//	std::cout << "watcher stopped. restart again" << std::endl;
//	watcher.Start();
//}
//
//void completeHandler(DeviceWatcher watcher, IInspectable inspectable)
//{
//	std::cout << "EnumerationComplete restart again" << std::endl;
//	watcher.Start();
//}
//
//
//int main()
//{
//	init_apartment();
//
//	//std::string requestedProperties[] = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected" };
//
//	DeviceWatcher deviceWatcher =
//		DeviceInformation::CreateWatcher(Bluetooth::BluetoothLEDevice::GetDeviceSelectorFromPairingState(false));
//	//Bluetooth::BluetoothLEDevice::GetDeviceSelectorFromPairingState(false),
//	//	requestedProperties,
//	//	Enumeration::DeviceInformationKind::AssociationEndpoint);
//
//
//	deviceWatcher.Added(addHandler);
//	deviceWatcher.Stopped(stopHandler);
//	deviceWatcher.EnumerationCompleted(completeHandler);
//	deviceWatcher.Updated(updateHandler);
//	deviceWatcher.Removed(removeHandler);
//	deviceWatcher.Start();
//	
//
//	std::cout << "start" << std::endl;
//
//	int a;
//	std::cin >> a;
//	return 0;
//
//}
//

concurrency::task<void> serviceCheck(unsigned long long bluetoothAddress) {
	auto leDevice = co_await Bluetooth::BluetoothLEDevice::FromBluetoothAddressAsync(bluetoothAddress);
	auto servicesResult = co_await leDevice.GetGattServicesForUuidAsync(serviceUUID);
	//auto service = servicesResult->Services->GetAt(0);
	//auto characteristicsResult = co_await service->GetCharacteristicsForUuidAsync(characteristicUUID);
	//auto characteristic = characteristicsResult->Characteristics->GetAt(0);

	//auto leDevice = co_await Bluetooth::BluetoothLEDevice::FromBluetoothAddressAsync(bluetoothAddress);
	//auto services = leDevice.GetGattServicesAsync().GetResults();
	//services.Services();
	////services.
	////for (uint32_t i = 0; i < leDevice.GattServices().Size(); i++) {
	////	std::cout << leDevice.GattServices().GetAt(i).Uuid().Data1 << std::endl;
	////}
	//
	////auto services = co_await leDevice.GetGattServicesForUuidAsync(Bluetooth::BluetoothUuidHelper::FromShortId(TimpointerServiceUUID));
	///*auto servicesResult = co_await leDevice.GetGattServicesForUuidAsync(Bluetooth::BluetoothUuidHelper::FromShortId(TimpointerServiceUUID));
	//servicesResult.Services();*/

	////auto characteristicsResult = co_await service.GetCharacteristicsForUuidAsync(Bluetooth::BluetoothUuidHelper::FromShortId(TimpointerCharacteristicUUID));
	////auto characteristic = characteristicsResult.Characteristics().GetAt(0);
	//
	////auto writer = winrt::Windows::Storage::Streams::DataWriter();
	////writer.WriteByte(0x00);
	////characteristic.WriteValueAsync(writer.DetachBuffer());
	////for (;;) {
	////	Bluetooth::GenericAttributeProfile::GattReadResult result = co_await characteristic.ReadValueAsync(Bluetooth::BluetoothCacheMode::Uncached);
	////	auto dataReader = winrt::Windows::Storage::Streams::DataReader::FromBuffer(result.Value());
	////	auto output = dataReader.ReadString(result.Value().Length());
	////	std::cout << output.c_str() << std::endl;
	////}
}


void receivedHandler(Bluetooth::Advertisement::BluetoothLEAdvertisementWatcher watcher, Bluetooth::Advertisement::BluetoothLEAdvertisementReceivedEventArgs args) {
	auto serviceUUID = args.Advertisement().ServiceUuids();

	std::cout << "received" << std::endl;
	for (uint32_t i = 0; i < serviceUUID.Size(); i++) {
		std::cout << std::hex << serviceUUID.GetAt(i).Data1 << ", " << serviceUUID.GetAt(i).Data2 << ", " << serviceUUID.GetAt(i).Data3 << ", " << serviceUUID.GetAt(i).Data4 << ", ";
		std::wcout << formatBluetoothAddress(args.BluetoothAddress()).c_str() << std::endl;
		if (serviceUUID.GetAt(i).Data1 == TimpointerServiceUUID) {
			std::cout << "connect" << std::endl;
			serviceCheck(args.BluetoothAddress());
		}
	}
	//watcher.Stop();
	//std::cout << "received, addr : " << args.BluetoothAddress() << "service UUID : " <<  << std::endl;
}


int main() {
	init_apartment();
	Bluetooth::Advertisement::BluetoothLEAdvertisementWatcher bleWatcher = Bluetooth::Advertisement::BluetoothLEAdvertisementWatcher();
	bleWatcher.ScanningMode(Bluetooth::Advertisement::BluetoothLEScanningMode::Active);
	bleWatcher.Received(receivedHandler);
	bleWatcher.Start();

	int a;
	std::cin >> a;
	return 0;
}