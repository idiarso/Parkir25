# Frequently Asked Questions (FAQ)

**Versi:** 1.0.0  
**Terakhir Diperbarui:** 25 Maret 2025

## Umum

### Q: Apa itu ParkIRC?
A: ParkIRC adalah sistem manajemen parkir modern yang menggabungkan perangkat keras (kamera, printer, gerbang) dan perangkat lunak untuk otomatisasi dan manajemen area parkir.

### Q: Apakah ParkIRC bisa dijalankan di Linux?
A: Ya, ParkIRC memiliki dua komponen:
- Web server: Berjalan di Windows dan Linux
- Desktop client: Native di Windows, alternatif berbasis web untuk Linux

## Instalasi

### Q: Bagaimana cara menginstal ParkIRC?
A: Lihat [Panduan Instalasi](installation.md) untuk instruksi lengkap berdasarkan sistem operasi Anda.

### Q: Apa persyaratan minimal sistem?
A: Persyaratan minimal:
- CPU: 2 cores
- RAM: 4GB
- Storage: 20GB
- OS: Windows 10/11 atau Linux (Ubuntu/Pop!_OS)

## Hardware

### Q: Printer apa yang didukung?
A: ParkIRC mendukung printer termal ESCPOS seperti:
- Epson TM-T82
- Epson TM-T88
- Custom TG2480

### Q: Kamera apa yang bisa digunakan?
A: Mendukung:
- Webcam USB standar
- IP Camera (RTSP)
- Kamera CCTV Hikvision/Dahua

## Operasional

### Q: Bagaimana jika internet mati?
A: ParkIRC memiliki mode offline yang akan:
- Menyimpan data lokal
- Sinkronisasi otomatis saat online
- Tetap bisa mencetak tiket

### Q: Bagaimana cara backup data?
A: Ada beberapa opsi:
1. Backup otomatis terjadwal
2. Backup manual dari menu admin
3. Export data ke CSV/Excel

## Troubleshooting

### Q: Printer tidak mencetak, apa yang harus dilakukan?
A: Cek langkah berikut:
1. Status koneksi printer
2. Driver printer
3. Antrian cetak
4. Log error di menu diagnostik

### Q: Kamera tidak terdeteksi?
A: Verifikasi:
1. Koneksi USB/jaringan
2. Driver kamera
3. Setting di aplikasi
4. Izin akses kamera

## Keamanan

### Q: Apakah data parkir aman?
A: Ya, ParkIRC menerapkan:
- Enkripsi data
- Role-based access
- Audit log
- Backup berkala

### Q: Bagaimana mengubah password admin?
A: Melalui menu Profile di dashboard admin.

## Support

### Q: Dimana bisa mendapat bantuan?
A: Beberapa opsi:
- Email: support@parkirc.com
- Telepon: (021) xxx-xxxx
- Dokumentasi: [Manual ParkIRC](ParkIRC-Manual.md)
- Ticket system: https://support.parkirc.com

### Q: Apakah ada pelatihan untuk operator?
A: Ya, kami menyediakan:
- Manual operator
- Video tutorial
- Pelatihan online
- Pelatihan on-site

## Lisensi & Update

### Q: Berapa lama update tersedia?
A: ParkIRC menyediakan:
- Update keamanan: 3 tahun
- Update fitur: 2 tahun
- Support teknis: 3 tahun

### Q: Bagaimana cara update sistem?
A: Update bisa dilakukan melalui:
1. Menu System Update di dashboard admin
2. Script update otomatis
3. Manual update (untuk kasus khusus)

## Catatan Perubahan FAQ
| Versi | Tanggal | Perubahan |
|-------|---------|-----------|
| 1.0.0 | 25/03/2025 | Versi awal FAQ | 