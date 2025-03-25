Berikut adalah file `installation.md` yang dirancang untuk instalasi aplikasi client ParkIRC di komputer client, mendukung arsitektur **32-bit** dan **64-bit** di Windows, serta memberikan panduan untuk instalasi di **Linux** (meskipun WPF secara native hanya berjalan di Windows, saya akan menyertakan alternatif berbasis cross-platform jika diperlukan). Panduan ini mencakup langkah-langkah untuk menginstal prasyarat, mengkonfigurasi perangkat keras (termasuk IP Kamera), dan menjalankan aplikasi di lingkungan produksi lokal.

---

# **installation.md**

## **Panduan Instalasi Client ParkIRC**

Dokumen ini memberikan panduan untuk menginstal aplikasi client ParkIRC di komputer client, mendukung sistem operasi Windows (32-bit dan 64-bit) serta opsi alternatif untuk Linux. Aplikasi ini mengendalikan gerbang parkir, kamera (webcam USB atau IP Kamera), dan printer dalam jaringan lokal tanpa ketergantungan internet.

Tanggal: 24 Maret 2025  
Versi Sistem: 1.0

---

## **Prasyarat**

### **Windows (32-bit dan 64-bit)**
- **Sistem Operasi**: Windows 10 atau 11 (32-bit atau 64-bit).
- **Spesifikasi Minimum**:
  - CPU: 2 cores.
  - RAM: 2GB (4GB direkomendasikan).
  - Disk: 10GB (termasuk gambar).
- **Perangkat Keras**:
  - Webcam USB (contoh: Logitech C270) atau IP Kamera (contoh: Hikvision DS-2CD1023G0E-I).
  - Printer termal (contoh: Epson TM-T82).
  - 2x Arduino Uno (entry: COM3, exit: COM4).
- **Jaringan**: Koneksi lokal ke server (contoh: `192.168.1.100:5127`).

### **Linux (Alternatif Cross-Platform)**
- **Sistem Operasi**: Ubuntu 20.04+ atau distribusi berbasis Debian.
- **Catatan**: WPF hanya berjalan di Windows. Untuk Linux, gunakan aplikasi alternatif berbasis .NET (contoh: console app atau Avalonia UI) dengan fungsi serupa.
- **Spesifikasi Minimum**: Sama seperti Windows.
- **Perangkat Keras**: Sama seperti Windows.

---

## **Langkah Instalasi**

### **1. Windows (32-bit dan 64-bit)**

#### **Instalasi Prasyarat**
1. **.NET 6.0 Desktop Runtime**
   - **Download**: [https://dotnet.microsoft.com/en-us/download/dotnet/6.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
     - Pilih "Desktop Runtime" sesuai arsitektur:
       - 32-bit: `windows-x86`.
       - 64-bit: `windows-x64`.
   - **Instalasi**: Jalankan installer `.exe`.
   - **Verifikasi**: 
     ```cmd
     dotnet --list-runtimes
     ```
     Pastikan `Microsoft.WindowsDesktop.App 6.0.x` terdaftar.

2. **Driver Webcam USB** (jika digunakan)
   - Colokkan webcam ke USB.
   - Windows biasanya menginstal driver otomatis. Jika tidak:
     - Unduh driver dari situs vendor (contoh: [Logitech Support](https://support.logi.com)).
   - **Verifikasi**: Buka "Device Manager" → "Imaging Devices", pastikan webcam terdeteksi.

3. **FFmpeg** (untuk IP Kamera)
   - **Download**: [https://ffmpeg.org/download.html](https://ffmpeg.org/download.html)
     - Pilih build Windows (32-bit atau 64-bit sesuai OS).
   - **Instalasi**: 
     - Ekstrak zip ke `C:\ffmpeg`.
     - Tambahkan ke PATH:
       - Buka "System Properties" → "Advanced" → "Environment Variables".
       - Edit "Path", tambah `C:\ffmpeg\bin`.
   - **Verifikasi**: 
     ```cmd
     ffmpeg -version
     ```

4. **Driver Printer Termal**
   - **Download**: Dari situs vendor (contoh: [Epson TM-T82](https://download.epson-biz.com/)).
   - **Instalasi**: Jalankan installer, sambungkan printer via USB.
   - **Verifikasi**: Cetak test page dari "Printers & Scanners".

5. **Arduino IDE**
   - **Download**: [https://www.arduino.cc/en/software](https://www.arduino.cc/en/software)
     - Pilih versi Windows (otomatis mendukung 32-bit dan 64-bit).
   - **Instalasi**: Jalankan installer `.exe`.
   - **Verifikasi**: Sambungkan Arduino, pastikan terdeteksi di "Tools" → "Port".

#### **Setup Kamera**
- **Webcam USB**:
  - Colokkan ke port USB client.
  - Pastikan terdeteksi di "Device Manager".
- **IP Kamera**:
  - Sambungkan ke jaringan lokal via Ethernet/Wi-Fi.
  - Konfigurasi via antarmuka web (contoh: `http://192.168.1.101`):
    - Set IP statis (misalnya, `192.168.1.101`).
    - Aktifkan RTSP (port default 554).
    - Catat URL RTSP (contoh: `rtsp://admin:password@192.168.1.101:554/Streaming/Channels/101`).
  - **Verifikasi**: Buka URL RTSP di VLC Media Player.

#### **Deploy Aplikasi**
1. **Unduh atau Build Kode**
   - Jika punya source:
     - Clone: `git clone https://github.com/idiarso/PARKIR19.git`.
     - Buka `ParkIRCDesktopClient.sln` di Visual Studio.
     - Build:
       ```cmd
       dotnet publish -c Release --self-contained -r win-x86  # Untuk 32-bit
       dotnet publish -c Release --self-contained -r win-x64  # Untuk 64-bit
       ```
     - Output di `bin/Release/net6.0-windows`.
   - Jika ada binary pre-built:
     - Unduh dari sumber Anda, ekstrak ke `C:\ParkIRC`.
2. **Konfigurasi**
   - Tambah `appsettings.json` di folder aplikasi:
     ```json
     {
       "ApiSettings": { "BaseUrl": "http://192.168.1.100:5127/" },
       "SerialSettings": { "EntryPort": "COM3", "ExitPort": "COM4", "BaudRate": 9600 },
       "PrinterSettings": { "DefaultPrinter": "Epson TM-T82" },
       "CameraSettings": {
         "UseIpCamera": false, // true untuk IP Kamera
         "IpCameraUrl": "rtsp://admin:password@192.168.1.101:554/Streaming/Channels/101",
         "DefaultIndex": 0,    // untuk webcam USB
         "SavePath": "C:\\ParkIRC\\Captures"
       }
     }
     ```
3. **Setup Arduino**
   - Upload `relay.ino` ke Arduino entry (COM3).
   - Upload `relay_exit.ino` ke Arduino exit (COM4).
   - Verifikasi via Serial Monitor (9600 baud): Event seperti `VEHICLE_DETECTED:ENTRY` muncul.
4. **Jalankan**
   - Klik `ParkIRCDesktopClient.exe` di `C:\ParkIRC`.
   - Opsional, buat service:
     ```powershell
     New-Service -Name "ParkIRCClient" -BinaryPathName "C:\ParkIRC\ParkIRCDesktopClient.exe" -StartupType Automatic
     Start-Service -Name "ParkIRCClient"
     ```

#### **Verifikasi**
- UI menunjukkan status gerbang.
- Kamera (webcam/IP) capture gambar ke `C:\ParkIRC\Captures`.
- Printer mencetak tiket saat kendaraan masuk.

---

### **2. Linux (Alternatif Cross-Platform)**

#### **Catatan Penting**
- WPF tidak mendukung Linux secara native. Sebagai alternatif:
  - Gunakan **Avalonia UI** (UI cross-platform untuk .NET) dengan logika serupa.
  - Atau buat aplikasi console sederhana untuk kontrol gerbang, kamera, dan printer.
- Panduan ini menggunakan Avalonia sebagai contoh.

#### **Instalasi Prasyarat**
1. **.NET 6.0 SDK**
   - **Download**: [https://dotnet.microsoft.com/en-us/download/dotnet/6.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
   - **Instalasi**:
     ```bash
     sudo apt-get update
     sudo apt-get install -y dotnet-sdk-6.0
     ```
   - **Verifikasi**: 
     ```bash
     dotnet --version  # Harus 6.0.x
     ```

2. **FFmpeg** (untuk IP Kamera)
   - **Instalasi**:
     ```bash
     sudo apt-get install -y ffmpeg
     ```
   - **Verifikasi**: 
     ```bash
     ffmpeg -version
     ```

3. **Driver Printer Termal**
   - Gunakan driver CUPS untuk printer termal:
     ```bash
     sudo apt-get install -y cups
     sudo lpadmin -p Epson-TM-T82 -E -v usb://EPSON/TM-T82 -m epson-tm-t82.ppd
     ```
   - Unduh PPD dari vendor jika perlu.
   - **Verifikasi**: 
     ```bash
     lpstat -p  # Pastikan printer terdaftar
     ```

4. **Arduino IDE**
   - **Download**: [https://www.arduino.cc/en/software](https://www.arduino.cc/en/software) (versi Linux).
   - **Instalasi**:
     ```bash
     tar -xvf arduino-1.x.x-linux64.tar.xz
     sudo mv arduino-1.x.x /opt/arduino
     cd /opt/arduino
     sudo ./install.sh
     ```
   - **Verifikasi**: Jalankan `/opt/arduino/arduino`, pastikan Arduino terdeteksi.

#### **Setup Kamera**
- **IP Kamera** (webcam USB tidak praktis di Linux untuk WPF):
  - Sambungkan ke jaringan lokal (contoh: `192.168.1.101`).
  - Konfigurasi:
    - Set IP statis.
    - Aktifkan RTSP (port 554).
    - Catat URL RTSP (contoh: `rtsp://admin:password@192.168.1.101:554/Streaming/Channels/101`).
  - **Verifikasi**: 
    ```bash
    ffplay "rtsp://admin:password@192.168.1.101:554/Streaming/Channels/101"
    ```

#### **Deploy Aplikasi (Avalonia Alternatif)**
1. **Setup Proyek Avalonia**
   - Clone repo:
     ```bash
     git clone https://github.com/idiarso/PARKIR19.git
     cd PARKIR19
     ```
   - Buat proyek Avalonia baru:
     ```bash
     dotnet new avalonia.app -o ParkIRCClientLinux
     cd ParkIRCClientLinux
     ```
   - Tambah dependensi:
     ```xml
     <ItemGroup>
       <PackageReference Include="Avalonia" Version="0.10.21" />
       <PackageReference Include="Microsoft.AspNetCore.SignalR.Client" Version="6.0.0" />
     </ItemGroup>
     ```
   - Porting logika dari `CameraService.cs`, `SerialService.cs`, dll., ke Avalonia (lihat contoh di bawah).
2. **Contoh CameraService untuk Linux**
   ```csharp
   public class CameraService
   {
       private readonly string _savePath;
       private readonly string _ipCameraUrl;

       public CameraService()
       {
           var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
           _savePath = config.GetSection("CameraSettings:SavePath").Value ?? "/opt/parkirc/captures";
           _ipCameraUrl = config.GetSection("CameraSettings:IpCameraUrl").Value;
           Directory.CreateDirectory(_savePath);
       }

       public async Task<string> CaptureImageAsync(string licensePlate = null)
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
           return process.ExitCode == 0 ? filePath : null;
       }
   }
   ```
3. **Konfigurasi**
   - Tambah `appsettings.json`:
     ```json
     {
       "ApiSettings": { "BaseUrl": "http://192.168.1.100:5127/" },
       "SerialSettings": { "EntryPort": "/dev/ttyUSB0", "ExitPort": "/dev/ttyUSB1", "BaudRate": 9600 },
       "PrinterSettings": { "DefaultPrinter": "Epson-TM-T82" },
       "CameraSettings": {
         "UseIpCamera": true,
         "IpCameraUrl": "rtsp://admin:password@192.168.1.101:554/Streaming/Channels/101",
         "SavePath": "/opt/parkirc/captures"
       }
     }
     ```
4. **Build dan Jalankan**
   - Build:
     ```bash
     dotnet publish -c Release -r linux-x64
     ```
   - Copy ke `/opt/parkirc`:
     ```bash
     sudo cp -r bin/Release/net6.0/linux-x64/publish /opt/parkirc
     ```
   - Jalankan:
     ```bash
     /opt/parkirc/ParkIRCClientLinux
     ```
   - Opsional, buat service:
     ```bash
     sudo nano /etc/systemd/system/parkirc-client.service
     ```
     Isi:
     ```
     [Unit]
     Description=ParkIRC Client
     After=network.target

     [Service]
     ExecStart=/opt/parkirc/ParkIRCClientLinux
     Restart=always
     User=parkirc-user

     [Install]
     WantedBy=multi-user.target
     ```
     Aktifkan:
     ```bash
     sudo systemctl enable parkirc-client
     sudo systemctl start parkirc-client
     ```

#### **Setup Arduino**
- Upload kode seperti di Windows, gunakan port Linux (`/dev/ttyUSB0`, `/dev/ttyUSB1`).
- Verifikasi:
  ```bash
  screen /dev/ttyUSB0 9600  # Install screen jika perlu: sudo apt-get install screen
  ```

#### **Verifikasi**
- Aplikasi menampilkan UI (Avalonia) atau log console.
- Kamera capture ke `/opt/parkirc/captures`.
- Printer mencetak via CUPS.

---

## **Catatan Tambahan**
- **Windows**: 
  - 32-bit lebih terbatas dalam memori, gunakan 64-bit untuk performa optimal.
  - Pastikan PATH mencakup FFmpeg untuk IP Kamera.
- **Linux**: 
  - Avalonia membutuhkan porting kode UI dari WPF, fokus pada fungsi inti (serial, kamera, printer).
  - IP Kamera lebih disarankan karena webcam USB kurang praktis di Linux.
- **Troubleshooting**:
  - Cek log di `C:\ParkIRC\logs` (Windows) atau `/opt/parkirc/logs` (Linux).
  - Pastikan port serial dan jaringan tidak diblokir.

---

## **Catatan Instalasi Terbaru**

Berikut adalah ringkasan komponen yang telah diinstal untuk kebutuhan sistem ParkIRC:

### **Komponen Terinstal**
- **FFmpeg** - Berhasil diinstal untuk dukungan IP camera
- **CUPS dan driver printer** - Berhasil diinstal untuk dukungan printer termal (Epson TM-T82)
- **Arduino IDE** - Berhasil diinstal untuk pemrograman dan komunikasi dengan kontroler gate Arduino

### **Konfigurasi Tambahan**
- CUPS dikonfigurasi untuk akses remote
- User saat ini ditambahkan ke grup lpadmin untuk manajemen printer
- Diinstal printer-driver-escpr untuk dukungan printer Epson

### **Langkah Berikutnya**
Karena tidak ada printer fisik atau perangkat Arduino yang terhubung ke sistem Anda (seperti ditunjukkan oleh perintah lsusb), kita tidak dapat menyelesaikan langkah-langkah pengaturan akhir untuk komponen-komponen tersebut. Ketika Anda menghubungkan perangkat keras:

#### **Untuk printer Epson TM-T82:**
- Hubungkan via USB
- Gunakan antarmuka web CUPS (http://localhost:631) untuk menambahkan printer
- Atau jalankan skrip setup-printer.sh (catatan: Anda mungkin perlu memodifikasi URL unduhan)

#### **Untuk kontroler gate Arduino:**
- Hubungkan via USB
- Gunakan Arduino IDE untuk mengunggah sketsa relay.ino (untuk gate masuk) dan relay_exit.ino (untuk gate keluar)
- Konfigurasikan port COM yang benar dalam pengaturan aplikasi Anda

Sistem sekarang siap dengan semua perangkat lunak yang diperlukan sudah terinstal. Setelah perangkat keras terhubung, Anda akan dapat menyelesaikan konfigurasi.

---

## **Panduan Instalasi Client Gate (Get In dan Get Out)**

Dokumen ini memberikan panduan khusus untuk menginstal dan mengkonfigurasi aplikasi client pada gate masuk (Get In) dan gate keluar (Get Out).

### **A. Spesifikasi Perangkat Gate Client**

#### **Spesifikasi Minimum Hardware**
- **CPU**: Intel Core i3 atau AMD Ryzen 3 (generasi terbaru)
- **RAM**: 4GB DDR4
- **Storage**: SSD 128GB
- **Port**: Minimal 4x USB 3.0
- **Monitor**: Touchscreen 15" (direkomendasikan) atau monitor standar 19"
- **OS**: Windows 10 atau 11 (64-bit) / Linux Ubuntu 20.04 LTS atau yang lebih baru

#### **Perangkat Tambahan**
- **Gate In (Masuk)**:
  - Arduino Uno
  - Sensor infrared 2 set
  - Printer termal 80mm (Epson TM-T82 atau setara)
  - Kamera webcam USB atau IP Camera (1080p)
  - Tombol darurat
  - Speaker untuk audio feedback
  
- **Gate Out (Keluar)**:
  - Arduino Uno
  - Sensor infrared 2 set
  - Printer termal 80mm (Epson TM-T82 atau setara)
  - Kamera webcam USB atau IP Camera (1080p)
  - Scanner barcode 2D (QR Code dan Barcode 1D)
  - Tombol darurat
  - Speaker untuk audio feedback

### **B. Instalasi Client Gate Pada Windows**

#### **1. Persiapan Instalasi**
1. **Membuat folder instalasi**:
   ```cmd
   mkdir C:\ParkIRCClient
   mkdir C:\ParkIRCClient\logs
   mkdir C:\ParkIRCClient\captures
   ```

2. **Download Aplikasi Client**:
   - Download file instalasi dari server atau repository internal
   - Ekstrak file aplikasi ke folder `C:\ParkIRCClient`

3. **Instalasi Driver Perangkat**:
   - **Arduino**: Unduh dan instal driver [CH340](https://sparks.gogo.co.nz/ch340.html) jika menggunakan clone Arduino
   - **Printer**: Instal driver Epson TM-T82 dari [website resmi Epson](https://epson.com/Support/Point-of-Sale/Receipt-Printers/Epson-TM-T82-Series/s/SPT_C31CA85834)
   - **Scanner Barcode**: Instal driver sesuai dengan merek scanner yang digunakan
   - **Webcam**: Instal driver jika diperlukan (umumnya Windows akan mendeteksi otomatis)

#### **2. Konfigurasi Aplikasi Gate In**

1. **Buat file konfigurasi** `C:\ParkIRCClient\appsettings.gate-in.json`:
   ```json
   {
     "GateSettings": {
       "GateType": "Entry",
       "GateId": "GATE-IN-01",
       "DisplayName": "Gate Masuk Utama"
     },
     "ServerSettings": {
       "ApiUrl": "https://192.168.1.100:5127/api",
       "SignalRUrl": "https://192.168.1.100:5127/gatehub"
     },
     "ArduinoSettings": {
       "PortName": "COM3",
       "BaudRate": 9600,
       "ReconnectInterval": 5000
     },
     "PrinterSettings": {
       "Enabled": true,
       "PrinterName": "EPSON TM-T82",
       "PrintTestOnStartup": true,
       "AutoCut": true
     },
     "CameraSettings": {
       "Enabled": true,
       "UseWebcam": true,
       "WebcamIndex": 0,
       "IpCameraUrl": "",
       "CaptureOnEntry": true,
       "SavePath": "C:\\ParkIRCClient\\captures"
     },
     "SecuritySettings": {
       "UserToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
       "EnableEmergencyMode": true,
       "RequireAuthentication": false
     },
     "LogSettings": {
       "LogLevel": "Information",
       "LogPath": "C:\\ParkIRCClient\\logs\\gate-in.log",
       "MaxFileSizeMB": 10,
       "MaxFileCount": 30
     }
   }
   ```

2. **Buat shortcut aplikasi** di Desktop:
   - Klik kanan di Desktop > New > Shortcut
   - Lokasi: `C:\ParkIRCClient\ParkIRC.GateClient.exe --config=appsettings.gate-in.json`
   - Nama: `ParkIRC Gate Masuk`
   - Klik kanan shortcut > Properties > Advanced > Run as administrator

3. **Konfigurasi Arduino Gate In**:
   - Sambungkan Arduino ke PC
   - Buka Arduino IDE
   - Upload kode berikut:
   
   ```cpp
   #include <SoftwareSerial.h>
   
   // Pin definitions
   const int SENSOR_ENTRY = 2;    // First IR sensor (entry detection)
   const int SENSOR_PRESENCE = 3; // Second IR sensor (vehicle presence)
   const int RELAY_GATE = 4;      // Gate relay control pin
   const int GATE_LED = 13;       // LED indicator
   
   // Variables
   bool vehicleDetected = false;
   bool gateOpen = false;
   unsigned long gateOpenTime = 0;
   const unsigned long GATE_TIMEOUT = 10000; // 10 seconds timeout
   
   void setup() {
     Serial.begin(9600);
     pinMode(SENSOR_ENTRY, INPUT);
     pinMode(SENSOR_PRESENCE, INPUT);
     pinMode(RELAY_GATE, OUTPUT);
     pinMode(GATE_LED, OUTPUT);
     
     digitalWrite(RELAY_GATE, LOW);
     digitalWrite(GATE_LED, LOW);
     
     Serial.println("GATE_ENTRY:READY");
   }
   
   void loop() {
     // Check for entry sensor
     if (digitalRead(SENSOR_ENTRY) == LOW && !vehicleDetected) {
       vehicleDetected = true;
       Serial.println("VEHICLE_DETECTED:ENTRY");
       delay(100);
     }
     
     // Check for presence sensor
     if (digitalRead(SENSOR_PRESENCE) == LOW) {
       vehicleDetected = true;
     } else if (vehicleDetected && digitalRead(SENSOR_PRESENCE) == HIGH) {
       // Vehicle has passed completely
       vehicleDetected = false;
       Serial.println("VEHICLE_PASSED:ENTRY");
       delay(100);
     }
     
     // Check for commands from PC
     if (Serial.available() > 0) {
       String command = Serial.readStringUntil('\n');
       command.trim();
       
       if (command == "GATE:OPEN") {
         openGate();
       } else if (command == "GATE:CLOSE") {
         closeGate();
       } else if (command == "STATUS") {
         sendStatus();
       }
     }
     
     // Auto-close gate after timeout
     if (gateOpen && (millis() - gateOpenTime > GATE_TIMEOUT)) {
       closeGate();
     }
   }
   
   void openGate() {
     digitalWrite(RELAY_GATE, HIGH);
     digitalWrite(GATE_LED, HIGH);
     gateOpen = true;
     gateOpenTime = millis();
     Serial.println("GATE:OPENED");
   }
   
   void closeGate() {
     digitalWrite(RELAY_GATE, LOW);
     digitalWrite(GATE_LED, LOW);
     gateOpen = false;
     Serial.println("GATE:CLOSED");
   }
   
   void sendStatus() {
     Serial.print("STATUS:");
     Serial.print(gateOpen ? "OPEN" : "CLOSED");
     Serial.print(",VEHICLE:");
     Serial.println(vehicleDetected ? "PRESENT" : "NONE");
   }
   ```

4. **Test Koneksi Gate In**:
   - Jalankan shortcut aplikasi
   - Verifikasi Arduino terdeteksi (LED status hijau di UI)
   - Verifikasi printer terdeteksi (LED status hijau di UI)
   - Verifikasi kamera terdeteksi (preview kamera muncul)
   - Verifikasi koneksi ke server (LED status hijau di UI)

#### **3. Konfigurasi Aplikasi Gate Out**

1. **Buat file konfigurasi** `C:\ParkIRCClient\appsettings.gate-out.json`:
   ```json
   {
     "GateSettings": {
       "GateType": "Exit",
       "GateId": "GATE-OUT-01",
       "DisplayName": "Gate Keluar Utama"
     },
     "ServerSettings": {
       "ApiUrl": "https://192.168.1.100:5127/api",
       "SignalRUrl": "https://192.168.1.100:5127/gatehub"
     },
     "ArduinoSettings": {
       "PortName": "COM4",
       "BaudRate": 9600,
       "ReconnectInterval": 5000
     },
     "PrinterSettings": {
       "Enabled": true,
       "PrinterName": "EPSON TM-T82",
       "PrintTestOnStartup": true,
       "AutoCut": true
     },
     "CameraSettings": {
       "Enabled": true,
       "UseWebcam": true,
       "WebcamIndex": 0,
       "IpCameraUrl": "",
       "CaptureOnExit": true,
       "SavePath": "C:\\ParkIRCClient\\captures"
     },
     "ScannerSettings": {
       "Enabled": true,
       "UseSerialScanner": false,
       "ScannerPortName": "COM5",
       "ScannerBaudRate": 9600,
       "AllowManualEntry": true,
       "RequireValidationBeforeOpen": true
     },
     "SecuritySettings": {
       "UserToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
       "EnableEmergencyMode": true,
       "RequireAuthentication": false
     },
     "LogSettings": {
       "LogLevel": "Information",
       "LogPath": "C:\\ParkIRCClient\\logs\\gate-out.log",
       "MaxFileSizeMB": 10,
       "MaxFileCount": 30
     }
   }
   ```

2. **Buat shortcut aplikasi** di Desktop:
   - Klik kanan di Desktop > New > Shortcut
   - Lokasi: `C:\ParkIRCClient\ParkIRC.GateClient.exe --config=appsettings.gate-out.json`
   - Nama: `ParkIRC Gate Keluar`
   - Klik kanan shortcut > Properties > Advanced > Run as administrator

3. **Konfigurasi Arduino Gate Out**:
   - Sambungkan Arduino ke PC
   - Buka Arduino IDE
   - Upload kode berikut:
   
   ```cpp
   #include <SoftwareSerial.h>
   
   // Pin definitions
   const int SENSOR_ENTRY = 2;    // First IR sensor (exit detection)
   const int SENSOR_PRESENCE = 3; // Second IR sensor (vehicle presence)
   const int RELAY_GATE = 4;      // Gate relay control pin
   const int GATE_LED = 13;       // LED indicator
   
   // Variables
   bool vehicleDetected = false;
   bool gateOpen = false;
   unsigned long gateOpenTime = 0;
   const unsigned long GATE_TIMEOUT = 10000; // 10 seconds timeout
   
   void setup() {
     Serial.begin(9600);
     pinMode(SENSOR_ENTRY, INPUT);
     pinMode(SENSOR_PRESENCE, INPUT);
     pinMode(RELAY_GATE, OUTPUT);
     pinMode(GATE_LED, OUTPUT);
     
     digitalWrite(RELAY_GATE, LOW);
     digitalWrite(GATE_LED, LOW);
     
     Serial.println("GATE_EXIT:READY");
   }
   
   void loop() {
     // Check for entry sensor
     if (digitalRead(SENSOR_ENTRY) == LOW && !vehicleDetected) {
       vehicleDetected = true;
       Serial.println("VEHICLE_DETECTED:EXIT");
       delay(100);
     }
     
     // Check for presence sensor
     if (digitalRead(SENSOR_PRESENCE) == LOW) {
       vehicleDetected = true;
     } else if (vehicleDetected && digitalRead(SENSOR_PRESENCE) == HIGH) {
       // Vehicle has passed completely
       vehicleDetected = false;
       Serial.println("VEHICLE_PASSED:EXIT");
       delay(100);
     }
     
     // Check for commands from PC
     if (Serial.available() > 0) {
       String command = Serial.readStringUntil('\n');
       command.trim();
       
       if (command == "GATE:OPEN") {
         openGate();
       } else if (command == "GATE:CLOSE") {
         closeGate();
       } else if (command == "STATUS") {
         sendStatus();
       }
     }
     
     // Auto-close gate after timeout
     if (gateOpen && (millis() - gateOpenTime > GATE_TIMEOUT)) {
       closeGate();
     }
   }
   
   void openGate() {
     digitalWrite(RELAY_GATE, HIGH);
     digitalWrite(GATE_LED, HIGH);
     gateOpen = true;
     gateOpenTime = millis();
     Serial.println("GATE:OPENED");
   }
   
   void closeGate() {
     digitalWrite(RELAY_GATE, LOW);
     digitalWrite(GATE_LED, LOW);
     gateOpen = false;
     Serial.println("GATE:CLOSED");
   }
   
   void sendStatus() {
     Serial.print("STATUS:");
     Serial.print(gateOpen ? "OPEN" : "CLOSED");
     Serial.print(",VEHICLE:");
     Serial.println(vehicleDetected ? "PRESENT" : "NONE");
   }
   ```

4. **Konfigurasi Scanner Barcode**:
   - Hubungkan scanner barcode via USB
   - Konfigurasi scanner untuk menambahkan Enter (CR/LF) setelah scan
   - Pastikan scanner terdeteksi sebagai keyboard device

5. **Test Koneksi Gate Out**:
   - Jalankan shortcut aplikasi
   - Verifikasi Arduino terdeteksi (LED status hijau di UI)
   - Verifikasi printer terdeteksi (LED status hijau di UI)
   - Verifikasi kamera terdeteksi (preview kamera muncul)
   - Verifikasi scanner barcode (status hijau di UI)
   - Verifikasi koneksi ke server (LED status hijau di UI)

### **C. Instalasi Client Gate Pada Linux**

#### **1. Persiapan Instalasi**
1. **Membuat folder instalasi**:
   ```bash
   sudo mkdir -p /opt/parkirc/client
   sudo mkdir -p /opt/parkirc/client/logs
   sudo mkdir -p /opt/parkirc/client/captures
   sudo chmod -R 755 /opt/parkirc
   sudo chown -R $USER:$USER /opt/parkirc
   ```

2. **Instal dependensi yang diperlukan**:
   ```bash
   sudo apt update
   sudo apt install -y libusb-1.0-0-dev libudev-dev ffmpeg cups librealsense2-dev
   sudo apt install -y dotnet-sdk-6.0
   ```

3. **Download dan ekstrak aplikasi client**:
   ```bash
   # Unduh dari repository atau server Anda
   # Contoh:
   # wget http://server-internal/ParkIRCClient-linux-x64.tar.gz -O /tmp/parkirc-client.tar.gz
   # tar -xzvf /tmp/parkirc-client.tar.gz -C /opt/parkirc/client
   ```

4. **Setup printer termal**:
   ```bash
   sudo apt install -y printer-driver-escpr
   sudo usermod -a -G lp $USER
   sudo usermod -a -G lpadmin $USER
   
   # Untuk Epson TM-T82
   sudo lpadmin -p TM-T82 -E -v usb://EPSON/TM-T82 -m epson-tm-t82.ppd
   sudo lpoptions -d TM-T82
   ```

5. **Atur izin port serial**:
   ```bash
   sudo usermod -a -G dialout $USER
   # Perlu logout dan login kembali agar grup baru diterapkan
   ```

#### **2. Konfigurasi Aplikasi Gate In (Linux)**

1. **Buat file konfigurasi** `/opt/parkirc/client/appsettings.gate-in.json`:
   ```json
   {
     "GateSettings": {
       "GateType": "Entry",
       "GateId": "GATE-IN-01",
       "DisplayName": "Gate Masuk Utama"
     },
     "ServerSettings": {
       "ApiUrl": "https://192.168.1.100:5127/api",
       "SignalRUrl": "https://192.168.1.100:5127/gatehub"
     },
     "ArduinoSettings": {
       "PortName": "/dev/ttyUSB0",
       "BaudRate": 9600,
       "ReconnectInterval": 5000
     },
     "PrinterSettings": {
       "Enabled": true,
       "PrinterName": "TM-T82",
       "PrintTestOnStartup": true,
       "AutoCut": true
     },
     "CameraSettings": {
       "Enabled": true,
       "UseWebcam": true,
       "WebcamIndex": 0,
       "IpCameraUrl": "rtsp://admin:admin123@192.168.1.101:554/stream",
       "CaptureOnEntry": true,
       "SavePath": "/opt/parkirc/client/captures"
     },
     "SecuritySettings": {
       "UserToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
       "EnableEmergencyMode": true,
       "RequireAuthentication": false
     },
     "LogSettings": {
       "LogLevel": "Information",
       "LogPath": "/opt/parkirc/client/logs/gate-in.log",
       "MaxFileSizeMB": 10,
       "MaxFileCount": 30
     }
   }
   ```

2. **Buat file service** `/etc/systemd/system/parkirc-gate-in.service`:
   ```
   [Unit]
   Description=ParkIRC Gate In Client
   After=network.target

   [Service]
   ExecStart=/opt/parkirc/client/ParkIRC.GateClient --config=appsettings.gate-in.json
   WorkingDirectory=/opt/parkirc/client
   Restart=always
   RestartSec=10
   User=$USER
   Environment=DOTNET_CLI_HOME=/tmp

   [Install]
   WantedBy=multi-user.target
   ```

3. **Aktifkan dan jalankan service**:
   ```bash
   sudo systemctl daemon-reload
   sudo systemctl enable parkirc-gate-in
   sudo systemctl start parkirc-gate-in
   ```

4. **Konfigurasi Arduino Gate In**:
   Upload kode yang sama dengan kode Arduino pada versi Windows menggunakan Arduino IDE.

#### **3. Konfigurasi Aplikasi Gate Out (Linux)**

1. **Buat file konfigurasi** `/opt/parkirc/client/appsettings.gate-out.json`:
   ```json
   {
     "GateSettings": {
       "GateType": "Exit",
       "GateId": "GATE-OUT-01",
       "DisplayName": "Gate Keluar Utama"
     },
     "ServerSettings": {
       "ApiUrl": "https://192.168.1.100:5127/api",
       "SignalRUrl": "https://192.168.1.100:5127/gatehub"
     },
     "ArduinoSettings": {
       "PortName": "/dev/ttyUSB1",
       "BaudRate": 9600,
       "ReconnectInterval": 5000
     },
     "PrinterSettings": {
       "Enabled": true,
       "PrinterName": "TM-T82",
       "PrintTestOnStartup": true,
       "AutoCut": true
     },
     "CameraSettings": {
       "Enabled": true,
       "UseWebcam": true,
       "WebcamIndex": 0,
       "IpCameraUrl": "rtsp://admin:admin123@192.168.1.102:554/stream",
       "CaptureOnExit": true,
       "SavePath": "/opt/parkirc/client/captures"
     },
     "ScannerSettings": {
       "Enabled": true,
       "UseSerialScanner": false,
       "ScannerPortName": "/dev/ttyUSB2",
       "ScannerBaudRate": 9600,
       "AllowManualEntry": true,
       "RequireValidationBeforeOpen": true
     },
     "SecuritySettings": {
       "UserToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
       "EnableEmergencyMode": true,
       "RequireAuthentication": false
     },
     "LogSettings": {
       "LogLevel": "Information",
       "LogPath": "/opt/parkirc/client/logs/gate-out.log",
       "MaxFileSizeMB": 10,
       "MaxFileCount": 30
     }
   }
   ```

2. **Buat file service** `/etc/systemd/system/parkirc-gate-out.service`:
   ```
   [Unit]
   Description=ParkIRC Gate Out Client
   After=network.target

   [Service]
   ExecStart=/opt/parkirc/client/ParkIRC.GateClient --config=appsettings.gate-out.json
   WorkingDirectory=/opt/parkirc/client
   Restart=always
   RestartSec=10
   User=$USER
   Environment=DOTNET_CLI_HOME=/tmp

   [Install]
   WantedBy=multi-user.target
   ```

3. **Aktifkan dan jalankan service**:
   ```bash
   sudo systemctl daemon-reload
   sudo systemctl enable parkirc-gate-out
   sudo systemctl start parkirc-gate-out
   ```

4. **Konfigurasi Arduino Gate Out**:
   Upload kode yang sama dengan kode Arduino pada versi Windows menggunakan Arduino IDE.

5. **Konfigurasi Scanner Barcode di Linux**:
   - Scanner biasanya terdeteksi sebagai keyboard input
   - Jika menggunakan scanner serial, pastikan port serial dikonfigurasi dengan benar

### **D. Troubleshooting**

#### **Masalah Umum Windows**
1. **Arduino tidak terdeteksi**:
   - Verifikasi driver CH340/FTDI terinstal dengan benar
   - Cek Device Manager untuk konfirmasi port COM
   - Coba port USB yang berbeda
   - Restart layanan jika diperlukan

2. **Printer tidak merespon**:
   - Verifikasi printer terinstal di Windows
   - Cek status printer di Control Panel > Devices and Printers
   - Cetak test page dari Windows untuk verifikasi
   - Pastikan printer online dan kertas tersedia

3. **Kamera tidak terlihat**:
   - Periksa Device Manager untuk memastikan kamera terdeteksi
   - Coba port USB berbeda
   - Pastikan driver kamera terinstal dengan benar
   - Pastikan tidak ada aplikasi lain yang menggunakan kamera

4. **Scanner barcode tidak berfungsi**:
   - Cek apakah scanner dikonfigurasi untuk mode keyboard emulation
   - Tes scanner di aplikasi Notepad
   - Pastikan scanner menambahkan Enter (CR/LF) di akhir kode

#### **Masalah Umum Linux**
1. **Arduino tidak terdeteksi**:
   - Cek apakah user adalah anggota grup dialout:
     ```bash
     groups $USER
     ```
   - Verifikasi port Arduino:
     ```bash
     ls -l /dev/ttyUSB*
     ```
   - Coba port USB berbeda

2. **Printer termal tidak berfungsi**:
   - Cek status CUPS:
     ```bash
     systemctl status cups
     lpstat -p
     ```
   - Pastikan printer terdeteksi:
     ```bash
     lsusb | grep -i epson
     ```
   - Tes printer dari CUPS:
     ```bash
     lp -d TM-T82 /etc/passwd
     ```

3. **Kamera tidak terdeteksi**:
   - Cek status kamera:
     ```bash
     ls -l /dev/video*
     v4l2-ctl --list-devices
     ```
   - Periksa status FFmpeg:
     ```bash
     ffmpeg -version
     ```
   - Tes kamera dengan FFplay:
     ```bash
     ffplay /dev/video0
     ```

4. **Service tidak berjalan**:
   - Periksa log:
     ```bash
     journalctl -u parkirc-gate-in.service
     tail -f /opt/parkirc/client/logs/gate-in.log
     ```
   - Verifikasi izin file:
     ```bash
     ls -l /opt/parkirc/client/ParkIRC.GateClient
     chmod +x /opt/parkirc/client/ParkIRC.GateClient
     ```

### **E. Pemeliharaan Rutin**

1. **Backup Database**:
   - Backup otomatis terjadwal di server
   - Pastikan backup manual sebelum upgrade sistem

2. **Pengelolaan Log**:
   - Log dirotasi secara otomatis (30 file, 10MB per file)
   - Pemeriksaan log bulanan untuk masalah potensial:
     ```bash
     # Linux
     grep -i error /opt/parkirc/client/logs/gate-*.log
     
     # Windows (PowerShell)
     Select-String -Path "C:\ParkIRCClient\logs\gate-*.log" -Pattern "error" -CaseSensitive
     ```

3. **Pembaruan Perangkat Lunak**:
   - Backup pengaturan dan database sebelum pembaruan
   - Update client sesuai jadwal pemeliharaan

4. **Pengelolaan Ruang Disk**:
   - Periksa ruang disk secara berkala:
     ```bash
     # Linux
     df -h
     du -sh /opt/parkirc/client/captures
     
     # Windows
     dir "C:\ParkIRCClient\captures" /s
     ```
   - Hapus gambar lama jika perlu:
     ```bash
     # Linux
     find /opt/parkirc/client/captures -type f -mtime +90 -delete
     
     # Windows
     forfiles /p "C:\ParkIRCClient\captures" /s /m *.* /d -90 /c "cmd /c del @path"
     ```

### **F. Integrasi dengan Server Utama**

1. **Koneksi API**:
   - Gate client berkomunikasi dengan server melalui API REST
   - Endpoint utama: `https://[SERVER-IP]:5127/api/`
   - Pengesahan tiket: `POST /api/parking/validate`
   - Kontrol gerbang: `POST /api/gate/control`

2. **Koneksi WebSocket/SignalR**:
   - Komunikasi real-time melalui SignalR WebSocket
   - URL hub: `https://[SERVER-IP]:5127/gatehub`
   - Metode client:
     - `ReceiveGateCommand`: Menerima perintah dari server
     - `SendGateStatus`: Mengirim status gate ke server
     - `SendVehicleDetection`: Mengirim deteksi kendaraan ke server

3. **Validasi Token**:
   - Gate client menggunakan token JWT untuk otentikasi
   - Token disimpan dalam file appsettings.json
   - Token harus diperbarui setiap 60 hari atau ketika keamanan terganggu

4. **Pemecahan Masalah Koneksi**:
   - Pastikan port 5127 terbuka di firewall server
   - Verifikasi sertifikat SSL jika menggunakan HTTPS
   - Cek ping dan traceroute dari client ke server:
     ```bash
     ping 192.168.1.100
     traceroute 192.168.1.100  # Linux
     tracert 192.168.1.100     # Windows
     ```
   - Verifikasi koneksi API dengan curl:
     ```bash
     curl -k https://192.168.1.100:5127/api/health
     ```

---
