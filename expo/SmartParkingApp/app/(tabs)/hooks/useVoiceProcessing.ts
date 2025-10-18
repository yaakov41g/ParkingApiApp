import { useState } from 'react';
import { Audio } from 'expo-av';

export function useVoiceProcessing() {
    const [cityName, setCityName] = useState('');
    const [cityStatus, setCityStatus] = useState('');

    const startVoiceProcess = async (uri: string, endpoint: string) => {
        try {
            const formData = new FormData();
            formData.append('file', { uri, name: 'voice_input.wav', type: 'audio/wav' } as any);

            const response = await fetch(endpoint, { method: 'POST', headers: { 'Content-Type': 'multipart/form-data' }, body: formData });
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
            //const responseText = await response.text();
            //console.log('TTS response:', responseText);
            console.log('TTS status:', response.status);

            if (!response.ok) throw new Error(await response.text());

            const result = await response.json();
            const audioUrl = `http://192.168.1.2:5203${result.audio}`;
            const { sound } = await Audio.Sound.createAsync({ uri: audioUrl }, { shouldPlay: true });

            sound.setOnPlaybackStatusUpdate((status) => {
                if (status.isLoaded && status.didJustFinish) sound.unloadAsync();
            });
        }  catch (err: any) {
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
                await convertTextToSpeech(errorData.message || "שגיאה לא ידועה.");
                return;
            }

            const data = await response.json();
            setCityStatus(data.message);
            await convertTextToSpeech(data.message);
            setTimeout(() => setCityStatus(''), 3000);
        } catch (err) {
            await convertTextToSpeech("אירעה שגיאה. אנא נסה שוב בעוד רגע.");
            console.error('Error in Confirm:', err);
        }
    };

    return { cityName, cityStatus, startVoiceProcess, convertTextToSpeech, Confirm };
}
