// styles.ts
import { StyleSheet } from 'react-native';

const styles = StyleSheet.create({
    container: { flex: 1, justifyContent: 'center', alignItems: 'center', padding: 20 },
    bigButton: {
        backgroundColor: '#007AFF',
        paddingVertical: 16,
        paddingHorizontal: 32,
        borderRadius: 10,
        marginVertical: 10,
        width: '80%',
        alignItems: 'center',
    },
    buttonText: {
        color: 'white',
        fontSize: 18,
        fontWeight: '600',
    },
    closeButton: {
        backgroundColor: '#FF3B30',
        paddingVertical: 12,
        paddingHorizontal: 24,
        borderRadius: 8,
        marginTop: 20,
        alignItems: 'center',
    },
    confirmButton: {
        backgroundColor: '#4CAF50', // Green
        paddingVertical: 14,
        paddingHorizontal: 30,
        borderRadius: 8,
        width: '90%',
        alignSelf: 'center',
        marginTop: 20,
    },

    repeatButton: {
        backgroundColor: '#FF9800', // Orange
        paddingVertical: 14,
        paddingHorizontal: 30,
        borderRadius: 8,
        width: '90%',
        alignSelf: 'center',
        marginTop: 12,
    },

    squareButtonText: {
        color: '#fff',
        fontSize: 18,
        textAlign: 'center',
        fontWeight: '600',
    },

});
export default styles;
