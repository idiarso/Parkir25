// Entry Gate JavaScript - Menangani operasi gate masuk
"use strict";

// Inisialisasi koneksi SignalR
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/parkingHub")
    .configureLogging(signalR.LogLevel.Information)
    .withAutomaticReconnect()
    .build();

// Variabel untuk camera feed
let stream = null;
let photoCapture = null;

// Handler event SignalR
connection.on("ReceiveSystemStatus", (status) => {
    console.log("Sistem status:", status);
    updateSystemStatus(status);
});

connection.on("TriggerCamera", (entryPoint) => {
    console.log("Trigger camera at:", entryPoint);
    if (document.getElementById("gate-id").textContent === entryPoint) {
        captureImage();
    }
});

connection.on("EntryButtonPressed", (entryPoint) => {
    console.log("Entry button pressed at:", entryPoint);
    if (document.getElementById("gate-id").textContent === entryPoint) {
        showAlert("info", "Tombol masuk ditekan!");
    }
});

connection.on("PrintTicket", (ticketData) => {
    console.log("Print ticket:", ticketData);
    // Handle printing ticket
    printTicket(ticketData);
});

connection.on("OpenEntryGate", (entryPoint) => {
    console.log("Open entry gate:", entryPoint);
    if (document.getElementById("gate-id").textContent === entryPoint) {
        showAlert("success", "Palang dibuka!");
        // Simulate gate opening
        simulateGateOpen();
    }
});

connection.on("GateStatusChanged", (gateId, isOpen) => {
    console.log(`Gate ${gateId} status changed: ${isOpen ? "Open" : "Closed"}`);
    updateGateStatus(gateId, isOpen);
});

// Fungsi untuk update status sistem
function updateSystemStatus(status) {
    document.getElementById("gate-status").textContent = status.isOnline ? "Online" : "Offline";
    document.getElementById("gate-status").className = status.isOnline ? "info-value status status-ready" : "info-value status status-error";
    
    // Update camera status
    const cameraStatus = document.querySelector(".device-item:nth-child(1)");
    cameraStatus.classList.toggle("active", status.isCameraActive);
    cameraStatus.classList.toggle("inactive", !status.isCameraActive);
    
    // Update printer status
    const printerStatus = document.querySelector(".device-item:nth-child(2)");
    printerStatus.classList.toggle("active", status.isPrinterActive);
    printerStatus.classList.toggle("inactive", !status.isPrinterActive);
    
    // Update network status
    const networkStatus = document.querySelector(".device-item:nth-child(3)");
    networkStatus.classList.toggle("active", status.isOnline);
    networkStatus.classList.toggle("inactive", !status.isOnline);
}

// Fungsi untuk update status palang
function updateGateStatus(gateId, isOpen) {
    if (document.getElementById("gate-id").textContent === gateId) {
        const status = document.getElementById("gate-status");
        status.textContent = isOpen ? "Palang Terbuka" : "Siap";
        
        if (isOpen) {
            status.className = "info-value status status-warning";
            setTimeout(() => {
                status.textContent = "Siap";
                status.className = "info-value status status-ready";
                connection.invoke("UpdateGateStatus", gateId, false).catch(err => {
                    console.error("Error updating gate status:", err);
                });
            }, 5000);
        } else {
            status.className = "info-value status status-ready";
        }
    }
}

// Fungsi untuk simulasi palang terbuka
function simulateGateOpen() {
    const gateId = document.getElementById("gate-id").textContent;
    updateGateStatus(gateId, true);
    
    // Simulasi untuk memberitahu server bahwa palang terbuka
    connection.invoke("UpdateGateStatus", gateId, true).catch(err => {
        console.error("Error updating gate status:", err);
    });
    
    // Setelah 5 detik, tutup palang
    setTimeout(() => {
        connection.invoke("UpdateGateStatus", gateId, false).catch(err => {
            console.error("Error updating gate status:", err);
        });
    }, 5000);
}

// Fungsi untuk menampilkan pesan
function showAlert(type, message) {
    const alertContainer = document.createElement("div");
    alertContainer.className = `alert alert-${type} alert-dismissible fade show`;
    alertContainer.innerHTML = `
        ${message}
        <button type="button" class="close" data-dismiss="alert" aria-label="Close">
            <span aria-hidden="true">&times;</span>
        </button>
    `;
    
    document.querySelector(".gate-content").insertAdjacentElement("afterbegin", alertContainer);
    
    // Auto remove after 5 seconds
    setTimeout(() => {
        alertContainer.remove();
    }, 5000);
}

// Fungsi untuk menangani capture gambar
async function startCamera() {
    try {
        stream = await navigator.mediaDevices.getUserMedia({ video: true });
        const videoElement = document.getElementById("camera-feed");
        videoElement.srcObject = stream;
        videoElement.style.display = "block";
        document.querySelector(".no-camera").style.display = "none";
        document.getElementById("capture-button").disabled = false;
        document.getElementById("toggle-camera-button").innerHTML = '<i class="fas fa-power-off"></i> Matikan Kamera';
        
        // Update status kamera
        const cameraDevice = document.querySelector(".device-item:nth-child(1)");
        cameraDevice.classList.remove("inactive");
        cameraDevice.classList.add("active");
    } catch (err) {
        console.error("Error starting camera:", err);
        showAlert("danger", "Tidak dapat mengakses kamera");
    }
}

function stopCamera() {
    if (stream) {
        stream.getTracks().forEach(track => track.stop());
        stream = null;
        const videoElement = document.getElementById("camera-feed");
        videoElement.srcObject = null;
        videoElement.style.display = "none";
        document.querySelector(".no-camera").style.display = "flex";
        document.getElementById("capture-button").disabled = true;
        document.getElementById("toggle-camera-button").innerHTML = '<i class="fas fa-power-off"></i> Hidupkan Kamera';
        
        // Update status kamera
        const cameraDevice = document.querySelector(".device-item:nth-child(1)");
        cameraDevice.classList.remove("active");
        cameraDevice.classList.add("inactive");
    }
}

function toggleCamera() {
    if (stream) {
        stopCamera();
    } else {
        startCamera();
    }
}

async function captureImage() {
    if (!stream) {
        showAlert("warning", "Kamera tidak aktif");
        return null;
    }
    
    // Buat canvas untuk capture
    const videoElement = document.getElementById("camera-feed");
    const canvas = document.createElement("canvas");
    canvas.width = videoElement.videoWidth;
    canvas.height = videoElement.videoHeight;
    const ctx = canvas.getContext("2d");
    ctx.drawImage(videoElement, 0, 0, canvas.width, canvas.height);
    
    // Tampilkan hasil capture
    const imgData = canvas.toDataURL("image/jpeg");
    photoCapture = imgData;
    
    // Tampilkan gambar capture (opsional)
    // document.getElementById("photo-preview").src = imgData;
    
    showAlert("success", "Gambar berhasil diambil");
    return imgData;
}

// Fungsi untuk upload gambar ke server
async function uploadImage(imageData) {
    if (!imageData) return null;
    
    try {
        // Convert base64 to blob
        const response = await fetch(imageData);
        const blob = await response.blob();
        
        // Upload to server
        const formData = new FormData();
        formData.append("file", blob, "vehicle.jpg");
        
        const uploadResponse = await fetch("/api/Upload/Image", {
            method: "POST",
            body: formData
        });
        
        if (!uploadResponse.ok) {
            throw new Error(`HTTP error! status: ${uploadResponse.status}`);
        }
        
        const result = await uploadResponse.json();
        return result.filePath;
    } catch (error) {
        console.error("Error uploading image:", error);
        showAlert("danger", "Gagal mengunggah gambar");
        return null;
    }
}

// Fungsi untuk mencetak tiket
function printTicket(ticketData) {
    // Logic untuk mencetak tiket
    console.log("Mencetak tiket:", ticketData);
    showAlert("info", `Mencetak tiket untuk ${ticketData.plateNumber}`);
}

// API service untuk berinteraksi dengan endpoint transaksi
const entryGateService = {
    vehicleEntry: async (plateNumber, vehicleType, photoPath = null, printTicket = true) => {
        try {
            const gateId = document.getElementById("gate-id").textContent;
            
            const response = await fetch("/api/ParkingTransaction/entry", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({
                    plateNumber,
                    vehicleType,
                    gateId,
                    photoPath,
                    printTicket
                })
            });
            
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            
            return await response.json();
        } catch (error) {
            console.error("Error processing vehicle entry:", error);
            throw error;
        }
    },
    
    // Metode untuk mendapatkan status sistem
    getSystemStatus: async () => {
        try {
            await connection.invoke("RequestSystemStatus");
        } catch (error) {
            console.error("Error requesting system status:", error);
            showAlert("danger", "Gagal mendapatkan status sistem");
        }
    }
};

// Event handlers
document.addEventListener("DOMContentLoaded", async () => {
    try {
        await connection.start();
        console.log("SignalR Connected.");
        entryGateService.getSystemStatus();
    } catch (err) {
        console.error("SignalR Connection Error: ", err);
        showAlert("danger", "Gagal terhubung ke server");
    }
    
    // Toggle camera button
    document.getElementById("toggle-camera-button").addEventListener("click", toggleCamera);
    
    // Capture button
    document.getElementById("capture-button").addEventListener("click", captureImage);
    
    // Form submit handler
    document.getElementById("vehicle-entry-form").addEventListener("submit", async (e) => {
        e.preventDefault();
        
        const plateNumber = document.getElementById("plate-number").value;
        const vehicleType = document.getElementById("vehicle-type").value;
        const shouldPrintTicket = document.getElementById("print-ticket").checked;
        
        // Upload photo if captured
        let photoPath = null;
        if (photoCapture) {
            photoPath = await uploadImage(photoCapture);
        } else {
            // Attempt to capture image if camera is active
            if (stream) {
                const imgData = await captureImage();
                if (imgData) {
                    photoPath = await uploadImage(imgData);
                }
            }
        }
        
        try {
            const result = await entryGateService.vehicleEntry(plateNumber, vehicleType, photoPath, shouldPrintTicket);
            
            if (result.success) {
                showAlert("success", "Kendaraan berhasil masuk");
                
                // Reset form
                document.getElementById("vehicle-entry-form").reset();
                photoCapture = null;
                
                // Menambahkan kendaraan ke daftar terakhir masuk
                addToRecentEntries({
                    id: result.data.id,
                    plateNumber: plateNumber,
                    vehicleType: vehicleType,
                    entryTime: new Date()
                });
                
                // Simulasi palang terbuka
                simulateGateOpen();
            } else {
                showAlert("danger", result.message || "Gagal memproses kendaraan masuk");
            }
        } catch (error) {
            showAlert("danger", `Error: ${error.message}`);
        }
    });
    
    // Print again buttons
    document.querySelectorAll(".print-again").forEach(button => {
        button.addEventListener("click", async (e) => {
            e.preventDefault();
            e.stopPropagation();
            
            const id = button.getAttribute("data-id");
            try {
                const response = await fetch(`/api/ParkingTransaction/GetById/${id}`);
                if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`);
                }
                
                const transaction = await response.json();
                
                // Print ticket again
                connection.invoke("PrintEntryTicket", {
                    plateNumber: transaction.vehicle?.plateNumber,
                    vehicleType: transaction.vehicle?.type,
                    entryTime: transaction.entryTime,
                    transactionId: transaction.id
                }).catch(err => {
                    console.error("Error printing ticket:", err);
                });
                
                showAlert("info", "Mencetak tiket ulang");
            } catch (error) {
                console.error("Error getting transaction details:", error);
                showAlert("danger", "Gagal mencetak tiket ulang");
            }
        });
    });
});

// Helper function untuk menambahkan kendaraan ke daftar terakhir masuk
function addToRecentEntries(entry) {
    const entriesList = document.getElementById("entries-list");
    const noEntries = entriesList.querySelector(".no-entries");
    
    if (noEntries) {
        noEntries.remove();
    }
    
    const entryItem = document.createElement("div");
    entryItem.className = "entry-item";
    entryItem.innerHTML = `
        <div class="entry-item-header">
            <span class="plate-number">${entry.plateNumber}</span>
            <span class="time">${new Date(entry.entryTime).toLocaleTimeString("id-ID")}</span>
        </div>
        <div class="entry-item-body">
            <span class="vehicle-type">${entry.vehicleType}</span>
            <button class="btn btn-sm btn-outline-primary print-again" data-id="${entry.id}">
                <i class="fas fa-print"></i>
            </button>
        </div>
    `;
    
    // Add print again handler
    entryItem.querySelector(".print-again").addEventListener("click", async (e) => {
        e.preventDefault();
        e.stopPropagation();
        
        const id = e.currentTarget.getAttribute("data-id");
        try {
            const response = await fetch(`/api/ParkingTransaction/GetById/${id}`);
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            
            const transaction = await response.json();
            
            // Print ticket again
            connection.invoke("PrintEntryTicket", {
                plateNumber: transaction.vehicle?.plateNumber,
                vehicleType: transaction.vehicle?.type,
                entryTime: transaction.entryTime,
                transactionId: transaction.id
            }).catch(err => {
                console.error("Error printing ticket:", err);
            });
            
            showAlert("info", "Mencetak tiket ulang");
        } catch (error) {
            console.error("Error getting transaction details:", error);
            showAlert("danger", "Gagal mencetak tiket ulang");
        }
    });
    
    // Add to the beginning of the list
    entriesList.insertAdjacentElement("afterbegin", entryItem);
    
    // Limit to 10 items
    const items = entriesList.querySelectorAll(".entry-item");
    if (items.length > 10) {
        items[items.length - 1].remove();
    }
} 