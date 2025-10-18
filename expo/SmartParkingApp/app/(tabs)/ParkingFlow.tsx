import { useParkingFlow } from './hooks/useParkingFlow';
//import React, { useState } from 'react';
import { View, Text,/* Modal,*/ TouchableOpacity, AppRegistry } from 'react-native';
import { Audio, Video, ResizeMode } from 'expo-av';
import appConfig from '../../app.json';
import styles from './styles';
import { Linking } from 'react-native';
//import { transliterate } from 'hebrew-transliteration';
const appName = appConfig.expo.name;


export default function VoiceCityRecognizer() {
    const {
        cityName,
        cityStatus,
        isIntroPlaying,
        startParkingFlow,
        stopRecording,
        startRecording,
        confirmCity,
    } = useParkingFlow();

    return (<View style={styles.container}>
        <TouchableOpacity style={styles.bigButton} onPress={startParkingFlow}>
            <Text style={styles.squareButtonText}>Start Parking</Text>
        </TouchableOpacity>

        <TouchableOpacity style={styles.bigButton} onPress={stopRecording}>
            <Text style={styles.squareButtonText}>עצור ושלח לשרת</Text>
        </TouchableOpacity>

        {/* New Buttons */}
        <TouchableOpacity style={styles.confirmButton} onPress={confirmCity}>
            <Text style={styles.squareButtonText}>1 - אישור</Text>
        </TouchableOpacity>
        <TouchableOpacity style={styles.repeatButton} onPress={startRecording}>
            <Text style={styles.squareButtonText}>2 - אמור שוב</Text>
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
    </View>


    );
}

AppRegistry.registerComponent(appName, () => VoiceCityRecognizer);

