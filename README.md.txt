# AlertaTemprana

Sistema de monitoreo en tiempo real del nivel de agua para la detección temprana de inundaciones

  
------
 Versión  0.9.0 (Release Candidate) 
 Plataforma  Android 5.0 (API 21) o superior 
 Framework  .NET MAUI 9.0 
 Identificador de paquete  `com.alertatemprana.app` 
 Desarrollador  AlertaTemprana Dev 

---

## Contenido

1. [Descripción general](#1-descripción-general)
2. [Características](#2-características)
3. [Requisitos](#3-requisitos)
4. [Requisito del firmware del Arduino](#4-requisito-del-firmware-del-arduino)
5. [Instalación](#5-instalación)
6. [Configuración inicial](#6-configuración-inicial)
7. [Uso de la aplicación](#7-uso-de-la-aplicación)
8. [Sistema de alertas](#8-sistema-de-alertas)
9. [Conexión Bluetooth](#9-conexión-bluetooth)
10. [Especificaciones técnicas](#10-especificaciones-técnicas)
11. [Solución de problemas](#11-solución-de-problemas)
12. [Limitaciones conocidas](#12-limitaciones-conocidas)
13. [Aviso de uso](#13-aviso-de-uso)
14. [Lista de verificación previa al uso](#14-lista-de-verificación-previa-al-uso)

---

## 1. Descripción general

AlertaTemprana es una aplicación para Android que monitorea el nivel del agua en zonas propensas a inundaciones. Se conecta mediante Bluetooth a un sensor ultrasónico instalado en una maqueta, recibe las mediciones de forma continua y las presenta de manera visual y comprensible.

La aplicación clasifica el nivel medido en tres estados (Seguro, Riesgo y Peligro), notifica al usuario cuando se alcanza una condición de riesgo y conserva un historial de los eventos relevantes para su consulta posterior.

---

## 2. Características

- Monitoreo en tiempo real. Visualización del nivel del agua en metros, con actualización continua.
- Indicador visual. Barra de nivel con escala de referencia de 0.00 a 2.00 m.
- Tres estados de alerta. Seguro, Riesgo y Peligro, diferenciados por color y descripción.
- Notificaciones del sistema. Avisos al alcanzar los estados Riesgo o Peligro, incluso con la aplicación en segundo plano.
- Historial persistente. Registro de los eventos de Riesgo y Peligro, con fecha, hora y nivel alcanzado.
- Filtros de consulta. Por rango de fechas y por tipo de evento.
- Selector de módulo Bluetooth. Selección del módulo a partir de la lista de dispositivos emparejados, independientemente de su nombre.
- Funcionamiento sin internet. Requiere únicamente Bluetooth.
- Almacenamiento local. Los datos permanecen en el dispositivo.

---

## 3. Requisitos

### Hardware

 Componente  Descripción 
------
 Dispositivo Android  Teléfono o tablet con Android 5.0 (API 21) o superior y Bluetooth 
 Maqueta  Sensor ultrasónico conectado a un microcontrolador Arduino 
 Módulo Bluetooth  HC-05 o HC-06 conectado al Arduino 
 Firmware  Sketch del Arduino actualizado (ver sección 4) 

### Software

- Archivo de instalación `.apk` de AlertaTemprana.
- Permisos de Bluetooth y de notificaciones, que la aplicación solicita durante el primer uso.

---

## 4. Requisito del firmware del Arduino

La aplicación no estima el nivel del agua por sí misma lo calcula a partir de la distancia que la maqueta transmite por Bluetooth. Por ello, el sketch del Arduino debe enviar un valor numérico por línea, de forma continua.

 Importante. Si la maqueta ejecuta una versión anterior del sketch (que únicamente transmitía el texto `PELIGRO`), la aplicación establece la conexión sin errores, pero no actualiza la gráfica ni registra eventos en el historial. Verifique el firmware de la maqueta antes de realizar pruebas.

Dentro de `loop()`, inmediatamente después de calcular la distancia, debe existir la siguiente instrucción

```cpp
duracion = pulseIn(echoPin, HIGH);
distancia = duracion  0.034  2;    distancia en cm

miBluetooth.println(distancia);      Obligatoria la aplicación lee este valor
```

Requisitos del sketch

- Enviar un valor por línea (`println`, no `print`), con únicamente la distancia en centímetros (por ejemplo `45` o `45.3`).
- Configurar la misma velocidad de transmisión en el módulo y en el sketch (normalmente 9600 baudios).
- La lógica de LED y zumbador puede mantenerse sin cambios. Si el sketch envía además texto como `PELIGRO`, la aplicación lo ignora.
- La aplicación interpreta el valor como la distancia entre el sensor y la superficie del agua a menor distancia, mayor nivel.

Verificación rápida. Con una aplicación genérica de terminal Bluetooth conectada al módulo, acerque y aleje la mano del sensor

- Si aparecen números continuos que varían, el firmware es correcto.
- Si solo aparece la palabra `PELIGRO` ocasionalmente, o no aparece nada, el firmware está desactualizado.

---

## 5. Instalación

1. Descargue el archivo `.apk` desde el enlace de distribución.
2. Si es la primera vez que instala una aplicación desde un archivo `.apk`, habilite la instalación desde fuentes desconocidas en Ajustes → Seguridad (o Privacidad y seguridad, según el fabricante), para la aplicación desde la cual abrirá el archivo.
3. Abra el archivo `.apk` y seleccione Instalar.
4. Al finalizar, seleccione Abrir o ejecute AlertaTemprana desde la lista de aplicaciones.

---

## 6. Configuración inicial

### 6.1 Emparejamiento del módulo Bluetooth

El emparejamiento se realiza una sola vez desde los ajustes del dispositivo.

1. Encienda la maqueta (Arduino, sensor y módulo Bluetooth alimentados).
2. Abra Ajustes → Bluetooth y verifique que Bluetooth esté activado.
3. Busque el módulo en la lista de dispositivos disponibles (por ejemplo `HC-05` o `HC-06`; el nombre puede incluir texto adicional).
4. Seleccione el módulo para emparejarlo e ingrese el PIN si se solicita (habitualmente `1234` o `0000`).
5. Verifique que el módulo aparezca en la lista de dispositivos emparejados.

### 6.2 Permisos de la aplicación

Al presionar Conectar módulo por primera vez, Android solicitará

- Permiso de Bluetooth  Dispositivos cercanos. Seleccione Permitir.
- Permiso de notificaciones (Android 13 o superior). Seleccione Permitir.

---

## 7. Uso de la aplicación

### 7.1 Pantalla de Inicio

Presenta los datos en tiempo real.

 Elemento  Función 
------
 Estado de Bluetooth  Indica si existe conexión con el módulo (Conectado en verde, Desconectado en rojo) 
 Botón Conectar módulo  Abre la lista de dispositivos emparejados para elegir el módulo. Una vez conectado, cambia a Cambiar módulo (nombre) 
 Nivel del agua actual  Valor en metros y barra de nivel con escala de referencia de 0.00 a 2.00 m 
 Nivel de alerta  Estado actual (Seguro, Riesgo o Peligro) con su descripción y leyenda de colores 
 Sistema activo  Confirma que las notificaciones se encuentran habilitadas 

Procedimiento de uso

1. Verifique que Bluetooth esté activado, que el módulo esté emparejado y que la maqueta esté encendida.
2. Presione Conectar módulo y seleccione el módulo en la lista.
3. Confirme que el estado cambie a Bluetooth conectado.
4. Observe el nivel del agua, que se actualiza con cada medición recibida.
5. Al alcanzar el estado Riesgo o Peligro, la aplicación emite una notificación.

### 7.2 Pantalla de Historial

Presenta el registro de eventos de Riesgo y Peligro, ordenados del más reciente al más antiguo. Cada registro muestra el nivel alcanzado, el estado, la fecha y hora, y un indicador visual del nivel.

 Control  Opciones 
------
 Filtro de fecha  Últimas 24 horas, Última semana, Último mes, Todo el historial 
 Filtro de evento  Todos los eventos, Solo Riesgo, Solo Peligro 
 Botón de borrar  Elimina todo el historial, previa confirmación 

Para cambiar de pantalla, utilice las pestañas Inicio e Historial.

---

## 8. Sistema de alertas

 Estado  Color  Significado  Acción recomendada 
------------
 Seguro  Verde  El nivel del agua se encuentra en rangos normales  Ninguna 
 Riesgo  Amarillo  El nivel está aumentando y se aproxima a un valor de atención  Mantener atención sobre la zona 
 Peligro  Rojo  El nivel alcanzó un valor crítico  Tomar precauciones inmediatas 

Cuando el estado cambia a Riesgo o Peligro, la aplicación

1. Actualiza la pantalla principal.
2. Registra el evento en el historial.
3. Emite una notificación del sistema con el título Alerta Temprana [estado] y la descripción correspondiente.

Los períodos en estado Seguro no se registran, con el fin de mantener el historial limitado a los eventos relevantes.

---

## 9. Conexión Bluetooth

Conexión manual. La conexión se establece presionando Conectar módulo en la pantalla de Inicio. Debe repetirse si se abre la aplicación con la maqueta apagada, si se reinicia la maqueta, si la conexión se pierde o si se desea cambiar de módulo.

Conexión persistente. Una vez establecida, la conexión se mantiene al navegar entre Inicio e Historial. No es necesario reconectar al cambiar de pestaña.

Si la conexión falla, verifique lo siguiente

- La maqueta está encendida.
- El módulo está emparejado en los ajustes de Bluetooth del dispositivo.
- Bluetooth está activado en el dispositivo.
- La aplicación tiene concedido el permiso de Bluetooth.
- Ningún otro dispositivo o aplicación está conectado al módulo (un HC-05 o HC-06 admite una única conexión a la vez).

En ocasiones el primer intento de conexión con un módulo HC-05 falla y el segundo tiene éxito; en ese caso, vuelva a presionar Conectar módulo.

---

## 10. Especificaciones técnicas

 Parámetro  Valor 
------
 Identificador de paquete  `com.alertatemprana.app` 
 Versión  0.9.0 (Release Candidate) 
 Framework  .NET MAUI 9.0 
 Plataforma  Android 5.0 (API 21) o superior 
 Tipo de Bluetooth  Clásico, perfil SPP (RFCOMM) 
 UUID de servicio  `00001101-0000-1000-8000-00805F9B34FB` 
 Permisos  `BLUETOOTH_CONNECT`, `POST_NOTIFICATIONS` (según la versión de Android) 
 Almacenamiento  Archivo JSON en el directorio privado de la aplicación 
 Privacidad  Los datos no se transmiten fuera del dispositivo 

Formato de datos esperado una línea de texto por medición, terminada en salto de línea, con la distancia en centímetros entre el sensor y el agua. Las líneas que no contienen un número se ignoran.

---

## 11. Solución de problemas

 Síntoma  Causa probable  Solución 
---------
 No se puede conectar el módulo  Módulo no emparejado, apagado o sin permisos  Emparejar el módulo en Ajustes → Bluetooth (PIN `1234` o `0000`), conceder el permiso de Bluetooth y volver a presionar Conectar módulo 
 Conecta, pero la gráfica no se mueve y no se guarda historial  Firmware antiguo en la maqueta, que no envía la distancia como número  Actualizar el sketch según la sección 4 y verificar con una terminal Bluetooth que llegan números continuos 
 No aparece ningún valor  Conexión inactiva o sensor sin transmitir  Confirmar que el estado indica Bluetooth conectado, encender la maqueta, revisar el cableado del sensor y reconectar 
 No se reciben notificaciones  Permiso de notificaciones no concedido  Activarlo en Ajustes → Aplicaciones → AlertaTemprana → Notificaciones 
 El historial aparece vacío  Filtro demasiado restrictivo o aún no hay eventos  Seleccionar Todo el historial y Todos los eventos; recordar que solo se registran Riesgo y Peligro 

---

## 12. Limitaciones conocidas

- La conexión Bluetooth y las notificaciones están disponibles únicamente en Android.
- No existe sincronización en la nube; los datos permanecen en el dispositivo.
- La maqueta debe enviar la distancia como valor numérico (sección 4).
- El módulo debe emparejarse previamente desde los ajustes del sistema; la aplicación no realiza búsqueda de dispositivos nuevos.

---

## 13. Aviso de uso

AlertaTemprana es una herramienta educativa desarrollada como proyecto académico. Constituye un sistema de monitoreo complementario no sustituye las indicaciones de las autoridades competentes ni garantiza la prevención de una inundación. Su función es informar oportunamente sobre variaciones en el nivel del agua para apoyar la toma de decisiones.

---

## 14. Lista de verificación previa al uso

- [ ] Aplicación instalada correctamente.
- [ ] Bluetooth activado en el dispositivo.
- [ ] Módulo HC-05 o HC-06 emparejado desde Ajustes.
- [ ] Sketch del Arduino actualizado (envía la distancia con `println`).
- [ ] Permisos de Bluetooth y notificaciones concedidos.
- [ ] Maqueta encendida y sensor operativo.