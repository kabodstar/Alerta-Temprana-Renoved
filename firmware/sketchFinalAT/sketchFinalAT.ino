#include <SoftwareSerial.h>
 
// Configura el Bluetooth: Pin 10 será RX (recibe), Pin 11 será TX (transmite)
SoftwareSerial miBluetooth(10, 11);
 
// Pines de tu hardware
const int trigPin = 2;
const int echoPin = 3;
const int ledVerde = 4;
const int ledAmarillo = 5;
const int ledRojo = 6;
const int buzzer = 7;
 
// Variables para medir distancia
long duracion;
int distancia;
 
// Variable de control para no saturar el Bluetooth
bool alertaEnviada = false;
 
// Variables para estabilizar la lectura del sensor
int lecturaAnterior = 0;
int lecturasEstables = 0;
const int lecturasNecesarias = 3;
 
void setup() {
  Serial.begin(9600);       // Monitor serie de la computadora
  miBluetooth.begin(9600);  // Comunicación con el módulo Bluetooth
 
  pinMode(trigPin, OUTPUT);
  pinMode(echoPin, INPUT);
 
  pinMode(ledVerde, OUTPUT);
  pinMode(ledAmarillo, OUTPUT);
  pinMode(ledRojo, OUTPUT);
  pinMode(buzzer, OUTPUT);
 
  noTone(buzzer);
}
 
void loop() {
  // 1. Leer el sensor ultrasónico
  digitalWrite(trigPin, LOW);
  delayMicroseconds(2);
 
  digitalWrite(trigPin, HIGH);
  delayMicroseconds(10);
 
  digitalWrite(trigPin, LOW);
 
  duracion = pulseIn(echoPin, HIGH, 30000);
 
  // Si no se recibe una lectura válida, mantener el estado anterior
  if (duracion == 0) {
    delay(100);
    return;
  }
 
  distancia = duracion * 0.034 / 2;
 
  // 2. Filtro de estabilidad
  // Solo acepta un cambio cuando se mantiene durante varias lecturas
  if (distancia == lecturaAnterior) {
    lecturasEstables++;
  } else {
    lecturasEstables = 0;
  }
 
  lecturaAnterior = distancia;
 
  if (lecturasEstables < lecturasNecesarias) {
    delay(100);
    return;
  }
 
  // >>> ÚNICA LÍNEA AÑADIDA: envía a la app la distancia ya filtrada (solo salida de datos) <<<
  miBluetooth.println(distancia);
 
  // 3. Lógica de las alertas
 
  if (distancia > 28) {
    // ESTADO NORMAL
    digitalWrite(ledVerde, HIGH);
    digitalWrite(ledAmarillo, LOW);
    digitalWrite(ledRojo, LOW);
 
    noTone(buzzer);
 
    alertaEnviada = false;
  }
 
  else if (distancia <= 28 && distancia > 23) {
    // ESTADO PRECAUCIÓN
    digitalWrite(ledVerde, LOW);
    digitalWrite(ledAmarillo, HIGH);
    digitalWrite(ledRojo, LOW);
 
    tone(buzzer, 1000);
    delay(500);
    noTone(buzzer);
    delay(500);
 
    alertaEnviada = false;
  }
 
  else if (distancia <= 23 && distancia > 0) {
    // ESTADO DE PELIGRO
    digitalWrite(ledVerde, LOW);
    digitalWrite(ledAmarillo, LOW);
    digitalWrite(ledRojo, HIGH);
 
    tone(buzzer, 1000);
 
    // Mandamos el mensaje por Bluetooth
    if (alertaEnviada == false) {
      miBluetooth.println("PELIGRO");
      Serial.println("Alerta enviada por Bluetooth");
      alertaEnviada = true;
    }
  }
 
  // 4. Pausa de estabilidad
  delay(100);
}