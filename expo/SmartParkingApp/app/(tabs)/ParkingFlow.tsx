import { useRecordingFlow } from './hooks/useRecordingFlow';
import { useVoiceProcessing } from './hooks/useVoiceProcessing';
import { View, Text, Linking, TouchableOpacity, AppRegistry } from 'react-native';
import { Video, ResizeMode } from 'expo-av';
import appConfig from '../../app.json';
import styles from './styles';
const appName = appConfig.expo.name;


export default function VoiceCityRecognizer() {
    const { cityName, cityStatus, showDigitButtons, startVoiceProcess, convertTextToSpeech, Confirm, HandleDigitPress, StopParkingSession } = useVoiceProcessing();
    const { isIntroPlaying, startParkingFlow, startRecording, stopRecording } = useRecordingFlow(startVoiceProcess);

return (
    <View style={styles.container}>
        {showDigitButtons ? (
            <View style={styles.digitGrid}>
                {/* First row: 1–3 */}
                <View style={styles.digitRow}>
                    {[1, 2, 3].map((digit) => (
                        <TouchableOpacity
                            key={digit}
                            style={styles.digitButton}
                            onPress={() => HandleDigitPress(digit)}
                        >
                            <Text style={styles.digitText}>{digit}</Text>
                        </TouchableOpacity>
                    ))}
                </View>

                {/* Second row: 4–6 */}
                <View style={styles.digitRow}>
                    {[4, 5, 6].map((digit) => (
                        <TouchableOpacity
                            key={digit}
                            style={styles.digitButton}
                            onPress={() => HandleDigitPress(digit)}
                        >
                            <Text style={styles.digitText}>{digit}</Text>
                        </TouchableOpacity>
                    ))}
                </View>

                {/* Third row: 7–9 */}
                <View style={styles.digitRow}>
                    {[7, 8, 9].map((digit) => (
                        <TouchableOpacity
                            key={digit}
                            style={styles.digitButton}
                            onPress={() => HandleDigitPress(digit)}
                        >
                            <Text style={styles.digitText}>{digit}</Text>
                        </TouchableOpacity>
                    ))}
                </View>

                {/* Bottom row: 0 centered */}
                <View style={styles.digitRow}>
                    <View style={{ width: 60 }} />
                    <TouchableOpacity
                        style={styles.digitButton}
                        onPress={() => HandleDigitPress(0)}
                    >
                        <Text style={styles.digitText}>0</Text>
                    </TouchableOpacity>
                    <View style={{ width: 60 }} />
                </View>
            </View>
        ) : (
            <>
                <TouchableOpacity style={styles.bigButton} onPress={startParkingFlow}>
                    <Text style={styles.squareButtonText}>התחל</Text>
                </TouchableOpacity>

                <TouchableOpacity style={styles.bigButton} onPress={stopRecording}>
                    <Text style={styles.squareButtonText}>עצור ושלח לשרת</Text>
                </TouchableOpacity>

                <TouchableOpacity style={styles.confirmButton} onPress={Confirm}>
                    <Text style={styles.squareButtonText}>1 - אישור</Text>
                </TouchableOpacity>

                <TouchableOpacity style={styles.repeatButton} onPress={startRecording}>
                    <Text style={styles.squareButtonText}>2 - אמור שוב</Text>
                </TouchableOpacity>

                {/* ✅ PLACE THE STOP PARKING BUTTON HERE */}
                <TouchableOpacity style={styles.stopParkingButton} onPress={StopParkingSession}>
                    <Text style={styles.squareButtonText}>עצור חניה</Text>
                </TouchableOpacity>
                {cityStatus !== '' && (
                    <Text style={styles.statusText}>{cityStatus}</Text>
                )}

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
                            style={{ width: 120, height: 120, marginTop: 30 }}
                        />
                        <TouchableOpacity onPress={() => Linking.openURL('https://iconscout.com/lottie-animations/audio-wave')}>
                            <Text style={{ textDecorationLine: 'underline', fontSize: 12 }}>
                                Audio Wave by MD. MURADUZZAMAN
                            </Text>
                        </TouchableOpacity>
                    </>
                )}
            </>
        )}
    </View>
);

}

AppRegistry.registerComponent(appName, () => VoiceCityRecognizer);

