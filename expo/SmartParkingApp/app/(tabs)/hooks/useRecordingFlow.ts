// demoImport.ts

import { useRef, useState } from 'react';
import { Audio } from 'expo-av';
import { SilenceDetector } from '../Utilities/SilenceDetector';
import { recordingOptions } from '../Utilities/recordingOptions';

export function useRecordingFlow(onRecordingComplete: (uri: string, endpoint: string) => void) {
    const [isIntroPlaying, setIsIntroPlaying] = useState(false);
    const recordingRef = useRef<Audio.Recording | null>(null);
    //const [recording, setRecording] = useState<Audio.Recording | null>(null);
    const endpointRef = useRef('');
    //const [endpoint, setEndpoint] = useState('');

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
                    const fullEndpoint = `http://192.168.1.2:5203${nextEndpoint}`;
                    //setEndpoint(fullEndpoint);
                    endpointRef.current = fullEndpoint; 
                   // console.log('!!!!!!!!!!!!!!!!!Sending voice data to:', endpoint);
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
            //setRecording(newRecording);

            // Auto-stop when silence is detected
            SilenceDetector(recordingRef.current, () => {

                stopRecording(); // You can also add a toast or log here
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
        } catch (err) {
            console.error('Error at recording stop:', err);
        }
    };
    return { isIntroPlaying, startParkingFlow, startRecording, stopRecording };
}
