import styles from './styles';
import React, { useState } from 'react';
import { View, Text, TouchableOpacity, ImageBackground } from 'react-native';
import { router } from 'expo-router';
import { MaterialIcons } from '@expo/vector-icons';

export default function MainScreen() {
    //const navigation = useNavigation();
    const [isStarting, setIsStarting] = useState(false);

    const handleStartParking = () => {
        setIsStarting(true);

        setTimeout(() => {
            setIsStarting(false);
            router.push('/(tabs)/ParkingFlow');

            //navigation.navigate('ParkingFlow'); // Replace with your operational screen route
        }, 1500);
    };

    return (
        <ImageBackground
            source={require('../../assets/images/CopilotParking_background.png')}
            style={styles.background}
            resizeMode="cover"
        >

            <View style={styles.container}>
                {/* Sign Up Button */}
                <TouchableOpacity style={styles.signUpButton}/*{ onPress={() => navigation.navigate('SignUp')}}*/>
                    <Text style={styles.signUpText}>Sign Up</Text>
                </TouchableOpacity>
                {/* Title */}
                <Text style={styles.title}>Welcome to</Text>
                <Text style={styles.appName}>AUTO PARK</Text>

                {/* Start Button */}
                <TouchableOpacity style={styles.mainButton} onPress={handleStartParking} disabled={isStarting}>
                    <View style={{ flexDirection: 'row', alignItems: 'center' }}>
                        <Text style={styles.buttonText}>Start Parking</Text>
                        <MaterialIcons name="login" size={20} color="white" style={{ marginLeft: 10 }} />
                    </View>
                </TouchableOpacity>
                {/* Speaker Animation */}
            </View>
        </ImageBackground>
    );
}
