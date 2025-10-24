export const recordingOptions = {
    android: {
        extension: '.m4a',
        outputFormat: 2, // MPEG_4
        audioEncoder: 3, // AAC
        sampleRate: 44100,
        numberOfChannels: 1,
        bitRate: 128000,
    },
    ios: {
        extension: '.caf',
        audioQuality: 2, // High
        sampleRate: 44100,
        numberOfChannels: 1,
        bitRate: 128000,
        linearPCMBitDepth: 16,
        linearPCMIsBigEndian: false,
        linearPCMIsFloat: false,
    },
    web: {}, // Required for type safety
    isMeteringEnabled: true,
};
