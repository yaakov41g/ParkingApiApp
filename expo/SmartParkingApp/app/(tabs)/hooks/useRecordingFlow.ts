// useRecordingFlow.ts

import { useRef, useState } from 'react';
import { Audio } from 'expo-av';
import { SilenceDetector } from '../Utilities/SilenceDetector';
import { recordingOptions } from '../Utilities/recordingOptions';
import { useRouter } from 'expo-router';

export function useRecordingFlow(onRecordingComplete: (uri: string, endpoint: string) => void) {
    const [isIntroPlaying, setIsIntroPlaying] = useState(false);
    const [isDeciphering, setIsDeciphering] = useState(false);
    const recordingRef = useRef<Audio.Recording | null>(null);
    const endpointRef = useRef('');
    const router = useRouter();

    const startParkingFlow = async () => {
        try {
            setIsIntroPlaying(true);
            const response = await fetch('http://192.168.1.2:5203/api/Parking/welcome');
            if (!response.ok) throw new Error(await response.text());

            const result = await response.json();
            const nextEndpoint = result.next;
            const isRegistered = result.isRegistered;

            if (!result.audio || typeof result.audio !== 'string' || !result.audio.trim()) {
                console.error('❌ Invalid audio path received:', result.audio);
                throw new Error('Invalid audio path received from server.');
            }

            const audioPath = result.audio.trim();
            const audioUrl = audioPath.startsWith('http')
                ? audioPath
                : `http://192.168.1.2:5203${audioPath}`;

            console.log('####### Endpoint :', nextEndpoint);
            console.log('🎧 Final audio URL:', audioUrl);
            console.log('👤 Registered:', isRegistered);

            const { sound } = await Audio.Sound.createAsync({ uri: audioUrl });

            sound.setOnPlaybackStatusUpdate(async (status) => {
                if (status.isLoaded && status.didJustFinish) {
                    await sound.unloadAsync();
                    setIsIntroPlaying(false);

                    if (!isRegistered) {
                        router.replace('/(tabs)/SignUpScreen');
                        return;
                    }

                    const fullEndpoint = `http://192.168.1.2:5203${nextEndpoint}`;
                    endpointRef.current = fullEndpoint;
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
            await Audio.setAudioModeAsync({
                allowsRecordingIOS: true,
                playsInSilentModeIOS: true,
            });

            const newRecording = new Audio.Recording();
            await newRecording.prepareToRecordAsync(recordingOptions);
            await newRecording.startAsync();
            recordingRef.current = newRecording;

            SilenceDetector(recordingRef.current, () => {
                stopRecording();
            });
        } catch (err) {
            console.error('Error at recording start:', err);
        }
    };

    const stopRecording = async () => {
        try {
            const activeRecording = recordingRef.current;
            if (!activeRecording) return;

            await activeRecording.stopAndUnloadAsync();
            const uri = activeRecording.getURI();

            if (uri) {
                onRecordingComplete(uri, endpointRef.current);
            }

            recordingRef.current = null;
            setIsDeciphering(true); // ✅ Trigger spinner
        } catch (err) {
            console.error('Error at recording stop:', err);
        }
    };

    return {
        isIntroPlaying,
        isDeciphering,
        setIsDeciphering,
        startParkingFlow,
        startRecording,
        stopRecording,
    };
}
