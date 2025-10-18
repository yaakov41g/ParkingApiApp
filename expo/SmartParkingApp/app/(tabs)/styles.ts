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
    statusText: {
        fontSize: 16,
        color: 'green',
        marginVertical: 10,
        textAlign: 'center',
    },
    container_main: {
        flex: 1,
        backgroundColor: '#f2f2f2',
        alignItems: 'center',
        justifyContent: 'center',
        padding: 20,
    },
    signUpButton: {
        position: 'absolute',
        top: 40,
        right: 20,
        padding: 10,
    },
    signUpText: {
        color: '#007AFF',
        fontSize: 16,
    },
    title: {
        fontSize: 24,
        fontWeight: '300',
        marginBottom: 10,
    },
    appName: {
        fontSize: 32,
        fontWeight: 'bold',
        marginBottom: 40,
    },
    mainButton: {
        backgroundColor: '#007AFF',
        paddingVertical: 15,
        paddingHorizontal: 40,
        borderRadius: 30,
    },
    mainButtonText: {
        color: '#fff',
        fontSize: 18,
        fontWeight: '600',
    },
    animationContainer: {
        marginTop: 30,
        alignItems: 'center',
    },
    background: {
        flex: 1,
        width: '100%',
        height: '100%',
    },
    overlay: {
        flex: 1,
        backgroundColor: 'rgba(0,0,0,0.3)', // optional dark overlay
        justifyContent: 'center',
        alignItems: 'center',
    },
});
export default styles;
