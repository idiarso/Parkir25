class WebSocketClient {
    constructor(url = 'ws://' + window.location.host + '/ws') {
        this.url = url;
        this.reconnectAttempts = 0;
        this.maxReconnectAttempts = 5;
        this.reconnectDelay = 1000; // Start with 1 second
        this.eventHandlers = new Map();
        this.connect();
    }

    connect() {
        try {
            this.ws = new WebSocket(this.url);
            this.setupEventListeners();
        } catch (error) {
            console.error('WebSocket connection error:', error);
            this.scheduleReconnect();
        }
    }

    setupEventListeners() {
        this.ws.onopen = () => {
            console.log('WebSocket connected');
            this.reconnectAttempts = 0;
            this.reconnectDelay = 1000;
            this.updateConnectionStatus('connected');
        };

        this.ws.onclose = () => {
            console.log('WebSocket disconnected');
            this.updateConnectionStatus('disconnected');
            this.scheduleReconnect();
        };

        this.ws.onerror = (error) => {
            console.error('WebSocket error:', error);
            this.updateConnectionStatus('error');
        };

        this.ws.onmessage = (event) => {
            try {
                const message = JSON.parse(event.data);
                this.handleMessage(message);
            } catch (error) {
                console.error('Error parsing WebSocket message:', error);
            }
        };
    }

    scheduleReconnect() {
        if (this.reconnectAttempts < this.maxReconnectAttempts) {
            console.log(`Reconnecting in ${this.reconnectDelay / 1000} seconds...`);
            setTimeout(() => {
                this.reconnectAttempts++;
                this.reconnectDelay *= 2; // Exponential backoff
                this.connect();
            }, this.reconnectDelay);
        } else {
            console.error('Max reconnection attempts reached');
            this.updateConnectionStatus('failed');
        }
    }

    updateConnectionStatus(status) {
        const statusIndicator = document.getElementById('connection-status');
        if (statusIndicator) {
            statusIndicator.className = `connection-status ${status}`;
            statusIndicator.textContent = status.charAt(0).toUpperCase() + status.slice(1);
        }

        // Dispatch connection status event
        this.dispatchEvent('connectionStatus', { status });
    }

    on(eventType, handler) {
        if (!this.eventHandlers.has(eventType)) {
            this.eventHandlers.set(eventType, new Set());
        }
        this.eventHandlers.get(eventType).add(handler);
    }

    off(eventType, handler) {
        if (this.eventHandlers.has(eventType)) {
            this.eventHandlers.get(eventType).delete(handler);
        }
    }

    dispatchEvent(eventType, data) {
        if (this.eventHandlers.has(eventType)) {
            this.eventHandlers.get(eventType).forEach(handler => {
                try {
                    handler(data);
                } catch (error) {
                    console.error(`Error in ${eventType} handler:`, error);
                }
            });
        }
    }

    handleMessage(message) {
        if (message.event) {
            this.dispatchEvent(message.event, message.data);
        }
    }

    send(eventType, data) {
        if (this.ws.readyState === WebSocket.OPEN) {
            const message = JSON.stringify({
                event: eventType,
                data: data
            });
            this.ws.send(message);
        } else {
            console.error('WebSocket is not connected');
        }
    }

    reconnect() {
        if (this.ws) {
            this.ws.close();
        }
        this.reconnectAttempts = 0;
        this.reconnectDelay = 1000;
        this.connect();
    }
}

// Create global instance
window.wsClient = new WebSocketClient();

// Example usage:
/*
wsClient.on('vehicleEntry', (data) => {
    console.log('New vehicle entered:', data);
    updateDashboard(data);
});

wsClient.on('vehicleExit', (data) => {
    console.log('Vehicle exited:', data);
    updateDashboard(data);
});

wsClient.on('connectionStatus', (data) => {
    console.log('Connection status changed:', data.status);
    updateConnectionIndicator(data.status);
});
*/ 