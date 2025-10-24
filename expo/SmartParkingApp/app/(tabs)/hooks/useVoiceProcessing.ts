import { useState } from 'react';
import { Audio } from 'expo-av';
import { NavigationProp } from '@react-navigation/native';

type VoiceProcessingParams = {
    navigation: NavigationProp<any>;
    setShowEndMessage: (value: boolean) => void;
};

export function useVoiceProcessing({ navigation, setShowEndMessage }: VoiceProcessingParams) {
    const [cityName, setCityName] = useState('');
    const [cityStatus, setCityStatus] = useState('');
    const [showDigitButtons, setShowDigitButtons] = useState(false);
    const [zoneNames, setZoneNames] = useState<string[]>([]);

    const startVoiceProcess = async (uri: string, endpoint: string) => {
        try {
            const formData = new FormData();
            formData.append('file', { uri, name: 'voice_input.wav', type: 'audio/wav' } as any);
            console.log('!!!!!!!!!!!!!!!!!Sending voice data to:', endpoint);

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
        } catch (err) {
            console.error('Error in startVoiceProcess:', err);
        }
    };

    const convertTextToSpeech = async (message: string) => {
        try {
            const response = await fetch('http://192.168.1.2:5203/api/Parking/speak-the-message', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(message),
            });

            console.log('TTS status:', response.status);
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
            setCityStatus(data.message);

            const rawZones = data.zones.map((z: string) => {
                const match = z.match(/ל־(.+)$/);
                return match ? match[1] : z;
            });

            setZoneNames(rawZones);
            await convertTextToSpeech(data.message);
            setShowDigitButtons(true);
        } catch (err) {
            await convertTextToSpeech('אירעה שגיאה. אנא נסה שוב בעוד רגע.');
            console.error('Error in Confirm:', err);
        }
    };

    const HandleDigitPress = async (digit: number) => {
        const selectedZone = zoneNames[digit - 1];

        if (!selectedZone) {
            await convertTextToSpeech('אזור לא חוקי. אנא נסה שוב.');
            return;
        }

        try {
            const response = await fetch('http://192.168.1.2:5203/api/Parking/speak-the-message', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(`בחרתָּ את האזור   .${selectedZone} .רישום החניה בוצע`),
            });

            if (!response.ok) throw new Error(await response.text());

            const result = await response.json();
            const audioUrl = `http://192.168.1.2:5203${result.audio}`;
            const { sound } = await Audio.Sound.createAsync({ uri: audioUrl });

            await sound.playAsync();

            setTimeout(async () => {
                await sound.unloadAsync();
                setCityStatus('');
                setShowDigitButtons(false);

                const startSessionResponse = await fetch(
                    `http://192.168.1.2:5203/api/Parking/start-session?selectedZone=${selectedZone}`,
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
            setCityStatus('');
            setShowDigitButtons(false);
        }
    };

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

                // ✅ Send TTS request
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
                    navigation.navigate('index'); // or your actual route name
                }, 3000);
            }
        } catch (error) {
            console.error('⚠️ Error ending session:', error);
        }
    };
    return {
        cityName,
        cityStatus,
        showDigitButtons,
        startVoiceProcess,
        convertTextToSpeech,
        Confirm,
        HandleDigitPress,
        StopParkingSession,
    };
}
