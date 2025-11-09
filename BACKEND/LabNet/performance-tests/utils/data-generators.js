/**
 * Generadores de datos de prueba
 */

import { randomIntBetween } from 'https://jslib.k6.io/k6-utils/1.2.0/index.js';

// Generar string aleatorio
export function randomString(length) {
    const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
    let result = '';
    for (let i = 0; i < length; i++) {
        result += chars.charAt(Math.floor(Math.random() * chars.length));
    }
    return result;
}

// Generar UUID v4 simple (para pruebas)
export function generateUUID() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
        const r = Math.random() * 16 | 0;
        const v = c === 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}

// Generar documento de identidad aleatorio
export function generateDocumento() {
    return `${randomIntBetween(10000000, 99999999)}`;
}

// Generar email aleatorio
export function generateEmail() {
    const domains = ['example.com', 'test.com', 'demo.com', 'loadtest.com'];
    const domain = domains[randomIntBetween(0, domains.length - 1)];
    return `user${randomIntBetween(1000, 9999)}@${domain}`;
}

// Generar nombre aleatorio
export function generateNombre() {
    const nombres = ['Juan', 'María', 'Pedro', 'Ana', 'Luis', 'Carmen', 'José', 'Laura'];
    return nombres[randomIntBetween(0, nombres.length - 1)];
}

// Generar apellido aleatorio
export function generateApellido() {
    const apellidos = ['García', 'Rodríguez', 'González', 'Fernández', 'López', 'Martínez', 'Sánchez', 'Pérez'];
    return apellidos[randomIntBetween(0, apellidos.length - 1)];
}

// Generar datos de usuario completo
export function generateUsuario() {
    return {
        documento: generateDocumento(),
        nombre: generateNombre(),
        apellido: generateApellido(),
        email: generateEmail(),
        passwordHash: '$2a$11$' + randomString(53), // BCrypt hash simulado
        estado: randomIntBetween(0, 2), // 0=Activo, 1=Inactivo, 2=Suspendido
    };
}

// Generar datos de espacio
export function generateEspacio() {
    const tipos = ['Comedor', 'Biblioteca', 'Gimnasio', 'Auditorio', 'Laboratorio'];
    const modos = ['Entrada', 'Salida', 'EntradaYSalida'];
    
    return {
        nombre: `Espacio ${randomString(8)}`,
        activo: true,
        tipo: tipos[randomIntBetween(0, tipos.length - 1)],
        modo: modos[randomIntBetween(0, modos.length - 1)],
    };
}

// Generar datos de credencial
export function generateCredencial() {
    return {
        numeroCredencial: `CRED${randomIntBetween(100000, 999999)}`,
        fechaEmision: new Date().toISOString(),
        fechaExpiracion: new Date(Date.now() + 365 * 24 * 60 * 60 * 1000).toISOString(),
        activo: true,
        usuarioId: generateUUID(),
    };
}

// Generar datos de canje
export function generateCanje(usuarioId, espacioId) {
    return {
        usuarioId: usuarioId || generateUUID(),
        espacioId: espacioId || generateUUID(),
        timestamp: new Date().toISOString(),
        tipo: randomIntBetween(0, 1) === 0 ? 'Entrada' : 'Salida',
    };
}

// Generar datos de evento de acceso
export function generateEventoAcceso(usuarioId, espacioId) {
    return {
        usuarioId: usuarioId || generateUUID(),
        espacioId: espacioId || generateUUID(),
        timestamp: new Date().toISOString(),
        tipoEvento: randomIntBetween(0, 2), // 0=Entrada, 1=Salida, 2=Intento
        exitoso: Math.random() > 0.1, // 90% exitosos
    };
}

// Generar datos de regla de acceso
export function generateReglaDeAcceso() {
    const diasSemana = [0, 1, 2, 3, 4]; // Lunes a Viernes
    return {
        nombre: `Regla ${randomString(8)}`,
        descripcion: 'Regla generada para pruebas de carga',
        horaInicio: '08:00:00',
        horaFin: '18:00:00',
        diasSemana: diasSemana,
        activa: true,
    };
}

// Generar datos de dispositivo
export function generateDispositivo(usuarioId) {
    const tipos = ['Android', 'iOS', 'Web'];
    return {
        usuarioId: usuarioId || generateUUID(),
        deviceId: generateUUID(),
        deviceType: tipos[randomIntBetween(0, tipos.length - 1)],
        deviceToken: randomString(64),
        activo: true,
    };
}

// Generar datos de rol
export function generateRol() {
    const nombres = ['Usuario', 'Funcionario', 'Administrador', 'Guardia'];
    return {
        nombre: nombres[randomIntBetween(0, nombres.length - 1)],
        descripcion: 'Rol generado para pruebas',
    };
}

// Generar datos de beneficio
export function generateBeneficio() {
    return {
        nombre: `Beneficio ${randomString(8)}`,
        descripcion: 'Beneficio para pruebas de carga',
        activo: true,
        fechaInicio: new Date().toISOString(),
        fechaFin: new Date(Date.now() + 180 * 24 * 60 * 60 * 1000).toISOString(),
    };
}

// Seleccionar elemento aleatorio de un array
export function randomElement(array) {
    return array[randomIntBetween(0, array.length - 1)];
}

// Generar array de elementos
export function generateArray(generator, count) {
    const result = [];
    for (let i = 0; i < count; i++) {
        result.push(generator());
    }
    return result;
}
