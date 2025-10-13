under construction
# ParkingApp 🚗  
A voice-driven parking registration system with an ASP.NET Core backend and a React Native mobile client.

---

## 🧭 User Flow

1. **Welcome Prompt**  
   - The app plays a short welcome message using Google Cloud Text-to-Speech (TTS).

2. **Speech Recording**  
   - The user records their voice using Expo Audio APIs.  
   - The audio is processed and converted (via FFmpeg).

3. **Speech-to-Text Conversion**  
   - The recorded audio is sent to Google Cloud Speech-to-Text (STT).  
   - The transcribed city name is extracted and sent to the mobile.
       
### *In next versions*

4. **User Confirmation**  
   - The app gets back the detected city name and sends back, using TTS, to the user for confirmation.  
   - The user confirms or corrects the city name.

5. **City Name Validation**  
   - The backend checks if the city exists in the MongoDB database.  
   - If needed, the name is translated using Google Cloud Translate.

6. **Zone Selection**  
   - If the city is valid, the app fetches available parking zones.  
   - The user selects a zone via the mobile UI.

7. **Parking Registration**  
   - The app registers the parking session.  
   - Optionally stores session data in Redis for fast access.

8. **Confirmation Playback**  
   - A final confirmation message is played using TTS.

---

## 🛠️ Tech Stack

### 🔙 Backend
- **ASP.NET Core** – RESTful API
- **MongoDB** – City and zone data
- **Redis** – Optional caching layer
- **Docker** – Containerized deployment
- **Google Cloud APIs**:
  - Text-to-Speech (TTS)
  - Speech-to-Text (STT)
  - Translate API
- **FFmpeg** – Audio format conversion

### 📱 Mobile
- **React Native** – Cross-platform mobile app
- **Expo** – Simplified development and audio recording
- **Fetch API** – Communication with backend
- **AsyncStorage** – Local caching (optional)

---

## 🧪 Features in Progress
- Multi-language support
- citie list from API
- Admin dashboard for city/zone management

---

## 📁 Configuration
- Cities and zones are seeded via JSON files.
- Environment variables control API keys and endpoints.
- Setting up local Wi-Fi communication between server and mobile device (took a full day to stabilize).
- Overcoming GitHub's handling issues.

---

## 🚧 Status
This project is under active development. Contributions and feedback are welcome!

