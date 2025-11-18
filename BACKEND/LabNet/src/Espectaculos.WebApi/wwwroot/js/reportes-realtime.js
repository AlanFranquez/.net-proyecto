console.log("reportes-realtime.js cargado");

const statusEl = document.getElementById("realtime-status");
const bodyEl = document.getElementById("realtime-body");

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/accesos")
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Information)
    .build();

// Handler del evento que manda el backend
connection.on("NuevoAcceso", (evento) => {
    console.log("NuevoAcceso recibido:", evento);

    if (!bodyEl) {
        console.warn("No se encontró #realtime-body");
        return;
    }

    const momento = evento.momento ?? evento.momentoDeAcceso;
    const espacio = evento.espacio ?? evento.espacioNombre;
    const usuario = evento.usuario ?? evento.usuarioNombre;

    const tr = document.createElement("tr");
    tr.innerHTML = `
        <td>${momento ?? ""}</td>
        <td>${espacio ?? ""}</td>
        <td>${usuario ?? ""}</td>
        <td>${evento.resultado ?? ""}</td>
        <td>${evento.modo ?? ""}</td>
        <td>${evento.motivo ?? ""}</td>
    `;

    bodyEl.prepend(tr);

    if (statusEl) {
        statusEl.textContent = "Conectado al servidor de eventos.";
    }
});

connection.onreconnecting(err => {
    console.warn("Reconectando...", err);
    if (statusEl) statusEl.textContent = "Reconectando al servidor de eventos...";
});

connection.onreconnected(id => {
    console.log("Reconectado, connectionId:", id);
    if (statusEl) statusEl.textContent = "Conectado al servidor de eventos.";
});

connection.onclose(err => {
    console.warn("Conexión cerrada:", err);
    if (statusEl) statusEl.textContent = "Desconectado del servidor de eventos.";
});

async function start() {
    try {
        await connection.start();
        console.log("Conectado a AccesosHub");
        if (statusEl) statusEl.textContent = "Conectado al servidor de eventos.";
    } catch (err) {
        console.error("Error al conectar a AccesosHub:", err);
        if (statusEl) statusEl.textContent = "Error al conectar al servidor de eventos.";
        setTimeout(start, 5000);
    }
}

start();
