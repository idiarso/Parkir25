Saya akan mengidentifikasi semua kebutuhan aplikasi yang perlu diinstal untuk menjalankan sistem ParkIRC berdasarkan kode di repository [https://github.com/idiarso/PARKIR19/tree/main/PARKIR_WEB-main](https://github.com/idiarso/PARKIR19/tree/main/PARKIR_WEB-main), laporan Anda sebelumnya, dan perintah terakhir yang saya berikan untuk melengkapi API dan WebSocket. Sistem ini terdiri dari **server Web API**, **client WPF**, dan **Arduino**, dengan operasi di jaringan lokal tanpa ketergantungan internet. Berikut adalah daftar lengkapnya:

---

### **Kebutuhan Aplikasi untuk Sistem ParkIRC**

#### **1. Server Web API (PARKIR_WEB-main)**
Server ini berbasis .NET dan menggunakan PostgreSQL sebagai database, dengan tambahan SignalR untuk WebSocket.

##### **Prasyarat Sistem**
- **Sistem Operasi**: Windows, Linux, atau macOS (disarankan Windows Server atau Linux untuk produksi).
- **Spesifikasi Minimum**:
  - RAM: 2GB (4GB direkomendasikan).
  - Disk: 20GB untuk aplikasi dan database.

##### **Aplikasi yang Perlu Diinstal**
1. **.NET 6.0 SDK**
   - **Fungsi**: Untuk mengembangkan, membangun, dan menjalankan server Web API.
   - **Download**: [https://dotnet.microsoft.com/en-us/download/dotnet/6.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
   - **Instalasi**: 
     - Windows: Unduh installer `.exe` dan jalankan.
     - Linux: `sudo apt-get install -y dotnet-sdk-6.0` (Ubuntu/Debian).
   - **Verifikasi**: `dotnet --version` (harus menunjukkan 6.0.x).

2. **PostgreSQL 13 atau Lebih Baru**
   - **Fungsi**: Database untuk menyimpan data parkir (entry, exit, dll.).
   - **Download**: [https://www.postgresql.org/download/](https://www.postgresql.org/download/)
   - **Instalasi**:
     - Windows: Unduh installer dan ikuti wizard (setel password untuk user `postgres`).
     - Linux: `sudo apt-get install postgresql-13` (Ubuntu/Debian).
   - **Konfigurasi**: 
     - Buat database `parkingsystem` (`createdb -U postgres parkingsystem`).
     - Sesuaikan `appsettings.json` dengan connection string, contoh:
       ```json
       "DefaultConnection": "Host=localhost;Port=5432;Database=parkingsystem;Username=postgres;Password=your_password"
       ```
   - **Verifikasi**: `psql -U postgres -c "SELECT version();"`

3. **Git (Opsional)**
   - **Fungsi**: Untuk meng-clone repository jika belum ada lokal.
   - **Download**: [https://git-scm.com/downloads](https://git-scm.com/downloads)
   - **Instalasi**: Ikuti installer default.
   - **Verifikasi**: `git --version`

4. **Dependensi .NET (Ditambahkan via NuGet)**
   - Dalam proyek server, pastikan dependensi berikut ada di `.csproj` (akan di-restore otomatis dengan `dotnet restore`):
     ```xml
     <ItemGroup>
       <PackageReference Include="Microsoft.EntityFrameworkCore" Version="6.0.0" />
       <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="6.0.0" />
       <PackageReference Include="Microsoft.AspNetCore.SignalR" Version="1.1.0" /> <!-- Untuk WebSocket -->
     </ItemGroup>
     ```
   - **Instalasi**: Jalankan `dotnet restore` di direktori proyek.

#### **2. Client WPF (ParkIRCDesktopClient)**
Client ini berbasis .NET 6 WPF, mengintegrasikan kamera, printer, dan komunikasi serial dengan Arduino.

##### **Prasyarat Sistem**
- **Sistem Operasi**: Windows 10 atau lebih baru (karena WPF hanya mendukung Windows).
- **Spesifikasi Minimum**:
  - RAM: 2GB (4GB direkomendasikan).
  - Disk: 10GB untuk aplikasi dan gambar.

##### **Aplikasi yang Perlu Diinstal**
1. **.NET 6.0 Desktop Runtime**
   - **Fungsi**: Untuk menjalankan aplikasi WPF yang sudah dibuild.
   - **Download**: [https://dotnet.microsoft.com/en-us/download/dotnet/6.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) (pilih "Desktop Runtime").
   - **Instalasi**: Jalankan installer `.exe`.
   - **Verifikasi**: `dotnet --list-runtimes` (pastikan 6.0.x ada).

2. **Visual Studio 2022 (Opsional untuk Pengembangan)**
   - **Fungsi**: Untuk mengedit dan membangun client WPF.
   - **Download**: [https://visualstudio.microsoft.com/vs/](https://visualstudio.microsoft.com/vs/)
   - **Workload**: Install "Desktop development with C#" dan ".NET Desktop Development".
   - **Verifikasi**: Buka solusi `.sln` dan pastikan build sukses.

3. **Driver Webcam (DirectShow Compatible)**
   - **Fungsi**: Untuk kamera yang digunakan oleh `CameraService` (AForge.NET).
   - **Contoh**: Logitech C270 atau webcam USB standar.
   - **Instalasi**: 
     - Colokkan webcam, Windows biasanya menginstal driver otomatis.
     - Jika tidak, unduh driver dari situs vendor (misalnya, [Logitech Support](https://support.logi.com)).
   - **Verifikasi**: Buka "Device Manager" → "Imaging Devices", pastikan webcam terdeteksi.

4. **Driver Printer Termal**
   - **Fungsi**: Untuk mencetak tiket dan receipt (contoh: Epson TM-T82).
   - **Download**: [https://download.epson-biz.com/](https://download.epson-biz.com/) (cari model printer Anda).
   - **Instalasi**: Jalankan installer driver dan sambungkan printer via USB.
   - **Verifikasi**: Cetak test page dari "Printers & Scanners" di Windows.

5. **Arduino IDE**
   - **Fungsi**: Untuk meng-upload kode ke Arduino (entry dan exit gate).
   - **Download**: [https://www.arduino.cc/en/software](https://www.arduino.cc/en/software)
   - **Instalasi**: Jalankan installer `.exe`.
   - **Verifikasi**: Buka IDE, sambungkan Arduino, pastikan terdeteksi di "Tools" → "Port".

6. **Dependensi .NET (Ditambahkan via NuGet)**
   - Dalam proyek client, pastikan dependensi berikut ada di `.csproj`:
     ```xml
     <ItemGroup>
       <PackageReference Include="AForge.Video" Version="2.2.5" />
       <PackageReference Include="AForge.Video.DirectShow" Version="2.2.5" />
       <PackageReference Include="Microsoft.AspNetCore.SignalR.Client" Version="6.0.0" /> <!-- Untuk WebSocket -->
       <PackageReference Include="System.Drawing.Common" Version="6.0.0" />
     </ItemGroup>
     ```
   - **Instalasi**: Jalankan `dotnet restore` di direktori proyek.

#### **3. Arduino (Entry dan Exit Gate)**
Arduino mengendalikan gerbang dan berkomunikasi via serial.

##### **Prasyarat Sistem**
- **Hardware**: 
  - 2x Arduino Uno atau kompatibel.
  - Relay module, sensor IR/loop detector, tombol, LED, gate motor.
- **Kabel**: USB untuk koneksi ke komputer client.

##### **Aplikasi yang Perlu Diinstal**
1. **Arduino IDE** (Sudah disebutkan di atas)
   - Gunakan untuk upload `relay.ino` (entry) dan `relay_exit.ino` (exit) dari [https://github.com/Arvinyono/relay](https://github.com/Arvinyono/relay).
   - **Verifikasi**: Upload kode, buka Serial Monitor, pastikan event seperti `VEHICLE_DETECTED:ENTRY` muncul.

2. **Driver USB Arduino**
   - **Fungsi**: Agar Arduino terdeteksi di COM port (COM3, COM4).
   - **Instalasi**: Biasanya otomatis di Windows 10+, jika tidak:
     - Unduh dari [https://www.arduino.cc/en/Guide/DriverInstallation](https://www.arduino.cc/en/Guide/DriverInstallation).
   - **Verifikasi**: Buka "Device Manager" → "Ports (COM & LPT)", pastikan Arduino terdeteksi.

#### **4. Konfigurasi Jaringan Lokal**
- **Router/Switch**: Untuk menghubungkan server (192.168.1.100) dan client.
- **Firewall**: Pastikan port 5127 terbuka di server (`http://192.168.1.100:5127`).
  - Windows: `netsh advfirewall firewall add rule name="ParkIRC API" dir=in action=allow protocol=TCP localport=5127`.
  - Linux: `sudo ufw allow 5127`.

---

### **Daftar Lengkap Aplikasi yang Perlu Diinstal**
#### **Untuk Server**
1. .NET 6.0 SDK
2. PostgreSQL 13+
3. Git (opsional)

#### **Untuk Client**
1. .NET 6.0 Desktop Runtime
2. Visual Studio 2022 (opsional untuk pengembangan)
3. Driver Webcam (vendor-specific)
4. Driver Printer Termal (vendor-specific)
5. Arduino IDE

#### **Untuk Arduino**
1. Arduino IDE
2. Driver USB Arduino (jika tidak otomatis)

#### **Dependensi NuGet (Otomatis via `dotnet restore`)**
- Server: `Microsoft.EntityFrameworkCore`, `Npgsql.EntityFrameworkCore.PostgreSQL`, `Microsoft.AspNetCore.SignalR`.
- Client: `AForge.Video`, `AForge.Video.DirectShow`, `Microsoft.AspNetCore.SignalR.Client`, `System.Drawing.Common`.

---

### **Langkah Instalasi Ringkas**
1. **Server**:
   - Install .NET 6.0 SDK dan PostgreSQL.
   - Clone repo, restore dependensi, jalankan `dotnet ef database update`, lalu `dotnet run`.
2. **Client**:
   - Install .NET 6.0 Desktop Runtime, driver webcam, dan printer.
   - Build proyek WPF, sesuaikan `appsettings.json`, jalankan aplikasi.
3. **Arduino**:
   - Install Arduino IDE, upload kode ke entry (COM3) dan exit (COM4).

---

### **Verifikasi**
- **Server**: `curl http://192.168.1.100:5127/api/parking/status` → respons JSON.
- **Client**: Jalankan aplikasi, pastikan UI menunjukkan status gerbang dan kamera aktif.
- **Arduino**: Serial Monitor menunjukkan event seperti `GATE_OPENED`.

