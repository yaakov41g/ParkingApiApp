// ParkingFlow.tsx (VoiceCityRecognizer.tsx)
// Main and full implementation of the parking flow with voice recognition
import React, { useState } from 'react';
import { View, Text, Linking, TouchableOpacity, AppRegistry, ActivityIndicator } from 'react-native';
import { useNavigation, NavigationProp } from '@react-navigation/native';
import { Video, ResizeMode } from 'expo-av';

import { useRecordingFlow } from './hooks/useRecordingFlow';
import { useVoiceProcessing } from './hooks/useVoiceProcessing';
import appConfig from '../../app.json';
import styles from './styles';

const appName = appConfig.expo.name;

export default function VoiceCityRecognizer() { // I need to change the name here to ParkingFlow

    const navigation = useNavigation<NavigationProp<any>>();
    const [isDeciphering, setIsDeciphering] = useState(false);

    const [showEndMessage, setShowEndMessage] = useState(false);
    const {
        cityStatus,
        showDigitButtons,
        startVoiceProcess,
        Confirm,
        HandleDigitPress,
        StopParkingSession
    } = useVoiceProcessing({ navigation, setShowEndMessage, setIsDeciphering }); // ✅ now receives it correctly
    const {
        isIntroPlaying,
        startParkingFlow,
        startRecording
    } = useRecordingFlow(startVoiceProcess); // ✅ must come after declaration



    return (
        <View style={styles.mainContainer}>
            {showEndMessage ? (
                <View style={{ flex: 1, justifyContent: 'center', alignItems: 'center', backgroundColor: '#fff' }}>
                    <Text style={{ fontSize: 28, fontWeight: 'bold', color: '#007AFF', textAlign: 'center', padding: 20 }}>
                        החניה הסתיימה{'\n'}תודה שחניתם אוטו פארק
                    </Text>
                </View>
            ) : showDigitButtons ? (
                <View style={styles.digitGrid}>
                    {[1, 2, 3, 4, 5, 6, 7, 8, 9].reduce((rows, digit, index) => {
                        if (index % 3 === 0) rows.push([]);
                        rows[rows.length - 1].push(digit);
                        return rows;
                    }, [] as number[][]).map((row, i) => (
                        <View key={i} style={styles.digitRow}>
                            {row.map((digit) => (
                                <TouchableOpacity key={digit} style={styles.digitButton} onPress={() => HandleDigitPress(digit)}>
                                    <Text style={styles.digitText}>{digit}</Text>
                                </TouchableOpacity>
                            ))}
                        </View>
                    ))}
                    <View style={styles.digitRow}>
                        <View style={{ width: 60 }} />
                        <TouchableOpacity style={styles.digitButton} onPress={() => HandleDigitPress(0)}>
                            <Text style={styles.digitText}>0</Text>
                        </TouchableOpacity>
                        <View style={{ width: 60 }} />
                    </View>
                </View>
            ) : (
                <>
                    <TouchableOpacity style={styles.startParkingButton} onPress={startParkingFlow}>
                        <Text style={styles.squareButtonText}>התחל חניה</Text>
                    </TouchableOpacity>

                    <TouchableOpacity style={styles.confirmButton} onPress={Confirm}>
                        <Text style={styles.squareButtonText}>אישור</Text>
                    </TouchableOpacity>

                    <TouchableOpacity style={styles.repeatButton} onPress={startRecording}>
                        <Text style={styles.squareButtonText}>אמור שוב</Text>
                    </TouchableOpacity>

                    <TouchableOpacity style={styles.stopParkingButton} onPress={StopParkingSession}>
                        <Text style={styles.squareButtonText}>סיים חניה</Text>
                    </TouchableOpacity>

                    {cityStatus !== '' && <Text style={styles.statusText}>{cityStatus}</Text>}

                    <View style={{ position: 'absolute', bottom: 20, width: '100%', alignItems: 'center' }}>
                        {isIntroPlaying && (
                            <>
                                <Video
                                    source={require('../../assets/gifs/Audio_Wave.mp4')}
                                    rate={1.0}
                                    volume={1.0}
                                    isMuted={false}
                                    resizeMode={ResizeMode.CONTAIN}
                                    shouldPlay
                                    isLooping
                                    style={{ width: 120, height: 120 }}
                                />
                                <TouchableOpacity onPress={() => Linking.openURL('https://iconscout.com/lottie-animations/audio-wave')}>
                                    <Text style={{ textDecorationLine: 'underline', fontSize: 12 }}>
                                        Audio Wave by MD. MURADUZZAMAN
                                    </Text>
                                </TouchableOpacity>
                            </>
                        )}
                    </View>
                </>
            )}
            <View style={{ position: 'absolute', bottom: 20, width: '100%', alignItems: 'center' }}>

            {isDeciphering && (
                <View style={{ alignItems: 'center' }}>
                    <ActivityIndicator size="large" color="#007AFF" />
                    <Text style={{ marginTop: 10 }}>מזהה את העיר שלך...</Text>
                </View>

            )}
            </View>
        </View>
    );
}

AppRegistry.registerComponent(appName, () => VoiceCityRecognizer);
