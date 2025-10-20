import React, { useState } from 'react';
import { View, Text, TextInput, TouchableOpacity, ScrollView } from 'react-native';
import styles from './styles';
import { router } from 'expo-router';

export default function SignUpScreen() {
    const [form, setForm] = useState({
        name: '',
        idNumber: '',
        phoneNumber: '',
        carNumber: '',
        bankNumber: '',
        accountNumber: '',
        creditNumber: ''
    });

    const handleChange = (field: string, value: string) => {
        setForm(prev => ({ ...prev, [field]: value }));
    };

    const handleSubmit = async () => {
        try {
            const response = await fetch('http://192.168.1.2:5203/api/carowner/register', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(form),
            });

            if (!response.ok) {
                const errorText = await response.text();
                console.error('Registration failed:', errorText);
                return;
            }

            const result = await response.json();
            console.log('Registration successful:', result);

            // Navigate to the next screen
            router.push('/(tabs)/ParkingFlow');
        } catch (error) {
            console.error('Network error:', error);
        }
    };

    return (
        <ScrollView contentContainerStyle={styles.container}>
            <Text style={styles.title}>Sign Up</Text>

            {Object.entries(form).map(([field, value]) => (
                <TextInput
                    key={field}
                    style={styles.input}
                    placeholder={field.replace(/([A-Z])/g, ' $1')}
                    value={value}
                    onChangeText={(text) => handleChange(field, text)}
                />
            ))}

            <TouchableOpacity style={styles.mainButton} onPress={handleSubmit}>
                <Text style={styles.buttonText}>Register</Text>
            </TouchableOpacity>
        </ScrollView>
    );
}
