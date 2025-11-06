import { StyleSheet, Dimensions } from 'react-native';
const screenHeight = Dimensions.get('window').height


const styles = StyleSheet.create({
    container_main: {
        marginTop: screenHeight * 0.05,
        padding: 20,
        paddingBottom: 100,
    },
    mainContainer: {
        flex: 1,
        justifyContent: 'center',
        alignItems: 'center',
        padding: 20,
    },

    title: {
        fontSize: 24,
        fontWeight: '600',
        marginBottom: 20,
        textAlign: 'center',
    },
    input: {
        height: 50,
        borderColor: '#ccc',
        borderWidth: 1,
        marginBottom: 15,
        paddingHorizontal: 10,
        borderRadius: 8,
        backgroundColor: '#fff',
    },
    mainButton: {
        backgroundColor: '#007AFF',
        paddingVertical: 15,
        paddingHorizontal: 40,
        borderRadius: 30,
    },

    bigButton: {
        backgroundColor: '#007AFF',
        paddingVertical: 16,
        paddingHorizontal: 32,
        borderRadius: 22,
        marginVertical: 10,
        width: '60%', 
        alignItems: 'center',
        alignSelf: 'center', 
    },

    buttonText: {
        color: 'white',
        fontSize: 20,
        fontWeight: '600',
    },
    signUpButton: {
        position: 'absolute',
        top: 52,
        right: 20,
        padding: 7,
        borderWidth: 2,
        borderColor: '#007AFF', // כחול
        backgroundColor: 'transparent',
        borderRadius: 8,
    },

    signUpText: {
        color: '#007AFF',
        fontSize: 16,
    },

    appName: {
        fontSize: 32,
        fontWeight: 'bold',
        marginBottom: 40,
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
        helpButton: {
            position: 'absolute',
            top: 50,
            left: 20,
            backgroundColor: '#007AFF',
            paddingVertical: 10,
            paddingHorizontal: 16,
            borderRadius: 8,
            zIndex: 10,
        },
        helpText: {
            color: '#fff',
            fontSize: 16,
            fontWeight: '600',
        },
        modalContent: {
            backgroundColor: '#fff',
            padding: 24,
            borderRadius: 12,
            width: '90%',
            maxHeight: '80%',
        },
        sectionTitle: {
            fontSize: 18,
            fontWeight: '600',
            marginTop: 16,
            marginBottom: 6,
            textAlign: 'right',
        },
        body: {
            fontSize: 16,
            lineHeight: 24,
            textAlign: 'right',
        },
        closeButton: {
            marginTop: 24,
            alignSelf: 'center',
            backgroundColor: '#007AFF',
            paddingVertical: 10,
            paddingHorizontal: 20,
            borderRadius: 8,
        },
        closeText: {
            color: '#fff',
            fontSize: 16,
            fontWeight: '600',
        },
    
    startParkingButton: {
        backgroundColor: '#007AFF',
        width: 120,
        height: 120,
        borderRadius: 60,
        justifyContent: 'center',
        alignItems: 'center',
        marginVertical: 10,
    },

    stopParkingButton: {
        backgroundColor: '#800000',
        width: 120,
        height: 120,
        borderRadius: 60,
        justifyContent: 'center',
        alignItems: 'center',
        marginTop: 30,
        marginVertical: 10,
    },

    confirmButton: {
        backgroundColor: '#4CAF50',
        paddingVertical: 14,
        paddingHorizontal: 20,
        borderRadius: 8,
        width: '50%', // narrower
        height: 60,
        alignSelf: 'center',
        marginTop: 20,
    },

    repeatButton: {
        backgroundColor: '#FF9800',
        paddingVertical: 14,
        paddingHorizontal: 20,
        borderRadius: 8,
        width: '50%', // narrower
        height: 60,
        alignSelf: 'center',
        marginTop: 18,
    },

    squareButtonText: {
        color: '#fff',
        fontSize: 20,
        textAlign: 'center',
        fontWeight: '600',
    },

    statusText: {
        fontSize: 16,
        color: 'green',
        marginVertical: 10,
        textAlign: 'center',
    },

    container: {
        flex: 1,
        backgroundColor: '#f2f2f2',
        alignItems: 'center',
        justifyContent: 'center',
        padding: 20,
    },


    overlay: {
        flex: 1,
        backgroundColor: 'rgba(0,0,0,0.3)',
        justifyContent: 'center',
        alignItems: 'center',
    },

    digitGrid: {
        marginTop: 20,
        alignItems: 'center',
    },

    digitRow: {
        flexDirection: 'row',
        justifyContent: 'center',
        marginVertical: 5,
    },

    digitButton: {
        width: 60,
        height: 60,
        marginHorizontal: 10,
        backgroundColor: '#eee',
        justifyContent: 'center',
        alignItems: 'center',
        borderRadius: 30,
        borderWidth: 1,
        borderColor: '#ccc',
    },

    digitText: {
        fontSize: 24,
        fontWeight: 'bold',
    },
});

export default styles;
