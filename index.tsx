import React, { useState } from 'react';
import { View, Text,/* Modal,*/ TouchableOpacity, AppRegistry } from 'react-native';
import { Audio } from 'expo-av';
import appConfig from '../../app.json';
import styles from './styles';

const appName = appConfig.expo.name;

export default function VoiceCityRecognizer() {
    const [recording, setRecording] = useState<Audio.Recording | null>(null);
    //const [cityName, setCityName] = useState('');
    //const [modalVisible, setModalVisible] = useState(false);
    //console.log("##################" );

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
            console.log("################## AudioUrl", audioUrl);

            await sound.playAsync();
            sound.setOnPlaybackStatusUpdate((status) => {
                if (status.isLoaded && status.didJustFinish) {
                    sound.unloadAsync();
                }
            });

            await sound.playAsync();
            await sound.setOnPlaybackStatusUpdate(async (status) => {
                if (status.isLoaded && status.didJustFinish) {
                    await sound.unloadAsync(); // Unload sound to free resources
                    // Step 3: Start recording after welcome finishes
                    startRecording(`http://192.168.1.2:5203${nextEndpoint}`);
                }
            });
        } catch (err) {
            console.error('Error in process:', err);
        }
    };

    const startRecording = async (endpoint: string) => {
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
            (newRecording as any).endpoint = endpoint;
        } catch (err) {
            console.error('Error at recording start:', err);
        }
    };

    const stopRecording = async () => {
        try {
            if (!recording) return;
            await recording.stopAndUnloadAsync();
            const uri = recording.getURI();
            const endpoint = (recording as any).endpoint || 'http://192.168.1.2:5203/api/Parking/listen-city';
            if (uri) {
                sendToBackend(uri, endpoint);
            }
            setRecording(null);
        } catch (err) {
            console.error('Error  at recording stop:', err);
        }
    };

    const sendToBackend = async (uri: string, endpoint: string) => {
        try {
            console.log("################## Uri :", uri)
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
            console.log('Google status:', JSON.stringify(result, null, 2));
            const transcript = result.city || 'לא זוהתה עיר';
            //setCityName(transcript);
            //setModalVisible(true);
            sendToTTS(transcript); 
        } catch (err) {
            console.error('Error while sending the voice file:', err);
        }
    };//sendToBackend

    // Send city name to backend for TTS conversion
    const sendToTTS = async (text: string) => {
        try {
            const response = await fetch('https://your-backend.com/tts', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    text: `זיהינו את העיר ${text}. אם זה נכון, הקש 1.`,
                    languageCode: 'he-IL', // Hebrew language
                    voiceName: 'he-IL-Wavenet-A', // Optional: specific Hebrew voice
                }),
            });

            const result = await response.json();
            const audioUrl = result.audio; // URL of generated voice file

            // Play the audio using expo-av
            const { sound } = await Audio.Sound.createAsync(
                { uri: audioUrl },
                { shouldPlay: true }
            );

            // Unload sound after playback finishes
            sound.setOnPlaybackStatusUpdate((status) => {
                if (status.isLoaded && status.didJustFinish) {
                    sound.unloadAsync();
                }
            });

        } catch (err) {
            console.error('Error sending text to TTS:', err);
        }
    };


    return (
        <View style={styles.container}>
            <TouchableOpacity style={styles.bigButton} onPress={startParkingFlow}>
                <Text style={styles.buttonText}>Start Parking</Text>
            </TouchableOpacity>

            <TouchableOpacity style={styles.bigButton} onPress={stopRecording}>
                <Text style={styles.buttonText}>עצור ושלח לשרת</Text>
            </TouchableOpacity>
        </View>
    );
}

//const styles = StyleSheet.create({
//    container: { flex: 1, justifyContent: 'center', alignItems: 'center' },
//    modal: { backgroundColor: 'white', padding: 20, margin: 40, borderRadius: 10 },
//});

AppRegistry.registerComponent(appName, () => VoiceCityRecognizer);

//A web version

//import React, { useState } from 'react';
//import { View, Button, Modal, Text, StyleSheet } from 'react-native';

//// Custom types for Web Speech API
//interface SpeechRecognitionEvent extends Event {
//    results: SpeechRecognitionResultList;
//}

//interface SpeechRecognitionErrorEvent extends Event {
//    error: string;
//    message: string;
//}
//navigator.mediaDevices.getUserMedia({ audio: true })
//    .then(stream => console.log('🎤 Mic access granted'))
//    .catch(err => console.error('🚫 Mic access denied:', err));
//export default function StartParkingScreen() {
//    const [cityName, setCityName] = useState('');
//    const [modalVisible, setModalVisible] = useState(false);

//    const playWelcomeMessage = async () => {
//        try {
//            const response = await fetch('http://localhost:5203/api/parking/welcome');
//            const data = await response.json();

//            const audioUrl = `http://localhost:5203${data.audio}`;
//            const audio = new window.Audio(audioUrl);
//            audio.crossOrigin = 'anonymous';
//            audio.load();

//            audio.onended = () => {
//                startSpeechRecognition();
//            };

//            audio.play().catch((err) => {
//                console.error('Playback failed:', err);
//            });
//        } catch (error) {
//            console.error('Error playing welcome message:', error);
//        }
//    };

//    const startSpeechRecognition = () => {
//        const SpeechRecognition = (window as any).SpeechRecognition || (window as any).webkitSpeechRecognition;
//        debugger;

//        if (!SpeechRecognition) {
//            console.error('Speech Recognition not supported in this browser.');
//            return;
//        }
//        console.log('SpeechRecognition: ' + SpeechRecognition);

//        const recognition = new SpeechRecognition();
//        recognition.lang = 'he-IL';
//        recognition.interimResults = false;
//        recognition.maxAlternatives = 1;
//        debugger;
//        //console.log('Recognized city name: ' + recognition);
//        //recognition.onaudiostart = () => console.log('🎧 Audio capturing started');
//        //recognition.onsoundstart = () => console.log('🔊 Sound detected');
//        //recognition.onspeechstart = () => console.log('🗣️ Speech detected');
//        //recognition.onspeechend = () => console.log('🛑 Speech ended');
//        //recognition.onsoundend = () => console.log('🔇 Sound stopped');
//        //recognition.onend = () => console.log('📴 Recognition ended');

//        recognition.onresult = async (event: SpeechRecognitionEvent) => {
//            const spokenText = event.results[0][0].transcript;
//            console.log('Recognized city name:', spokenText);
//            setCityName(spokenText);
//            setModalVisible(true);

//            try {
//                const response = await fetch('http://localhost:5203/api/parking/listen-city', {
//                    method: 'POST',
//                    headers: { 'Content-Type': 'application/json' },
//                    body: JSON.stringify({ city: spokenText }),
//                });

//                const result = await response.json();
//                console.log('Backend response:', result);
//            } catch (error) {
//                console.error('Error sending city name:', error);
//            }
//        };

//        recognition.onerror = (event: SpeechRecognitionErrorEvent) => {
//            if (event.error === 'no-speech') {
//                console.warn('No speech detected. Please try again.');
//                alert('לא זוהתה דיבור. נסה שוב.');
//            } else {
//                console.error('Speech recognition error:', event.error);
//                alert(`שגיאה בזיהוי דיבור: ${event.error}`);
//            }
//        };

//        recognition.start();
//    };

//    return (
//        <View style={styles.container}>
//            <Button title="Start Parking" onPress={playWelcomeMessage} />

//            <Modal
//                visible={modalVisible}
//                transparent={true}
//                animationType="fade"
//                onRequestClose={() => setModalVisible(false)}
//            >
//                <View style={styles.modalOverlay}>
//                    <View style={styles.modalContent}>
//                        <Text style={styles.modalText}>Recognized City: {cityName}</Text>
//                        <Button title="Close" onPress={() => setModalVisible(false)} />
//                    </View>
//                </View>
//            </Modal>
//        </View>
//    );
//}

//const styles = StyleSheet.create({
//    container: {
//        flex: 1,
//        justifyContent: 'center',
//        alignItems: 'center',
//    },
//    modalOverlay: {
//        flex: 1,
//        backgroundColor: 'rgba(0,0,0,0.5)',
//        justifyContent: 'center',
//        alignItems: 'center',
//        position: 'absolute',
//        top: 0,
//        left: 0,
//        right: 0,
//        bottom: 0,
//        zIndex: 999,
//    },
//    modalContent: {
//        backgroundColor: '#fff',
//        padding: 24,
//        borderRadius: 8,
//        elevation: 10,
//        zIndex: 1000,
//        alignItems: 'center',
//    }
//,
//    modalText: {
//        fontSize: 18,
//        marginBottom: 12,
//    },
//});
