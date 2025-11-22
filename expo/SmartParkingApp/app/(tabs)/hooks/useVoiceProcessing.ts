// useVoiceProcessing.ts
// Custom hook to manage voice processing and zone selection for the Smart Parking App
import { useState, Dispatch, SetStateAction } from 'react';
import { Audio } from 'expo-av';
import { NavigationProp } from '@react-navigation/native';
import { router } from 'expo-router';

type VoiceProcessingParams = {
    navigation: NavigationProp<any>;
    setShowEndMessage: Dispatch<SetStateAction<boolean>>;
    setIsDeciphering: Dispatch<SetStateAction<boolean>>; // ✅ Added this
};

export function useVoiceProcessing({ navigation, setShowEndMessage, setIsDeciphering }: VoiceProcessingParams) {
    const [cityName, setCityName] = useState('');
    const [textZones, setTextZones] = useState('');  // For displaying text zones info
    const [showDigitButtons, setShowDigitButtons] = useState(false);
    const [zoneNames, setZoneNames] = useState<string[]>([]);
    const [englishZoneNames, setEnglishZoneNames] = useState<string[]>([]);

    const startVoiceProcess = async (uri: string, endpoint: string) => {
        try {
            setIsDeciphering(true); // ✅ Show spinner

            const formData = new FormData();
            formData.append('file', { uri, name: 'voice_input.wav', type: 'audio/wav' } as any);
            console.log('🎙️ Sending voice data to:', endpoint);

            const response = await fetch(endpoint, {
                method: 'POST',
                headers: { 'Content-Type': 'multipart/form-data' },
                body: formData,
            });

            if (!response.ok) throw new Error(await response.text());

            const result = await response.json();
            const transcript = result.city || 'לא זוהתה עיר';

            const message = `זִיהִינו את העיר ${transcript}. אם זה נכון, הַקֵשׁ אישור. אם לא, הַקֵשׁ אֱמוֹר שׁוּב.`;
            await convertTextToSpeech(message);

            setCityName(transcript);
            setIsDeciphering(false); // ✅ Hide spinner
        } catch (err) {
            console.error('Error in startVoiceProcess:', err);
            setIsDeciphering(false); // ✅ Hide spinner on error
        }
    };
    // A service to convert text to speech and play the audio
    const convertTextToSpeech = async (message: string) => {
        try {
            const response = await fetch('http://192.168.1.2:5203/api/Parking/speak-the-message', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(message),
            });

            console.log('🔊 TTS status:', response.status);
            if (!response.ok) throw new Error(await response.text());

            const result = await response.json();
            const audioUrl = `http://192.168.1.2:5203${result.audio}`;
            const { sound } = await Audio.Sound.createAsync({ uri: audioUrl }, { shouldPlay: true });

            sound.setOnPlaybackStatusUpdate((status) => {
                if (status.isLoaded && status.didJustFinish) sound.unloadAsync();
            });
        } catch (err) {
            console.error('Error in convertTextToSpeech:', err);
        }
    };
    // Confirm the detected city and fetch parking zones. Invoked by pressing the Confirm(אישור) button.
    const Confirm = async () => {
        try {
            const response = await fetch('http://192.168.1.2:5203/api/Parking/validate-city', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(cityName),
            });

            if (!response.ok) {
                const errorData = await response.json();
                await convertTextToSpeech(errorData.message || 'שגיאה לא ידועה.');
                return;
            }
            const data = await response.json();
            //We need hebre zone names for user info of zone confirmation and english zone names for backend processing
            const rawZones = data.hebrewZones//.map((z: string) => {
            //    const match = z.match(/ל־(.+)$/);
            //    return match ? match[1] : z;
            //});

            const englishRawZones = data.zones//.map((z: string) => {
            //    const match = z.match(/to(.+)$/);
            //    return match ? match[1] : z;
            //});
            console.log('✅✅✅✅((((((((((((()))))))))))');
            setEnglishZoneNames(englishRawZones);
            setZoneNames(rawZones);
            await convertTextToSpeech(data.message);
            setTextZones(data.message);

            setTimeout(() => {
                setShowDigitButtons(true);
            }, 4000);
        } catch (err) {
            await convertTextToSpeech('אירעה שגיאה. אנא נסה שוב בעוד רגע.');
            console.error('Error in Confirm:', err);
        }
    };
    // Handle digit button presses for zone selection
    const HandleDigitPress = async (digit: number) => {
        if (digit === 0) {
            try {
                const cancelResponse = await fetch('http://192.168.1.2:5203/api/parking/cancel-session', {
                    method: 'POST',
                });

                if (!cancelResponse.ok) {
                    const errorText = await cancelResponse.text();
                    console.error('Failed to cancel session:', errorText);
                }

                await convertTextToSpeech('החניה בוטלה. חזור להתחלה.');
                setTextZones('');
                setShowDigitButtons(false);
                router.push('/');
            } catch (err) {
                console.error('Error cancelling session:', err);
            }
            return;
        }

        const selectedZone = zoneNames[digit - 1];
        const englishSelectedZone = englishZoneNames[digit - 1];

        if (!selectedZone) {
            await convertTextToSpeech('אזור לא חוקי. אנא נסה שוב.');
            return;
        }

        try {
            const response = await fetch('http://192.168.1.2:5203/api/Parking/speak-the-message', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(`בחרתָּ את האזור   .${selectedZone} .רישום החניה בוצע `),
            });

            if (!response.ok) throw new Error(await response.text());

            const result = await response.json();
            const audioUrl = `http://192.168.1.2:5203${result.audio}`;
            const { sound } = await Audio.Sound.createAsync({ uri: audioUrl });

            await sound.playAsync();

            setTimeout(async () => {
                await sound.unloadAsync();
                setTextZones('');
                setShowDigitButtons(false);

                const startSessionResponse = await fetch(
                    `http://192.168.1.2:5203/api/Parking/start-session?selectedZone=${englishSelectedZone}`,
                    { method: 'POST' }
                );

                if (!startSessionResponse.ok) {
                    const errorText = await startSessionResponse.text();
                    console.error('Failed to start session:', errorText);
                } else {
                    console.log('Parking session started successfully');
                }
            }, 5000);
        } catch (err) {
            console.error('Error playing zone name or starting session:', err);
            setTextZones('');
            setShowDigitButtons(false);
        }
    };
    // Stop the parking session and complete database operations adding EndTime
    const StopParkingSession = async () => {
        try {
            const response = await fetch('http://192.168.1.2:5203/api/parking/end-session', {
                method: 'POST',
            });

            if (!response.ok) {
                const errorText = await response.text();
                console.error('❌ Failed to end session:', errorText);
            } else {
                console.log('✅ Parking session ended successfully');
                setShowEndMessage(true);

                const ttsResponse = await fetch('http://192.168.1.2:5203/api/Parking/speak-the-message', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify('החניה הסתיימה, תודה שחניתם אוטו פארק'),
                });

                if (!ttsResponse.ok) throw new Error(await ttsResponse.text());

                const result = await ttsResponse.json();
                const audioUrl = `http://192.168.1.2:5203${result.audio}`;
                const { sound } = await Audio.Sound.createAsync({ uri: audioUrl });

                await sound.playAsync();

                setTimeout(() => {
                    setShowEndMessage(false);
                    navigation.navigate('index');
                }, 3000);
            }
        } catch (error) {
            console.error('⚠️ Error ending session:', error);
        }
    };

    return {
        cityName,
        textZones,
        showDigitButtons,
        startVoiceProcess,
        convertTextToSpeech,
        Confirm,
        HandleDigitPress,
        StopParkingSession,
    };
}
