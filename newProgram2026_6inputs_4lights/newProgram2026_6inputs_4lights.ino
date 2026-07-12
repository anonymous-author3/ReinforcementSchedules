const byte INPUT_COUNT = 6;
const byte LIGHT_COUNT = 4;

const byte ResponsePins[INPUT_COUNT] = {2, 3, 4, 5, 6, 7};

// Change these pins if your input actuators use a different wiring layout.
const byte InputOutputPins[INPUT_COUNT] = {22, 23, 24, 25, 26, 27};

const byte Houselight = 8;
const byte LightPins[LIGHT_COUNT] = {9, 10, 11, 12};
const byte Sonalert = A3;
const byte Pump = A2;
const byte Step_Right = A0;
const byte Dir_Right = A1;

int StepCount = 0;

void setInputOutput(byte index, bool turnOn) {
  if (index >= INPUT_COUNT) return;
  digitalWrite(InputOutputPins[index], turnOn ? HIGH : LOW);
}

void setLight(byte index, bool turnOn) {
  if (index >= LIGHT_COUNT) return;
  digitalWrite(LightPins[index], turnOn ? HIGH : LOW);
}

void pulseFeeder() {
  for (StepCount = 0; StepCount < 80; StepCount++) {
    digitalWrite(Step_Right, HIGH);
    delay(5);
    digitalWrite(Step_Right, LOW);
    delay(5);
  }
}

void pulsePump() {
  digitalWrite(Pump, HIGH);
  delay(50);
  digitalWrite(Pump, LOW);
}

void allOff() {
  digitalWrite(Houselight, LOW);
  noTone(Sonalert);
  digitalWrite(Pump, LOW);

  for (byte i = 0; i < LIGHT_COUNT; i++) {
    setLight(i, false);
  }

  for (byte i = 0; i < INPUT_COUNT; i++) {
    setInputOutput(i, false);
  }
}

void setup() {
  Serial.begin(9600);

  for (byte i = 0; i < INPUT_COUNT; i++) {
    pinMode(ResponsePins[i], INPUT_PULLUP);
    pinMode(InputOutputPins[i], OUTPUT);
    setInputOutput(i, false);
  }

  pinMode(Houselight, OUTPUT);
  pinMode(Sonalert, OUTPUT);
  pinMode(Pump, OUTPUT);
  pinMode(Step_Right, OUTPUT);
  pinMode(Dir_Right, OUTPUT);

  for (byte i = 0; i < LIGHT_COUNT; i++) {
    pinMode(LightPins[i], OUTPUT);
    setLight(i, false);
  }

  allOff();
}

void loop() {
  if (Serial.available() > 0) {
    char event = Serial.read();

    switch (event) {
      case 'H': digitalWrite(Houselight, HIGH); break;
      case 'h': digitalWrite(Houselight, LOW); break;

      case 'T': tone(Sonalert, 1500); break;
      case 't': noTone(Sonalert); break;
      case 'Z': tone(Sonalert, 1500, 250); break;

      case 'A': setLight(0, true); break;
      case 'a': setLight(0, false); break;
      case 'B': setLight(1, true); break;
      case 'b': setLight(1, false); break;
      case 'E': setLight(2, true); break;
      case 'e': setLight(2, false); break;
      case 'F': setLight(3, true); break;
      case 'f': setLight(3, false); break;

      case 'L': setInputOutput(0, true); break;
      case 'l': setInputOutput(0, false); break;
      case 'M': setInputOutput(1, true); break;
      case 'm': setInputOutput(1, false); break;
      case 'C': setInputOutput(2, true); break;
      case 'c': setInputOutput(2, false); break;
      case 'D': setInputOutput(3, true); break;
      case 'd': setInputOutput(3, false); break;
      case 'N': setInputOutput(4, true); break;
      case 'n': setInputOutput(4, false); break;
      case 'O': setInputOutput(5, true); break;
      case 'o': setInputOutput(5, false); break;

      case 'R': pulseFeeder(); break;
      case 'W': pulsePump(); break;
      case 'P': digitalWrite(Pump, HIGH); break;
      case 'p': digitalWrite(Pump, LOW); break;
      case 'X': allOff(); break;
    }
  }

  for (byte i = 0; i < INPUT_COUNT; i++) {
    Serial.print(digitalRead(ResponsePins[i]));
    if (i < INPUT_COUNT - 1) Serial.print(",");
  }
  Serial.println();

  delay(4);
}
