// Exit Gate JavaScript
document.addEventListener('DOMContentLoaded', function() {
    // Setup SignalR connection
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/parkinghub")
        .configureLogging(signalR.LogLevel.Information)
        .build();

    // DOM elements
    const cameraPlaceholder = document.getElementById('camera-placeholder');
    const cameraStream = document.getElementById('camera-stream');
    const cameraCanvas = document.getElementById('camera-canvas');
    const startCameraBtn = document.getElementById('start-camera');
    const stopCameraBtn = document.getElementById('stop-camera');
    const captureImageBtn = document.getElementById('capture-image');
    const findVehicleBtn = document.getElementById('find-vehicle');
    const plateNumberInput = document.getElementById('plate-number');
    const vehicleInfoDiv = document.getElementById('vehicle-info');
    const processExitBtn = document.getElementById('process-exit');
    const openGateBtn = document.getElementById('open-gate');
    const gateIdInput = document.getElementById('gate-id');
    const photoPathInput = document.getElementById('photo-path');
    const exitForm = document.getElementById('exit-form');

    // Camera variables
    let stream = null;
    let isCameraActive = false;

    // SignalR event handlers
    connection.on("ReceiveSystemStatus", function(status) {
        updateSystemStatus(status);
    });

    connection.on("TriggerCamera", function(gateId) {
        if (gateId === gateIdInput.value && isCameraActive) {
            captureImage();
        }
    });

    connection.on("ExitButtonPressed", function(gateId) {
        if (gateId === gateIdInput.value) {
            console.log("Exit button pressed on gate: " + gateId);
            // Start camera automatically if not active
            if (!isCameraActive) {
                startCamera();
            }
        }
    });

    connection.on("PrintReceipt", function(data) {
        console.log("Printing receipt for vehicle: " + data.plateNumber);
        // Here you would typically send data to a printer service
        // For demo purposes we'll just show an alert
        alert(`Receipt printed for ${data.plateNumber}, Fee: ${formatCurrency(data.cost)}`);
    });

    connection.on("OpenExitGate", function(gateId) {
        if (gateId === gateIdInput.value) {
            console.log("Opening exit gate: " + gateId);
            // Here you would typically send a command to the physical gate
            // For demo purposes we'll show a notification
            showNotification("Gate opened", "The exit gate is now open", "success");
        }
    });

    // Start SignalR connection
    connection.start().then(function() {
        console.log("SignalR Connected");
        updateGateStatus();
    }).catch(function(err) {
        console.error(err.toString());
        showNotification("Connection Error", "Failed to connect to server. Please refresh the page.", "error");
    });

    // Camera control functions
    startCameraBtn.addEventListener('click', startCamera);
    stopCameraBtn.addEventListener('click', stopCamera);
    captureImageBtn.addEventListener('click', captureImage);

    function startCamera() {
        if (navigator.mediaDevices && navigator.mediaDevices.getUserMedia) {
            navigator.mediaDevices.getUserMedia({ video: true })
                .then(function(mediaStream) {
                    stream = mediaStream;
                    cameraStream.srcObject = mediaStream;
                    cameraStream.onloadedmetadata = function(e) {
                        cameraStream.play();
                    };
                    cameraPlaceholder.style.display = 'none';
                    cameraStream.style.display = 'block';
                    isCameraActive = true;
                    startCameraBtn.disabled = true;
                    stopCameraBtn.disabled = false;
                    captureImageBtn.disabled = false;
                })
                .catch(function(err) {
                    console.log("Camera error: " + err);
                    showNotification("Camera Error", "Failed to access camera. " + err.message, "error");
                });
        } else {
            showNotification("Camera Error", "Your browser does not support camera access", "error");
        }
    }

    function stopCamera() {
        if (stream) {
            stream.getTracks().forEach(track => {
                track.stop();
            });
            cameraStream.style.display = 'none';
            cameraPlaceholder.style.display = 'flex';
            isCameraActive = false;
            startCameraBtn.disabled = false;
            stopCameraBtn.disabled = true;
            captureImageBtn.disabled = true;
        }
    }

    function captureImage() {
        if (isCameraActive) {
            cameraCanvas.getContext('2d').drawImage(cameraStream, 0, 0, cameraCanvas.width, cameraCanvas.height);
            const imageData = cameraCanvas.toDataURL('image/png');
            
            // Upload the image to the server
            uploadImage(imageData);
        }
    }

    function uploadImage(imageData) {
        fetch('/api/upload/image', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ data: imageData, type: 'exit' })
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                photoPathInput.value = data.filePath;
                showNotification("Image Captured", "Vehicle image captured successfully", "success");
            } else {
                showNotification("Error", "Failed to upload image: " + data.message, "error");
            }
        })
        .catch(error => {
            console.error('Error uploading image:', error);
            showNotification("Error", "Failed to upload image", "error");
        });
    }

    // Vehicle search
    findVehicleBtn.addEventListener('click', findVehicle);
    plateNumberInput.addEventListener('keypress', function(e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            findVehicle();
        }
    });

    function findVehicle() {
        const plateNumber = plateNumberInput.value.trim();
        if (!plateNumber) {
            showNotification("Input Error", "Please enter a plate number", "warning");
            return;
        }

        fetch(`/ExitGate/FindVehicle?plateNumber=${encodeURIComponent(plateNumber)}`)
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    // Show vehicle information
                    document.getElementById('vehicle-type').textContent = data.vehicleType;
                    document.getElementById('entry-time').textContent = formatDateTime(data.entryTime);
                    document.getElementById('duration').textContent = `${data.durationHours} hours (${data.durationMinutes} minutes)`;
                    document.getElementById('parking-space').textContent = data.parkingSpaceId || 'Unknown';
                    document.getElementById('parking-fee').textContent = formatCurrency(data.cost);
                    vehicleInfoDiv.style.display = 'block';
                } else {
                    vehicleInfoDiv.style.display = 'none';
                    showNotification("Search Error", data.message, "error");
                }
            })
            .catch(error => {
                console.error('Error finding vehicle:', error);
                showNotification("Search Error", "Failed to find vehicle information", "error");
            });
    }

    // Open gate emergency function
    openGateBtn.addEventListener('click', function() {
        if (confirm("Are you sure you want to open the gate? This is for emergencies only.")) {
            const gateId = gateIdInput.value;
            
            fetch(`/ExitGate/ProcessButtonPress?gateId=${encodeURIComponent(gateId)}`, {
                method: 'POST'
            })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    showNotification("Gate Opened", "Exit gate opened successfully", "success");
                } else {
                    showNotification("Error", data.message, "error");
                }
            })
            .catch(error => {
                console.error('Error opening gate:', error);
                showNotification("Error", "Failed to open gate", "error");
            });
        }
    });

    // Print receipt again functionality
    document.querySelectorAll('.print-again').forEach(button => {
        button.addEventListener('click', function() {
            const transactionId = this.getAttribute('data-transaction-id');
            
            fetch('/ExitGate/PrintReceipt', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ transactionId: transactionId })
            })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    showNotification("Receipt Printed", "Receipt printed successfully", "success");
                } else {
                    showNotification("Error", data.message, "error");
                }
            })
            .catch(error => {
                console.error('Error printing receipt:', error);
                showNotification("Error", "Failed to print receipt", "error");
            });
        });
    });

    // Gate status update
    function updateGateStatus() {
        const gateId = gateIdInput.value;
        
        fetch(`/ExitGate/GetGateStatus?gateId=${encodeURIComponent(gateId)}`)
            .then(response => response.json())
            .then(data => {
                // Update the UI based on the gate status
                const cameraStatus = document.querySelector('.status-item:nth-child(1) .status-indicator');
                const printerStatus = document.querySelector('.status-item:nth-child(2) .status-indicator');
                const onlineStatus = document.querySelector('.status-item:nth-child(3) .status-indicator');
                const lastSyncEl = document.querySelector('.status-item:nth-child(4) span:last-child');
                
                cameraStatus.className = `status-indicator ${data.isCameraActive ? 'active' : 'inactive'}`;
                cameraStatus.textContent = data.isCameraActive ? 'Active' : 'Inactive';
                
                printerStatus.className = `status-indicator ${data.isPrinterActive ? 'active' : 'inactive'}`;
                printerStatus.textContent = data.isPrinterActive ? 'Active' : 'Inactive';
                
                onlineStatus.className = `status-indicator ${data.isOnline ? 'active' : 'warning'}`;
                onlineStatus.textContent = data.isOnline ? 'Online' : 'Offline';
                
                lastSyncEl.textContent = formatDateTime(data.lastSync);
            })
            .catch(error => {
                console.error('Error updating gate status:', error);
            });
    }

    // Set up polling for gate status
    setInterval(updateGateStatus, 30000); // Update every 30 seconds

    // Helper functions
    function showNotification(title, message, type) {
        // You can implement a custom notification system or use an existing library
        // For simplicity, we'll use alert for now
        alert(`${title}: ${message}`);
    }

    function formatDateTime(dateString) {
        const date = new Date(dateString);
        return date.toLocaleString();
    }

    function formatCurrency(amount) {
        return new Intl.NumberFormat('id-ID', { style: 'currency', currency: 'IDR' }).format(amount);
    }
}); 