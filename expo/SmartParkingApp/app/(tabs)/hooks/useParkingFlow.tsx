import { useState } from 'react';
import { Audio } from 'expo-av';

export function useParkingFlow() {
    const [cityName, setCityName] = useState('');
    const [cityStatus, setCityStatus] = useState('');
    const [endpoint, setEndpoint] = useState('');
    const [recording, setRecording] = useState<Audio.Recording | null>(null);
    const [isIntroPlaying, setIsIntroPlaying] = useState(false);

    const startParkingFlow = async () => {
        try {
            setIsIntroPlaying(true);
            const response = await fetch('http://192.168.1.2:5203/api/Parking/welcome');
            if (!response.ok) {
                console.error('Server error:', await response.text());
                setIsIntroPlaying(false);
                return;
            }

            const result = await response.json();
            const audioUrl = `http://192.168.1.2:5203${result.audio}`;
            const nextEndpoint = result.next;
            const { sound } = await Audio.Sound.createAsync({ uri: audioUrl });

            sound.setOnPlaybackStatusUpdate(async (status) => {
                if (status.isLoaded && status.didJustFinish) {
                    await sound.unloadAsync();
                    setIsIntroPlaying(false);
                    setEndpoint(`http://192.168.1.2:5203${nextEndpoint}`);
                    startRecording();
                }
            });

            await sound.playAsync();
        } catch (err) {
            console.error('Error in process:', err);
            setIsIntroPlaying(false);
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
            setEndpoint('http://192.168.1.2:5203/api/Parking/listen-city');
            if (uri) {
                sendToBackend(uri, endpoint);
            }
            setRecording(null);
        } catch (err) {
            console.error('Error at recording stop:', err);
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

            const response = await fetch(endpoint, {
                method: 'POST',
                headers: { 'Content-Type': 'multipart/form-data' },
                body: formData,
            });

            if (!response.ok) {
                console.error('Server Error:', await response.text());
                return;
            }

            const result = await response.json();
            const transcript = result.city || 'לא זוהתה עיר';
            const message = `זִיהִינו את העיר ${transcript}. אם זה נכון, הַקֵשׁ אישור. אם לא, הַקֵשׁ אֱמוֹר שׁוּב.`;
            sendToTTS(message);
            setCityName(transcript);
        } catch (err) {
            console.error('Error while sending the voice file:', err);
        }
    };

    const sendToTTS = async (message: string) => {
        try {
            const response = await fetch('http://192.168.1.2:5203/api/Parking/speak-city', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(message),
            });

            if (!response.ok) {
                console.error('TTS Server Error:', await response.text());
                return;
            }

            const result = await response.json();
            const audioUrl = `http://192.168.1.2:5203${result.audio}`;
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
                body: JSON.stringify(cityName),
            });

            if (!response.ok) {
                const errorData = await response.json();
                const voiceMessage = errorData.message || "שגיאה לא ידועה.";
                await sendToTTS(voiceMessage);
                return;
            }

            const data = await response.json();
            setCityStatus(data.message);
            await sendToTTS(data.message);
            setTimeout(() => setCityStatus(''), 3000);
        } catch (err) {
            await sendToTTS("אירעה שגיאה. אנא נסה שוב בעוד רגע.");
            console.error('Network error', err);
        }
    };

    return {
        cityName,
        cityStatus,
        isIntroPlaying,
        startParkingFlow,
        stopRecording,
        startRecording,
        confirmCity,
    };
}
