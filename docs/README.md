# ParkIRC - Sistem Manajemen Parkir Modern

ParkIRC adalah sistem manajemen parkir modern yang menggabungkan otomatisasi di pintu masuk dan verifikasi operator di pintu keluar untuk pengalaman parkir yang efisien dan aman.

## Fitur Utama

### 1. Pintu Masuk (Full Otomatis)
- Operasi dipicu oleh push button yang ditekan pengguna
- Deteksi otomatis kendaraan dan plat nomor menggunakan kamera
- Cetak tiket dengan barcode secara otomatis
- Gate terbuka otomatis setelah tiket diambil
- Pencatatan otomatis data masuk ke database (waktu, foto, plat nomor)
- Integrasi dengan Arduino untuk kontrol gerbang via REST API dan WebSocket

### 2. Pintu Keluar (Operator)
- Scan barcode tiket oleh operator
- Perhitungan biaya otomatis berdasarkan durasi
- Verifikasi pembayaran oleh operator
- Cetak struk pembayaran
- Gate terbuka setelah pembayaran selesai
- Komunikasi real-time dengan gerbang via WebSocket

### 3. Monitoring Real-time
- Status pintu masuk dan keluar melalui WebSocket
- Status perangkat (kamera, printer, push button, gate)
- Riwayat transaksi terkini
- Riwayat kendaraan dengan filter:
  - Status (Masuk/Keluar)
  - Rentang waktu
  - Jenis kendaraan
  - Plat nomor
- Notifikasi kejadian penting melalui SignalR
- Monitoring jumlah kendaraan

### 4. Manajemen
- Kelola slot parkir
- Kelola operator dan shift
- Konfigurasi tarif parkir
- Pengaturan kamera dan printer
- Riwayat lengkap kendaraan:
  - Pencarian berdasarkan status masuk/keluar
  - Filter berdasarkan tanggal
  - Export data ke Excel/PDF
  - Detail transaksi per kendaraan
- Laporan dan statistik

### 5. API dan WebSocket Integration
- REST API lengkap untuk integrasi dengan sistem eksternal
- Endpoint gate control: pengendalian gerbang, status, dan event
- WebSocket endpoint untuk komunikasi real-time dengan perangkat
- Dukungan event dan notifikasi berbasis SignalR 

## Teknologi

- **Backend**: ASP.NET Core 6.0
- **Frontend**: Bootstrap 5, jQuery
- **Database**: PostgreSQL
- **Real-time**: SignalR WebSocket
- **Hardware Integration**:
  - Kamera IP untuk ANPR (Automatic Number Plate Recognition)
  - Thermal printer untuk tiket dan struk
  - Push button untuk trigger pintu masuk
  - Gate controller untuk palang pintu via Arduino
  - Barcode scanner di pintu keluar
  - Sistem event berbasis WebSocket untuk Arduino

## Persyaratan Sistem

- .NET 6.0 SDK
- PostgreSQL 14+
- Apache (untuk production)
- Modern web browser
- Perangkat keras yang kompatibel
- Arduino Uno atau compatible (untuk kontrol gerbang)

## Instalasi

1. Clone repository
```bash
git clone https://github.com/idiarso/PARKIR19.git
cd PARKIR_WEB-main
```

2. Restore dependencies
```bash
dotnet restore
```

3. Setup database (PostgreSQL)
```bash
# Login ke PostgreSQL
sudo -u postgres psql

# Buat database
CREATE DATABASE parkirc;

# Set password (jika belum)
ALTER USER postgres PASSWORD '1q2w3e4r5t';

# Keluar dari PostgreSQL
\q
```

4. Migrasi database
```bash
# Jika tidak memiliki EF Core tools, install terlebih dahulu
dotnet tool install --global dotnet-ef --version 6.0.27

# Migrasi database
cd MigrationHelper/DbMigration
dotnet ef database update
cd ../..
```

5. Setup konfigurasi
- Sesuaikan `appsettings.json` jika diperlukan
- Pastikan koneksi database sudah benar
- Konfigurasi printer dan kamera
- Setup email untuk notifikasi

6. Jalankan aplikasi
```bash
dotnet run
```

7. Akses aplikasi di browser
```
http://localhost:5126
```

## Integrasi Arduino Gate Controller

### Setup Arduino
1. Upload kode dari folder `arduino` ke Arduino Uno.
2. Sambungkan relay dan sensor sesuai pin konfigurasi.
3. Sambungkan Arduino ke komputer server melalui USB.

### Konfigurasi API dan WebSocket
1. Gunakan endpoint API di `/api/v1/parking/gates/{gateId}/command` untuk mengirim perintah.
2. Terhubung ke WebSocket di `/gatehub` untuk menerima event gerbang real-time.
3. Test menggunakan halaman `/gatetest.html` untuk verifikasi koneksi.

### Gate API Commands
```
POST /api/v1/parking/gates/{gateId}/command
Content-Type: application/json

{
  "command": "OPEN_GATE"
}
```

```
GET /api/v1/parking/gates/{gateId}/status
```

## Konfigurasi Hardware

### Pintu Masuk
1. Setup kamera IP
   - Konfigurasi di `CameraSettings`
   - Pastikan posisi optimal untuk ANPR
   
2. Koneksi Arduino dan relay untuk kontrol gerbang
   - Ikuti panduan di `arduino/README.md`
   - Koneksikan pin sesuai skematik

3. Setup printer tiket
   - Ikuti langkah di `setup-printer.sh`
   - Test cetak tiket

### Pintu Keluar
1. Koneksi barcode scanner
   - Konfigurasi sebagai keyboard emulation
   - Test pembacaan tiket

2. Setup printer struk
   - Konfigurasi di `PrinterSettings`
   - Test cetak struk

## Dokumentasi

- [Panduan Administrator](manualbook/administrator_server_guide.md)
- [Setup Mikrokontroler](manualbook/mikrocontroller_avr_setup.md)
- [Database Real-time](manualbook/realtime_database.md)
- [Arduino Gate API](arduino/Arduino_Gate_API_Documentation.md)
- [Persyaratan Sistem](manualbook/kebutuhan.md)
- [Alur Sistem](system_flow.md)
- [Dokumentasi Teknis Lengkap](readmeplus.md)
- [Manual Penggunaan](ParkIRC-Manual.md)

## Kontribusi

1. Fork repository
2. Buat branch fitur (`git checkout -b feature/AmazingFeature`)
3. Commit perubahan (`git commit -m 'Add some AmazingFeature'`)
4. Push ke branch (`git push origin feature/AmazingFeature`)
5. Buat Pull Request

## Lisensi

Distributed under the MIT License. See `LICENSE` for more information.

## Kontak

Project Link: [https://github.com/idiarso/PARKIR19](https://github.com/idiarso/PARKIR19)
