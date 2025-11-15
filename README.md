# ParkingApp 🚗  
A voice-driven parking registration system 
with an ASP.NET Core backend and a React Native mobile client.
This project was built with extensive support from **AI Copilot**, which generated the code.  
My role has been to understand the code (in main parts),  
to design and refine the system — making architectural decisions,  
improving the flow, and shaping the user experience.
⚠️ **Documentation and final polish are still in progress.**

Development is done using Visual Studio 22 as the primary IDE.
---

## 🧭 User Flow
1. **Parking Registration**  
   - The app registers the parking session.  
   - Optionally stores session data in Redis for fast access.

2. **Welcome Prompt**  
   - The app plays a short welcome message using Google Cloud Text-to-Speech (TTS).

3. **Speech Recording**  
   - The user records their voice using Expo Audio APIs.  
   - The audio is processed and converted (via FFmpeg).

4. **Speech-to-Text Conversion**  
   - The recorded audio is sent to Google Cloud Speech-to-Text (STT).  
   - The transcribed city name is extracted and sent to the mobile.
       
5. **User Confirmation**  
   - The app gets back the detected city name and sends back, using TTS (Text To Speech), to the user for confirmation.  
   - The user confirms or corrects the city name.

6. **City Name Validation**  
   - The backend checks if the city exists in the Redis/MongoDB database.  
   - If needed, the name is translated using Google Cloud Translate.

7. **Zone Selection**  
   - If the city is valid, the app fetches available parking zones.  
   - The user selects a zone via the mobile UI.

8. **Confirmation Playback**  
   - A final confirmation message is played using TTS.

9. **Session Completion**  
   - The parking session is completed and stored in the database including ending session process.

---

## 📊 Dashboard
The admin dashboard provides:
- **Main Parking Table** – all sessions: city, zone, start/end, etc.  
- **Monthly Summary (per driver)** – aggregated data: sessions, hours, email, etc.  
- **Driver Details Table** – breakdown of each driver’s sessions.  
- **Charts (Chart.js)** – diagrams.  



# 🛠 Tools & Technologies

## 🌐 Server Side
- **ASP.NET Core MVC** – RESTful API  
- **C#**  
- **Redis** – Optional caching layer  

## 🗄 Database
- **MongoDB / MongoDB Compass** – Data of: car_owners, cities, city_zone_rates, parking_sessions, etc.

## 📱 Client Side (Mobile)
- **TypeScript**  
- **React Native** – Cross‑platform mobile app  
- **JSX**  
- **Fetch API (AJAX)** – Communication with backend  
- **HTML**  
- **JavaScript**  

## 🖥 Editing & Execution
- **Visual Studio 22**  
- **CLI**  
- **Expo / Expo Go** – Simplified development, linking mobile to computer and execution  

## ⚙️ Additional Services & Utilities
- **Google Cloud APIs**:  
  - Text‑to‑Speech (TTS)  
  - Speech‑to‑Text (STT)  
  - Translate API  
- **FFmpeg**  
- **Chart.js**  
- **Ngrok**  
- **GitHub**

## 🚀 Deployment / DevOps
- **Docker** - For hosting Redis locally 

### 📌 Notes
- **TTS / STT** – Services for converting speech to text and vice versa.  
- **FFmpeg** – Software for audio format conversion and decompression.  
- **Ngrok** – Provides a public URL for local server applications.  
- **Expo Go** – Connects mobile devices to the app’s computer for testing and execution.  
- **Chart.js** – Library for rendering charts and graphs in the app.

---

## 🧪 Features and Future Development Options
- Connecting to Twilio for real-time voice calls  
- Multi-language support  
- Cities/zones list from API  
- Complete parking session management  
- Full admin dashboard for city/zone management  

---

## 📁 Setup and Issues
- Cities and zones are seeded via JSON files.  
- Environment variables control API keys and endpoints.  
- Setting up local Wi-Fi communication between server and mobile device (took a full day to stabilize).  
- Overcoming GitHub's handling issues.  
