# PM5 BLE Probe

Minimal Windows .NET 8 console app for testing Concept2 PM5 BLE outside Unity.

Purpose:
- Scan for PM5 by BLE advertisement name or Concept2 service UUID.
- Connect directly with Windows.Devices.Bluetooth APIs.
- Subscribe to PM5 rowing service characteristics.
- Log CCCD write status and raw notification packets as HEX.

Run:

```powershell
cd Tools\Pm5BleProbe
dotnet run -- --scan-seconds 30
```

Useful options:

```powershell
dotnet run -- --scan-seconds 60 --verbose-advertisements
dotnet run -- --address 237390097829415
```

Expected flow:
1. Put PM5 into app/connect/discoverable mode.
2. Run the probe.
3. Select the PM5 if multiple devices are discovered.
4. Start rowing/skiing.
5. Look for `RAW|...` lines.

If this app receives `RAW` packets, Unity integration is the likely problem.
If this app also cannot receive notifications, focus on Windows Bluetooth, PM5 access, pairing/bonding, or adapter behavior.
