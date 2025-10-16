import React, { useState } from 'react';
import { View, Text,/* Modal,*/ TouchableOpacity, AppRegistry } from 'react-native';
import { Audio } from 'expo-av';
import appConfig from '../../app.json';
import styles from './styles';
//import { transliterate } from 'hebrew-transliteration';
const appName = appConfig.expo.name;

export default function VoiceCityRecognizer() {
    const [cityNane, setTransc] = useState(''); // Store recognized city name //amazing to see how copilot suggests this.
    const [cityStatus, setCityStatus] = useState('');// Store status of city validation 
    const [endPoint, setEndpoint] = useState(''); // Store recognized city name //amazing to see how copilot suggests this.
    const [recording, setRecording] = useState<Audio.Recording | null>(null);

    const startParkingFlow = async () => {
        try {
            // Step 1: Fetch welcome audio and next endpoint
            const response = await fetch('http://192.168.1.2:5203/api/Parking/welcome');

            if (!response.ok) {
                const errorText = await response.text();
                console.error('########### Server error:', errorText);
                return; // Stop the flow if server failed
            }
            const result = await response.json();// here we get { audio: string, next: string } 
            const audioUrl = `http://192.168.1.2:5203${result.audio}`; // full URL to audio
            const nextEndpoint = result.next;
            // Step 2: Play welcome audio
            const { sound } = await Audio.Sound.createAsync({ uri: audioUrl });
            // Set up the listener first
            sound.setOnPlaybackStatusUpdate(async (status) => {
                if (status.isLoaded && status.didJustFinish) {
                    await sound.unloadAsync();
                    setEndpoint(`http://192.168.1.2:5203${nextEndpoint}`)
                    startRecording();
                }
            });
            // Then start playback
            await sound.playAsync();
        } catch (err) {
            console.error('Error in process:', err);
        }
    };

    const startRecording = async () => {
        try {
            await Audio.requestPermissionsAsync();
            await Audio.setAudioModeAsync({ allowsRecordingIOS: true, playsInSilentModeIOS: true });

            const newRecording = new Audio.Recording();
            await newRecording.prepareToRecordAsync({
                android: {
                    extension: '.wav',
                    outputFormat: 1,
                    audioEncoder: 1,
                    sampleRate: 44100,
                    numberOfChannels: 1,
                    bitRate: 128000,
                },
                ios: {
                    extension: '.wav',
                    audioQuality: 2,
                    sampleRate: 44100,
                    numberOfChannels: 1,
                    bitRate: 128000,
                    linearPCMBitDepth: 16,
                    linearPCMIsBigEndian: false,
                    linearPCMIsFloat: false,
                },
                web: {
                    mimeType: 'audio/webm',
                    bitsPerSecond: 128000,
                },
            });
            await newRecording.startAsync();
            setRecording(newRecording);
            // Save endpoint for later
            (newRecording as any).endpoint = endPoint;//Why is this needed? don't know.
        } catch (err) {
            console.error('Error at recording start:', err);
        }
    };

    const stopRecording = async () => {
        try {
            if (!recording) return;
            await recording.stopAndUnloadAsync();
            const uri = recording.getURI();// local file uri
            setEndpoint('http://192.168.1.2:5203/api/Parking/listen-city');//
            if (uri) {
                sendToBackend(uri, endPoint);
            }
            setRecording(null);
        } catch (err) {
            console.error('Error  at recording stop:', err);
        }
    };

    const sendToBackend = async (uri: string, endpoint: string) => {
        try {
            const formData = new FormData();
            formData.append('file', {
                uri,
                name: 'voice_input.wav',
                type: 'audio/wav',
            } as any);

            const response = await fetch(endpoint, {//send user voice file to the backend
                method: 'POST',
                headers: {
                    'Content-Type': 'multipart/form-data',
                },
                body: formData,
            });
            if (!response.ok) {
                const text = await response.text();
                console.error('Server Error:', text);
                return;
            }
            const result = await response.json();
            const transcript = result.city || 'לא זוהתה עיר';
            const message = `זִיהִינו את העיר ${transcript}. אם זה נכון, הַקֵשׁ אישור. אם לא, הַקֵשׁ אֱמוֹר שׁוּב.`;
            sendToTTS(message);
            setTransc(transcript);//again, amazing how copilot suggests this.
        } catch (err) {
            console.error('Error while sending the voice file:', err);
        }
    };//sendToBackend

    // Send city name to backend for TTS conversion
    const sendToTTS = async (message: string) => {
        try {
            const response = await fetch('http://192.168.1.2:5203/api/Parking/speak-city', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(message), // Send raw string as JSON
            });

            if (!response.ok) {
                const errorText = await response.text();
                console.error('TTS Server Error:', errorText);
                return;
            }

            const result = await response.json();
            const audioUrl = `http://192.168.1.2:5203${result.audio}`;// /TTS/audio/filename.wav
            const { sound } = await Audio.Sound.createAsync({ uri: audioUrl }, { shouldPlay: true });
            sound.setOnPlaybackStatusUpdate((status) => {
                if (status.isLoaded && status.didJustFinish) {
                    sound.unloadAsync();
                }
            });

        } catch (err) {
            console.error('Error sending city name to TTS:', err);
        }
    };

    const confirmCity = async () => {
        try {
            const response = await fetch('http://192.168.1.2:5203/api/Parking/validate-city', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(cityNane),
            });

            if (!response.ok) {
                const errorData = await response.json();
                const voiceMessage = errorData.message || "שגיאה לא ידועה.";
                await sendToTTS(voiceMessage); // Send to Google TTS
                return;
            }

            const data = await response.json();
            setCityStatus(data.message);
            await sendToTTS(data.message); // Confirm city found   //VVVVVVVVVV
            setTimeout(() => setCityStatus(''), 3000);
        } catch (err) {
            await sendToTTS("אירעה שגיאה. אנא נסה שוב בעוד רגע.");
            console.error('Network error', err);
        }
    };

    return (
        <View style={styles.container}>
            <TouchableOpacity style={styles.bigButton} onPress={startParkingFlow}>
                <Text style={styles.squareButtonText}>Start Parking</Text>
            </TouchableOpacity>

            <TouchableOpacity style={styles.bigButton} onPress={stopRecording}>
                <Text style={styles.squareButtonText}>עצור ושלח לשרת</Text>
            </TouchableOpacity>

            {/* New Buttons */}
            <TouchableOpacity style={styles.confirmButton} onPress={confirmCity}>
                <Text style={styles.squareButtonText}>1 - אישור</Text>
            </TouchableOpacity>
            <TouchableOpacity style={styles.repeatButton} onPress={startRecording}>
                <Text style={styles.squareButtonText}>2 - אמור שוב</Text>
            </TouchableOpacity>
            {cityStatus !== '' && (
                <Text style={styles.statusText}>{cityStatus}</Text>
            )}
        </View>
    );
}

AppRegistry.registerComponent(appName, () => VoiceCityRecognizer);

