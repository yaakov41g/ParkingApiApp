// components/HelpScreen.tsx

import React, { useState } from 'react';
import { View, Text, TouchableOpacity, Modal, StyleSheet, ScrollView } from 'react-native';
import styles from './styles';  
export default function HelpScreen() {
    const [visible, setVisible] = useState(false);

    return (
        <>
            {/* כפתור עזרה */}
            <TouchableOpacity style={styles.helpButton} onPress={() => setVisible(true)}>
                <Text style={styles.helpText}>עזרה</Text>
            </TouchableOpacity>

            {/* חלון עזרה */}
            <Modal visible={visible} transparent animationType="slide">
                <View style={styles.overlay}>
                    <View style={styles.modalContent}>
                        <ScrollView>
                            <Text style={styles.title}>עזרה ואודות</Text>

                            <Text style={styles.sectionTitle}>🅰️ מהי האפליקציה?</Text>
                            <Text style={styles.body}>
                                אוטו פארק היא אפליקציה חדשנית שמאפשרת להתחיל ולסיים חניה באמצעות פקודות קוליות בלבד.
                                אין צורך להקליד, לחפש אזורים או לזכור מספרי חניה – פשוט מדברים, והאפליקציה עושה את השאר.
                            </Text>

                            <Text style={styles.sectionTitle}>🎤 איך משתמשים?</Text>
                            <Text style={styles.body}>
                                1. לחץ על "התחל חניה";{'\n'}
                                2. אמור את שם העיר בקול ברור{'\n'}
                                3. אשר את העיר והאזור המוצע{'\n'}
                                4. החניה נרשמת אוטומטית!{'\n\n'}
                                ניתן גם לבטל או לסיים חניה בכל שלב.
                            </Text>

                            <Text style={styles.sectionTitle}>ℹ️ אודות</Text>
                            <Text style={styles.body}>
                                האפליקציה פותחה כדי להקל על תהליך החניה בערים בישראל, במיוחד עבור נהגים שמעדיפים ממשק קולי.
                                המערכת משתמשת בזיהוי דיבור, ניתוח טקסט, והמרת טקסט לדיבור כדי ליצור חוויית שימוש חלקה.
                            </Text>

                            <Text style={styles.sectionTitle}>📞 תמיכה</Text>
                            <Text style={styles.body}>
                                אם נתקלת בבעיה או יש לך הצעה לשיפור – נשמח לשמוע ממך!
                                {'\n'}שלח מייל ל: support@autopark.co.il
                            </Text>

                            <TouchableOpacity style={styles.closeButton} onPress={() => setVisible(false)}>
                                <Text style={styles.closeText}>סגור</Text>
                            </TouchableOpacity>
                        </ScrollView>
                    </View>
                </View>
            </Modal>
        </>
    );
}
