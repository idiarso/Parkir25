// parking.js - Klien WebSocket untuk komunikasi dengan ParkingHub
"use strict";

// Inisialisasi koneksi SignalR
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/parkingHub")
    .configureLogging(signalR.LogLevel.Information)
    .withAutomaticReconnect()
    .build();

// Event handlers
connection.on("ReceiveVehicleEntry", (transaction) => {
    console.log("Kendaraan masuk:", transaction);
    updateTransactionList(transaction, "entry");
    playNotificationSound("entry");
    showNotification("Kendaraan Masuk", `Kendaraan dengan plat ${transaction.vehicle?.plateNumber || "tidak diketahui"} telah masuk`);
});

connection.on("ReceiveVehicleExit", (transaction) => {
    console.log("Kendaraan keluar:", transaction);
    updateTransactionList(transaction, "exit");
    playNotificationSound("exit");
    showNotification("Kendaraan Keluar", `Kendaraan dengan plat ${transaction.vehicle?.plateNumber || "tidak diketahui"} telah keluar`);
});

connection.on("ReceiveSpaceUpdate", (parkingSpace) => {
    console.log("Update ruang parkir:", parkingSpace);
    updateParkingSpaceUI(parkingSpace);
});

connection.on("ReceiveTransactionUpdate", (transaction) => {
    console.log("Update transaksi:", transaction);
    updateTransactionDetails(transaction);
});

connection.on("UserConnected", (connectionId) => {
    console.log("User terhubung:", connectionId);
    showConnectionStatus(true);
});

connection.on("UserDisconnected", (connectionId) => {
    console.log("User terputus:", connectionId);
});

// Fungsi untuk update UI
function updateTransactionList(transaction, type) {
    const transactionsList = document.getElementById("transactions-list");
    if (!transactionsList) return;

    const itemHtml = `
        <div class="transaction-item ${type}" data-id="${transaction.id}">
            <div class="transaction-header">
                <span class="plate-number">${transaction.vehicle?.plateNumber || "Tidak diketahui"}</span>
                <span class="time">${formatDateTime(type === "entry" ? transaction.entryTime : transaction.exitTime)}</span>
            </div>
            <div class="transaction-details">
                <span class="space">Ruang: ${transaction.parkingSpace?.number || "N/A"}</span>
                <span class="status">${type === "entry" ? "Masuk" : "Keluar"}</span>
                ${type === "exit" ? `<span class="fee">Biaya: Rp${transaction.fee.toLocaleString()}</span>` : ""}
            </div>
        </div>
    `;

    if (type === "entry") {
        transactionsList.insertAdjacentHTML("afterbegin", itemHtml);
    } else {
        // Cari item yang sudah ada dan update
        const existingItem = document.querySelector(`.transaction-item[data-id="${transaction.id}"]`);
        if (existingItem) {
            existingItem.outerHTML = itemHtml;
        } else {
            transactionsList.insertAdjacentHTML("afterbegin", itemHtml);
        }
    }
}

function updateParkingSpaceUI(parkingSpace) {
    const spaceElement = document.querySelector(`.parking-space[data-id="${parkingSpace.id}"]`);
    if (!spaceElement) return;

    spaceElement.classList.toggle("occupied", parkingSpace.isOccupied);
    spaceElement.classList.toggle("available", !parkingSpace.isOccupied);
    
    const statusText = spaceElement.querySelector(".status-text");
    if (statusText) {
        statusText.textContent = parkingSpace.isOccupied ? "Terisi" : "Kosong";
    }
}

function updateTransactionDetails(transaction) {
    const detailsElement = document.getElementById("transaction-details");
    if (!detailsElement) return;

    if (detailsElement.dataset.id === transaction.id.toString()) {
        document.getElementById("vehicle-plate").textContent = transaction.vehicle?.plateNumber || "N/A";
        document.getElementById("entry-time").textContent = formatDateTime(transaction.entryTime);
        document.getElementById("exit-time").textContent = transaction.exitTime ? formatDateTime(transaction.exitTime) : "Belum keluar";
        document.getElementById("parking-fee").textContent = `Rp${transaction.fee.toLocaleString()}`;
        document.getElementById("payment-status").textContent = transaction.isPaid ? "Dibayar" : "Belum dibayar";
    }
}

function showConnectionStatus(isConnected) {
    const statusElement = document.getElementById("connection-status");
    if (!statusElement) return;
    
    statusElement.classList.toggle("connected", isConnected);
    statusElement.classList.toggle("disconnected", !isConnected);
    statusElement.textContent = isConnected ? "Terhubung" : "Terputus";
}

function playNotificationSound(type) {
    const audio = new Audio(type === "entry" ? "/sounds/entry.mp3" : "/sounds/exit.mp3");
    audio.play().catch(err => console.error("Gagal memainkan suara:", err));
}

function showNotification(title, message) {
    if (!("Notification" in window)) {
        console.log("Browser tidak mendukung notifikasi desktop");
        return;
    }

    if (Notification.permission === "granted") {
        const notification = new Notification(title, {
            body: message,
            icon: "/images/logo.png"
        });
        
        // Tutup notifikasi setelah 5 detik
        setTimeout(() => notification.close(), 5000);
    } else if (Notification.permission !== "denied") {
        Notification.requestPermission().then(permission => {
            if (permission === "granted") {
                showNotification(title, message);
            }
        });
    }
}

function formatDateTime(dateString) {
    const date = new Date(dateString);
    return date.toLocaleString("id-ID", {
        day: "2-digit",
        month: "2-digit",
        year: "numeric",
        hour: "2-digit",
        minute: "2-digit",
        second: "2-digit"
    });
}

// Mulai koneksi ke server
async function startConnection() {
    try {
        await connection.start();
        console.log("SignalR Connected.");
        showConnectionStatus(true);
    } catch (err) {
        console.log("SignalR Connection Error: ", err);
        showConnectionStatus(false);
        setTimeout(startConnection, 5000);
    }
}

// Menangani kejadian terputus (retry connect)
connection.onclose(async () => {
    showConnectionStatus(false);
    console.log("SignalR Disconnected. Attempting reconnect...");
    await startConnection();
});

// Inisialisasi koneksi ketika dokumen siap
document.addEventListener("DOMContentLoaded", startConnection);

// API service untuk berinteraksi dengan endpoint transaksi
const parkingService = {
    vehicleEntry: async (plateNumber, vehicleType, operatorId, photoPath) => {
        try {
            const response = await fetch("/api/ParkingTransaction/entry", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({
                    plateNumber,
                    vehicleType,
                    operatorId,
                    photoPath
                })
            });
            
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            
            return await response.json();
        } catch (error) {
            console.error("Error entering vehicle:", error);
            throw error;
        }
    },
    
    vehicleExit: async (transactionId, fee, isPaid, photoPath) => {
        try {
            const response = await fetch("/api/ParkingTransaction/exit", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({
                    transactionId,
                    fee,
                    isPaid,
                    photoPath
                })
            });
            
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            
            return await response.json();
        } catch (error) {
            console.error("Error exiting vehicle:", error);
            throw error;
        }
    }
}; 