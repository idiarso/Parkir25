Saya akan memperbarui **Catatan Produksi Sistem ParkIRC** dengan menambahkan pengaturan untuk **IP Kamera** sebagai alternatif atau tambahan untuk webcam USB, serta detail pengaturan kamera di lingkungan produksi. IP Kamera menawarkan fleksibilitas lebih (misalnya, jarak jauh dari client via jaringan) dibandingkan webcam USB yang terbatas pada koneksi fisik. Berikut adalah versi yang diperbarui dengan tambahan tersebut:

---

# **Catatan Produksi Sistem ParkIRC**

Tanggal: 24 Maret 2025  
Versi Sistem: 1.0 (berdasarkan laporan terbaru dan repository)

## **Ikhtisar Sistem**
ParkIRC adalah sistem manajemen parkir yang terdiri dari:
- **Server Web API**: Mengelola data parkir (entry, exit, status) dan komunikasi real-time via SignalR.
- **Client WPF**: Antarmuka operator untuk kontrol gerbang, kamera, dan printer.
- **Arduino**: Mengendalikan gerbang masuk (entry) dan keluar (exit) via sensor dan relay.

Sistem beroperasi di jaringan lokal (contoh: `192.168.1.x`) tanpa koneksi internet.

---

## **Prasyarat Produksi**

### **1. Server Web API**
- **Sistem Operasi**: Windows Server 2019/2022, atau Linux (Ubuntu 20.04+ direkomendasikan).
- **Spesifikasi Hardware**:
  - CPU: 2 cores.
  - RAM: 4GB (minimum 2GB).
  - Disk: 20GB (termasuk database dan log).
- **Jaringan**: IP statis (contoh: `192.168.1.100`), port 5127 terbuka.

#### **Perangkat Lunak**
1. **.NET 6.0 SDK**
   - Download: [https://dotnet.microsoft.com/en-us/download/dotnet/6.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
2. **PostgreSQL 13+**
   - Download: [https://www.postgresql.org/download/](https://www.postgresql.org/download/)
3. **Git** (opsional)
   - Download: [https://git-scm.com/downloads](https://git-scm.com/downloads)

### **2. Client WPF**
- **Sistem Operasi**: Windows 10/11.
- **Spesifikasi Hardware**:
  - CPU: 2 cores.
  - RAM: 4GB (minimum 2GB).
  - Disk: 10GB (termasuk gambar).
- **Perangkat Keras Tambahan**:
  - **Pilihan Kamera**:
    - Webcam USB (contoh: Logitech C270).
    - IP Kamera (contoh: Hikvision DS-2CD1023G0E-I, mendukung RTSP).
  - Printer termal (contoh: Epson TM-T82).
  - 2x Arduino Uno (entry: COM3, exit: COM4).

#### **Perangkat Lunak**
1. **.NET 6.0 Desktop Runtime**
   - Download: [https://dotnet.microsoft.com/en-us/download/dotnet/6.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
2. **Driver Webcam** (jika menggunakan webcam USB)
   - Vendor-specific (contoh: Logitech Support).
3. **Driver Printer Termal**
   - Vendor-specific (contoh: Epson TM-T82 driver).
4. **Arduino IDE**
   - Download: [https://www.arduino.cc/en/software](https://www.arduino.cc/en/software)
5. **FFmpeg** (untuk IP Kamera)
   - **Fungsi**: Mengakses stream RTSP dari IP Kamera.
   - Download: [https://ffmpeg.org/download.html](https://ffmpeg.org/download.html)
   - Instalasi: Tambahkan ke PATH sistem (contoh: `C:\ffmpeg\bin` di Windows).

### **3. Arduino**
- **Hardware**:
  - 2x Arduino Uno.
  - Relay module, sensor IR/loop detector, tombol, LED, gate motor.
  - Kabel USB untuk koneksi ke client.
- **Perangkat Lunak**:
  - Arduino IDE (termasuk driver USB Arduino).

### **4. Jaringan**
- Router/switch untuk koneksi lokal.
- Firewall: Buka port 5127 (API) dan port RTSP IP Kamera (default 554).

---

## **Langkah Instalasi Produksi**

### **1. Server Web API**
1. **Instal Prasyarat**
   - Windows: Jalankan installer `.NET 6.0 SDK` dan `PostgreSQL`.
   - Linux:
     ```bash
     sudo apt-get update
     sudo apt-get install -y dotnet-sdk-6.0 postgresql-13
     ```
2. **Setup Database**
   - Buat database:
     ```bash
     psql -U postgres -c "CREATE DATABASE parkingsystem;"
     ```
   - Setel password di `pg_hba.conf` jika perlu.
3. **Deploy Kode**
   - Clone repository:
     ```bash
     git clone https://github.com/idiarso/PARKIR19.git
     cd PARKIR19/PARKIR_WEB-main
     ```
   - Update `appsettings.json`:
     ```json
     {
       "ConnectionStrings": {
         "DefaultConnection": "Host=192.168.1.100;Port=5432;Database=parkingsystem;Username=postgres;Password=your_password"
       },
       "Kestrel": {
         "Endpoints": {
           "Http": { "Url": "http://192.168.1.100:5127" }
         }
       }
     }
     ```
   - Restore dan build:
     ```bash
     dotnet restore
     dotnet build
     dotnet ef database update
     ```
   - Jalankan sebagai service:
     - Windows: `dotnet publish -c Release`, lalu buat service dengan `sc create`.
     - Linux:
       ```bash
       dotnet publish -c Release
       sudo cp -r bin/Release/net6.0/publish /opt/parkirc-api
       sudo nano /etc/systemd/system/parkirc-api.service
       ```
       Isi file service:
       ```
       [Unit]
       Description=ParkIRC Web API
       After=network.target

       [Service]
       ExecStart=/usr/bin/dotnet /opt/parkirc-api/PARKIR_WEB-main.dll --urls "http://192.168.1.100:5127"
       Restart=always
       User=www-data

       [Install]
       WantedBy=multi-user.target
       ```
       Aktifkan:
       ```bash
       sudo systemctl enable parkirc-api
       sudo systemctl start parkirc-api
       ```
4. **Verifikasi**
   - `curl http://192.168.1.100:5127/api/parking/status` → respons JSON.

### **2. Client WPF**
1. **Instal Prasyarat**
   - Install `.NET 6.0 Desktop Runtime`, driver webcam (jika digunakan), driver printer, dan FFmpeg (untuk IP Kamera).
2. **Setup Kamera**
   - **Webcam USB**:
     - Colokkan ke client, pastikan terdeteksi di "Device Manager" → "Imaging Devices".
   - **IP Kamera**:
     - Sambungkan ke jaringan lokal (contoh IP: `192.168.1.101`).
     - Konfigurasi via antarmuka web kamera:
       - Set IP statis (misalnya, `192.168.1.101`).
       - Aktifkan RTSP (port default 554).
       - Catat URL RTSP (contoh: `rtsp://admin:password@192.168.1.101:554/Streaming/Channels/101`).
     - Verifikasi: Buka URL RTSP di VLC Media Player untuk memastikan stream aktif.
3. **Deploy Kode**
   - Build proyek:
     ```bash
     dotnet publish -c Release --self-contained -r win-x64
     ```
   - Copy folder `bin/Release/net6.0-windows` ke `C:\ParkIRC`.
   - Tambah `appsettings.json`:
     ```json
     {
       "ApiSettings": { "BaseUrl": "http://192.168.1.100:5127/" },
       "SerialSettings": { "EntryPort": "COM3", "ExitPort": "COM4", "BaudRate": 9600 },
       "PrinterSettings": { "DefaultPrinter": "Epson TM-T82" },
       "CameraSettings": {
         "UseIpCamera": true, // false untuk webcam USB
         "IpCameraUrl": "rtsp://admin:password@192.168.1.101:554/Streaming/Channels/101",
         "DefaultIndex": 0, // untuk webcam USB
         "SavePath": "C:\\ParkIRC\\Captures"
       }
     }
     ```
   - Update `CameraService.cs` untuk mendukung IP Kamera:
     ```csharp
     public class CameraService : IDisposable
     {
         private VideoCaptureDevice _videoDevice; // Webcam USB
         private Process _ffmpegProcess; // IP Kamera
         private readonly string _savePath;
         private readonly bool _useIpCamera;
         private readonly string _ipCameraUrl;

         public CameraService(string saveDirectoryPath = null)
         {
             _savePath = saveDirectoryPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "captures");
             Directory.CreateDirectory(_savePath);

             var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
             _useIpCamera = config.GetSection("CameraSettings:UseIpCamera").Get<bool>();
             _ipCameraUrl = config.GetSection("CameraSettings:IpCameraUrl").Value;
         }

         public bool Initialize(int cameraIndex = 0)
         {
             if (_useIpCamera)
             {
                 // Test RTSP connection
                 _ffmpegProcess = new Process
                 {
                     StartInfo = new ProcessStartInfo
                     {
                         FileName = "ffmpeg",
                         Arguments = $"-i \"{_ipCameraUrl}\" -f image2 -frames:v 1 test.jpg",
                         RedirectStandardOutput = true,
                         RedirectStandardError = true,
                         UseShellExecute = false,
                         CreateNoWindow = true
                     }
                 };
                 _ffmpegProcess.Start();
                 _ffmpegProcess.WaitForExit(5000);
                 return _ffmpegProcess.ExitCode == 0;
             }
             else
             {
                 // Webcam USB (kode existing)
                 _filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                 if (_filterInfoCollection.Count == 0) return false;
                 _videoDevice = new VideoCaptureDevice(_filterInfoCollection[cameraIndex].MonikerString);
                 _videoDevice.NewFrame += VideoDevice_NewFrame;
                 _videoDevice.Start();
                 return true;
             }
         }

         public async Task<string> CaptureImageAsync(string licensePlate = null)
         {
             if (_useIpCamera)
             {
                 string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                 string filename = string.IsNullOrEmpty(licensePlate) ? $"vehicle_{timestamp}.jpg" : $"vehicle_{licensePlate}_{timestamp}.jpg";
                 string filePath = Path.Combine(_savePath, filename);

                 var process = new Process
                 {
                     StartInfo = new ProcessStartInfo
                     {
                         FileName = "ffmpeg",
                         Arguments = $"-i \"{_ipCameraUrl}\" -f image2 -frames:v 1 \"{filePath}\"",
                         RedirectStandardOutput = true,
                         RedirectStandardError = true,
                         UseShellExecute = false,
                         CreateNoWindow = true
                     }
                 };
                 process.Start();
                 await Task.Run(() => process.WaitForExit(5000));
                 if (process.ExitCode == 0)
                 {
                     OnImageCaptured(filePath);
                     return filePath;
                 }
                 OnCameraError("Failed to capture from IP camera");
                 return null;
             }
             else
             {
                 // Webcam USB (kode existing)
                 // ... (logika existing untuk _currentFrame)
             }
         }

         // ... (method lain seperti Dispose, event handlers)
     }
     ```
   - Tambah dependensi FFmpeg di client `.csproj` (opsional, FFmpeg dijalankan sebagai proses eksternal):
     ```xml
     <ItemGroup>
       <PackageReference Include="AForge.Video" Version="2.2.5" Condition="'$(UseIpCamera)' != 'true'" />
       <PackageReference Include="AForge.Video.DirectShow" Version="2.2.5" Condition="'$(UseIpCamera)' != 'true'" />
     </ItemGroup>
     ```
4. **Setup Autostart**
   - Buat service Windows:
     ```powershell
     New-Service -Name "ParkIRCClient" -BinaryPathName "C:\ParkIRC\ParkIRCDesktopClient.exe" -StartupType Automatic
     Start-Service -Name "ParkIRCClient"
     ```
5. **Verifikasi**
   - Jalankan aplikasi, pastikan gambar dari webcam atau IP Kamera tersimpan di `C:\ParkIRC\Captures`.

### **3. Arduino**
1. **Instal Arduino IDE**
   - Unduh dan jalankan installer.
2. **Upload Kode**
   - Entry gate: Buka `relay.ino`, pilih COM3, upload.
   - Exit gate: Buka `relay_exit.ino`, pilih COM4, upload.
3. **Koneksi Hardware**
   - Ikuti `wiring_diagram.txt`:
     - Pin 2: Relay.
     - Pin 3: Sensor.
     - Pin 4: Button.
     - Pin 13: LED.
4. **Verifikasi**
   - Buka Serial Monitor (9600 baud), pastikan event seperti `VEHICLE_DETECTED:ENTRY` muncul.

---

## **Konfigurasi Produksi**

### **Server**
- **IP**: `192.168.1.100:5127`.
- **Database**: Backup terjadwal setiap 6 jam:
  ```bash
  # Tambah ke crontab (Linux)
  0 */6 * * * /path/to/backup.sh
  ```
- **Log**: Disimpan di `/opt/parkirc-api/logs` (Linux) atau `C:\ParkIRC\logs` (Windows).

### **Client**
- **Port Serial**: Entry (COM3), Exit (COM4).
- **Pengaturan Kamera di Produksi**:
  - **Webcam USB**:
    - Index: 0 (default), sesuaikan di `appsettings.json` jika ada banyak kamera.
    - Resolusi: 640x480 (default AForge), sesuaikan via kode jika perlu.
    - Koneksi: USB langsung ke client, maksimum panjang kabel 5m.
  - **IP Kamera**:
    - IP: Statis (contoh: `192.168.1.101`), port RTSP 554.
    - URL RTSP: `rtsp://username:password@192.168.1.101:554/Streaming/Channels/101`.
    - Resolusi: 1280x720 (disarankan), sesuaikan di kamera untuk performa optimal.
    - Latensi: <1 detik di jaringan lokal, pastikan bandwidth >2Mbps.
    - Koneksi: Ethernet atau Wi-Fi stabil ke router lokal.
  - **Penyimpanan**: `C:\ParkIRC\Captures`, cleanup otomatis >30 hari.
- **Printer**: Nama printer sesuai driver (contoh: "Epson TM-T82").
- **Log**: Disimpan di `C:\ParkIRC\logs`.

### **Jaringan**
- Pastikan firewall mengizinkan:
  - Port 5127 (API): `netsh advfirewall firewall add rule name="ParkIRC API" dir=in action=allow protocol=TCP localport=5127`.
  - Port 554 (RTSP IP Kamera): `netsh advfirewall firewall add rule name="RTSP Camera" dir=in action=allow protocol=TCP localport=554`.

---

## **Panduan Operasional**

### **Memulai Sistem**
1. Start server: `systemctl start parkirc-api` (Linux) atau pastikan service berjalan (Windows).
2. Start client: Jalankan `ParkIRCDesktopClient.exe` atau pastikan service aktif.
3. Verifikasi Arduino terhubung di COM3 dan COM4.
4. Verifikasi Kamera:
   - Webcam: Terlihat di "Device Manager".
   - IP Kamera: Stream RTSP aktif di VLC.

### **Operasi Harian**
- **Kendaraan Masuk**:
  1. Input plat nomor (opsional).
  2. Pilih tipe kendaraan.
  3. Sensor mendeteksi → kamera capture, tiket dicetak, gate terbuka.
- **Kendaraan Keluar**:
  1. Input nomor tiket.
  2. Pilih metode pembayaran.
  3. Sensor mendeteksi → kamera capture, receipt dicetak, gate terbuka.
- **Kontrol Manual**: Gunakan tombol "Open Gate" atau "Close Gate" di UI.

### **Troubleshooting**
- **Kamera Gagal**:
  - Webcam: Cek koneksi USB, restart client.
  - IP Kamera: Cek IP dan RTSP URL, pastikan jaringan stabil.
- **Printer Gagal**: Pastikan printer menyala, cek driver.
- **Gerbang Tidak Bergerak**: Cek relay dan sensor di Arduino.
- **Log**: Lihat `logs/parkiRC_YYYYMMDD.log` untuk detail.

---

## **Catatan Tambahan**
- **Kapasitas**: Hingga 1000 kendaraan/hari (240MB penyimpanan gambar dengan webcam, ~500MB dengan IP Kamera 720p).
- **Maintenance**: 
  - Periksa hardware Arduino dan IP Kamera bulanan.
  - Backup database mingguan, hapus log/gambar >30 hari.
- **Pengembangan Lanjutan**:
  - Integrasi ALPR dengan IP Kamera.
  - Dashboard real-time via SignalR.

---

### **Penjelasan Tambahan**
- **IP Kamera**: Ditambahkan dengan FFmpeg untuk capture RTSP stream, fleksibel untuk jarak jauh dari client.
- **Pengaturan Produksi**: Mendukung webcam dan IP Kamera, dengan konfigurasi di `appsettings.json` dan kode `CameraService.cs`.

