import { useState } from 'react';
import { Audio } from 'expo-av';

export function useRecordingFlow(onRecordingComplete: (uri: string, endpoint: string) => void) {
    const [isIntroPlaying, setIsIntroPlaying] = useState(false);
    const [recording, setRecording] = useState<Audio.Recording | null>(null);
    const [endpoint, setEndpoint] = useState('');

    const startParkingFlow = async () => {
        try {
            setIsIntroPlaying(true);
            const response = await fetch('http://192.168.1.2:5203/api/Parking/welcome');
            if (!response.ok) throw new Error(await response.text());

            const result = await response.json();
            const audioUrl = `http://192.168.1.2:5203${result.audio}`;
            const nextEndpoint = result.next;
            console.log('####### Endpoint :', nextEndpoint); 
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
            console.error('Error in startParkingFlow:', err);
            setIsIntroPlaying(false);
        }
    };

    const startRecording = async () => {
        try {
            await Audio.requestPermissionsAsync();
            await Audio.setAudioModeAsync({ allowsRecordingIOS: true, playsInSilentModeIOS: true });

            const newRecording = new Audio.Recording();
            await newRecording.prepareToRecordAsync();
            await newRecording.startAsync();
            setRecording(newRecording);
        } catch (err) {
            console.error('Error at recording start:', err);
        }
    };

    const stopRecording = async () => {
        try {
            if (!recording) return;
            await recording.stopAndUnloadAsync();
            const uri = recording.getURI();
            if (uri) onRecordingComplete(uri, endpoint);
            setRecording(null);
        } catch (err) {
            console.error('Error at recording stop:', err);
        }
    };

    return { isIntroPlaying, startParkingFlow, startRecording, stopRecording };
}
