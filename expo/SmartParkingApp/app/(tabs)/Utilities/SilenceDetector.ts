// mathUtils.ts
import { Audio } from 'expo-av';


type SilenceDetectorCallback = () => void;

export const SilenceDetector = (
    recording: Audio.Recording,
    onSilenceDetected: SilenceDetectorCallback,
    thresholdDb: number = -40,
    durationMs: number = 2000
): void => {
    let silenceTimer: ReturnType<typeof setTimeout> | null = null;

    recording.setOnRecordingStatusUpdate((status) => {
        const volume = status.metering;

        if (volume !== undefined && volume < thresholdDb) {
            if (!silenceTimer) {
                silenceTimer = setTimeout(() => {
                    onSilenceDetected();
                }, durationMs);
            }
        } else {
            if (silenceTimer) {
                clearTimeout(silenceTimer);
                silenceTimer = null;
            }
        }
    });
};
