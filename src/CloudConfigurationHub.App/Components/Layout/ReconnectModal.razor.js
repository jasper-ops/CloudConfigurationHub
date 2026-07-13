// Set up event handlers
const reconnectModal = document.getElementById("components-reconnect-modal");
reconnectModal.addEventListener("components-reconnect-state-changed", handleReconnectStateChanged);

const retryButton = document.getElementById("components-reconnect-button");
retryButton.addEventListener("click", retry);

const resumeButton = document.getElementById("components-resume-button");
resumeButton.addEventListener("click", resume);

const serviceStatus = document.querySelector(".cch-service-status");
const serviceStatusText = serviceStatus?.querySelector("span:last-child");
const connectedLabel = serviceStatusText?.textContent?.trim() || "Connected";
const statusLabels = {
    connected: serviceStatus?.dataset.connectedLabel || connectedLabel,
    reconnecting: serviceStatus?.dataset.reconnectingLabel || "Reconnecting",
    disconnected: serviceStatus?.dataset.disconnectedLabel || "Disconnected",
    paused: serviceStatus?.dataset.pausedLabel || "Session paused"
};

function handleReconnectStateChanged(event) {
    if (event.detail.state === "show") {
        showReconnectNotice();
        setServiceStatus("reconnecting");
    } else if (event.detail.state === "hide") {
        closeReconnectNotice();
        setServiceStatus("connected");
    } else if (event.detail.state === "failed") {
        showReconnectNotice();
        setServiceStatus("disconnected");
        document.addEventListener("visibilitychange", retryWhenDocumentBecomesVisible);
    } else if (event.detail.state === "rejected") {
        location.reload();
    }
}

async function retry() {
    document.removeEventListener("visibilitychange", retryWhenDocumentBecomesVisible);
    setServiceStatus("reconnecting");

    try {
        // Reconnect will asynchronously return:
        // - true to mean success
        // - false to mean we reached the server, but it rejected the connection (e.g., unknown circuit ID)
        // - exception to mean we didn't reach the server (this can be sync or async)
        const successful = await Blazor.reconnect();
        if (!successful) {
            // We have been able to reach the server, but the circuit is no longer available.
            // We'll reload the page so the user can continue using the app as quickly as possible.
            const resumeSuccessful = await Blazor.resumeCircuit();
            if (!resumeSuccessful) {
                location.reload();
            } else {
                closeReconnectNotice();
                setServiceStatus("connected");
            }
        } else {
            closeReconnectNotice();
            setServiceStatus("connected");
        }
    } catch (err) {
        // We got an exception, server is currently unavailable
        setServiceStatus("disconnected");
        document.addEventListener("visibilitychange", retryWhenDocumentBecomesVisible);
    }
}

async function resume() {
    setServiceStatus("reconnecting");

    try {
        const successful = await Blazor.resumeCircuit();
        if (!successful) {
            location.reload();
        } else {
            closeReconnectNotice();
            setServiceStatus("connected");
        }
    } catch {
        reconnectModal.classList.replace("components-reconnect-paused", "components-reconnect-resume-failed");
        setServiceStatus("paused");
    }
}

async function retryWhenDocumentBecomesVisible() {
    if (document.visibilityState === "visible") {
        await retry();
    }
}

function showReconnectNotice() {
    if (!reconnectModal.open) {
        reconnectModal.show();
    }
}

function closeReconnectNotice() {
    if (reconnectModal.open) {
        reconnectModal.close();
    }
}

function setServiceStatus(state) {
    if (!serviceStatus || !serviceStatusText) {
        return;
    }

    serviceStatus.classList.remove("is-reconnecting", "is-disconnected", "is-paused");

    if (state === "reconnecting") {
        serviceStatus.classList.add("is-reconnecting");
    } else if (state === "disconnected") {
        serviceStatus.classList.add("is-disconnected");
    } else if (state === "paused") {
        serviceStatus.classList.add("is-paused");
    }

    serviceStatusText.textContent = statusLabels[state] || connectedLabel;
}
